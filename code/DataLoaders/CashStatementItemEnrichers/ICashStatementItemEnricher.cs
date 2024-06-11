using Database.Entities;

namespace DataLoaders.CashStatementItemEnrichers;

public interface ICashStatementItemEnricher
{
    void Enrich(CashStatementItem cashStatementItem);
}