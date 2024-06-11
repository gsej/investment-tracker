namespace Api.QueryHandlers.Summary;

public record IAccountSummaryResult(IList<Holding> Holdings, decimal CashBalanceInGbp, TotalValue TotalValue);
