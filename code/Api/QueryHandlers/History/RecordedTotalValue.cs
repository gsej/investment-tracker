namespace Api.QueryHandlers.History;

public record RecordedTotalValue(DateOnly Date, string AccountCode, decimal TotalValueInGbp, string Source);
