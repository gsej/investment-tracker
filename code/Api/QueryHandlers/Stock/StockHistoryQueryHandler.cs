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

        if (deduplicated.Count == 0)
        {
            return new StockHistoryResult(request.StockSymbol, [], []);
        }

        var prices = new List<StockPriceResult>();
        var unitValues = new List<UnitAccount>();

        var startDate = deduplicated[0].Date;
        var endDate = request.QueryDate;

        decimal currentPrice = 0;
        DateOnly lastPriceDate = startDate;
        int priceIdx = 0;

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            while (priceIdx < deduplicated.Count && deduplicated[priceIdx].Date <= date)
            {
                currentPrice = deduplicated[priceIdx].Price;
                lastPriceDate = deduplicated[priceIdx].Date;
                priceIdx++;
            }

            var ageInDays = date.DayNumber - lastPriceDate.DayNumber;
            prices.Add(new StockPriceResult(date, currentPrice, ageInDays));
            unitValues.Add(new UnitAccount(date, 1, currentPrice));
        }

        var trailingReturns = TrailingReturnsCalculator.Calculate(unitValues, request.QueryDate);

        return new StockHistoryResult(request.StockSymbol, prices, trailingReturns);
    }
}
