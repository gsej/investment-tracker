using System.Globalization;
using AjBellParserConsole.InputModels;
using CsvHelper;
using Microsoft.Extensions.Logging;

namespace AjBellParserConsole.Parsers;

public class CashStatementParser
{
    private readonly ILogger<CashStatementParser> _logger;
    
    public CashStatementParser(ILogger<CashStatementParser> logger)
    {
        _logger = logger;
    }
    
    public List<AjBellCashStatementItem> Parse(params string[] filePaths)
    {
        var cashStatementItems = new List<AjBellCashStatementItem>();

        foreach (var filePath in filePaths)
        {
            _logger.LogInformation("Parsing cash statement file: {filePath}", filePath);
            using var reader = new StreamReader(filePath);

            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<AjBellCashStatementItemMap>();
            var records = csv.GetRecords<AjBellCashStatementItem>();
            cashStatementItems.AddRange(records.ToList());
        }
        
        return cashStatementItems;
    } 
}
