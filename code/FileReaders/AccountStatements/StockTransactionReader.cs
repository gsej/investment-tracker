using System.Text.Json;

namespace FileReaders.AccountStatements;

public class StockTransactionReader : IStockTransactionReader
{
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    public IEnumerable<StockTransaction> Read(string fileName)
    {
        var jsonString = File.ReadAllText(fileName);
        var items = JsonSerializer.Deserialize<IList<StockTransaction>>(jsonString, _options);

        return items;
    }
}
