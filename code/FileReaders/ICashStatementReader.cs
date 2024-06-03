using FileReaders.AccountStatements;

namespace FileReaders;

public interface ICashStatementReader
{
    IEnumerable<CashStatementItem> Read(string fileName);
}
