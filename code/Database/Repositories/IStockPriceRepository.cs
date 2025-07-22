using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Entities;

namespace Database.Repositories;

public interface IStockPriceRepository
{
    Task BulkAdd(IEnumerable<StockPrice> stockPrices);
    Task<IList<StockPrice>> GetAll(string stockSymbol);
}
