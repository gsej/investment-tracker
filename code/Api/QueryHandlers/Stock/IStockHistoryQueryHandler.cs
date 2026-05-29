namespace Api.QueryHandlers.StockHistory;

public interface IStockHistoryQueryHandler
{
    Task<StockHistoryResult> Handle(StockHistoryRequest request);
}
