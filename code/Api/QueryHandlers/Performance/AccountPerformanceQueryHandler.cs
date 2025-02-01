using Api.QueryHandlers.Fetchers;
using Api.QueryHandlers.History;
using Api.QueryHandlers.Portfolio;
using Common;
using Database.Entities;

namespace Api.QueryHandlers.Performance;

// returns a summary of account performance, over the years.
// really just a place to sit some performance metrics for now. 

public class AccountPerformanceQueryHandler : IAccountPerformanceQueryHandler
{
    private readonly ILogger<AccountPerformanceQueryHandler> _logger;
    private readonly IAccountValueHistoryQueryHandler _accountValueHistoryQueryHandler;

    private readonly IStockFetcher _stockFetcher;
    private readonly IStockPriceFetcher _stockPriceFetcher;
    private readonly ICashStatementItemFetcher _cashStatementItemFetcher;
    private readonly IStockTransactionFetcher _stockTransactionFetcher;
    
    private IList<Stock> _stocks;

    public AccountPerformanceQueryHandler(
        ILogger<AccountPerformanceQueryHandler> logger,
        IAccountValueHistoryQueryHandler accountValueHistoryQueryHandler
        // IStockFetcher stockFetcher,
        // IStockPriceFetcher stockPriceFetcher, 
        // ICashStatementItemFetcher cashStatementItemFetcher, 
        // IStockTransactionFetcher stockTransactionFetcher


    )
    {
        _logger = logger;
        _accountValueHistoryQueryHandler = accountValueHistoryQueryHandler;
    }


