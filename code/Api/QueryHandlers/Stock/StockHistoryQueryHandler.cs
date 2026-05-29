using Api.QueryHandlers.Fetchers;
using Api.QueryHandlers.History;

namespace Api.QueryHandlers.StockHistory;

public class StockHistoryQueryHandler : IStockHistoryQueryHandler
{
    private readonly IStockPriceFetcher _stockPriceFetcher;

    public StockHistoryQueryHandler(IStockPriceFetcher stockPriceFetcher)
    {
        _stockPriceFetcher = stockPriceFetcher;
    }

    public async Task<StockHistoryResult> Handle(StockHistoryRequest request)
    {
        var rawPrices = await _stockPriceFetcher.GetAllPrices(request.StockSymbol);

        var deduplicated = rawPrices
            .GroupBy(p => p.Date)
            .Select(g => g.First())
            .OrderBy(p => p.Date)
            .ToList();

        var prices = deduplicated
            .Select(p => new StockPriceResult(p.Date, p.Price))
            .ToList();

        var unitValues = deduplicated
            .Select(p => new UnitAccount(p.Date, 1, p.Price))
            .ToList();

        var trailingReturns = TrailingReturnsCalculator.Calculate(unitValues, request.QueryDate);

        return new StockHistoryResult(request.StockSymbol, prices, trailingReturns);
    }
}
