namespace Api.QueryHandlers.Portfolio;

public interface IAccountPortfolioQueryHandler
{
    Task<AccountPortfolioResult> Handle(AccountPortfolioRequest request);
}
