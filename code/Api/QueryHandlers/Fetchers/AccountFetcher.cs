using Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Api.QueryHandlers.Fetchers;

public class AccountFetcher : IAccountFetcher
{
    private const string CacheKey = "Accounts";
    private readonly IMemoryCache _memoryCache;
    private readonly InvestmentsDbContext _context;

    public AccountFetcher(IMemoryCache memoryCache, InvestmentsDbContext context)
    {
        _memoryCache = memoryCache;
        _context = context;
    }

    public async Task<IList<Database.Entities.Account>> GetAccounts()
    {
        if (_memoryCache.TryGetValue(CacheKey, out object cached) && cached is IList<Database.Entities.Account> accounts)
        {
            return accounts;
        }

        accounts = await _context.Accounts
            .AsNoTracking()
            .ToListAsync();

        _memoryCache.Set(CacheKey, accounts,
            new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60) });

        return accounts;
    }
}
