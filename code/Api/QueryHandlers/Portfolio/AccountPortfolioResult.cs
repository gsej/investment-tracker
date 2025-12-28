namespace Api.QueryHandlers.Portfolio;

public record AccountPortfolioResult(
    string[] AccountCodes,
    IList<Holding> Holdings,
    decimal CashBalanceInGbp,
    decimal Contributions,
    TotalValue TotalValue,
    IList<Allocation> Allocations
);
