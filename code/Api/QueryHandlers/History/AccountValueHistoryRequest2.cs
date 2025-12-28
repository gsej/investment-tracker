namespace Api.QueryHandlers.History;

public record struct AccountValueHistoryRequest2(string[] AccountCodes, DateOnly QueryDate);
