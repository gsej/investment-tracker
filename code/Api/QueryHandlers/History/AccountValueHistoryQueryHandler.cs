using Api.QueryHandlers.Fetchers;
using Common;
using Database.Entities;

namespace Api.QueryHandlers.History;

public class AccountValueHistoryQueryHandler : IAccountValueHistoryQueryHandler
{
    private readonly IAccountFetcher _accountFetcher;
    private readonly ICashStatementItemFetcher _cashStatementItemFetcher;
    private readonly IStockTransactionFetcher _stockTransactionFetcher;
    private readonly IStockPriceFetcher _stockPriceFetcher;
    private readonly IRecordedTotalValueQueryHandler _recordedTotalValueQueryHandler;
    private readonly ILogger<AccountValueHistoryQueryHandler> _logger;

    public AccountValueHistoryQueryHandler(
        IAccountFetcher accountFetcher,
        ICashStatementItemFetcher cashStatementItemFetcher,
        IStockTransactionFetcher stockTransactionFetcher,
        IStockPriceFetcher stockPriceFetcher,
        IRecordedTotalValueQueryHandler recordedTotalValueQueryHandler,
        ILogger<AccountValueHistoryQueryHandler> logger)
    {
        _accountFetcher = accountFetcher;
        _cashStatementItemFetcher = cashStatementItemFetcher;
        _stockTransactionFetcher = stockTransactionFetcher;
        _stockPriceFetcher = stockPriceFetcher;
        _recordedTotalValueQueryHandler = recordedTotalValueQueryHandler;
        _logger = logger;
    }

    // Returns the total value of the account at the end of each day in the date range.
    // Walks the date range once, maintaining running cash balance and holdings, applying each
    // cash item / stock transaction exactly once as its date is crossed. Per-stock price
    // cursors advance forward through the date range, so each price is examined at most once.
    public async Task<AccountValueHistoryResult> Handle(AccountValueHistoryRequest request)
    {
        var accountCode = request.AccountCode;

        var account = (await _accountFetcher.GetAccounts()).SingleOrDefault(a => a.AccountCode == accountCode);

        if (account == null)
        {
            _logger.LogError("The account with code {AccountCode} does not exist", accountCode);
            throw new InvalidOperationException($"The account with code {accountCode} does not exist");
        }

        var cashItems = (await _cashStatementItemFetcher.GetCashStatementItems([accountCode]))
            .OrderBy(c => c.Date)
            .ToList();

        var stockTransactions = (await _stockTransactionFetcher.GetStockTransactions([accountCode]))
            .OrderBy(t => t.Date)
            .ToList();

        var recordedTotalValues = (await _recordedTotalValueQueryHandler.Handle(new RecordedTotalValuesRequest(accountCode)))
            .RecordedTotalValues
            .ToDictionary(r => r.Date);

        var priceCursors = await BuildPriceCursors(stockTransactions);

        var results = new List<AccountHistoricalValue>();

        var cashIndex = 0;
        var transactionIndex = 0;
        var cashBalance = 0m;
        var holdings = new Dictionary<string, decimal>();

        decimal? previousDayTotal = null;

        for (var currentDate = account.OpeningDate; currentDate <= request.QueryDate; currentDate = currentDate.AddDays(1))
        {
            var todayContributions = 0m;

            while (cashIndex < cashItems.Count && cashItems[cashIndex].Date <= currentDate)
            {
                var item = cashItems[cashIndex];
                cashBalance += item.ReceiptAmountGbp + item.PaymentAmountGbp;

                if (item.Date == currentDate && IsContribution(item.CashStatementItemType))
                {
                    todayContributions += item.ReceiptAmountGbp + item.PaymentAmountGbp;
                }

                cashIndex++;
            }

            while (transactionIndex < stockTransactions.Count && stockTransactions[transactionIndex].Date <= currentDate)
            {
                var txn = stockTransactions[transactionIndex];
                ApplyTransaction(holdings, txn);

                if (txn.Date == currentDate && txn.TransactionType == StockTransactionTypes.TransferIn)
                {
                    todayContributions += txn.AmountGbp;
                }

                transactionIndex++;
            }

            var (stockValue, priceAgeInDays, comment) = ValueHoldings(holdings, priceCursors, currentDate);

            var totalValue = cashBalance + stockValue;

            var historicalValue = new AccountHistoricalValue(
                currentDate,
                totalValue,
                todayContributions,
                priceAgeInDays,
                comment);

            if (recordedTotalValues.TryGetValue(currentDate, out var recorded))
            {
                historicalValue.RecordedTotalValueInGbp = recorded.TotalValueInGbp;
                historicalValue.RecordedTotalValueSource = recorded.Source;

                if (historicalValue.ValueInGbp != 0)
                {
                    historicalValue.DiscrepancyRatio = (historicalValue.ValueInGbp - historicalValue.RecordedTotalValueInGbp) / historicalValue.ValueInGbp;
                }
            }

            if (previousDayTotal.HasValue)
            {
                historicalValue.DifferenceToPreviousDay = historicalValue.ValueInGbp - historicalValue.NetInflows - previousDayTotal.Value;

                if (previousDayTotal.Value != 0)
                {
                    historicalValue.DifferenceRatio = historicalValue.DifferenceToPreviousDay / previousDayTotal.Value;
                }
            }

            results.Add(historicalValue);
            previousDayTotal = historicalValue.ValueInGbp;
        }

        var unitValues = new UnitCalculator().Calculate(results, 100);

        foreach (var result in results)
        {
            var matchingUnitValues = unitValues.SingleOrDefault(u => u.Date == result.Date);
            result.Units = matchingUnitValues ?? new UnitAccount(result.Date, null, null);
        }

        return new AccountValueHistoryResult(results, []);
    }

