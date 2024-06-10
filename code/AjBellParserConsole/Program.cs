using System.Text.Json;
using AjBellParserConsole.InputModels;
using AjBellParserConsole.Mappers;
using AjBellParserConsole.Parsers;
using Common;
using Common.Tracing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AjBellParserConsole;

class Program
{
    private static ParserConfiguration _configuration;
    private static ILogger<Program> _logger;

    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.AddConsole();
                    loggingBuilder.AddSeq();
                });

                var configurationRoot = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .AddEnvironmentVariables()
                    .Build();

                _configuration = configurationRoot.GetRequiredSection(nameof(ParserConfiguration)).Get<ParserConfiguration>();

                services.AddTransient<CashStatementParser>();
                services.AddTransient<StockTransactionParser>();

            })
            .Build();

        using var traceProvider = TracerProviderFactory.GetTracerProvider("AjBellParser", _configuration.AppInsightsConnectionString);
          
        using var activity = InvestmentTrackerActivitySource.Instance.StartActivity();

        _logger = host.Services.GetRequiredService<ILogger<Program>>();

        var cashStatementParser = host.Services.GetRequiredService<CashStatementParser>();
        var stockTransactionParser = host.Services.GetRequiredService<StockTransactionParser>();

        var dataFolder = DirectoryHelper.GetDataFolder(_configuration.DataFolder);
        var accountStatementsFolder = Path.Combine(dataFolder, "AccountStatements");

        _logger.LogInformation("Account statements folder: {accountsStatementsFolder}", accountStatementsFolder);

        // treat each folder in the accounts folder as an account

        var accountStatementFolders = Directory.EnumerateDirectories(accountStatementsFolder);

        foreach (var accountFolder in accountStatementFolders)
        {
            _logger.LogInformation("Account folder: {accountFolder}", accountFolder);

            ProcessCashStatements(accountFolder, cashStatementParser);
            ProcessStockTransactions(accountFolder, stockTransactionParser);
        }

        // causes the log to be flushed            
        ServiceProvider services = (ServiceProvider)host.Services;
        await services.DisposeAsync();
    }

    private static void ProcessCashStatements(string accountFolder, CashStatementParser cashStatementParser)
    {
        var allInputCashStatementItems = new List<AjBellCashStatementItem>();
            
        var fileNames = Directory.GetFiles(accountFolder, "*cash*.csv");

        var items = cashStatementParser.Parse(fileNames);
            
        allInputCashStatementItems.AddRange(items);

        var accountCode = Path.GetFileName(accountFolder);

        var outputCashStatementItems = new CashStatementItemMapper(accountCode)
            .Map(allInputCashStatementItems)
            .OrderBy(i => i.Date);
            
        var jsonString = JsonSerializer.Serialize(outputCashStatementItems, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        string filePath = Path.Combine(accountFolder, "cashstatement_items.json");

        File.WriteAllText(filePath, jsonString);
    }
    
    private static void ProcessStockTransactions(string accountFolder, StockTransactionParser parser)
    {
        var inputItems = new List<AjBellTransaction>();
            
        var fileNames = Directory.GetFiles(accountFolder, "*transaction*.csv");

        var items = parser.Parse(fileNames);
            
        inputItems.AddRange(items);

        var accountCode = Path.GetFileName(accountFolder);

        var outputItems = new StockTransactionMapper(accountCode)
            .Map(inputItems)
            .OrderBy(i => i.Date);
            
        var jsonString = JsonSerializer.Serialize(outputItems, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        string filePath = Path.Combine(accountFolder, "transactions.json");

        File.WriteAllText(filePath, jsonString);
    }
}
