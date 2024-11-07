namespace Api.QueryHandlers.Fetchers;

public interface IAccountFetcher
{
    Task<IList<Database.Entities.Account>> GetAccounts();
}
