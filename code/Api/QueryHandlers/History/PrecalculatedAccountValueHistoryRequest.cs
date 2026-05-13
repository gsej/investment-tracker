namespace Api.QueryHandlers.History;

public record struct PrecalculatedAccountValueHistoryRequest(string[] AccountCodes, DateOnly QueryDate);
