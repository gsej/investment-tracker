using System.Text.Json;
using Common.Tracing;
using FileReaders.JsonConverters;
using Microsoft.Extensions.Logging;

namespace FileReaders.ExchangeRates;

public class ExchangeRateReader : IExchangeRateReader
{
    private readonly ILogger<ExchangeRateReader> _logger;
    private readonly JsonSerializerOptions _options;

    public ExchangeRateReader(ILogger<ExchangeRateReader> logger)
    {
        _logger = logger;
        _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        _options.Converters.Add(new StringConverter());
    }

    public async Task<IEnumerable<ExchangeRate>> ReadFile(string fileName)
    {
        using var _ = InvestmentTrackerActivitySource.Instance.StartActivity($"Reading exchange rate file {fileName}");
        _logger.LogInformation("Reading exchange rate {fileName}", fileName);
        
        await using var stream = File.OpenRead(fileName);

        try
        {
            var items = await JsonSerializer.DeserializeAsync<IList<ExchangeRate>>(stream, _options);
            return items;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to read exchange rate price file {fileName}", fileName);
            throw;
        }
    }
}
