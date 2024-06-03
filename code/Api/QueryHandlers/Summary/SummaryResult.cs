namespace Api.QueryHandlers.Summary;

public record SummaryResult(IList<Holding> Holdings, decimal CashBalanceInGbp, TotalValue TotalValue);
