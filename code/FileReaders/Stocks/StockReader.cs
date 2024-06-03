using System.Text.Json;
using Common.Tracing;
using Microsoft.Extensions.Logging;

namespace FileReaders.Stocks;

public class StockReader : IStockReader
{
    private readonly ILogger<StockReader> _logger;
    private readonly JsonSerializerOptions _options;

    public StockReader(ILogger<StockReader> logger)
    {
        _logger = logger;
        _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public async Task<IList<Stock>> ReadFile(string fileName)
    {
        using var _ = InvestmentTrackerActivitySource.Instance.StartActivity($"Reading Stock file {fileName}");
        _logger.LogInformation("Reading Stock file {fileName}", fileName);
        
        await using var stream = File.OpenRead(fileName);

        try
        {
            var items = await JsonSerializer.DeserializeAsync<IList<Stock>>(stream, _options);
            return items;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to read Stock file {fileName}", fileName);
            throw;
        }
    }
}
