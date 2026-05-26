namespace Api.QueryHandlers.History;

public record AccountValueHistoryResult(IList<AccountHistoricalValue> Items, IList<CommentResult> Comments)
{
    public IReadOnlyList<TrailingReturnResult> TrailingReturns { get; init; } = [];
}
