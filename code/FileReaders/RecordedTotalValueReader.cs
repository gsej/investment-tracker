using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace FileReaders;

public class RecordedTotalValueReader :  IReader<RecordedTotalValue>
{
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    private readonly ILogger<RecordedTotalValueReader> _logger;

    public RecordedTotalValueReader(ILogger<RecordedTotalValueReader> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<RecordedTotalValue>> Read(string fileName)
    {
        if (!File.Exists(fileName))
        {
            _logger.LogWarning("File ${fileName} does not exist", fileName);
            return Array.Empty<RecordedTotalValue>();
        }
        
        await using var stream = File.OpenRead(fileName);
        var items = await JsonSerializer.DeserializeAsync<IList<RecordedTotalValue>>(stream, _options);
        return items;
    }
}
