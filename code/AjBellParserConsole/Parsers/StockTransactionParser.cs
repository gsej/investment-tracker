using System.Globalization;
using AjBellParserConsole.InputModels;
using CsvHelper;
using Microsoft.Extensions.Logging;

namespace AjBellParserConsole.Parsers;

public class StockTransactionParser
{
    private readonly ILogger<StockTransactionParser> _logger;
    
    public StockTransactionParser(ILogger<StockTransactionParser> logger)
    {
        _logger = logger;
    }
    
    public List<AjBellTransaction> Parse(params string[] filePaths)
    {
        var stockTransactionItems = new List<AjBellTransaction>();

        foreach (var filePath in filePaths)
        {
            _logger.LogInformation("Parsing transaction file: {filePath}", filePath);
            using var reader = new StreamReader(filePath);

            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<AjBellTransactionMap>();
            var records = csv.GetRecords<AjBellTransaction>();
            stockTransactionItems.AddRange(records.ToList());
        }
        
        return stockTransactionItems;
    } 
}
