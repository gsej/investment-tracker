namespace Api.QueryHandlers.History;

public interface IRecordedTotalValueQueryHandler
{
    Task<RecordedTotalValuesResult> Handle(RecordedTotalValuesRequest request);
}
