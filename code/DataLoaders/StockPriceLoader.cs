using Common.Tracing;
using Database.Entities;
using Database.Repositories;
using FileReaders.Prices;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using StockPrice = Database.Entities.StockPrice;

namespace DataLoaders;

public class StockPriceLoader
{
    private readonly ILogger<StockPriceLoader> _logger;
    private readonly IStockRepository _stockRepository;
    private readonly IStockPriceRepository _stockPriceRepository;
    private readonly IStockPriceReader _reader;
    private IList<Stock> _stocks;
    
    // Need to deduplicate against existing values in the database 
    // and against previous values in the incoming file.
    
    private readonly IMemoryCache _memoryCache;

    public StockPriceLoader(
        ILogger<StockPriceLoader> logger,
        IStockRepository stockRepository,
        IStockPriceRepository stockPriceRepository,
        IStockPriceReader reader,
        IMemoryCache memoryCache
    )
    {
        _logger = logger;
        _stockRepository = stockRepository;
        _stockPriceRepository = stockPriceRepository;
        _reader = reader;
        _memoryCache = memoryCache;
    }

    public async Task LoadFile(string fileName, string source, bool deduplicate)
    {
        using (InvestmentTrackerActivitySource.Instance.StartActivity($"File: {fileName}"))
        using (_logger.BeginScope(new Dictionary<string, string> { ["File"] = fileName }))
            await LoadFileInternal(fileName, source, deduplicate); 
    }

    // [Obsolete]
    // private async Task LoadFiles(string directoryName, string source)
    // {
    //     using var _ = InvestmentTrackerActivitySource.Instance.StartActivity($"Directory: {directoryName}");
    //     var fileNames = Directory.GetFiles(directoryName, "*.json");
    //
    //
    //     foreach (var fileName in fileNames)
    //     {
    //         using (InvestmentTrackerActivitySource.Instance.StartActivity($"File: {fileName}"))
    //         using (_logger.BeginScope(new Dictionary<string, string> { ["File"] = fileName }))
    //             await LoadFile(fileName, source);
    //     }
    // }

    private async Task LoadFileInternal(string fileName, string source, bool deduplicate)
    {
        // TODO: use LazyAsync here? or LazyCache
        _stocks = await _stockRepository.GetStocks();
        
        _logger.LogInformation("Loading stock prices from {fileName}", fileName);

        var stockPricesAdded = new List<StockPrice>();
        
        var stockPrices = (await _reader.ReadFile(fileName)).ToList();

        foreach (var stockPriceDto in stockPrices)
        {
            using var _ = _logger.BeginScope(new Dictionary<string, string> { ["File"] = fileName });
            
            _logger.LogInformation("beginning to process stock price {stockPriceDto}", stockPriceDto);
            var stockSymbol = stockPriceDto.StockSymbol;

            var matchingStock = _stocks.SingleOrDefault(s =>
                s.StockSymbol.Equals(stockSymbol, StringComparison.InvariantCultureIgnoreCase) ||
                s.AlternativeSymbols.Any(a =>
                    a.Alternative.Equals(stockSymbol, StringComparison.InvariantCultureIgnoreCase)));
            
            if (matchingStock == null)
            {
                _logger.LogInformation("Stock symbol {stockSymbol} not found in database", stockSymbol);
                continue;
            }

            if (stockPriceDto.Price.Equals("ERROR"))
            {
                _logger.LogInformation("Stock price was 'ERROR' for {stockSymbol}, skipping", stockSymbol);
                continue;
            }
            
            if (string.IsNullOrWhiteSpace(stockPriceDto.StockSymbol))
            {
                _logger.LogError("Stock symbol should not be null in file: {fileName}", fileName);
                throw new InvalidOperationException("Stock symbol should not be null in file: " + fileName);
            }
            var priceParsable = decimal.TryParse(stockPriceDto.Price, null, out var price);

            if (!priceParsable)
            {
                // TODO: use logging scope to include file details etc.
                _logger.LogError("Stock price in file {fileName} not a valid number: {price}", fileName, stockPriceDto.Price);
                continue;
            }
            
            string date = stockPriceDto.Date.Substring(0, 10);
            
            var currency = stockPriceDto.Currency;

            StockPrice stockPrice;

             if (deduplicate)
             {
                 stockPrice = await DeduplicateStockPrice(source, stockSymbol, date, matchingStock, stockPricesAdded, price, stockPriceDto, currency);
             }
             else
             {
                 stockPrice = new StockPrice(
                     stockSymbol: matchingStock.StockSymbol,
                     date: date,
                     price: price,
                     currency: currency,
                     source: source);

                 _stockPriceRepository.Add(stockPrice);

             }
        }
        await _stockPriceRepository.SaveChangesAsync();
        
        // TODO: this cache is only used if deduplicate is true.
        if (_memoryCache is MemoryCache concreteMemoryCache)
        {
            _logger.LogInformation("Clearing cache");
            concreteMemoryCache.Clear();
        }
    }

