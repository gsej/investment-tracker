using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly InvestmentsDbContext _context;

    public AccountRepository(InvestmentsDbContext context)
    {
        _context = context;
    }
    
    public async Task<IList<Account>> GetAll()
    {
        return await _context.Accounts.ToListAsync();
    }

    public void Add(Account account)
    {
        _context.Add(account);
    }
    
    
    public async Task<int> SaveChangesAsync()
    {
        // TODO: move to UoW
        return await _context.SaveChangesAsync();
    }
}
