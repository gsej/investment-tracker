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
        using (InvestmentTrackerActivitySource.Instance.StartActivity($"File: {fileName}"))
        using (_logger.BeginScope(new Dictionary<string, string> { ["File"] = fileName }))
        {
            // Preload stocks and exchange rates.
            // TODO: check that we are only going to the db once per run for this.
            _stocks = await _stockRepository.GetStocks();
            _exchangeRates = await _exchangeRateRepository.GetAll();

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

                _stockPriceRepository.Add(stockPrice);
            }

            await _stockPriceRepository.SaveChangesAsync();
        }
    }
}
