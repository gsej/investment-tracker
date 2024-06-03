namespace FileReaders.Prices;

public interface IStockPriceReader
{
    Task<IList<StockPrice>> ReadFile(string fileName);
}
