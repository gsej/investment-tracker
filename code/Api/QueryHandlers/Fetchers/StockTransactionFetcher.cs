using Database;
using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Api.QueryHandlers.Fetchers;

public class StockTransactionFetcher : IStockTransactionFetcher
{
    private const string CacheKey = "StockTransactions_{0}";
    private readonly IMemoryCache _memoryCache;
    private readonly InvestmentsDbContext _context;

    public StockTransactionFetcher(IMemoryCache memoryCache, InvestmentsDbContext context)
    {
        _memoryCache = memoryCache;
        _context = context;
    }
    
    public async Task<IList<StockTransaction>> GetStockTransactions(string[] accountCodes)
    {
        var stockTransactions = new List<StockTransaction>();
        foreach (var accountCode in accountCodes)
        {
            var transactions = await GetStockTransactionsForAccount(accountCode);
            stockTransactions.AddRange(transactions);
        }
        return stockTransactions;
    }

    private async Task<IList<StockTransaction>> GetStockTransactionsForAccount(string accountCode)
    {
        var key = string.Format(CacheKey, accountCode);

        if (_memoryCache.TryGetValue(key, out object cached) &&
            cached is IList<StockTransaction> stockTransactions)
        {
            return stockTransactions;
        }

        stockTransactions = await _context.StockTransactions
            .Include(st => st.Stock)
            .Where(c => c.AccountCode == accountCode)
            .AsNoTracking()
            .ToListAsync();

        _memoryCache.Set(key, stockTransactions,
            new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60) });

        return stockTransactions;
    }
}
