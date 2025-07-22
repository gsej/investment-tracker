using System.Text.Json;
using Common.Extensions;
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
    private readonly IExchangeRateRepository _exchangeRateRepository;
    private readonly IStockPriceReader _reader;
    private readonly ExchangeRateFetcher _exchangeRateFetcher;
    private IList<Stock> _stocks;

    private IDictionary<string, Stock> _stocksDictionary;
    
    private IList<ExchangeRate> _exchangeRates;
   
    public StockPriceLoader(
        ILogger<StockPriceLoader> logger,
        IStockRepository stockRepository,
        IStockPriceRepository stockPriceRepository,
        IExchangeRateRepository exchangeRateRepository,
        IStockPriceReader reader
    )
    {
        _logger = logger;
        _stockRepository = stockRepository;
        _stockPriceRepository = stockPriceRepository;
        _exchangeRateRepository = exchangeRateRepository;
        _reader = reader;
        _exchangeRateFetcher = new ExchangeRateFetcher();
    }

    public async Task LoadFile(string fileName, string source, bool deduplicate)
    {
        var stockPrices = new List<StockPrice>();
        
        using (InvestmentTrackerActivitySource.Instance.StartActivity($"File: {fileName}"))
        using (_logger.BeginScope(new Dictionary<string, string> { ["File"] = fileName }))
        {
            // Preload stocks and exchange rates.
            
            // TODO: fix this, appearst o be happening many times. 
            _logger.LogWarning("DEBUG: loading stocks");
            _stocks = await _stockRepository.GetStocks();


            _stocksDictionary = new Dictionary<string, Stock>(StringComparer.InvariantCultureIgnoreCase);
            
            
            foreach (var stock in _stocks)
            {
                _stocksDictionary[stock.StockSymbol] = stock;
                
                foreach (var alternativeSymbol in stock.AlternativeSymbols)
                {
                    _stocksDictionary[alternativeSymbol.Alternative] = stock;
                }
            }
            
            _exchangeRates = await _exchangeRateRepository.GetAll();

            _logger.LogInformation("Loading stock prices from {fileName}", fileName);

            var stockPriceDtos = (await _reader.ReadFile(fileName)).ToList();

            foreach (var stockPriceDto in stockPriceDtos)
            {
                using var _ = _logger.BeginScope(new Dictionary<string, string> { ["File"] = fileName });

                var stockSymbol = stockPriceDto.StockSymbol;
                
                if (string.IsNullOrWhiteSpace(stockPriceDto.StockSymbol))
                {
                    _logger.LogError("Stock symbol should not be null in file: {fileName}, incorrect record: {record}", fileName, JsonSerializer.Serialize(stockPriceDto));
                    throw new InvalidOperationException("Stock symbol should not be null in file: " + fileName);
                }

                Stock matchingStock = null;
                if (!_stocksDictionary.TryGetValue(stockSymbol, out matchingStock))
                {
                    continue;
                }

                if (stockPriceDto.Price.Equals("ERROR"))
                {
                    _logger.LogInformation("Stock price was 'ERROR' for {stockSymbol}, skipping", stockSymbol);
                    continue;
                }

                var priceParsable = decimal.TryParse(stockPriceDto.Price, null, out var price);

                if (!priceParsable)
                {
                    _logger.LogError("Stock price not a valid number: {price}", stockPriceDto.Price);
                    continue;
                }

                var date = stockPriceDto.Date.ToDateOnly();
                var currency = stockPriceDto.Currency;
                int? exchangeRateAgeInDays = null;
                string comment = null;

                if (currency.Equals("GBp"))
                {
                    price /= 100;
                    currency = "GBP";
                }
                else if (currency.Equals("USD"))
                {
                    var exchangeRateResult = _exchangeRateFetcher.GetExchangeRate(_exchangeRates, date);

                    if (exchangeRateResult.HasRate)
                    {
                      price /= exchangeRateResult.Rate.Value;
                      currency = "GBP";
                      exchangeRateAgeInDays = exchangeRateResult.AgeInDays;
                    }
                    else
                    {
                        comment = "Missing GBP_USD exchange rate";
                    }
                }

                var stockPrice = new StockPrice(
                    stockSymbol: matchingStock.StockSymbol,
                    date: date,
                    price: price,
                    currency: currency,
                    source: source,
                    originalCurrency: stockPriceDto.Currency,
                    exchangeRateAgeInDays: exchangeRateAgeInDays,
                    comment: comment
                    );
                
                stockPrices.Add(stockPrice);
            }

            await _stockPriceRepository.BulkAdd(stockPrices);
        }
    }
}
