using System.Globalization;
using Common.Tracing;
using Database;
using FileReaders;
using FileReaders.ExchangeRates;
using Microsoft.Extensions.Logging;
using ExchangeRate = Database.Entities.ExchangeRate;

namespace LoaderConsole;

public class ExchangeRateLoader
{
    private readonly ILogger<ExchangeRateLoader> _logger;
    private readonly IReader<FileReaders.ExchangeRates.ExchangeRate> _reader;
    private readonly InvestmentsDbContext _context;

    public ExchangeRateLoader(
        ILogger<ExchangeRateLoader> logger,
        IReader<FileReaders.ExchangeRates.ExchangeRate> reader,
        InvestmentsDbContext context)
    {
        _logger = logger;
        _reader = reader;
        _context = context;
    }

    public async Task LoadFile(string fileName, string source)
    {
        using var _ = InvestmentTrackerActivitySource.Instance.StartActivity();

        if (!File.Exists(fileName))
        {
            _logger.LogError("File {fileName} does not exist", fileName);
            return;
        }

        var exchangeRates = (await _reader.Read(fileName)).ToList();

        foreach (var exchangeRate in exchangeRates)
        {
            var exchangeRateEntity = new ExchangeRate
            {
                BaseCurrency = exchangeRate.BaseCurrency, 
                AlternateCurrency = exchangeRate.AlternateCurrency, 
                Date = DateOnly.ParseExact(exchangeRate.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture), 
                Rate = Decimal.Parse(exchangeRate.Rate),
                Source = source
            };

            _context.ExchangeRates.Add(exchangeRateEntity);
        }

        await _context.SaveChangesAsync();
    }
}
