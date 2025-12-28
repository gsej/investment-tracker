namespace Api.QueryHandlers.Portfolio;

public record struct AccountPortfolioRequest(string[] AccountCodes, DateOnly Date);
