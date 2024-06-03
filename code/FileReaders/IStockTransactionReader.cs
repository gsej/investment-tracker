using FileReaders.AccountStatements;

namespace FileReaders;

public interface IStockTransactionReader
{
    IEnumerable<StockTransaction> Read(string fileName);
}
