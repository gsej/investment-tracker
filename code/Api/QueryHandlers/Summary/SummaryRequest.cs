namespace Api.QueryHandlers.Summary;

public record struct SummaryRequest(string AccountCode, DateOnly Date);
