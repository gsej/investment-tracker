namespace FileReaders.Dividends;

public interface IDividendReader
{
    Task<IList<Dividend>> ReadFile(string fileName);
}
