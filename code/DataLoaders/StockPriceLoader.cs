using Common.Tracing;
using Database.Entities;
using Database.Repositories;
using FileReaders.Prices;
using Microsoft.Extensions.Logging;
using StockPrice = Database.Entities.StockPrice;

namespace DataLoaders;

public class StockPriceLoader
{
    private readonly ILogger<StockPriceLoader> _logger;
    private readonly IStockRepository _stockRepository;
    private readonly IStockPriceRepository _stockPriceRepository;
    private readonly IStockPriceReader _reader;
    private IList<Stock> _stocks;
   
    public StockPriceLoader(
        ILogger<StockPriceLoader> logger,
        IStockRepository stockRepository,
        IStockPriceRepository stockPriceRepository,
        IStockPriceReader reader
    )
    {
        _logger = logger;
        _stockRepository = stockRepository;
        _stockPriceRepository = stockPriceRepository;
        _reader = reader;
    }

    public async Task LoadFile(string fileName, string source, bool deduplicate)
    {
        using (InvestmentTrackerActivitySource.Instance.StartActivity($"File: {fileName}"))
        using (_logger.BeginScope(new Dictionary<string, string> { ["File"] = fileName }))
            await LoadFileInternal(fileName, source);
    }

    private async Task LoadFileInternal(string fileName, string source)
    {
        // TODO: use LazyAsync here? or LazyCache
        _stocks = await _stockRepository.GetStocks();

        _logger.LogInformation("Loading stock prices from {fileName}", fileName);

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

            var stockPrice = new StockPrice(
                stockSymbol: matchingStock.StockSymbol,
                date: date,
                price: price,
                currency: currency,
                source: source);

            _stockPriceRepository.Add(stockPrice);
        }

        await _stockPriceRepository.SaveChangesAsync();
    }
}
