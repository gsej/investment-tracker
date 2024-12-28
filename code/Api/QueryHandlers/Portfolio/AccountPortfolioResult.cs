namespace Api.QueryHandlers.Portfolio;

public record AccountPortfolioResult(
    string AccountCode,
    IList<Holding> Holdings,
    decimal CashBalanceInGbp,
    decimal Contributions,
    IList<Allocation> Allocations,
    TotalValue TotalValue);
