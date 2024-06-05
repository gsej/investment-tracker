using Database.Entities;
using Database.Repositories;
using DataLoaders;
using FileReaders.Prices;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using UnitTests.Fakes;

namespace UnitTests.Loaders;

public class StockPriceLoaderTests
{
    private readonly StockPriceLoader _loader;
    private readonly IStockPriceReader _reader;
    private readonly FakeStockPriceRepository _stockPriceRepository = new FakeStockPriceRepository();
    
    public StockPriceLoaderTests()
    {
        _reader = Substitute.For<IStockPriceReader>();
        
        var stockRepository = Substitute.For<IStockRepository>();
        
        var stock = new Stock
        {
            StockSymbol = "SMT.L"
        };
        
        stockRepository.GetStocks().Returns(new List<Stock> {stock});
        
        _loader = new StockPriceLoader(Substitute.For<ILogger<StockPriceLoader>>(), stockRepository, _stockPriceRepository, _reader);
        
    }
    
    [Fact]
    public async Task LoadFile_WithNoPreexistingPrice_SavesStockPrices()
    {
        // arrange
        var fileName = "test.json";
        var price = 123.21m;

        var readStockPrice = new global::FileReaders.Prices.StockPrice("SMT.L", "2022-05-20", price.ToString("F2"), "GBp");
        
        _reader.ReadFile(fileName).Returns(new List<global::FileReaders.Prices.StockPrice> {readStockPrice});
        
        // act
        await _loader.LoadFile(fileName, source: "Test", true);
        
        // assert
        var addedStock = _stockPriceRepository.StockPrices.Single();

        addedStock.StockSymbol.Should().Be(readStockPrice.StockSymbol);
        addedStock.Currency.Should().Be(readStockPrice.Currency);
        addedStock.Price.Should().Be(price);
        addedStock.Source.Should().Be("Test");
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
