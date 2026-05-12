using Api.QueryHandlers.Fetchers;
using Api.QueryHandlers.History;
using DbEntities = Database.Entities;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace UnitTests.Api.QueryHandlers;

public class AccountValueHistoryQueryHandler2Tests
{
    private readonly IAccountValueHistoryQueryHandler2 _queryHandler;
    private readonly IAccountHistoricalValueFetcher _fetcher;

    private readonly DateOnly _startDate = new(2024, 1, 1);

    private const string AccountCodeA = "AccountA";
    private const string AccountCodeB = "AccountB";

    public AccountValueHistoryQueryHandler2Tests()
    {
        _fetcher = Substitute.For<IAccountHistoricalValueFetcher>();

        _queryHandler = new AccountValueHistoryQueryHandler2(
            Substitute.For<ILogger<AccountValueHistoryQueryHandler2>>(),
            _fetcher
        );
    }

    [Fact]
    public async Task Handle_WhenNoData_ReturnsEmptyResult()
    {
        _fetcher.Get(Arg.Any<string[]>()).Returns(new List<DbEntities.AccountHistoricalValue>());

        var result = await _queryHandler.Handle(new AccountValueHistoryRequest2([AccountCodeA], _startDate));

        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_SingleAccount_ReturnsOneResultPerDay()
    {
        var days = 3;
        var queryDate = _startDate.AddDays(days - 1);

        _fetcher.Get(Arg.Any<string[]>()).Returns(BuildEntities(AccountCodeA, _startDate, days));

        var result = await _queryHandler.Handle(new AccountValueHistoryRequest2([AccountCodeA], queryDate));

        result.Items.Count.Should().Be(days);
    }

    [Fact]
    public async Task Handle_SingleAccount_WhenNoRecordedValue_RecordedTotalValueInGbpIsNull()
    {
        var queryDate = _startDate;

        _fetcher.Get(Arg.Any<string[]>()).Returns(
        [
            new DbEntities.AccountHistoricalValue
            {
                Date = _startDate,
                AccountCode = AccountCodeA,
                ValueInGbp = 1000m,
                NetInflows = 500m,
                RecordedTotalValueInGbp = null
            }
        ]);

        var result = await _queryHandler.Handle(new AccountValueHistoryRequest2([AccountCodeA], queryDate));

        result.Items.Single().RecordedTotalValueInGbp.Should().BeNull();
    }

    [Fact]
    public async Task Handle_SingleAccount_WhenRecordedValueExists_RecordedTotalValueInGbpIsSet()
    {
        var queryDate = _startDate;

        _fetcher.Get(Arg.Any<string[]>()).Returns(
        [
            new DbEntities.AccountHistoricalValue
            {
                Date = _startDate,
                AccountCode = AccountCodeA,
                ValueInGbp = 1000m,
                NetInflows = 500m,
                RecordedTotalValueInGbp = 1050m
            }
        ]);

        var result = await _queryHandler.Handle(new AccountValueHistoryRequest2([AccountCodeA], queryDate));

        result.Items.Single().RecordedTotalValueInGbp.Should().Be(1050m);
    }

    [Fact]
    public async Task Handle_MultipleAccounts_WhenNoAccountHasRecordedValue_RecordedTotalValueInGbpIsNull()
    {
        var queryDate = _startDate;

        _fetcher.Get(Arg.Any<string[]>()).Returns(
        [
            new DbEntities.AccountHistoricalValue
            {
                Date = _startDate, AccountCode = AccountCodeA,
                ValueInGbp = 1000m, RecordedTotalValueInGbp = null
            },
            new DbEntities.AccountHistoricalValue
            {
                Date = _startDate, AccountCode = AccountCodeB,
                ValueInGbp = 2000m, RecordedTotalValueInGbp = null
            }
        ]);

        var result = await _queryHandler.Handle(new AccountValueHistoryRequest2([AccountCodeA, AccountCodeB], queryDate));

        result.Items.Single().RecordedTotalValueInGbp.Should().BeNull();
    }

    [Fact]
    public async Task Handle_MultipleAccounts_SumsValueInGbpAcrossAccounts()
    {
        var queryDate = _startDate;

        _fetcher.Get(Arg.Any<string[]>()).Returns(
        [
            new DbEntities.AccountHistoricalValue
            {
                Date = _startDate, AccountCode = AccountCodeA,
                ValueInGbp = 1000m, RecordedTotalValueInGbp = null
            },
            new DbEntities.AccountHistoricalValue
            {
                Date = _startDate, AccountCode = AccountCodeB,
                ValueInGbp = 2000m, RecordedTotalValueInGbp = null
            }
        ]);

        var result = await _queryHandler.Handle(new AccountValueHistoryRequest2([AccountCodeA, AccountCodeB], queryDate));

        result.Items.Single().ValueInGbp.Should().Be(3000m);
    }

    [Fact]
    public async Task Handle_MultipleAccounts_SumsRecordedTotalValueInGbpAcrossAccounts()
    {
        var queryDate = _startDate;

        _fetcher.Get(Arg.Any<string[]>()).Returns(
        [
            new DbEntities.AccountHistoricalValue
            {
                Date = _startDate, AccountCode = AccountCodeA,
                ValueInGbp = 1000m, RecordedTotalValueInGbp = 1050m
            },
            new DbEntities.AccountHistoricalValue
            {
                Date = _startDate, AccountCode = AccountCodeB,
                ValueInGbp = 2000m, RecordedTotalValueInGbp = 2100m
            }
        ]);

        var result = await _queryHandler.Handle(new AccountValueHistoryRequest2([AccountCodeA, AccountCodeB], queryDate));

        result.Items.Single().RecordedTotalValueInGbp.Should().Be(3150m);
    }

    [Fact]
    public async Task Handle_SingleAccount_WhenNoRecordedValue_DiscrepancyRatioIsNull()
    {
        _fetcher.Get(Arg.Any<string[]>()).Returns(
        [
            new DbEntities.AccountHistoricalValue
            {
                Date = _startDate, AccountCode = AccountCodeA,
                ValueInGbp = 1000m, RecordedTotalValueInGbp = null
            }
        ]);

        var result = await _queryHandler.Handle(new AccountValueHistoryRequest2([AccountCodeA], _startDate));

        result.Items.Single().DiscrepancyRatio.Should().BeNull();
    }

    [Fact]
    public async Task Handle_SingleAccount_WhenRecordedValueExists_DiscrepancyRatioIsCalculated()
    {
        _fetcher.Get(Arg.Any<string[]>()).Returns(
        [
            new DbEntities.AccountHistoricalValue
            {
                Date = _startDate, AccountCode = AccountCodeA,
                ValueInGbp = 1000m, RecordedTotalValueInGbp = 1050m
            }
        ]);

        var result = await _queryHandler.Handle(new AccountValueHistoryRequest2([AccountCodeA], _startDate));

        result.Items.Single().DiscrepancyRatio.Should().Be((1000m - 1050m) / 1000m);
    }

    [Fact]
    public async Task Handle_MultipleAccounts_DiscrepancyRatioUsesCombined()
    {
        _fetcher.Get(Arg.Any<string[]>()).Returns(
        [
            new DbEntities.AccountHistoricalValue
            {
                Date = _startDate, AccountCode = AccountCodeA,
                ValueInGbp = 1000m, RecordedTotalValueInGbp = 1050m
            },
            new DbEntities.AccountHistoricalValue
            {
                Date = _startDate, AccountCode = AccountCodeB,
                ValueInGbp = 2000m, RecordedTotalValueInGbp = 2100m
            }
        ]);

        var result = await _queryHandler.Handle(new AccountValueHistoryRequest2([AccountCodeA, AccountCodeB], _startDate));

        // combined: ValueInGbp=3000, RecordedTotalValueInGbp=3150
        result.Items.Single().DiscrepancyRatio.Should().Be((3000m - 3150m) / 3000m);
    }

    [Fact]
    public async Task Handle_StopsAtQueryDate()
    {
        // fetcher returns 5 days of data, but query date is day 2
        _fetcher.Get(Arg.Any<string[]>()).Returns(BuildEntities(AccountCodeA, _startDate, 5));

        var queryDate = _startDate.AddDays(2);
        var result = await _queryHandler.Handle(new AccountValueHistoryRequest2([AccountCodeA], queryDate));

        using var _ = new AssertionScope();
        result.Items.Count.Should().Be(3);
        result.Items.Last().Date.Should().Be(queryDate);
    }

    private static List<DbEntities.AccountHistoricalValue> BuildEntities(
        string accountCode, DateOnly startDate, int days) =>
        Enumerable.Range(0, days)
            .Select(i => new DbEntities.AccountHistoricalValue
            {
                Date = startDate.AddDays(i),
                AccountCode = accountCode,
                ValueInGbp = 1000m,
                NetInflows = 0m
            })
            .ToList();
}
