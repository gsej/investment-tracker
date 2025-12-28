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

    public async Task<IList<Database.Entities.AccountHistoricalValue>> Get(string[] accountCodes)
    {        
        var values = await _context.AccountHistoricalValues
            .AsNoTracking()
            .Where(account => accountCodes.Contains(account.AccountCode))
            .ToListAsync();

        return values;
    }
}
