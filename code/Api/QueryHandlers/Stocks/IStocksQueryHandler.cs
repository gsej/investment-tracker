namespace Api.QueryHandlers.Stocks;

public interface IStocksQueryHandler
{
    Task<IReadOnlyList<StockResult>> Handle();
}
