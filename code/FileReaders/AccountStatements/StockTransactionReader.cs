using System.Text.Json;

namespace FileReaders.AccountStatements;

public class StockTransactionReader : IReader<StockTransaction>
{
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    public async Task<IEnumerable<StockTransaction>> Read(string fileName)
    {
        // TODO: use streams like with exchange rate
        var jsonString = await File.ReadAllTextAsync(fileName);
        var items = JsonSerializer.Deserialize<IList<StockTransaction>>(jsonString, _options);

        return items;
    }
}
