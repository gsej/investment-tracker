namespace Api.QueryHandlers.Portfolio;

public record AccountPortfolioResult(
    string[] AccountCodes,
    IList<Holding> Holdings,
    decimal CashBalanceInGbp,
    decimal Contributions,
    ValueWithAge TotalValue,
    IList<Allocation> Allocations
);
