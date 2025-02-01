namespace Api.QueryHandlers.Performance;

public record AccountPerformanceValue(
    string Period, // i.e. 2014, April 2014, etc.
    string AccountCode,
    decimal Inflows,
    decimal Growth,
    decimal PercentageUnitChange
);

