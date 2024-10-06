using Common;
using Database;
using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Api.QueryHandlers.Fetchers;

public class CashStatementItemFetcher : ICashStatementItemFetcher
{
    private const string CacheKey = "CashStatementItems_{0}";
    private readonly IMemoryCache _memoryCache;
    private readonly InvestmentsDbContext _context;

    public CashStatementItemFetcher(IMemoryCache memoryCache, InvestmentsDbContext context)
    {
        _memoryCache = memoryCache;
        _context = context;
    }

    public async Task<IList<CashStatementItem>> GetCashStatementItems(string accountCode)
    {
        var key = string.Format(CacheKey, accountCode);

        if (_memoryCache.TryGetValue(key, out object cached) && cached is IList<CashStatementItem> cashStatementItems)
        {
            return cashStatementItems;
        }

        cashStatementItems = await _context.CashStatementItems
            .Where(c => c.AccountCode == accountCode
                        && c.CashStatementItemType != CashStatementItemTypes.Balance)
            .AsNoTracking()
            .ToListAsync();

        _memoryCache.Set(key, cashStatementItems,
            new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60) });

        return cashStatementItems;
    }
}
