using Api.QueryHandlers;
using Api.QueryHandlers.Fetchers;
using Api.QueryHandlers.History;
using Common;
using Database.Entities;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RecordedTotalValue = Api.QueryHandlers.History.RecordedTotalValue;

namespace UnitTests.Api.QueryHandlers;

public class AccountValueHistoryQueryHandlerTests
{
    private readonly IAccountValueHistoryQueryHandler _queryHandler;
    private readonly ICashStatementItemFetcher _cashStatementItemFetcher;
    private readonly IStockTransactionFetcher _stockTransactionFetcher;
    private readonly IStockPriceFetcher _stockPriceFetcher;

    private readonly DateOnly _startDate = new(2024, 1, 1);

    private const string AccountCode = "Account";
    private const string Symbol = "STOCK";

    public AccountValueHistoryQueryHandlerTests()
    {
        var accountFetcher = Substitute.For<IAccountFetcher>();
        accountFetcher.GetAccounts().Returns(new List<Account> { new(AccountCode, _startDate) });

        _cashStatementItemFetcher = Substitute.For<ICashStatementItemFetcher>();
        _cashStatementItemFetcher.GetCashStatementItems(Arg.Any<string[]>()).Returns(new List<CashStatementItem>());

        _stockTransactionFetcher = Substitute.For<IStockTransactionFetcher>();
        _stockTransactionFetcher.GetStockTransactions(Arg.Any<string[]>()).Returns(new List<StockTransaction>());

        _stockPriceFetcher = Substitute.For<IStockPriceFetcher>();
        _stockPriceFetcher.GetStockPrice(Arg.Any<string>(), Arg.Any<DateOnly>())
            .Returns(StockPriceResult.Missing("default"));
        _stockPriceFetcher.GetAllPrices(Arg.Any<string>())
            .Returns(new List<StockPrice>());

        var recordedTotalValueQueryHandler = Substitute.For<IRecordedTotalValueQueryHandler>();
        recordedTotalValueQueryHandler.Handle(Arg.Any<RecordedTotalValuesRequest>()).Returns(
            new RecordedTotalValuesResult(new List<RecordedTotalValue>()));

        _queryHandler = new AccountValueHistoryQueryHandler(
            accountFetcher,
            _cashStatementItemFetcher,
            _stockTransactionFetcher,
            _stockPriceFetcher,
            recordedTotalValueQueryHandler,
            Substitute.For<ILogger<AccountValueHistoryQueryHandler>>()
        );
    }

    [Fact]
    public async Task Handle_WhenNoDataExists_ReturnsEmptyItemsForEachDate()
    {
        var days = 10;
        var queryDate = _startDate.AddDays(days - 1);

        var result = await _queryHandler.Handle(new AccountValueHistoryRequest(AccountCode, queryDate));

        using var _ = new AssertionScope();
        result.Items.Count.Should().Be(days);
        result.Items.Should().AllSatisfy(item =>
        {
            item.ValueInGbp.Should().Be(0);
            item.NetInflows.Should().Be(0);
        });
    }

    [Fact]
    public async Task Handle_WhenContributionMade_TotalValueAndNetInflowsAreTracked()
    {
        // Day 0: £100 contribution → cash balance £100
        _cashStatementItemFetcher.GetCashStatementItems(Arg.Any<string[]>()).Returns(new List<CashStatementItem>
        {
            Contribution(_startDate, 100m)
        });

        var result = await _queryHandler.Handle(new AccountValueHistoryRequest(AccountCode, _startDate.AddDays(3)));

        using var _ = new AssertionScope();
        result.Items.Count.Should().Be(4);

        result.Items[0].ValueInGbp.Should().Be(100);
        result.Items[0].NetInflows.Should().Be(100);

        // Subsequent days: cash balance persists, no new contributions
        result.Items[1].ValueInGbp.Should().Be(100);
        result.Items[1].NetInflows.Should().Be(0);
        result.Items[2].ValueInGbp.Should().Be(100);
        result.Items[3].ValueInGbp.Should().Be(100);
    }

