namespace Api.QueryHandlers.Fetchers;

public interface ICommentFetcher
{
    Task<IList<Database.Entities.Comment>> Get(string[] accountCodes);
}
