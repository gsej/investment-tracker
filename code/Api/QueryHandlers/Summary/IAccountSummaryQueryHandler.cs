namespace Api.QueryHandlers.Summary;

public interface IAccountSummaryQueryHandler
{
    Task<AccountSummaryResult> Handle(AccountSummaryRequest request);
}
