using System.Text.Json;
using Common.Extensions;
using Database.Entities;
using Database.Repositories;
using FileReaders.Dividends;
using Microsoft.Extensions.Logging;
using Dividend = Database.Entities.Dividend;

namespace DataLoaders;

public class DividendLoader
{
    private readonly ILogger<DividendLoader> _logger;
    private readonly IStockRepository _stockRepository;
    private readonly IDividendRepository _dividendRepository;
    private readonly IExchangeRateRepository _exchangeRateRepository;
    private readonly IDividendReader _reader;
    private readonly ExchangeRateFetcher _exchangeRateFetcher;

    private IDictionary<string, Stock> _stocksDictionary;
    private List<ExchangeRate> _sortedExchangeRates;

    public DividendLoader(
        ILogger<DividendLoader> logger,
        IStockRepository stockRepository,
        IDividendRepository dividendRepository,
        IExchangeRateRepository exchangeRateRepository,
        IDividendReader reader)
    {
        _logger = logger;
        _stockRepository = stockRepository;
        _dividendRepository = dividendRepository;
        _exchangeRateRepository = exchangeRateRepository;
        _reader = reader;
        _exchangeRateFetcher = new ExchangeRateFetcher();
    }

    public async Task LoadFile(string fileName, string source)
    {
        await EnsureReferenceDataLoaded();

        var dividends = new List<Dividend>();

        using (_logger.BeginScope(new Dictionary<string, string> { ["File"] = fileName }))
        {
            _logger.LogInformation("Loading dividends from {fileName}", fileName);

            var dividendDtos = (await _reader.ReadFile(fileName)).ToList();

            foreach (var dividendDto in dividendDtos)
            {
                var stockSymbol = dividendDto.Symbol;

                if (string.IsNullOrWhiteSpace(stockSymbol))
                {
                    _logger.LogError("Stock symbol should not be null in file: {fileName}, incorrect record: {record}", fileName, JsonSerializer.Serialize(dividendDto));
                    throw new InvalidOperationException("Stock symbol should not be null in file: " + fileName);
                }

                if (!_stocksDictionary.TryGetValue(stockSymbol, out var matchingStock))
                {
                    continue;
                }

                var amountParsable = decimal.TryParse(dividendDto.Amount, null, out var amount);

                if (!amountParsable)
                {
                    _logger.LogError("Dividend amount is not a valid number. {dividendDto}", JsonSerializer.Serialize(dividendDto));
                    continue;
                }

                var date = dividendDto.Date.ToDateOnly();
                var currency = dividendDto.Currency;
                int? exchangeRateAgeInDays = null;
                string comment = null;

                if (currency.Equals("GBp"))
                {
                    amount /= 100;
                    currency = "GBP";
                }
                else if (currency.Equals("USD"))
                {
                    var exchangeRateResult = _exchangeRateFetcher.GetExchangeRate(_sortedExchangeRates, date);

                    if (exchangeRateResult.HasRate)
                    {
                        amount /= exchangeRateResult.Rate.Value;
                        currency = "GBP";
                        exchangeRateAgeInDays = exchangeRateResult.AgeInDays;
                    }
                    else
                    {
                        comment = "Missing GBP_USD exchange rate";
                    }
                }

                var dividend = new Dividend(
                    stockSymbol: matchingStock.StockSymbol,
                    date: date,
                    amount: amount,
                    currency: currency,
                    source: source,
                    originalCurrency: dividendDto.Currency,
                    exchangeRateAgeInDays: exchangeRateAgeInDays,
                    comment: comment);

                dividends.Add(dividend);
            }

            await _dividendRepository.BulkAdd(dividends);
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
