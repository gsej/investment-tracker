namespace Api.QueryHandlers;

public record struct StockPriceRequest(string StockSymbol, string Date);