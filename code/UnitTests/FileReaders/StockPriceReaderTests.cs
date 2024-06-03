using FileReaders.Prices;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace UnitTests.FileReaders;

public class StockPriceReaderTests
{
    [Fact]
    public async Task Read_GivenFileName_ReturnsStockPrices()
    {
        // Arrange
        var fileName = "FileReaders/SampleFiles/stock-prices.json";
        var stockPriceReader = new StockPriceReader(Substitute.For<ILogger<StockPriceReader>>());
        
        // Act
        var result = await stockPriceReader.ReadFile(fileName);
        
        // Assert
        using var scope = new AssertionScope();
        
        result.Should().NotBeNull();
        result.Count().Should().Be(2);
        
        var firstStockPrice = result.First();
        firstStockPrice.Should().BeEquivalentTo(new StockPrice("SMT.L", "2024-05-01", "828.52", "GBp"));
    }
    
    [Fact]
    // Some of the early stock price files don't have a currency field. This test ensures that the reader can handle this.
    public async Task Read_GivenFileWithMissingCurrency_ReturnsStockPrices()
    {
        // Arrange
        var fileName = "FileReaders/SampleFiles/stock-prices-missing-currency.json";
        var stockPriceReader = new StockPriceReader(Substitute.For<ILogger<StockPriceReader>>());
        
        // Act
        var result = await stockPriceReader.ReadFile(fileName);
        
        // Assert
        using var scope = new AssertionScope();
        
        result.Should().NotBeNull();
        result.Count().Should().Be(2);
        
        var firstStockPrice = result.First();
        firstStockPrice.Should().BeEquivalentTo(new StockPrice("SMT.L", "2024-05-01", "828.52", null));
    }
    
    [Fact]
    // We expect the price to be given as a string, but some files may have it as a number. This test ensures that the reader can handle this.
    public async Task Read_GivenFileWithPriceAsNumber_ReturnsStockPrices()
    {
        // Arrange
        var fileName = "FileReaders/SampleFiles/stock-prices-number-price.json";
        var stockPriceReader = new StockPriceReader(Substitute.For<ILogger<StockPriceReader>>());
        
        // Act
        var result = await stockPriceReader.ReadFile(fileName);
        
        // Assert
        using var scope = new AssertionScope();
        
        result.Should().NotBeNull();
        result.Count().Should().Be(1);
        
        var firstStockPrice = result.Single();
        firstStockPrice.Should().BeEquivalentTo(new StockPrice("SMT.L", "2024-05-01", "828.52", null));
    }
}
