namespace Api.QueryHandlers.Summary;

public interface IAccountSummaryQueryHandler
{
    Task<IAccountSummaryResult> Handle(AccountSummaryRequest request);
}
