using System.Text.Json;

namespace FileReaders;

public class RecordedTotalValueReader : IRecordedTotalValueReader
{
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    public IEnumerable<RecordedTotalValue> Read(string fileName)
    {
        return ReadFile(fileName);
    }

    private IList<RecordedTotalValue> ReadFile(string fileName)
    {
        var jsonString = File.ReadAllText(fileName);
        var items = JsonSerializer.Deserialize<IList<RecordedTotalValue>>(jsonString, _options);
        return items;
    }
}
