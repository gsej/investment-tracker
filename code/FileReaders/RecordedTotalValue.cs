using System.Text.Json.Serialization;

namespace FileReaders;

public record RecordedTotalValue
{
    [JsonPropertyName("account_code")] 
    public string AccountCode{ get; init; }

    public string Date{ get; init; }
 
    [JsonPropertyName("total_value_in_gbp")] 
    public string TotalValueInGbp{ get; init; }
    
    public string Source { get;init; }
}
