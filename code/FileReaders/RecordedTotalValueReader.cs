using System.Text.Json;

namespace FileReaders;

public class RecordedTotalValueReader :  IReader<RecordedTotalValue>
{
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    public async Task<IEnumerable<RecordedTotalValue>> Read(string fileName)
    {
        var jsonString = await File.ReadAllTextAsync(fileName);
        var items = JsonSerializer.Deserialize<IList<RecordedTotalValue>>(jsonString, _options);
        return items;
    }
}
