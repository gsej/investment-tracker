using Database.Entities;

namespace Api.QueryHandlers.Fetchers;

public interface IStockTransactionFetcher
{
    Task<IList<StockTransaction>> GetStockTransactions(string accountCode);
}
