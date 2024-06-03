namespace AjBellParserConsole.InputModels;

public record AjBellTransaction
{
    public string Date { get; init; }
    public string Transaction { get; init; }
    public string Description { get; init; }
    public string Quantity { get; init; }
    public string AmountGbp { get; init; }
    public string Reference { get; init; }
}
