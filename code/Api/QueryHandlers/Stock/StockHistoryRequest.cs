namespace Api.QueryHandlers.StockHistory;

public record StockHistoryRequest(string StockSymbol, DateOnly QueryDate);
