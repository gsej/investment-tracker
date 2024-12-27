using Api.QueryHandlers.Fetchers;
using Api.QueryHandlers.History;
using Api.QueryHandlers.Portfolio;
using Database.Entities;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RecordedTotalValue = Api.QueryHandlers.History.RecordedTotalValue;

namespace UnitTests.Api.QueryHandlers;

public class AccountValueHistoryQueryHandlerTests
{
    private readonly IAccountValueHistoryQueryHandler _queryHandler;
    private readonly FakeAccountPortfolioQueryHandler _accountPortfolioQueryHandler;

    private readonly DateOnly _startDate = new(2024, 1, 1);

    private const string AccountCode = "Account";

    public AccountValueHistoryQueryHandlerTests()
    {
        _accountPortfolioQueryHandler = new FakeAccountPortfolioQueryHandler();

        var mockAccountFetcher = Substitute.For<IAccountFetcher>();

        mockAccountFetcher.GetAccounts().Returns(new List<Account> { new(AccountCode, _startDate) });

        var mockRecordedTotalValueQueryHandler = Substitute.For<IRecordedTotalValueQueryHandler>();
        mockRecordedTotalValueQueryHandler.Handle(Arg.Any<RecordedTotalValuesRequest>()).Returns(
            new RecordedTotalValuesResult(new List<RecordedTotalValue>()));

        _queryHandler = new AccountValueHistoryQueryHandler(
            mockAccountFetcher,
            _accountPortfolioQueryHandler,
            mockRecordedTotalValueQueryHandler,
            Substitute.For<ILogger<AccountValueHistoryQueryHandler>>()
        );
    }

    [Fact]
    public async Task Handle_WhenNoDataExists_ReturnsEmptyItemsForEachDate()
    {
        // arrange
        var days = 10;
        var queryDate = _startDate.AddDays(days - 1);

        var request = new AccountValueHistoryRequest(AccountCode, queryDate);

        // act
        var result = await _queryHandler.Handle(request);

        // assert
        using var _ = new AssertionScope();
        result.Items.Count.Should().Be(days);

        result.Items.Should().AllSatisfy(item =>
        {
            item.ValueInGbp.Should().Be(0);
            item.Contributions.Should().Be(0);
        });
    }

    [Fact]
    public async Task Handle_WhenPortfolioDataExists_ReturnsValueData()
    {
        // arrange
        var days = 4;
        var queryDate = _startDate.AddDays(days - 1);

        var request = new AccountValueHistoryRequest(AccountCode, queryDate);

        // On the first day, contribute £100
        _accountPortfolioQueryHandler.Add(_startDate, cashBalanceInGbp: 100, valueInGbp: 100, contribution: 100);

        // Second day, cash balance shifts into stocks:
        _accountPortfolioQueryHandler.Add(_startDate.AddDays(1), cashBalanceInGbp: 0, valueInGbp: 100, contribution: 0);

        // Third day, another contribution of £100
        _accountPortfolioQueryHandler.Add(_startDate.AddDays(2), cashBalanceInGbp: 100, valueInGbp: 200, contribution: 100);

        // Fourth day, value doubles due to stock price rises
        _accountPortfolioQueryHandler.Add(_startDate.AddDays(3), cashBalanceInGbp: 100, valueInGbp: 400, contribution: 0);

        // act
        var result = await _queryHandler.Handle(request);

        // assert
        using var _ = new AssertionScope();
        result.Items.Count.Should().Be(days);

        result.Items[0].ValueInGbp.Should().Be(100);
        result.Items[0].Contributions.Should().Be(100);

        result.Items[1].ValueInGbp.Should().Be(100);
        result.Items[1].Contributions.Should().Be(0);

        result.Items[2].ValueInGbp.Should().Be(200);
        result.Items[2].Contributions.Should().Be(100);

        result.Items[3].ValueInGbp.Should().Be(400);
        result.Items[3].Contributions.Should().Be(0);
    }


    [Fact]
    public async Task Handle_WhenPortfolioDataExists_CalculatesNumberAndValueOfUnits()
    {
        
        // TODO: 
        // test when initial portfolio values are 0.
        // test when there's a withdrawal - i.e. negative contribution.
        
        // arrange
        var days = 4;
        var queryDate = _startDate.AddDays(days - 1);

        var request = new AccountValueHistoryRequest(AccountCode, queryDate);

        // On day 0, contribute £100
        _accountPortfolioQueryHandler.Add(_startDate, cashBalanceInGbp: 100, valueInGbp: 100, contribution: 100);

        // day 1, cash balance shifts into stocks:
        _accountPortfolioQueryHandler.Add(_startDate.AddDays(1), cashBalanceInGbp: 0, valueInGbp: 100, contribution: 0);

        // day 2, another contribution of £100
        _accountPortfolioQueryHandler.Add(_startDate.AddDays(2), cashBalanceInGbp: 100, valueInGbp: 200, contribution: 100);

        // day 3, value doubles due to stock price rises
        _accountPortfolioQueryHandler.Add(_startDate.AddDays(3), cashBalanceInGbp: 100, valueInGbp: 400, contribution: 0);

        // act
        var result = await _queryHandler.Handle(request);

        var units = new UnitCalculator().Calculate(result.Items, 100);

        // assert
        using var _ = new AssertionScope();

        units.Count.Should().Be(days);

        units[0].Should().Be(new UnitAccount(_startDate, NumberOfUnits: 1, ValueInGbpPerUnit: 100m));
        units[1].Should().Be(new UnitAccount(_startDate.AddDays(1), NumberOfUnits: 1, ValueInGbpPerUnit: 100m));
        units[2].Should().Be(new UnitAccount(_startDate.AddDays(2), NumberOfUnits: 2, ValueInGbpPerUnit: 100m));
        units[3].Should().Be(new UnitAccount(_startDate.AddDays(3), NumberOfUnits: 2, ValueInGbpPerUnit: 200m));
        
        (units[0].NumberOfUnits * units[0].ValueInGbpPerUnit).Should().Be(100);
        (units[1].NumberOfUnits * units[1].ValueInGbpPerUnit).Should().Be(100);
        (units[2].NumberOfUnits * units[2].ValueInGbpPerUnit).Should().Be(200);
        (units[3].NumberOfUnits * units[3].ValueInGbpPerUnit).Should().Be(400);
    }

    public class FakeAccountPortfolioQueryHandler : IAccountPortfolioQueryHandler
    {
        private Dictionary<DateOnly, AccountPortfolioResult> _items = new();

        public void Add(DateOnly date, decimal cashBalanceInGbp, decimal valueInGbp, decimal contribution)
        {
            _items.Add(date, new AccountPortfolioResult("AccountCode", new List<Holding>(), cashBalanceInGbp, contribution, new TotalValue(valueInGbp, 0), new List<Allocation>()));
        }

        public Task<AccountPortfolioResult> Handle(AccountPortfolioRequest request)
        {
            if (_items.ContainsKey(request.Date))
            {
                return Task.FromResult(_items[request.Date]);
            }
            else
            {
                return Task.FromResult(new AccountPortfolioResult("AccountCode", new List<Holding>(), 0, 0, new TotalValue(0, 0), new List<Allocation>()));
            }

        }
    }
}
