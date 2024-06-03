using Database;
using Microsoft.EntityFrameworkCore;

namespace Api.QueryHandlers.Account;

public record struct AccountRequest;

public record Account(string AccountCode);

public class AccountQueryHandler : IAccountQueryHandler
{
    private readonly InvestmentsDbContext _context;

    public AccountQueryHandler(InvestmentsDbContext context)
    {
        _context = context;
    }
    
    public async Task<IList<Account>> Handle(AccountRequest _)
    {
        var accounts = await _context.Accounts
            .AsNoTracking()
            .Select(a => new Account(a.AccountCode))
            .ToListAsync();

        return accounts;
    }
}
