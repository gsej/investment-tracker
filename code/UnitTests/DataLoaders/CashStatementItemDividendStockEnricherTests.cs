using Database;
using Database.Entities;
using Database.ValueTypes;
using DataLoaders.CashStatementItemEnrichers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace UnitTests.DataLoaders;

public class CashStatementItemDividendStockEnricherTests
{
    private static InvestmentsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<InvestmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new InvestmentsDbContext(options);
    }

    private static CashStatementItemDividendStockEnricher CreateEnricher(InvestmentsDbContext context) =>
        new(context, Substitute.For<ILogger<CashStatementItemDividendStockEnricher>>());

    private static CashStatementItem CreateDividendItem(string description, decimal receiptAmountGbp = 10m) =>
        new("ISA", new DateOnly(2024, 1, 1), description, receiptAmountGbp, 0m);

    [Fact]
    public async Task Enrich_WithStandardFormat_MatchingByStockDescription_SetsStockSymbolAndQuantity()
    {
        using var context = CreateContext();
        var stock = new Stock.StockBuilder("VUCP.L", "Vanguard USD Corp Bd UCITS ETF GBP", StockTypes.Etf, "Bonds").Build();
        context.Stocks.Add(stock);
        await context.SaveChangesAsync();

        var item = CreateDividendItem("Dividend 19   Vanguard USD Corp Bd UCITS ETF GBP", receiptAmountGbp: 5.23m);

        await CreateEnricher(context).Enrich(item);

        item.StockSymbol.Should().Be("VUCP.L");
        item.DividendQuantity.Should().Be(19m);
    }

    [Fact]
    public async Task Enrich_WithStandardFormat_MatchingByAlias_SetsStockSymbolAndQuantity()
    {
        using var context = CreateContext();
        var alias = new StockAlias("VANGUARD FUNDS PLC  VANGUARD USD CORP BOND UCITS ETF USD DIS");
        var stock = new Stock.StockBuilder("VUCP.L", "Vanguard USD Corp Bd UCITS ETF GBP", StockTypes.Etf, "Bonds")
            .WithAliases(new List<StockAlias> { alias })
            .Build();
        context.Stocks.Add(stock);
        await context.SaveChangesAsync();

        var item = CreateDividendItem("Dividend 19   VANGUARD FUNDS PLC  VANGUARD USD CORP BOND UCITS ETF USD DIS", receiptAmountGbp: 5.23m);

        await CreateEnricher(context).Enrich(item);

        item.StockSymbol.Should().Be("VUCP.L");
        item.DividendQuantity.Should().Be(19m);
    }

    [Fact]
    public async Task Enrich_WithStandardFormat_IsCaseInsensitive()
    {
        using var context = CreateContext();
        var stock = new Stock.StockBuilder("SMT.L", "SCOTTISH MORTGAGE ORD GBP0.05", StockTypes.Share, "Growth").Build();
        context.Stocks.Add(stock);
        await context.SaveChangesAsync();

        var item = CreateDividendItem("Dividend 1009   scottish mortgage ord gbp0.05");

        await CreateEnricher(context).Enrich(item);

        item.StockSymbol.Should().Be("SMT.L");
    }

    [Fact]
    public async Task Enrich_WithDecimalQuantityInStandardFormat_SetsDecimalDividendQuantity()
    {
        using var context = CreateContext();
        var stock = new Stock.StockBuilder("TR25.L", "HM TREASURY  GILT 4.125% (29/01/27)", StockTypes.Gilt, "Bonds").Build();
        context.Stocks.Add(stock);
        await context.SaveChangesAsync();

        var item = CreateDividendItem("Dividend 16080.50   HM TREASURY  GILT 4.125% (29/01/27)");

        await CreateEnricher(context).Enrich(item);

        item.StockSymbol.Should().Be("TR25.L");
        item.DividendQuantity.Should().Be(16080.50m);
    }

    [Fact]
    public async Task Enrich_WithGroupDividendFormat_SetsStockSymbolAndQuantity()
    {
        using var context = CreateContext();
        var alias = new StockAlias("VANGUARD INVESTMENTS MONEY MKT FDS  VANGUARD STG SHT TR MNY MKT INV");
        var stock = new Stock.StockBuilder("VGOV.L", "Vanguard Sterling Short-Term Money Market Fund", StockTypes.Etf, "Cash")
            .WithAliases(new List<StockAlias> { alias })
            .Build();
        context.Stocks.Add(stock);
        await context.SaveChangesAsync();

        var item = CreateDividendItem("Dividend Grp 1 12978.22890 VANGUARD INVESTMENTS MONEY MKT FDS  VANGUARD STG SHT TR MNY MKT INV");

        await CreateEnricher(context).Enrich(item);

        item.StockSymbol.Should().Be("VGOV.L");
        item.DividendQuantity.Should().Be(12978.22890m);
    }

    [Fact]
    public async Task Enrich_WithEqualisationFormat_SetsNullQuantityAndMatchesStock()
    {
        using var context = CreateContext();
        var alias = new StockAlias("VANGUARD INVESTMENTS MONEY MKT FDS  VANGUARD STG SHT TR MNY MKT INV");
        var stock = new Stock.StockBuilder("VGOV.L", "Vanguard Sterling Short-Term Money Market Fund", StockTypes.Etf, "Cash")
            .WithAliases(new List<StockAlias> { alias })
            .Build();
        context.Stocks.Add(stock);
        await context.SaveChangesAsync();

        var item = CreateDividendItem("Equalisation VANGUARD INVESTMENTS MONEY MKT FDS  VANGUARD STG SHT TR MNY MKT INV");

        await CreateEnricher(context).Enrich(item);

        item.StockSymbol.Should().Be("VGOV.L");
        item.DividendQuantity.Should().BeNull();
    }

    [Fact]
    public async Task Enrich_WhenNoStockMatches_DoesNotSetStockSymbolButDoesSetQuantity()
    {
        using var context = CreateContext();
        var stock = new Stock.StockBuilder("VUCP.L", "Vanguard USD Corp Bd UCITS ETF GBP", StockTypes.Etf, "Bonds").Build();
        context.Stocks.Add(stock);
        await context.SaveChangesAsync();

        var item = CreateDividendItem("Dividend 50   UNKNOWN COMPANY  UNKNOWN FUND");

        await CreateEnricher(context).Enrich(item);

        item.StockSymbol.Should().BeNull();
        item.DividendQuantity.Should().Be(50m);
    }

    [Fact]
    public async Task Enrich_WithBareDistribution_DoesNotSetStockSymbolOrQuantity()
    {
        using var context = CreateContext();

        var item = CreateDividendItem("Distribution");

        await CreateEnricher(context).Enrich(item);

        item.StockSymbol.Should().BeNull();
        item.DividendQuantity.Should().BeNull();
    }
}
