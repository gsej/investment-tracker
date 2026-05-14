namespace Api.QueryHandlers.History;

public record AccountValueHistoryResult(IList<AccountHistoricalValue> Items, IList<CommentResult> Comments);
