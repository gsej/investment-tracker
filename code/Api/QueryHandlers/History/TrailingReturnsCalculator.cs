namespace Api.QueryHandlers.History;

public static class TrailingReturnsCalculator
{
    private record PeriodDefinition(string Label, int Months, bool IsAnnualised, double Years = 0);

    private static readonly PeriodDefinition[] Periods =
    [
        new("3 months",  3,   false),
        new("6 months",  6,   false),
        new("1 year",    12,  false),
        new("3 years",   36,  true,  3),
        new("5 years",   60,  true,  5),
        new("10 years",  120, true, 10),
    ];

    public static IReadOnlyList<TrailingReturnResult> Calculate(
        IList<UnitAccount> unitValues,
        DateOnly queryDate)
    {
        if (unitValues.Count == 0)
            return AllNull();

        var endUnit = unitValues[unitValues.Count - 1];

        if (endUnit.ValueInGbpPerUnit is null or 0)
            return AllNull();

        var endValue = endUnit.ValueInGbpPerUnit.Value;
        var results = new List<TrailingReturnResult>(Periods.Length + 1);

        foreach (var period in Periods)
        {
            var startDate = endUnit.Date.AddMonths(-period.Months);
            results.Add(ComputeReturn(unitValues, startDate, endValue, period.Label, period.IsAnnualised, period.Years));
        }

        var ytdStart = new DateOnly(endUnit.Date.Year, 1, 1);
        results.Add(ComputeReturn(unitValues, ytdStart, endValue, "YTD", false, 0));

        return results;
    }

    private static TrailingReturnResult ComputeReturn(
        IList<UnitAccount> unitValues,
        DateOnly startDate,
        decimal endValue,
        string label,
        bool isAnnualised,
        double years)
    {
        var startUnit = FindLatestOnOrBefore(unitValues, startDate);

        if (startUnit?.ValueInGbpPerUnit is null or 0)
            return new TrailingReturnResult(label, null, isAnnualised);

        var ratio = endValue / startUnit.ValueInGbpPerUnit.Value;
        var returnPct = isAnnualised
            ? (decimal)(Math.Pow((double)ratio, 1.0 / years) - 1)
            : ratio - 1;

        return new TrailingReturnResult(label, returnPct, isAnnualised);
    }

    private static UnitAccount FindLatestOnOrBefore(IList<UnitAccount> unitValues, DateOnly date)
    {
        UnitAccount result = null;
        foreach (var unit in unitValues)
        {
            if (unit.Date <= date)
                result = unit;
            else
                break;
        }
        return result;
    }

    private static IReadOnlyList<TrailingReturnResult> AllNull() =>
        Periods.Select(p => new TrailingReturnResult(p.Label, null, p.IsAnnualised))
               .Append(new TrailingReturnResult("YTD", null, false))
               .ToList();
}
