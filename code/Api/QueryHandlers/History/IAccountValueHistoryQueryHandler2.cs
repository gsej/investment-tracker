namespace Api.QueryHandlers.History;

public interface IAccountValueHistoryQueryHandler2
{
    Task<AccountValueHistoryResult> Handle(AccountValueHistoryRequest2 request);
}
