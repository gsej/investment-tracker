using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Entities;

namespace Database.Repositories;

public interface IStockPriceRepository
{
    void Add(StockPrice stockPrice);
    Task<int> SaveChangesAsync();
    Task<IList<StockPrice>> GetAll(string stockSymbol);
}
