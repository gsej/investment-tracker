namespace Api.QueryHandlers.Portfolio;

public class Holding
{
    public Holding(string stockSymbol, 
        string stockDescription,
        string allocation,
        decimal quantity)
    {
        StockSymbol = stockSymbol;
        StockDescription = stockDescription;
        Allocation = allocation;
        Quantity = quantity;
    }

    public string StockSymbol { get; init; }
    public string StockDescription { get; init; }
    public string Allocation { get; init; }
    public decimal Quantity { get; init; }
    public StockPriceResult StockPrice { get; set; } // TODO: make this readonly and add a method to set this and value
    public decimal ValueInGbp { get; set; }
    public string Comment { get; set; }
}
