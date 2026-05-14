namespace Api.QueryHandlers.History;

public record CommentResult(Guid CommentId, DateOnly Date, string Text, IList<string> AccountCodes);
