using System.Text.Json;
using Common.Extensions;
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

    private IDictionary<string, Stock> _stocksDictionary;
    private List<ExchangeRate> _sortedExchangeRates;

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
        await EnsureReferenceDataLoaded();

        var stockPrices = new List<StockPrice>();

        using (_logger.BeginScope(new Dictionary<string, string> { ["File"] = fileName }))
        {
            _logger.LogInformation("Loading stock prices from {fileName}", fileName);

            var stockPriceDtos = (await _reader.ReadFile(fileName)).ToList();

            foreach (var stockPriceDto in stockPriceDtos)
            {
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
                    _logger.LogError("Stock price is not a valid number. {stockPriceDto}", JsonSerializer.Serialize(stockPriceDto));
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
                    var exchangeRateResult = _exchangeRateFetcher.GetExchangeRate(_sortedExchangeRates, date);

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

    private async Task EnsureReferenceDataLoaded()
    {
        if (_stocksDictionary != null)
        {
            return;
        }

        var stocks = await _stockRepository.GetStocks();

        var dictionary = new Dictionary<string, Stock>(StringComparer.InvariantCultureIgnoreCase);

        foreach (var stock in stocks)
        {
            dictionary[stock.StockSymbol] = stock;

            foreach (var alternativeSymbol in stock.AlternativeSymbols)
            {
                dictionary[alternativeSymbol.Alternative] = stock;
            }
        }

        var exchangeRates = await _exchangeRateRepository.GetAll();

        _sortedExchangeRates = exchangeRates
            .OrderBy(r => r.Date)
            .ThenBy(r => r.ExchangeRateId)
            .ToList();

        _stocksDictionary = dictionary;
    }
}
