using System.Diagnostics;
using Api.QueryHandlers.Account;
using Api.QueryHandlers.Fetchers;
using Api.QueryHandlers.History;
using Api.QueryHandlers.Portfolio;
using Database;
using Database.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HistoryCalculatorConsole;

/// <summary>
/// Precalcuates the historical values of accounts, and stores them in the AccountHistoricalValue table.
///
/// Initially will use the query from the Api.  
/// </summary>
class Program
{
    private static CalculatorConfiguration _calculatorConfiguration;
    private static ILogger<Program> _logger;
    
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddMemoryCache();
                services.AddLogging((loggingBuilder) =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.AddConsole();
                    loggingBuilder.AddSeq();
                });
                
                var configurationRoot = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .AddEnvironmentVariables()
                    .Build();

                _calculatorConfiguration = configurationRoot.GetRequiredSection(nameof(CalculatorConfiguration)).Get<CalculatorConfiguration>();
                services.AddSingleton(_calculatorConfiguration);
                
                services.AddScoped<IAccountPortfolioQueryHandler, AccountPortfolioQueryHandler>();
                services.AddScoped<IAccountQueryHandler, AccountQueryHandler>();
                services.AddScoped<IRecordedTotalValueQueryHandler, RecordedTotalValueQueryHandler>();
                services.AddScoped<IAccountValueHistoryQueryHandler, AccountValueHistoryQueryHandler>();
                services.AddDbContext<InvestmentsDbContext>(
                    opts => opts.UseSqlServer(_calculatorConfiguration.SqlConnectionString)
                );
             
                services.AddTransient<IAccountFetcher, AccountFetcher>();
                services.AddTransient<IStockFetcher, StockFetcher>();
                services.AddTransient<IStockPriceFetcher, StockPriceFetcher>();
                services.AddTransient<ICashStatementItemFetcher, CashStatementItemFetcher>();
                services.AddTransient<IAccountPortfolioQueryHandler, AccountPortfolioQueryHandler>();
                services.AddTransient<IStockTransactionFetcher, StockTransactionFetcher>();
                services.AddSingleton<DateOnlyConverter>();

            })
            .Build();
        
        _logger = host.Services.GetRequiredService<ILogger<Program>>();

        try
        {
            await LoadData(host.Services);
        }
        catch (Exception e)
        {
            _logger.LogError("Error loading data: {e}", e);
            
            // causes the log to be flushed            
            ServiceProvider services = (ServiceProvider)host.Services;
            await services.DisposeAsync();
        }
    }

    private static async Task LoadData(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<InvestmentsDbContext>();
        await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM AccountHistoricalValue");
        var queryHandler = serviceProvider.GetRequiredService<IAccountValueHistoryQueryHandler>();

        var accountFetcher = serviceProvider.GetRequiredService<IAccountFetcher>();
        var accounts = await accountFetcher.GetAccounts();

        foreach (var account in accounts)
        {
            await CalculateForAccount(queryHandler, dbContext, account.AccountCode);
        }
    }

    private static async Task CalculateForAccount(IAccountValueHistoryQueryHandler queryHandler, InvestmentsDbContext dbContext, string accountCode)
    {
        var sw = Stopwatch.StartNew();
        
        _logger.LogInformation("Calculating history for {AccountCode}", accountCode);

        var request = new AccountValueHistoryRequest(accountCode, DateOnly.FromDateTime(DateTime.Now));

        var result = await queryHandler.Handle(request);

        foreach (var item in result.Items)
        {
            var entity = new Database.Entities.AccountHistoricalValue
            {
                Date = item.Date,
                AccountCode = accountCode,
                ValueInGbp = item.ValueInGbp,
                NetInflows = item.NetInflows,
                TotalPriceAgeInDays = item.TotalPriceAgeInDays,
                Comment = item.Comment,
                RecordedTotalValueInGbp = item.RecordedTotalValueInGbp,
                RecordedTotalValueSource = item.RecordedTotalValueSource
            };

            dbContext.AccountHistoricalValues.Add(entity);
        }

        await dbContext.SaveChangesAsync();

        sw.Stop();

        _logger.LogInformation("Timing: Calculated history for {AccountCode} in {elapsedMilliseconds}ms ({elapsedSeconds}s)", request.AccountCode, sw.ElapsedMilliseconds, sw.Elapsed.TotalSeconds);
        _logger.LogInformation("Found {Count} historical values", result.Items.Count);
    }
}
