namespace Api.QueryHandlers.Summary;

public record AccountSummaryResult(IList<Holding> Holdings, decimal CashBalanceInGbp, TotalValue TotalValue);
