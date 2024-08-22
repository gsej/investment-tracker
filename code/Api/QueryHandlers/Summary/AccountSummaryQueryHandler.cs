using Common;
using Common.Extensions;
using Database;
using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Api.QueryHandlers.Summary;

public static class CacheKeys
{
    public const string CashStatementItems = "CashStatementItems_{0}";
    public const string StockTransactions = "StockTransactions_{0}";
    public const string StockPrices = "StockPrices_{0}";
}

public class AccountSummaryQueryHandler : IAccountSummaryQueryHandler
{
    private readonly ILogger<AccountSummaryQueryHandler> _logger;
    private readonly InvestmentsDbContext _context;
    private readonly IMemoryCache _memoryCache;

    public AccountSummaryQueryHandler(
        ILogger<AccountSummaryQueryHandler> logger, 
        InvestmentsDbContext context, 
        IMemoryCache memoryCache)
    {
        _logger = logger;
        _context = context;
        _memoryCache = memoryCache;
    }
  
    // Summarizes the position of an account or set of accounts on a given day
    public async Task<AccountSummaryResult> Handle(AccountSummaryRequest request)
    {
        var cashStatementItems = await GetCashStatementItems(request.AccountCode);
        var cashBalance = cashStatementItems
                .Where(c => request.Date.DayNumber >= c.Date.DayNumber)
            .Sum(c => c.ReceiptAmountGbp + c.PaymentAmountGbp);

        var holdings = new List<Holding>();
        
        var stockTransactions = await GetStockTransactions(request.AccountCode);
       
        var groupedStockTransactions = stockTransactions
            .Where(s =>
                request.Date.DayNumber >= s.Date.DayNumber)
            .GroupBy(s => s.Stock)
            .ToList();

        foreach (var group in groupedStockTransactions)
        {
            var stock = group.Key;
            
            var stocksAdded = group.Where(st =>
                    st.TransactionType is StockTransactionTypes.Purchase or StockTransactionTypes.TransferIn or StockTransactionTypes.Receipt)
                .Sum(st => st.Quantity);

            var stocksRemoved = group.Where(st =>
                    st.TransactionType is StockTransactionTypes.Sale or StockTransactionTypes.Removal)
                .Sum(st => st.Quantity);

            var totalHeld = stocksAdded - stocksRemoved;

            // TODO: sometimes stock is null. need to find out why and enforce integrity.
            
            if (totalHeld != 0 && stock != null)
            {
                var stockPrices = await GetStockPrices(stock.StockSymbol);
                
                var stockPrice = GetStockPrice(stock.StockSymbol, stockPrices, request.Date);
                
                // TODO: this is nullable because we might not have a value
                decimal? value = null;
                
                string comment = null;

                if (stockPrice.HasPrice)
                {
                    // TODO: deal with currencies
                    if (stockPrice.Currency == "GBP")
                    {
                        value = totalHeld * stockPrice.Price;
                    } 
                    else if (stockPrice.Currency == "GBp")
                    {
                        value = (totalHeld * stockPrice.Price) / 100;
                    }  else if (stockPrice.Currency == "USD")
                    {
                        var temporaryExchangeRate = 1.2667m;
                        value = (totalHeld * stockPrice.Price) / temporaryExchangeRate;
                        comment = "TEMPORARY EXCHANGE RATE FOR USD";
                    }
                    else
                    {
                        comment = "UNKNOWN CURRENCY";
                    }
                }
                else
                {
                    comment = stockPrice.Error;
                }
                
                holdings.Add(new Holding(
                    stock.StockSymbol, 
                    stock.Description, 
                    totalHeld, 
                    stockPrice, 
                    value ?? 0, comment));
            }
        }

        var totalValueInGbp = holdings.Sum(h => h.ValueInGbp) + cashBalance;
        var totalPriceAgeInDays = holdings.Sum(h => h.StockPrice?.AgeInDays ?? 0);

        return new AccountSummaryResult(
            request.AccountCode,
            Holdings: holdings, 
            CashBalanceInGbp: cashBalance, 
            new TotalValue(totalValueInGbp, totalPriceAgeInDays));
    }

    private async Task<IList<CashStatementItem>> GetCashStatementItems(string accountCode)
    {
        var key = string.Format(CacheKeys.CashStatementItems, accountCode);

        if (_memoryCache.TryGetValue(key, out object cached) &&
            cached is IList<CashStatementItem> cashStatementItems)
        {
            _logger.LogInformation($"{key} is in cache");
        }
        else
        {
            _logger.LogInformation($"{key} is not in the cache");
            cashStatementItems = await _context.CashStatementItems
                .Where(c => c.AccountCode == accountCode
                            && c.CashStatementItemType != CashStatementItemTypes.Balance)
                .AsNoTracking()
                .ToListAsync();

            _memoryCache.Set(key, cashStatementItems, 
                new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
        }

        return cashStatementItems;
    }
    
    private async Task<IList<StockTransaction>> GetStockTransactions(string accountCode)
    {
        var key = string.Format(CacheKeys.StockTransactions, accountCode);

        if (_memoryCache.TryGetValue(key, out object cached) &&
            cached is IList<StockTransaction> stockTransactions)
        {
            _logger.LogInformation($"{key} is in cache");
        }
        else
        {
            _logger.LogInformation($"{key} is not in the cache");
            stockTransactions = await _context.StockTransactions
                .Include(st => st.Stock)
                .Where(c => c.AccountCode == accountCode)
             //   .AsNoTracking()
                .ToListAsync();

            _memoryCache.Set(key, stockTransactions, 
                new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
        }

        return stockTransactions;
    }
    
    private async Task<IList<StockPrice>> GetStockPrices(string stockSymbol)
    {
        var key = string.Format(CacheKeys.StockPrices, stockSymbol);

        if (_memoryCache.TryGetValue(key, out object cached) &&
            cached is IList<StockPrice> stockPrices)
        {
            _logger.LogInformation($"{key} is in cache");
        }
        else
        {
            _logger.LogInformation($"{key} is not in the cache");
            stockPrices = await _context.StockPrices
                .Where(c => c.StockSymbol == stockSymbol)
                .AsNoTracking()
                .ToListAsync();

            _memoryCache.Set(key, stockPrices, 
                new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
        }

        return stockPrices;
    }
    
    private StockPriceResult GetStockPrice(
        string stockSymbol,
        IList<StockPrice> stockPrices,
        DateOnly date)
    {
        var stockPrice = stockPrices
            .Where(s =>
                s.Date.DayNumber <= date.DayNumber)
                //s.Date.CompareTo(date) <= 0)
            .OrderByDescending(s => s.Date)
            .FirstOrDefault();

        
        // TODO: abstract this into a fetcher like with exchange rates.
        if (stockPrice != null)
        {
            var requestDate = date;//.ToDateOnly();
            var priceDate = stockPrice.Date;

            var ageInDays = requestDate.DayNumber - priceDate.DayNumber;
            
            return new StockPriceResult(stockPrice.Price, stockPrice.Currency, stockPrice.OriginalCurrency, ageInDays);
        }

        return StockPriceResult.Missing(stockSymbol);
    }
}
