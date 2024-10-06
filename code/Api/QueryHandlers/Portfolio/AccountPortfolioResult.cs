namespace Api.QueryHandlers.Portfolio;

public record AccountPortfolioResult(
    string AccountCode,
    IList<Holding> Holdings,
    decimal CashBalanceInGbp,
    TotalValue TotalValue,
    IList<Allocation> Allocations
);
