using System.Text.Json.Serialization;

namespace AjBellParserConsole.OutputModels;

public class CashStatementItem
{
    [JsonPropertyName("account_code")]
    public string AccountCode { get; set; }
    
    [JsonPropertyName("date")]
    public string Date { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; }
    
    [JsonPropertyName("receipt_amount_gbp")]
    public decimal ReceiptAmountGbp { get; set; }
    
    [JsonPropertyName("payment_amount_gbp")]
    public decimal PaymentAmountGbp { get; set; }
}
