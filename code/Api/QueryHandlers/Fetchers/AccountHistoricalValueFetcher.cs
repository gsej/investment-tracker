using Database;
using Microsoft.EntityFrameworkCore;

namespace Api.QueryHandlers.Fetchers;

public class AccountHistoricalValueFetcher : IAccountHistoricalValueFetcher
{
    private readonly InvestmentsDbContext _context;

    public AccountHistoricalValueFetcher(InvestmentsDbContext context)
    {
        _context = context;
    }

    public async Task<IList<Database.Entities.AccountHistoricalValue>> Get(string accountCode)
    {
        // TODO: could be cached, or restricted by date
        
        var values = await _context.AccountHistoricalValues
            .AsNoTracking()
            .Where(account => account.AccountCode == accountCode)
            .ToListAsync();

        return values;
    }
}
