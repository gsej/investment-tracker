namespace Api.QueryHandlers.History;

public interface IPrecalculatedAccountValueHistoryQueryHandler
{
    Task<AccountValueHistoryResult> Handle(PrecalculatedAccountValueHistoryRequest request);
}