    private static bool IsContribution(string cashStatementItemType) =>
        cashStatementItemType == CashStatementItemTypes.Contribution ||
        cashStatementItemType == CashStatementItemTypes.TaxRelief ||
        cashStatementItemType == CashStatementItemTypes.Withdrawal ||
        cashStatementItemType == CashStatementItemTypes.TransferIn;

    private static void ApplyTransaction(Dictionary<string, decimal> holdings, StockTransaction txn)
    {
        if (!holdings.ContainsKey(txn.StockSymbol))
            holdings[txn.StockSymbol] = 0m;

        if (txn.TransactionType is StockTransactionTypes.Purchase or StockTransactionTypes.TransferIn or StockTransactionTypes.Receipt)
            holdings[txn.StockSymbol] += txn.Quantity;
        else if (txn.TransactionType is StockTransactionTypes.Sale or StockTransactionTypes.Removal)
            holdings[txn.StockSymbol] -= txn.Quantity;
    }

    private async Task<Dictionary<string, PriceCursor>> BuildPriceCursors(IList<StockTransaction> stockTransactions)
    {
        var cursors = new Dictionary<string, PriceCursor>();

        var symbols = stockTransactions.Select(t => t.StockSymbol).Distinct();

        foreach (var symbol in symbols)
        {
            // The cached list is sorted by SQL Server's (Date desc, StockPriceId desc) — SQL Server's
            // GUID ordering differs from .NET's, so we can't re-sort by ID in memory and get the same
            // tie-break. Reversing preserves SQL Server's within-date ID order, giving us asc-by-date
            // with the cursor's "last same-date item" matching the orig handler's pick.
            var prices = (await _stockPriceFetcher.GetAllPrices(symbol)).ToList();
            prices.Reverse();
            cursors[symbol] = new PriceCursor(prices);
        }

        return cursors;
    }

    private static (decimal stockValue, int priceAgeInDays, string comment) ValueHoldings(
        Dictionary<string, decimal> holdings,
        Dictionary<string, PriceCursor> priceCursors,
        DateOnly date)
    {
        var stockValue = 0m;
        var priceAgeInDays = 0;
        var errors = new List<string>();

        foreach (var (symbol, quantity) in holdings)
        {
            if (quantity == 0m) continue;

            var cursor = priceCursors[symbol];
            var price = cursor.AdvanceTo(date);

            if (price != null)
            {
                stockValue += quantity * price.Price;
                priceAgeInDays += date.DayNumber - price.Date.DayNumber;
            }
            else
            {
                errors.Add($"Missing:{symbol}");
            }
        }

        return (stockValue, priceAgeInDays, string.Join(", ", errors));
    }

    // Walks a stock's prices ascending by date. AdvanceTo(d) moves the cursor forward to the
    // latest price with date <= d, or null if no such price exists. Calls must be made with
    // non-decreasing dates.
    private class PriceCursor
    {
        private readonly IList<StockPrice> _prices;
        private int _index = -1;
        private DateOnly _lastDate = DateOnly.MinValue;

        public PriceCursor(IList<StockPrice> prices)
        {
            _prices = prices;
        }

        public StockPrice AdvanceTo(DateOnly date)
        {
            if (date < _lastDate)
                throw new InvalidOperationException($"PriceCursor.AdvanceTo called with date {date} which is before previous date {_lastDate}.");

            _lastDate = date;

            while (_index + 1 < _prices.Count && _prices[_index + 1].Date <= date)
                _index++;

            return _index >= 0 ? _prices[_index] : null;
        }
    }
}
