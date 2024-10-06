using Api.QueryHandlers.Portfolio;

namespace Api.QueryHandlers.History;

public record AnnualPerformanceRequest(string[] AccountCodes, DateOnly AsOfDate);

public record PerformanceResult(int Year)
{
    public decimal NetInflowsInGbp { get; set; } = 0;
    public TotalValue ValueAtStart { get; set; }
    public TotalValue ValueAtEnd { get; set; }
    
    public decimal? IncreaseInValueGbp => ValueAtEnd?.ValueInGbp - ValueAtStart?.ValueInGbp;
    public decimal? IncreaseExcludingNetInflows => IncreaseInValueGbp - NetInflowsInGbp;

    public decimal? IncreaseExcludingNetInflowsPercentage
    {
        get
        {
            if (ValueAtStart.ValueInGbp == 0)
            {
                return null;
            }
            else
            {
                return ((IncreaseInValueGbp - NetInflowsInGbp) / ValueAtStart.ValueInGbp) * 100;
            }
        }
    }


    // public decimal? IncreaseExcludingNetInflowsPercentage => 100 * (IncreaseInValueGbp - NetInflowsInGbp) / IncreaseInValueGbp;
}

public record AnnualPerformanceResult
{
    public required IList<PerformanceResult> Years { get; init; }
   
      public decimal OverallIncreaseInValueGbp => Years.Select(y => y.IncreaseInValueGbp ?? 0).Sum();
      public decimal OverAllIncreaseExcludingNetInflows => Years.Select(y => y.IncreaseExcludingNetInflows ?? 0).Sum();
      
    //  public decimal? OverAllIncreaseExcludingNetInflowsPercentage => 100 * (OverallIncreaseInValueGbp - OverAllIncreaseExcludingNetInflows) / OverallIncreaseInValueGbp;
}

