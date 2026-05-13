using Api.QueryHandlers.Fetchers;

namespace Api.QueryHandlers.History;

public class PrecalculatedAccountValueHistoryQueryHandler : IPrecalculatedAccountValueHistoryQueryHandler
{
    private readonly IAccountHistoricalValueFetcher _accountHistoricalValueFetcher;
    private readonly ILogger<PrecalculatedAccountValueHistoryQueryHandler> _logger;

    public PrecalculatedAccountValueHistoryQueryHandler(
        ILogger<PrecalculatedAccountValueHistoryQueryHandler> logger,
        IAccountHistoricalValueFetcher accountHistoricalValueFetcher)
    {
        _logger = logger;
        _accountHistoricalValueFetcher = accountHistoricalValueFetcher;
    }

    public async Task<AccountValueHistoryResult> Handle(PrecalculatedAccountValueHistoryRequest request)
    {
        var values = (await _accountHistoricalValueFetcher.Get(request.AccountCodes))
            .OrderBy(value => value.Date)
            .ToList();

        if (!values.Any())
            return new AccountValueHistoryResult([]);

        // iterate over each day in the date range
        var valuesByDate = values
            .GroupBy(v => v.Date)
            .ToDictionary(g => g.Key, g => g.ToList());

        var results = new List<AccountHistoricalValue>();

        var currentDate = values.First().Date;

        decimal? previousDayTotal = null;

        while (currentDate <= request.QueryDate)
        {
            if (!valuesByDate.TryGetValue(currentDate, out var daysValues))
                break;

            var historicalValue = new AccountHistoricalValue(
                currentDate,
                daysValues.Select(v => v.ValueInGbp).Sum(),
                daysValues.Select(v => v.NetInflows).Sum(),
                daysValues.Select(v => v.TotalPriceAgeInDays).Sum(),
                string.Join(",", daysValues.Select(v => v.Comment))
            )
            {
                RecordedTotalValueInGbp = daysValues.Any(v => v.RecordedTotalValueInGbp.HasValue)
                    ? daysValues.Sum(v => v.RecordedTotalValueInGbp)
                    : null,
                RecordedTotalValueSource = string.Join(",", daysValues.Select(v => v.RecordedTotalValueSource)),
            };

            if (historicalValue.ValueInGbp != 0 && historicalValue.RecordedTotalValueInGbp.HasValue)
                historicalValue.DiscrepancyRatio = (historicalValue.ValueInGbp - historicalValue.RecordedTotalValueInGbp) / historicalValue.ValueInGbp;

            if (previousDayTotal.HasValue)
            {
                historicalValue.DifferenceToPreviousDay = historicalValue.ValueInGbp - historicalValue.NetInflows - previousDayTotal.Value;
                if (previousDayTotal.Value != 0)
                    historicalValue.DifferenceRatio = historicalValue.DifferenceToPreviousDay / previousDayTotal.Value;
            }

            results.Add(historicalValue);
            previousDayTotal = historicalValue.ValueInGbp;
            currentDate = currentDate.AddDays(1);
        }

        results = results.OrderBy(r => r.Date).ToList();

        var unitValues = new UnitCalculator().Calculate(results, 100);

        for (int i = 0; i < results.Count; i++)
        {
            results[i].Units = i < unitValues.Count
                ? unitValues[i]
                : new UnitAccount(results[i].Date, null, null);
        }

        return new AccountValueHistoryResult(results);
    }
}
