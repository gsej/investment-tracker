namespace Api.QueryHandlers.History;

public record TrailingReturnResult(string PeriodLabel, decimal? ReturnPercentage, bool IsAnnualised);