    [Fact]
    public async Task Handle_WhenStockHeld_TotalValueIncludesStockValue()
    {
        // Day 0: contribute £100
        _cashStatementItemFetcher.GetCashStatementItems(Arg.Any<string[]>()).Returns(new List<CashStatementItem>
        {
            Contribution(_startDate, 100m),
            // Day 1: spend £100 on a stock (cash balance drops to 0)
            Purchase(_startDate.AddDays(1), -100m)
        });

        _stockTransactionFetcher.GetStockTransactions(Arg.Any<string[]>()).Returns(new List<StockTransaction>
        {
            // Day 1: buy 10 shares
            StockPurchase(_startDate.AddDays(1), Symbol, quantity: 10m)
        });

        _stockPriceFetcher.GetAllPrices(Symbol).Returns(new List<StockPrice>
        {
            new(Symbol, _startDate.AddDays(1), 12m, "GBP", "test", "GBP")
        });

        var result = await _queryHandler.Handle(new AccountValueHistoryRequest(AccountCode, _startDate.AddDays(2)));

        using var _ = new AssertionScope();
        // Day 0: just cash
        result.Items[0].ValueInGbp.Should().Be(100);
        // Day 1: cash gone, 10 shares @ £12 = £120
        result.Items[1].ValueInGbp.Should().Be(120);
        // Day 2: same holdings
        result.Items[2].ValueInGbp.Should().Be(120);
    }

    [Fact]
    public async Task Handle_DiscrepancyRatio_UsesRecordedTotalValue()
    {
        _cashStatementItemFetcher.GetCashStatementItems(Arg.Any<string[]>()).Returns(new List<CashStatementItem>
        {
            Contribution(_startDate, 100m)
        });

        var recordedTotalValueQueryHandler = Substitute.For<IRecordedTotalValueQueryHandler>();
        recordedTotalValueQueryHandler.Handle(Arg.Any<RecordedTotalValuesRequest>()).Returns(
            new RecordedTotalValuesResult(new List<RecordedTotalValue>
            {
                new(_startDate, AccountCode, 95m, "TestSource")
            }));

        var accountFetcher = Substitute.For<IAccountFetcher>();
        accountFetcher.GetAccounts().Returns(new List<Account> { new(AccountCode, _startDate) });

        var queryHandler = new AccountValueHistoryQueryHandler(
            accountFetcher,
            _cashStatementItemFetcher,
            _stockTransactionFetcher,
            _stockPriceFetcher,
            recordedTotalValueQueryHandler,
            Substitute.For<ILogger<AccountValueHistoryQueryHandler>>());

        var result = await queryHandler.Handle(new AccountValueHistoryRequest(AccountCode, _startDate));

        result.Items.Single().RecordedTotalValueInGbp.Should().Be(95m);
        result.Items.Single().DiscrepancyRatio.Should().Be((100m - 95m) / 100m);
    }

    [Fact]
    public async Task Handle_DifferenceToPreviousDay_StripsOutContributions()
    {
        // Day 0: contribute £100 (no previous day → DifferenceToPreviousDay null)
        // Day 1: contribute another £50 (cash 150, previous total was 100, contribution 50 → diff 0)
        _cashStatementItemFetcher.GetCashStatementItems(Arg.Any<string[]>()).Returns(new List<CashStatementItem>
        {
            Contribution(_startDate, 100m),
            Contribution(_startDate.AddDays(1), 50m)
        });

        var result = await _queryHandler.Handle(new AccountValueHistoryRequest(AccountCode, _startDate.AddDays(1)));

        using var _ = new AssertionScope();
        result.Items[0].DifferenceToPreviousDay.Should().BeNull();
        result.Items[1].DifferenceToPreviousDay.Should().Be(0m);
        result.Items[1].DifferenceRatio.Should().Be(0m);
    }

    private static CashStatementItem Contribution(DateOnly date, decimal amount) =>
        CashItem(date, amount, CashStatementItemTypes.Contribution);

    private static CashStatementItem Purchase(DateOnly date, decimal amount) =>
        CashItem(date, amount, CashStatementItemTypes.Purchase);

    private static CashStatementItem CashItem(DateOnly date, decimal amount, string type)
    {
        var receipt = amount > 0 ? amount : 0m;
        var payment = amount < 0 ? amount : 0m;
        return new CashStatementItem(AccountCode, date, "test", receipt, payment) { CashStatementItemType = type };
    }

    private static StockTransaction StockPurchase(DateOnly date, string symbol, decimal quantity) =>
        new StockTransaction(AccountCode, date, "Purchase", "test", quantity, 0m, "ref", symbol)
        {
            TransactionType = StockTransactionTypes.Purchase,
            Fee = 0m,
            StampDuty = 0m
        };
}
