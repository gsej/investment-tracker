using System.Reflection;
using Common.Tracing;
using Database;
using Database.Converters;
using Database.Repositories;
using DataLoaders;
using FileReaders;
using FileReaders.Accounts;
using FileReaders.AccountStatements;
using FileReaders.ExchangeRates;
using FileReaders.Prices;
using FileReaders.Stocks;
using LoaderConsole.StockTransactionEnrichers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LoaderConsole;

/// <summary>
/// loads various data files and adds to the database.
/// </summary>
class Program
{
    private static LoaderConfiguration _configuration;
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

                _configuration = configurationRoot.GetRequiredSection(nameof(LoaderConfiguration)).Get<LoaderConfiguration>();

                services.AddDbContext<InvestmentsDbContext>(
                    opts => opts.UseSqlServer(_configuration.SqlConnectionString)
                );

                services.AddTransient<IAccountRepository, AccountRepository>();
                services.AddTransient<IStockRepository, StockRepository>();
                services.AddTransient<IStockPriceRepository, StockPriceRepository>();
                services.AddTransient<IExchangeRateRepository, ExchangeRateRepository>();

                services.AddTransient<ICashStatementReader, CashStatementReader>();
                services.AddTransient<CashStatementItemLoader>();

                services.AddTransient<StockTransactionTypeEnricher>();
                services.AddTransient<StockTransactionFeeEnricher>();
                services.AddTransient<StockTransactionStampDutyEnricher>();

                services.AddTransient<IStockTransactionReader, StockTransactionReader>();
                services.AddTransient<StockTransactionLoader>();

                services.AddTransient<IAccountReader, AccountReader>();
                services.AddTransient<AccountLoader>();
                
                services.AddTransient<IStockReader, StockReader>();
                services.AddTransient<StockLoader>();
                
                services.AddTransient<IExchangeRateReader, ExchangeRateReader>();
                services.AddTransient<ExchangeRateLoader>();

                
                services.AddTransient<IStockPriceReader, StockPriceReader>();
                services.AddTransient<StockPriceLoader>();

                services.AddTransient<IRecordedTotalValueReader, RecordedTotalValueReader>();
                services.AddTransient<RecordedTotalValueLoader>();

                services.AddSingleton<DateOnlyConverter>();

            })
            .Build();
        
        using var traceProvider = TracerProviderFactory.GetTracerProvider("Loader", _configuration.AppInsightsConnectionString);
            
        using var activity = InvestmentTrackerActivitySource.Instance.StartActivity();

        using (InvestmentTrackerActivitySource.Instance.StartActivity("EnsureDatabase"))
        {
            EnsureDatabase(host.Services);
        }
        
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
        var accountLoader = serviceProvider.GetRequiredService<AccountLoader>();
        var stockLoader = serviceProvider.GetRequiredService<StockLoader>();
        var cashStatementLoader = serviceProvider.GetRequiredService<CashStatementItemLoader>();
        var stockTransactionLoader = serviceProvider.GetRequiredService<StockTransactionLoader>();
        var exchangeRateLoader = serviceProvider.GetRequiredService<ExchangeRateLoader>();
        var stockPriceLoader = serviceProvider.GetRequiredService<StockPriceLoader>();
        var recordedTotalValueLoader = serviceProvider.GetRequiredService<RecordedTotalValueLoader>();
        
        var accountRepository = serviceProvider.GetRequiredService<IAccountRepository>();
        
        var datafolder = GetPathToDataFolder();

        await accountLoader.LoadFile(Path.Combine(datafolder, "accounts.json"));
        await stockLoader.LoadFile(Path.Combine(datafolder, "stocks.json"));
            
        using (InvestmentTrackerActivitySource.Instance.StartActivity("LoadCashStatements"))
        {
            var accounts = await accountRepository.GetAll();

            foreach (var account in accounts)
            {
                await cashStatementLoader.Load(Path.Combine(datafolder, "AccountStatements", account.AccountCode, "cashstatement_items.json"));
                await stockTransactionLoader.Load(Path.Combine(datafolder, "AccountStatements", account.AccountCode, "transactions.json"));
            }
        }
        
        using (InvestmentTrackerActivitySource.Instance.StartActivity("LoadExchangeRates"))
        {
            var exchangeRateFolder = GetPathToExchangeRateFolder();
           
            foreach (var exchangeRateFile in Directory.EnumerateFiles(exchangeRateFolder, "*.json", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(exchangeRateFolder, exchangeRateFile);
                await exchangeRateLoader.LoadFile(exchangeRateFile, relativePath);
            }
        }
        
        using (InvestmentTrackerActivitySource.Instance.StartActivity("LoadStockPrices"))
        {
            var priceFolder = GetPathToPriceFolder();
            
            foreach (var priceFile in Directory.EnumerateFiles(priceFolder, "*.json", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(priceFolder, priceFile);
                await stockPriceLoader.LoadFile(priceFile, relativePath, _configuration.DeduplicateStockPrices);
            }
        }
    }

    private static string GetPathToPriceFolder()
    {
        var priceFolder = _configuration.PriceFolder;
        
        if (!string.IsNullOrWhiteSpace(priceFolder))
        {
            return priceFolder;
        }
        
        return Path.Combine(GetPathToDataFolder(), "Prices");
    }
    
    private static string GetPathToExchangeRateFolder()
    {
        var exchangeRateFolder = _configuration.ExchangeRateFolder;
        
        if (!string.IsNullOrWhiteSpace(exchangeRateFolder))
        {
            return exchangeRateFolder;
        }
        
        return Path.Combine(GetPathToDataFolder(), "ExchangeRates");
    }

    private static string GetPathToDataFolder()
    {
        // We walk up the from where we're running from to find the SampleData folder, unless we've been given a configured directory
        // in appsettings.json, in which case that will be used. The file structure is important and the names of the files are significant. 
        // This is documented in the readme in the SampleData folder.
        
        var dataFolder = _configuration.DataFolder;

        if (!string.IsNullOrWhiteSpace(dataFolder))
        {
            return dataFolder;
        }

        var location = Assembly.GetExecutingAssembly().Location;
        var directory = new FileInfo(location).Directory; 
            
        while (directory != null &&directory.EnumerateDirectories().All(d => d.Name != "SampleData"))
        {
            directory = directory.Parent;
        }

        if (directory == null)
        {
            throw new InvalidOperationException("could not find data folder");
        }
            
        dataFolder = directory.EnumerateDirectories().Single(d => d.Name == "SampleData").FullName;

        _logger.LogInformation("Datafolder: {dataFolder}", dataFolder);
        return dataFolder;
    }

    /// <summary>
    /// Creates database (using migrations)
    /// Loads some static data (e.g. stocks)
    /// </summary>
    /// <param name="services"></param>
    private static void EnsureDatabase(IServiceProvider services)
    {
        var context = services.GetRequiredService<InvestmentsDbContext>();
        context.Database.EnsureDeleted();
        context.Database.Migrate();

        context.SaveChanges();
    }
}
