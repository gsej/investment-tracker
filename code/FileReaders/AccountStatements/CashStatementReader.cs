using System.Text.Json;

namespace FileReaders.AccountStatements;

public class CashStatementReader : ICashStatementReader
{
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    public IEnumerable<CashStatementItem> Read(string fileName)
    {
        var jsonString = File.ReadAllText(fileName);
        var items = JsonSerializer.Deserialize<IList<CashStatementItem>>(jsonString, _options);
        return items;
    }
}
