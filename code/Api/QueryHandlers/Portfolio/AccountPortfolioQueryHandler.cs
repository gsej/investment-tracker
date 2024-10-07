using Api.QueryHandlers.Fetchers;
using Common;
using Database.Entities;

namespace Api.QueryHandlers.Portfolio;

public class AccountPortfolioQueryHandler : IAccountPortfolioQueryHandler
{
    private readonly ILogger<AccountPortfolioQueryHandler> _logger;
    private readonly IStockFetcher _stockFetcher;
    private readonly IStockPriceFetcher _stockPriceFetcher;
    private readonly ICashStatementItemFetcher _cashStatementItemFetcher;
    private readonly IStockTransactionFetcher _stockTransactionFetcher;
    private IList<Stock> _stocks;

    public AccountPortfolioQueryHandler(
        ILogger<AccountPortfolioQueryHandler> logger, 
        IStockFetcher stockFetcher,
        IStockPriceFetcher stockPriceFetcher, 
        ICashStatementItemFetcher cashStatementItemFetcher, 
        IStockTransactionFetcher stockTransactionFetcher)
    {
        _logger = logger;
        _stockFetcher = stockFetcher;
        _stockPriceFetcher = stockPriceFetcher;
        _cashStatementItemFetcher = cashStatementItemFetcher;
        _stockTransactionFetcher = stockTransactionFetcher;
    }
  
    // Summarizes the position of an account or set of accounts on a given day
    public async Task<AccountPortfolioResult> Handle(AccountPortfolioRequest request)
    {
        _stocks = await _stockFetcher.GetStocks();
        var cashBalance = await GetCashBalance(request);
        var holdings = await GetHoldings(request);

        var contributions = await GetContributions(request);

        await PriceHoldings(holdings, request.Date);

        var totalValueInGbp = holdings.Sum(h => h.ValueInGbp) + cashBalance;
        var totalPriceAgeInDays = holdings.Sum(h => h.StockPrice?.AgeInDays ?? 0);
        
        var allocations = GetAllocations(holdings, totalValueInGbp, cashBalance);

        return new AccountPortfolioResult(
            request.AccountCode,
            Holdings: holdings, 
            CashBalanceInGbp: cashBalance,
            Contributions: contributions,
            new TotalValue(totalValueInGbp, totalPriceAgeInDays),
            
            Allocations: allocations);
    }

    private static List<Allocation> GetAllocations(List<Holding> holdings, decimal totalValueInGbp, decimal cashBalance)
    {
        var allocations = holdings
            .GroupBy(h => h.Allocation)
            .Select(g => new Allocation(
                g.Key, 
                g.Sum(h => h.ValueInGbp), 
                totalValueInGbp != 0 ? g.Sum(h => h.ValueInGbp) / totalValueInGbp : 0))
            .ToList();

        if (cashBalance > 0)
        {
            allocations.Add(new Allocation("Cash", cashBalance, cashBalance / totalValueInGbp));
        }

        return allocations;
    }

    private async Task<List<Holding>> GetHoldings(AccountPortfolioRequest request)
    {
        var holdings = new List<Holding>();
        
        var stockTransactions = await _stockTransactionFetcher.GetStockTransactions(request.AccountCode);
       
        var groupedStockTransactions = stockTransactions
            .Where(s =>
                request.Date.DayNumber >= s.Date.DayNumber)
            .GroupBy(s => s.StockSymbol)
            .ToList();

        foreach (var group in groupedStockTransactions)
        {
            var stockSymbol = group.Key;
            
            var stocksAdded = group.Where(st =>
                    st.TransactionType is StockTransactionTypes.Purchase or StockTransactionTypes.TransferIn or StockTransactionTypes.Receipt)
                .Sum(st => st.Quantity);

            var stocksRemoved = group.Where(st =>
                    st.TransactionType is StockTransactionTypes.Sale or StockTransactionTypes.Removal)
                .Sum(st => st.Quantity);

            var totalHeld = stocksAdded - stocksRemoved;

            // TODO: sometimes stock is null. need to find out why and enforce integrity.
            
            if (stockSymbol == null)
            {
                _logger.LogError($"Stock is null for account {request.AccountCode}");
                throw new InvalidOperationException($"Stock is null for account {request.AccountCode}");
            }
            
            if (totalHeld != 0)
            {
                var stock = _stocks.Single(s => s.StockSymbol == stockSymbol);
                
                holdings.Add(new Holding(
                    stock.StockSymbol,
                    stock.Description,
                    stock.Allocation,
                    totalHeld));
            }
        }

        return holdings;
    }

    private async Task PriceHoldings(IList<Holding> holdings, DateOnly requestDate)
    {
        foreach (var holding in holdings)
        {
            var stockPrice = await _stockPriceFetcher.GetStockPrice(holding.StockSymbol, requestDate);

            if (stockPrice.HasPrice)
            {
                var value = holding.Quantity * stockPrice.Price!.Value;
                
                holding.StockPrice = stockPrice;
                holding.ValueInGbp = value;
            }
            else
            {
                holding.Comment = stockPrice.Error;
            }
        }
    }

    private async Task<decimal> GetCashBalance(AccountPortfolioRequest request)
    {
        var cashStatementItems = await _cashStatementItemFetcher.GetCashStatementItems(request.AccountCode);

        var cashBalance = cashStatementItems
            .Where(c => request.Date.DayNumber >= c.Date.DayNumber)
            .Sum(c => c.ReceiptAmountGbp + c.PaymentAmountGbp);
        return cashBalance;
    }
    
    private async Task<decimal> GetContributions(AccountPortfolioRequest request)
    {
        // Returns contributions for the day in question
        var cashStatementItems = await _cashStatementItemFetcher.GetCashStatementItems(request.AccountCode);
        
        var contribution = cashStatementItems.Where(c => c.Date.DayNumber == request.Date.DayNumber && c.CashStatementItemType == CashStatementItemTypes.Contribution)
            .Sum(c => c.ReceiptAmountGbp);

        return contribution;
    }
}
