namespace Api.QueryHandlers.Portfolio;

public record struct AccountPortfolioRequest(string AccountCode, DateOnly Date);
