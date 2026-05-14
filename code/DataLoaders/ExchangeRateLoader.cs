using System.Globalization;
using Database.Entities;
using Database.Repositories;
using FileReaders;
using Microsoft.Extensions.Logging;
using ExchangeRate = Database.Entities.ExchangeRate;

namespace DataLoaders;

public class ExchangeRateLoader
{
    private readonly ILogger<ExchangeRateLoader> _logger;
    private readonly IReader<FileReaders.ExchangeRates.ExchangeRate> _reader;
    private readonly IExchangeRateRepository _repository;

    public ExchangeRateLoader(
        ILogger<ExchangeRateLoader> logger,
        IReader<FileReaders.ExchangeRates.ExchangeRate> reader,
        IExchangeRateRepository repository)
    {
        _logger = logger;
        _reader = reader;
        _repository = repository;
    }

    public async Task LoadAll(string folder)
    {
        var allRates = new List<ExchangeRate>();

        foreach (var fileName in Directory.EnumerateFiles(folder, "*.json", SearchOption.AllDirectories))
        {
            if (!File.Exists(fileName))
            {
                _logger.LogError("File {fileName} does not exist", fileName);
                continue;
            }

            var source = Path.GetRelativePath(folder, fileName);
            var exchangeRates = (await _reader.Read(fileName)).ToList();

            foreach (var exchangeRate in exchangeRates)
            {
                allRates.Add(new ExchangeRate
                {
                    BaseCurrency = exchangeRate.BaseCurrency,
                    AlternateCurrency = exchangeRate.AlternateCurrency,
                    Date = DateOnly.ParseExact(exchangeRate.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Rate = Decimal.Parse(exchangeRate.Rate),
                    Source = source
                });
            }
        }

        await _repository.BulkAdd(allRates);

        _logger.LogInformation("Loaded {count} exchange rates from {folder}", allRates.Count, folder);
    }
}
