using System.Text.Json;

namespace FileReaders.AccountStatements;

public class CashStatementReader : IReader<CashStatementItem>
{
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    public async Task<IEnumerable<CashStatementItem>> Read(string fileName)
    {
        var jsonString = await File.ReadAllTextAsync(fileName);
        var items = JsonSerializer.Deserialize<IList<CashStatementItem>>(jsonString, _options);
        return items;
    }
}
