namespace Api.QueryHandlers.Fetchers;

public interface IStockPriceFetcher
{
    Task<StockPriceResult> GetStockPrice(string stockSymbol, DateOnly requestedDate);
}
