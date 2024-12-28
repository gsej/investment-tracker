using Api.QueryHandlers.History;

namespace Api.QueryHandlers.Fetchers;

public interface IRecordedTotalValueFetcher
{
    Task<IList<RecordedTotalValue>> GetRecordedTotalValues(string accountCode);
}
