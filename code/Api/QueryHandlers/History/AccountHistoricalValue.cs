namespace Api.QueryHandlers.History;

public record AccountHistoricalValue(DateOnly Date, string AccountCode, decimal ValueInGbp, int TotalPriceAgeInDays, string Comment)
{
    public decimal? RecordedTotalValueInGbp { get; set; }
    public decimal? DiscrepancyPercentage { get; set; }
}
