using Database.Entities;

namespace LoaderConsole.CashStatementItemEnrichers;

public interface ICashStatementItemEnricher
{
    void Enrich(CashStatementItem cashStatementItem);
}