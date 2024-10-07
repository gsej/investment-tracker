namespace Api.QueryHandlers.Portfolio;

public record AccountPortfolioResult(
    string AccountCode,
    IList<Holding> Holdings,
    decimal CashBalanceInGbp,
    decimal Contributions,
    TotalValue TotalValue,
    IList<Allocation> Allocations
);
