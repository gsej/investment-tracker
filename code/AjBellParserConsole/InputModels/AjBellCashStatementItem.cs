namespace AjBellParserConsole.InputModels;

public record AjBellCashStatementItem
{
    public string Date { get; init; }
    public string Description { get; init; }
    public string ReceiptAmountGbp { get; init; }
    public string PaymentAmountGbp { get; init; }
}
