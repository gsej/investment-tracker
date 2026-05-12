using Api.QueryHandlers.Fetchers;

namespace Api.QueryHandlers.History;

public class AccountValueHistoryQueryHandler2 : IAccountValueHistoryQueryHandler2
{
    private readonly IAccountHistoricalValueFetcher _accountHistoricalValueFetcher;
    private readonly ILogger<AccountValueHistoryQueryHandler2> _logger;

    public AccountValueHistoryQueryHandler2(
        ILogger<AccountValueHistoryQueryHandler2> logger,
        IAccountHistoricalValueFetcher accountHistoricalValueFetcher)
    {
        _logger = logger;
        _accountHistoricalValueFetcher = accountHistoricalValueFetcher;
    }
    
    public async Task<AccountValueHistoryResult> Handle(AccountValueHistoryRequest2 request)
    {
        var values = (await _accountHistoricalValueFetcher.Get(request.AccountCodes))
            .OrderBy(value => value.Date)
            .ToList();

        if (!values.Any())
            return new AccountValueHistoryResult([]);

        // iterate over each day in the date range
        var results = new List<AccountHistoricalValue>();

        var currentDate = values.First().Date;
        
        decimal? previousDayTotal = null;
        
        while (currentDate <= request.QueryDate)
        {
            var daysValues = values
                .Where(v => v.Date == currentDate)
                .ToList();

            if (!daysValues.Any())
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

        foreach (var result in results)
        {
            var matchingUnitValues = unitValues.SingleOrDefault(u => u.Date == result.Date);

            if (matchingUnitValues == null)
            {
                result.Units = new UnitAccount(result.Date, null, null);
            }
            else
            {
                result.Units = matchingUnitValues;
            }
        }
        
        return new AccountValueHistoryResult(results);
    }
}
