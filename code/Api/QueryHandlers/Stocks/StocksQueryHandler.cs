using Api.QueryHandlers.Fetchers;

namespace Api.QueryHandlers.Stocks;

public class StocksQueryHandler : IStocksQueryHandler
{
    private readonly IStockFetcher _stockFetcher;

    public StocksQueryHandler(IStockFetcher stockFetcher)
    {
        _stockFetcher = stockFetcher;
    }

    public async Task<IReadOnlyList<StockResult>> Handle()
    {
        var stocks = await _stockFetcher.GetStocks();
        return stocks
            .OrderBy(s => s.Description)
            .Select(s => new StockResult(s.StockSymbol, s.Description, s.Benchmark))
            .ToList();
    }
}
