using System.Runtime.InteropServices.JavaScript;
using Api.QueryHandlers;
using Database;
using Database.Entities;
using Database.ValueTypes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTests;

public class GetStockPriceTests : IDisposable, IAsyncDisposable
{
    private readonly InvestmentsDbContext _context;

    public GetStockPriceTests()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider();

        var builder = new DbContextOptionsBuilder<InvestmentsDbContext>();
        builder.UseInMemoryDatabase(databaseName:"InvestmentsDb")
            .UseInternalServiceProvider(serviceProvider);
        
        _context = new InvestmentsDbContext(builder.Options);
    }
    //
    // [Fact]
    // public async Task Handle_WhenNoStockPriceIsFound_ReturnsError()
    // {
    //     // act
    //     var stockPriceResult = await _context.GetStockPrice("ABC.L", "2029-10-10");
    //
    //     // assert
    //     stockPriceResult.HasPrice.Should().BeFalse();
    // }
    
    // [Theory]
    // [InlineData("2023-01-01", 0)]
    // [InlineData("2023-01-02", 1)]
    // [InlineData("2023-01-03", 2)]
    // [InlineData("2024-01-01", 365)]
    // public async Task Handle_WhenOneStockPriceIsFound_ReturnsPriceWithCorrectAge(string queryDate, int expectedAgeInDays)
    // {
    //     // arrange
    //     var stock = new Stock.StockBuilder("ABC.L", "some stock", StockTypes.Share).Build();
    //     _context.Stocks.Add(stock);
    //
    //     var expectedStockPrice = new StockPrice(stock.StockSymbol, new DateOnly(2023, 1, 1), 100m, "GBP", "CI", "GBP");
    //     _context.StockPrices.Add(expectedStockPrice);
    //
    //     await _context.SaveChangesAsync();
    //
    //     // act
    //     var actualStockPrice = await _context.GetStockPrice("ABC.L", queryDate);
    //
    //     // assert
    //     actualStockPrice.Should().NotBeNull();
    //     actualStockPrice.Price.Should().Be(100m);
    //     actualStockPrice.Currency.Should().Be("GBP");
    //     actualStockPrice.AgeInDays.Should().Be(expectedAgeInDays);
    // }
    
 // [Fact]
 //    public async Task Handle_WhenMultipleStockPricesExist_ReturnsNewestPriceNotInTheFuture()
 //    {
 //        // arrange
 //        var stock = new Stock.StockBuilder("ABC.L", "some stock", StockTypes.Share).Build();
 //        _context.Stocks.Add(stock);
 //
 //        var olderStockPrice = new StockPrice(stock.StockSymbol, new DateOnly(2023, 1, 1), 90m, "GBP", "CI", "GBP");
 //        _context.StockPrices.Add(olderStockPrice);
 //
 //        var expectedStockPrice = new StockPrice(stock.StockSymbol, new DateOnly(2023, 1, 15), 100m, "GBP", "CI", "GBP");
 //        _context.StockPrices.Add(expectedStockPrice);
 //
 //        var futureStockPrice = new StockPrice(stock.StockSymbol, new DateOnly(2023, 1, 31), 120m, "GBP", "CI", "GBP");
 //        _context.StockPrices.Add(futureStockPrice);
 //
 //        await _context.SaveChangesAsync();
 //
 //        // act
 //        var actualStockPrice = await _context.GetStockPrice("ABC.L", "2023-01-16");
 //
 //        // assert
 //        actualStockPrice.Should().NotBeNull();
 //        actualStockPrice.Price.Should().Be(100m);
 //        actualStockPrice.Currency.Should().Be("GBP");
 //        actualStockPrice.AgeInDays.Should().Be(1);
 //    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}
