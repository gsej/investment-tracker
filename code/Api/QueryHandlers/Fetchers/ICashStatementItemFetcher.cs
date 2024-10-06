using Database.Entities;

namespace Api.QueryHandlers.Fetchers;

public interface ICashStatementItemFetcher
{
    Task<IList<CashStatementItem>> GetCashStatementItems(string accountCode);
}
