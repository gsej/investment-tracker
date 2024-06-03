using System.Text.Json.Serialization;

namespace AjBellParserConsole.OutputModels;

public class StockTransaction
{
    [JsonPropertyName("account_code")]
    public string AccountCode { get; set; }
    
    [JsonPropertyName("date")]
    public string Date { get; set; }
    
    [JsonPropertyName("transaction")]
    public string Transaction { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; }
    
    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }
    
    [JsonPropertyName("amount_gbp")]
    public decimal AmountGbp { get; set; }
    
    [JsonPropertyName("reference")]
    public string Reference { get; set; }
}
