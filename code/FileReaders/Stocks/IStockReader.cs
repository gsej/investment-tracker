namespace FileReaders.Stocks;

public interface IStockReader
{
    Task<IList<Stock>> ReadFile(string fileName);
}

