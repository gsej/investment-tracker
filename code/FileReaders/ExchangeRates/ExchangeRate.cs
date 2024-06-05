using System.Text.Json.Serialization;

namespace FileReaders.ExchangeRates;

public class ExchangeRate
{
    [JsonPropertyName("base_currency")] 
    public string BaseCurrency { get; init; }
    
    [JsonPropertyName("alternate_currency")]
    public string AlternateCurrency { get; init; }
    
    public string Date { get; init; }
    
    public string Rate { get; init; }
    
    public string Source { get; init; }
}
