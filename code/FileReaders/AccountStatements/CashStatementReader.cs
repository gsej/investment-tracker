using System.Text.Json;

namespace FileReaders.AccountStatements;

public class CashStatementReader : IReader<CashStatementItem>
{
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    public async Task<IEnumerable<CashStatementItem>> Read(string fileName)
    {
        await using var stream = File.OpenRead(fileName);
        var items = await JsonSerializer.DeserializeAsync<IList<CashStatementItem>>(stream, _options);
        return items;
    }
}
