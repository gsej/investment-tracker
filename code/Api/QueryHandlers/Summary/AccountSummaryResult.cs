namespace Api.QueryHandlers.Summary;

public record AccountSummaryResult(
    string AccountCode,
    IList<Holding> Holdings,
    decimal CashBalanceInGbp,
    TotalValue TotalValue);
