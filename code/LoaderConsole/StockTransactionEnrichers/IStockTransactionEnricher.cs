using Database.Entities;

namespace LoaderConsole.StockTransactionEnrichers;

public interface IStockTransactionEnricher
{
    void Enrich(StockTransaction stockTransaction);
}