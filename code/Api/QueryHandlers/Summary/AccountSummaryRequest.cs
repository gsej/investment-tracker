namespace Api.QueryHandlers.Summary;

public record struct AccountSummaryRequest(string AccountCode, DateOnly Date);
