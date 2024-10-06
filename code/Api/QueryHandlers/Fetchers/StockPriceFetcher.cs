using Database;
using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Api.QueryHandlers.Fetchers;

public class StockPriceFetcher : IStockPriceFetcher
{
    private const string CacheKey = "StockPrices_{0}";
    private readonly IMemoryCache _memoryCache;
    private readonly InvestmentsDbContext _context;

    public StockPriceFetcher(IMemoryCache memoryCache, InvestmentsDbContext context)
    {
        _memoryCache = memoryCache;
        _context = context;
    }

    public async Task<StockPriceResult> GetStockPrice(string stockSymbol, DateOnly requestedDate)
    {
        var prices = await GetStockPrices(stockSymbol);
   
        var stockPrice = prices
            .Where(s =>
                s.Date.DayNumber <= requestedDate.DayNumber)
            .OrderByDescending(s => s.Date)
            .FirstOrDefault();
     
        if (stockPrice != null)
        {
            var ageInDays = requestedDate.DayNumber - stockPrice.Date.DayNumber;
            return new StockPriceResult(stockPrice.Price, stockPrice.Currency, stockPrice.OriginalCurrency, ageInDays);
        }

        return StockPriceResult.Missing(stockSymbol);
    }

    private async Task<IList<StockPrice>> GetStockPrices(string stockSymbol)
    {
        var key = string.Format(CacheKey, stockSymbol);

        if (_memoryCache.TryGetValue(key, out object cached) && cached is IList<StockPrice> stockPrices)
        {
            return stockPrices;
        }

        stockPrices = await _context.StockPrices
            .Where(stockPrice => stockPrice.StockSymbol == stockSymbol)
            .OrderByDescending(stockPrice => stockPrice.Date)
            .AsNoTracking()
            .ToListAsync();

        _memoryCache.Set(key, stockPrices,
            new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60) });

        return stockPrices;
    }
}
