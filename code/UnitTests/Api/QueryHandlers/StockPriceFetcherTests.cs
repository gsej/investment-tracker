using Api.QueryHandlers.Fetchers;
using Database;
using Database.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace UnitTests.Api.QueryHandlers;

public class StockPriceFetcherTests
{
    private static InvestmentsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<InvestmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new InvestmentsDbContext(options);
    }

    private static StockPriceFetcher CreateFetcher(InvestmentsDbContext context) =>
        new StockPriceFetcher(new MemoryCache(new MemoryCacheOptions()), context);

    [Fact]
    public async Task GetStockPrice_WhenMultiplePricesExistForSameDate_AlwaysReturnsSamePrice()
    {
        using var context = CreateContext();

        var date = new DateOnly(2024, 6, 1);
        const string symbol = "VWRL.L";

        // Two prices for the same date — the one with the later (higher) GUID should win
        var earlier = new StockPrice(symbol, date, 100m, "GBP", "source", "GBP") { StockPriceId = new Guid("00000000-0000-0000-0000-000000000001") };
        var later   = new StockPrice(symbol, date, 200m, "GBP", "source", "GBP") { StockPriceId = new Guid("00000000-0000-0000-0000-000000000002") };

        context.StockPrices.AddRange(earlier, later);
        await context.SaveChangesAsync();

        var fetcher = CreateFetcher(context);

        var result1 = await fetcher.GetStockPrice(symbol, date);
        var result2 = await fetcher.GetStockPrice(symbol, date);

        result1.Price.Should().Be(result2.Price);
    }

    [Fact]
    public async Task GetStockPrice_WhenMultiplePricesExistForSameDate_ReturnsHighestStockPriceId()
    {
        using var context = CreateContext();

        var date = new DateOnly(2024, 6, 1);
        const string symbol = "VWRL.L";

        var earlier = new StockPrice(symbol, date, 100m, "GBP", "source", "GBP") { StockPriceId = new Guid("00000000-0000-0000-0000-000000000001") };
        var later   = new StockPrice(symbol, date, 200m, "GBP", "source", "GBP") { StockPriceId = new Guid("00000000-0000-0000-0000-000000000002") };

        context.StockPrices.AddRange(earlier, later);
        await context.SaveChangesAsync();

        var fetcher = CreateFetcher(context);

        var result = await fetcher.GetStockPrice(symbol, date);

        result.Price.Should().Be(200m);
    }
}
