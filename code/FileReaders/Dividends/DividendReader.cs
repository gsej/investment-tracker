using System.Text.Json;
using FileReaders.JsonConverters;
using Microsoft.Extensions.Logging;

namespace FileReaders.Dividends;

public class DividendReader : IDividendReader
{
    private readonly ILogger<DividendReader> _logger;
    private readonly JsonSerializerOptions _options;

    public DividendReader(ILogger<DividendReader> logger)
    {
        _logger = logger;
        _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        _options.Converters.Add(new StringConverter());
    }

    public async Task<IList<Dividend>> ReadFile(string fileName)
    {
        _logger.LogInformation("Reading dividend file {fileName}", fileName);

        await using var stream = File.OpenRead(fileName);

        try
        {
            var items = await JsonSerializer.DeserializeAsync<IList<Dividend>>(stream, _options);
            return items;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to read dividend file {fileName}", fileName);
            throw;
        }
    }
}
