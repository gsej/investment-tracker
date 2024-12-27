namespace Api.QueryHandlers.History;

public record AccountValueHistoryRequest(string AccountCode, DateOnly QueryDate);
