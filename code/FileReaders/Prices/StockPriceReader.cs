using System.Text.Json;
using Common.Tracing;
using FileReaders.JsonConverters;
using Microsoft.Extensions.Logging;

namespace FileReaders.Prices;

public class StockPriceReader : IStockPriceReader
{
    private readonly ILogger<StockPriceReader> _logger;
    private readonly JsonSerializerOptions _options;

    public StockPriceReader(ILogger<StockPriceReader> logger)
    {
        _logger = logger;
        _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        _options.Converters.Add(new StringConverter());
    }

    public async Task<IList<StockPrice>> ReadFile(string fileName)
    {
        using var _ = InvestmentTrackerActivitySource.Instance.StartActivity($"Reading stock price file {fileName}");
        _logger.LogInformation("Reading stock price file {fileName}", fileName);
        
        await using var stream = File.OpenRead(fileName);

        try
        {
            var items = await JsonSerializer.DeserializeAsync<IList<StockPrice>>(stream, _options);
            return items;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to read stock price file {fileName}", fileName);
            throw;
        }
    }
}
