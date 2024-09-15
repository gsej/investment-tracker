namespace Api.QueryHandlers.Summary;

public record Holding(string StockSymbol, 
    string StockDescription,
    string Allocation,
    decimal Quantity, 
    StockPriceResult StockPrice, 
    decimal ValueInGbp, 
    string Comment);
