using Database.Entities;

namespace Api.QueryHandlers.Fetchers;

public interface IStockFetcher
{
    Task<IList<Stock>> GetStocks();
}
