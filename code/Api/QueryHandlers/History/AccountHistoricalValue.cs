using Api.QueryHandlers.Common;

namespace Api.QueryHandlers.History;

public record AccountHistoricalValue(
        DateOnly Date,
        string AccountCode,
        decimal ValueInGbp,
        decimal Inflows,
        int TotalPriceAgeInDays,
        string Comment)
{
    public decimal? RecordedTotalValueInGbp { get; set; }
    public string RecordedTotalValueSource { get; set; }
    public decimal? DiscrepancyRatio { get; set; }
    public decimal? DifferenceToPreviousDay { get; set; }
    public decimal? DifferenceRatio { get; set; }
    
    public UnitAccount Units { get; set; }
}
