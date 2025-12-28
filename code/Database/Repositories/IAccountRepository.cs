using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Entities;

namespace Database.Repositories;

public interface IAccountRepository
{
    void Add(Account account);

    Task<IList<Account>> GetAll();
    
    Task<int> SaveChangesAsync();
}
