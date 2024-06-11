using Database.Entities;

namespace DataLoaders.StockTransactionEnrichers;

public interface IStockTransactionEnricher
{
    void Enrich(StockTransaction stockTransaction);
}