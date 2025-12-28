using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Entities;

namespace Database.Repositories;

public interface IStockRepository
{
    Task<IList<Stock>> GetStocks();
    
    void Add(Stock stock);
    
    Task<int> SaveChangesAsync();
}
