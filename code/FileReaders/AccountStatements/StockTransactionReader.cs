using System.Text.Json;

namespace FileReaders.AccountStatements;

public class StockTransactionReader : IReader<StockTransaction>
{
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    public async Task<IEnumerable<StockTransaction>> Read(string fileName)
    {
        await using var stream = File.OpenRead(fileName);
        var items = await JsonSerializer.DeserializeAsync<IList<StockTransaction>>(stream, _options);

        return items;
    }
}
