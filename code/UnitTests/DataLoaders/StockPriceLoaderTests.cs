using Database.Entities;
using Database.Repositories;
using Database.ValueTypes;
using DataLoaders;
using FileReaders.Prices;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using NSubstitute;
using UnitTests.Fakes;
using StockPrice = FileReaders.Prices.StockPrice;

namespace UnitTests.DataLoaders;

public class StockPriceLoaderTests
{
    private readonly StockPriceLoader _loader;
    private readonly IStockPriceReader _reader;
    private readonly FakeStockPriceRepository _stockPriceRepository = new FakeStockPriceRepository();
    private readonly IExchangeRateRepository _exchangeRateRepository = Substitute.For<IExchangeRateRepository>();
    
    public StockPriceLoaderTests()
    {
        _reader = Substitute.For<IStockPriceReader>();
        
        var stockRepository = Substitute.For<IStockRepository>();

        var stocks = new List<Stock>
        {
            new Stock.StockBuilder("VWRL.L", "", StockTypes.Etf, "Growth").Build(),
            new Stock.StockBuilder("SMT.L", "", StockTypes.Share, "Growth").Build(),
            new Stock.StockBuilder("IT25.L", "", StockTypes.Share, "MinimalRisk").Build(),
        };
        
        stockRepository.GetStocks().Returns(stocks);
        
        _loader = new StockPriceLoader(Substitute.For<ILogger<StockPriceLoader>>(),
            stockRepository,
            _stockPriceRepository,
            _exchangeRateRepository,
            _reader);
    }
    
    [Fact]
    public async Task LoadFile_SavesStockPrices()
    {
        // arrange
        var fileName = "test.json";
        var price = 102.07m;

        var readStockPrice = new StockPrice("VWRL.L", "2022-05-20", price.ToString("F2"), "GBP");
        
        _reader.ReadFile(fileName).Returns(new List<StockPrice> {readStockPrice});
        
        // act
        await _loader.LoadFile(fileName, source: "Test", true);
        
        // assert
        var addedStock = _stockPriceRepository.StockPrices.Single();

        addedStock.StockSymbol.Should().Be(readStockPrice.StockSymbol);
        addedStock.Currency.Should().Be(readStockPrice.Currency);
        addedStock.Price.Should().Be(price);
        addedStock.Source.Should().Be("Test");
        addedStock.OriginalCurrency.Should().Be("GBP");
    }
    
    [Fact]
    public async Task LoadFile_WhenStockPriceIsInGBp_ConvertsPriceToGBP()
    {
        // arrange
        var fileName = "test.json";
        var price = 899.12m;

        var readStockPrice = new StockPrice("SMT.L", "2022-05-20", price.ToString("F2"), "GBp");
        
        _reader.ReadFile(fileName).Returns(new List<StockPrice> {readStockPrice});
        
        // act
        await _loader.LoadFile(fileName, source: "Test", true);
        
        // assert
        var addedStock = _stockPriceRepository.StockPrices.Single();

        using var _ = new AssertionScope();
        addedStock.StockSymbol.Should().Be(readStockPrice.StockSymbol);
        addedStock.Currency.Should().Be("GBP");
        addedStock.Price.Should().Be(price / 100);
        addedStock.Source.Should().Be("Test");
        addedStock.OriginalCurrency.Should().Be("GBp");
    }
    
    [Fact]
    public async Task LoadFile_WhenStockPriceIsInUSD_ConvertsPriceToGBP()
    {
        // arrange
        var fileName = "test.json";
        var price = 103.75m;
        var exchangeRateUsdToGbp = 1.24930m;

        var readStockPrice = new StockPrice("IT25.L", "2022-05-20", price.ToString("F2"), "USD");
        
        _reader.ReadFile(fileName).Returns(new List<StockPrice> {readStockPrice});

        var exchangeRate = new ExchangeRate { Date = new DateOnly(2022, 5, 15), BaseCurrency = "GBP", AlternateCurrency = "USD", Rate = exchangeRateUsdToGbp};
        
        _exchangeRateRepository.GetAll().Returns(new List<ExchangeRate> {exchangeRate});
        
        // act
        await _loader.LoadFile(fileName, source: "Test", true);
        
        // assert
        var addedStock = _stockPriceRepository.StockPrices.Single();

        using var _ = new AssertionScope();
        addedStock.StockSymbol.Should().Be(readStockPrice.StockSymbol);
        addedStock.Currency.Should().Be("GBP");
        addedStock.Price.Should().Be(price / exchangeRateUsdToGbp);
        addedStock.Source.Should().Be("Test");
        addedStock.OriginalCurrency.Should().Be("USD");
        addedStock.ExchangeRateAgeInDays.Should().Be(5);
    }
    
    [Fact]
    public async Task LoadFile_WithMissingStock_DoesNotSaveStockPrice()
    {
        // arrange
        var fileName = "test.json";
        var price = 123.21m;

        var readStockPrice = new global::FileReaders.Prices.StockPrice("BP.L", "2022-05-20", price.ToString("F2"), "GBp");
        
        _reader.ReadFile(fileName).Returns(new List<global::FileReaders.Prices.StockPrice> {readStockPrice});
        
        // act
        await _loader.LoadFile(fileName, source: "Test", true);
        
        // assert
        _stockPriceRepository.StockPrices.Should().BeEmpty();
    }
    
    [Fact]
    public async Task LoadFile_WithERRORForStockPrice_DoesNotSaveStockPrice()
    {
        // We expect the string "ERROR" in some price files occasionally. Skip these.
        
        // arrange
        var fileName = "test.json";

        var readStockPrice = new global::FileReaders.Prices.StockPrice("BP.L", "2022-05-20", "ERROR", "GBp");
        
        _reader.ReadFile(fileName).Returns(new List<global::FileReaders.Prices.StockPrice> {readStockPrice});
        
        // act
        await _loader.LoadFile(fileName, source: "Test", true);
        
        // assert
        _stockPriceRepository.StockPrices.Should().BeEmpty();
    }
}
