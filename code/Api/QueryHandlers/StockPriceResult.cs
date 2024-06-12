
namespace Api.QueryHandlers;

public record StockPriceResult
{
    public string Error { get; }
    public decimal? Price { get; }
    public string Currency { get; }
    public string OriginalCurrency { get; }
    public int? AgeInDays { get; }

    public bool HasPrice => Price.HasValue;

    public StockPriceResult(decimal price, string currency, string originalCurrency, int ageInDays)
    {
        Price = price;
        Currency = currency;
        OriginalCurrency = originalCurrency;
        AgeInDays = ageInDays;
    }

    private StockPriceResult(string error)
    {
        Error = error;
    }
    
    public static StockPriceResult Missing(string stockSymbol) => new($"Missing:{stockSymbol}");
}