    public async Task<AccountPerformanceResult> Handle(AccountPerformanceRequest request)
    {
        var accountValueHistory = await _accountValueHistoryQueryHandler
            .Handle(new AccountValueHistoryRequest(request.AccountCode, request.QueryDate));
        
        var items = accountValueHistory.Items.OrderBy(i => i.Date).ToList();

        // TODO: check when there are no items.
        
        var startYear = items.First().Date.Year;
        var endYear = items.Last().Date.Year;
        
        var performanceValues = new List<AccountPerformanceValue>();

        decimal previousYearClosingValue = 0; // TODO: value can't be less than 0. Enforce this.

        decimal? previousYearUnitValue = 100; // TODO: make sure this is correct, the value comes from a hardcoded assumption. 
        
        for (int year = startYear; year <= endYear; year++)
        {
            var matchingItems = items.Where(i => i.Date.Year == year).ToList();
            var inflows = matchingItems.Sum(i => i.Inflows);
            
            var closingValue = matchingItems.OrderBy(i => i.Date).Last().ValueInGbp;
            var growth = closingValue - previousYearClosingValue - inflows;
            previousYearClosingValue = closingValue;

            var closingUnitValue = matchingItems.OrderBy(i => i.Date).Last().Units.ValueInGbpPerUnit;

            var unitValueChange = (closingUnitValue - previousYearUnitValue) / previousYearUnitValue;
            previousYearUnitValue = closingUnitValue;
            
            performanceValues.Add(new AccountPerformanceValue(Period: year.ToString(), request.AccountCode, inflows, growth, unitValueChange.Value));; 
        }
        
        var result = new AccountPerformanceResult(performanceValues);

        return result;
        
        
        // _stocks = await _stockFetcher.GetStocks();
        //
        // var cashBalance = await GetCashBalance(request);
        // var holdings = await GetHoldings(request);
        // var inflows = await GetInflows(request);
        //
        // var totalValueInGbp = holdings.Sum(h => h.ValueInGbp) + cashBalance;
        // var totalPriceAgeInDays = holdings.Sum(h => h.StockPrice?.AgeInDays ?? 0);
        //
        // var allocations = GetAllocations(holdings, totalValueInGbp, cashBalance);
        //
        // return new AccountPortfolioResult(
        //     request.AccountCode,
        //     Holdings: holdings,
        //     CashBalanceInGbp: cashBalance,
        //     Contributions: inflows, // TODO: rename to inflows
        //     Allocations: allocations,
        //     new TotalValue(totalValueInGbp, totalPriceAgeInDays)
        // );
        return null;
    }
    //
    // private async Task<List<Holding>> GetHoldings(AccountPortfolioRequest request)
    // {
    //     var holdings = new List<Holding>();
    //     
    //     var stockTransactions = await _stockTransactionFetcher.GetStockTransactions(request.AccountCode);
    //    
    //     var groupedStockTransactions = stockTransactions
    //         .Where(s =>
    //             request.Date.DayNumber >= s.Date.DayNumber)
    //         .GroupBy(s => s.StockSymbol)
    //         .ToList();
    //
    //     foreach (var group in groupedStockTransactions)
    //     {
    //         var stockSymbol = group.Key;
    //         
    //         var stocksAdded = group.Where(st =>
    //                 st.TransactionType is StockTransactionTypes.Purchase or StockTransactionTypes.TransferIn or StockTransactionTypes.Receipt)
    //             .Sum(st => st.Quantity);
    //
    //         var stocksRemoved = group.Where(st =>
    //                 st.TransactionType is StockTransactionTypes.Sale or StockTransactionTypes.Removal)
    //             .Sum(st => st.Quantity);
    //
    //         var totalHeld = stocksAdded - stocksRemoved;
    //         
    //         if (totalHeld != 0)
    //         {
    //             var stock = _stocks.Single(s => s.StockSymbol == stockSymbol);
    //
    //             var holding = new Holding(
    //                 stock.StockSymbol,
    //                 stock.Description,
    //                 stock.Allocation,
    //                 totalHeld);
    //
    //             await PriceHolding(holding, request.Date);
    //
    //             holdings.Add(holding);
    //         }
    //     }
    //
    //     return holdings;
    // }
    //
    // private async Task PriceHolding(Holding holding, DateOnly requestDate)
    // {
    //     var stockPrice = await _stockPriceFetcher.GetStockPrice(holding.StockSymbol, requestDate);
    //
    //     if (stockPrice.HasPrice)
    //     {
    //         var value = holding.Quantity * stockPrice.Price!.Value;
    //
    //         holding.StockPrice = stockPrice;
    //         holding.ValueInGbp = value;
    //     }
    //     else
    //     {
    //         holding.Comment = stockPrice.Error;
    //     }
    // }
    //
    // private async Task<decimal> GetCashBalance(AccountPortfolioRequest request)
    // {
    //     var cashStatementItems = await _cashStatementItemFetcher.GetCashStatementItems(request.AccountCode);
    //
    //     var cashBalance = cashStatementItems
    //         .Where(c => request.Date.DayNumber >= c.Date.DayNumber)
    //         .Sum(c => c.ReceiptAmountGbp + c.PaymentAmountGbp);
    //     return cashBalance;
    // }
    //
    // private async Task<decimal> GetInflows(AccountPortfolioRequest request)
    // {
    //     // Returns contributions made on the query date
    //     var cashStatementItems = await _cashStatementItemFetcher.GetCashStatementItems(request.AccountCode);
    //     
    //     var inflows = cashStatementItems.Where(c => c.Date.DayNumber == request.Date.DayNumber && (
    //             c.CashStatementItemType == CashStatementItemTypes.Contribution || c.CashStatementItemType == CashStatementItemTypes.Withdrawal))
    //         .Sum(c => c.ReceiptAmountGbp + c.PaymentAmountGbp);
    //     
    //     return inflows;
    // }
    //
    // private static List<Allocation> GetAllocations(List<Holding> holdings, decimal totalValueInGbp, decimal cashBalance)
    // {
    //     var allocations = holdings
    //         .GroupBy(h => h.Allocation)
    //         .Select(g => new Allocation(
    //             g.Key, 
    //             g.Sum(h => h.ValueInGbp), 
    //             totalValueInGbp != 0 ? g.Sum(h => h.ValueInGbp) / totalValueInGbp : 0))
    //         .ToList();
    //
    //     if (cashBalance > 0)
    //     {
    //         allocations.Add(new Allocation("Cash", cashBalance, cashBalance / totalValueInGbp));
    //     }
    //
    //     return allocations;
    // }
}
