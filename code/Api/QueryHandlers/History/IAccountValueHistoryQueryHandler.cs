namespace Api.QueryHandlers.History;

public interface IAccountValueHistoryQueryHandler
{
    Task<AccountValueHistoryResult> Handle(AccountValueHistoryRequest request);
}
