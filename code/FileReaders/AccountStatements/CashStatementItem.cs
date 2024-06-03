using System.Text.Json.Serialization;

namespace FileReaders.AccountStatements;

public record CashStatementItem
{
    [JsonPropertyName("account_code")] 
    public string AccountCode { get; init; }

    public string Date { get; init; }
    public string Description { get; init; }

    [JsonPropertyName("receipt_amount_gbp")]
    public decimal ReceiptAmountGbp { get; init; }

    [JsonPropertyName("payment_amount_gbp")]
    public decimal Payment_Amount_Gbp { get; init; }
}
