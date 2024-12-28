namespace Api.QueryHandlers.Fetchers;

public record RecordedTotalValue(DateOnly Date, string AccountCode, decimal TotalValueInGbp, string Source);