    private async Task<StockPrice> DeduplicateStockPrice(string source, string stockSymbol, string date, Stock matchingStock, List<StockPrice> stockPricesAdded, decimal price, FileReaders.Prices.StockPrice stockPriceDto, string currency)
    {
        var existingStockPricesInDb = await GetStockPrices(stockSymbol);

        // TODO: need to also check ones loaded from the current file but yet unsaved.

        var existing = existingStockPricesInDb.SingleOrDefault(s =>
            s.Date == date && s.StockSymbol == matchingStock.StockSymbol);

        if (existing == null)
        {
            existing = stockPricesAdded.SingleOrDefault(s =>
                s.Date == date && s.StockSymbol == matchingStock.StockSymbol);
        }

        if (existing != null)
        {
            if (existing.Price != price)
            {
                var difference = (existing.Price - price) / existing.Price;

                // TODO: take currency into consideration. 
                var logLevel = (Math.Abs(difference) > 0.01m) ? LogLevel.Error : LogLevel.Information;

                if (existing.Currency != stockPriceDto.Currency && logLevel == LogLevel.Error)
                {
                    logLevel = LogLevel.Warning;
                }

                _logger.Log(logLevel, "Price discrepancy ({difference} %) for {symbol} on {date}, preferring {price} in {currency} from {source} over {newPrice} in {newCurrency} from {newSource}",
                    difference.ToString("P3"),
                    matchingStock.StockSymbol,
                    date,
                    existing.Price,
                    existing.Currency,
                    existing.Source,
                    stockPriceDto.Price,
                    stockPriceDto.Currency,
                    source);
            }
        }

        if (existing == null)
        {
            if (string.IsNullOrWhiteSpace(currency))
            {
                _logger.LogError("Currency not found for {stockSymbol} on {date}", matchingStock.StockSymbol, date);
            }

            var stockPrice = new StockPrice(
                stockSymbol: matchingStock.StockSymbol,
                date: date,
                price: price,
                currency: currency,
                source: source);

       //     _stockPriceRepository.Add(stockPrice);
            stockPricesAdded.Add(stockPrice);
        }

        return existing;
    }

    private async Task<IList<StockPrice>> GetStockPrices(string stockSymbol)
    {
        var key = string.Format(CacheKeys.StockPrices, stockSymbol);

        if (_memoryCache.TryGetValue(key, out object cached) &&
            cached is IList<StockPrice> stockPrices)
        {
            _logger.LogInformation($"{key} is in cache");
        }
        else
        {
            _logger.LogInformation($"{key} is not in the cache");
            stockPrices = await _stockPriceRepository.GetAll(stockSymbol);
            _memoryCache.Set(key, stockPrices, 
                new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
        }

        return stockPrices;
    }

    private static class CacheKeys
    {
        public const string StockPrices = "StockPrices_{0}";
    }
}
