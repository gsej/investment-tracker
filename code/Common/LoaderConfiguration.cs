namespace Common;

// TODO: this is in the wrong place.
public class LoaderConfiguration
{
    public string DataFolder { get; set; }
    public string PriceFolder { get; set; }
    public string ExchangeRateFolder { get; set; }
    public string SqlConnectionString { get; set; }
    public string AppInsightsConnectionString { get; set; }

    public bool DeduplicateStockPrices { get; set; } = false;
}

