using System.Text.Json.Serialization;

namespace FileReaders.AccountStatements;

public record StockTransaction
{
    [JsonPropertyName("account_code")] 
    public string AccountCode{ get; init; }

    public string Date{ get; init; }

    public string Transaction{ get; init; }

    public string Description{ get; init; }

    public  decimal Quantity { get; init; }

    [JsonPropertyName("amount_gbp")] 
    public decimal AmountGbp{ get; init; }

    public string Reference { get; init; }
}
