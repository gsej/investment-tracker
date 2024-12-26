using Database;
using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Api.QueryHandlers.Fetchers;

public class StockFetcher : IStockFetcher
{
    private const string CacheKey = "StockPs_{0}";
    private readonly IMemoryCache _memoryCache;
    private readonly InvestmentsDbContext _context;

    public StockFetcher(IMemoryCache memoryCache, InvestmentsDbContext context)
    {
        _memoryCache = memoryCache;
        _context = context;
    }

    public async Task<IList<Stock>> GetStocks()
    {
        if (_memoryCache.TryGetValue(CacheKey, out object cached) && cached is IList<Stock> stocks)
        {
            return stocks;
        }
        stocks = await _context.Stocks
            .AsNoTracking()
            .ToListAsync();

        _memoryCache.Set(CacheKey, stocks,
            new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60) });

        return stocks;
    }
}
