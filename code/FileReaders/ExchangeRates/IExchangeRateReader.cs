namespace FileReaders.ExchangeRates;

public interface IExchangeRateReader
{
    Task<IEnumerable<ExchangeRate>> ReadFile(string fileName);
}
