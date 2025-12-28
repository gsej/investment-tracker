namespace Api.QueryHandlers.Fetchers;

public interface IAccountHistoricalValueFetcher
{
    Task<IList<Database.Entities.AccountHistoricalValue>> Get(string[] accountCodes);
}
