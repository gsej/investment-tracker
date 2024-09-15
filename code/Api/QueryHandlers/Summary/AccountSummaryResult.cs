namespace Api.QueryHandlers.Summary;

public record AccountSummaryResult(
    string AccountCode,
    IList<Holding> Holdings,
    decimal CashBalanceInGbp,
    
    [property: Obsolete("this should be removed and calculated in the client")]
    TotalValue TotalValue);
