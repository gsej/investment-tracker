
namespace Api.QueryHandlers;

public record StockPriceResult
{
    public string Error { get; }
    public decimal? Price { get; }
    public string Currency { get; }
    public int? AgeInDays { get; }

    public bool HasPrice => Price.HasValue;

    public StockPriceResult(decimal price, string currency, int ageInDays)
    {
        Price = price;
        Currency = currency;
        AgeInDays = ageInDays;
    }

    private StockPriceResult(string error)
    {
        Error = error;
    }
    
    public static StockPriceResult Missing(string stockSymbol) => new($"Missing:{stockSymbol}");
}

