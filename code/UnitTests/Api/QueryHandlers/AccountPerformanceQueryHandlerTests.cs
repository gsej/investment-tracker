using Api.QueryHandlers;
using Api.QueryHandlers.Fetchers;
using Api.QueryHandlers.History;
using Api.QueryHandlers.Performance;
using Api.QueryHandlers.Portfolio;
using Common;
using Database.Entities;
using Database.ValueTypes;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RecordedTotalValue = Api.QueryHandlers.Fetchers.RecordedTotalValue;

namespace UnitTests.Api.QueryHandlers;

public class AccountPerformanceQueryHandlerTests
{
    private readonly IAccountPerformanceQueryHandler _queryHandler;

    private readonly ICashStatementItemFetcher _mockCashStatementItemFetcher;
    private readonly IStockTransactionFetcher _mockStockTransactionFetcher;

    private readonly DateOnly _startDate = new(2020, 1, 1);
    private readonly IStockPriceFetcher _mockStockPriceFetcher;
    private readonly IRecordedTotalValueFetcher _mockRecordedTotalValueFetcher;

    private const string AccountCode = "Account";

    public AccountPerformanceQueryHandlerTests()
    {
        var mockAccountFetcher = Substitute.For<IAccountFetcher>();
        mockAccountFetcher.GetAccounts().Returns(new List<Account> { new(AccountCode, _startDate) });

        _mockRecordedTotalValueFetcher = Substitute.For<IRecordedTotalValueFetcher>();
        _mockRecordedTotalValueFetcher.GetRecordedTotalValues(AccountCode).Returns(new List<RecordedTotalValue>());
        
        var stocks = new List<Stock> { new() { StockSymbol = "SMT.L", Description = "Scottish Mortgage Trust", StockType = StockTypes.Share, Allocation = "Growth" } };
        var stockFetcher = Substitute.For<IStockFetcher>();
        stockFetcher.GetStocks().Returns(stocks);
        
        _mockCashStatementItemFetcher = Substitute.For<ICashStatementItemFetcher>();
        _mockStockTransactionFetcher = Substitute.For<IStockTransactionFetcher>();
        _mockStockPriceFetcher = Substitute.For<IStockPriceFetcher>();
        
        _mockStockPriceFetcher.GetStockPrice(Arg.Any<string>(), Arg.Any<DateOnly>())
            .Returns(callInfo => StockPriceResult.Missing(callInfo.Arg<string>()));
        
        var accountPortfolioQueryHandler = new AccountPortfolioQueryHandler(
            Substitute.For<ILogger<AccountPortfolioQueryHandler>>(),
            stockFetcher,
            _mockStockPriceFetcher,
            _mockCashStatementItemFetcher,
            _mockStockTransactionFetcher
        );

        var accountValueHistoryQueryHandler = new AccountValueHistoryQueryHandler(
            mockAccountFetcher,
            accountPortfolioQueryHandler,
            _mockRecordedTotalValueFetcher,
            Substitute.For<ILogger<AccountValueHistoryQueryHandler>>()
        );

        _queryHandler = new AccountPerformanceQueryHandler(Substitute.For<ILogger<AccountPerformanceQueryHandler>>(), accountValueHistoryQueryHandler);
    }

    [Fact]
    public async Task Handle_ReturnsItemForEachCalendarYear()
    {
        // arrange
        var years = 3;
        var queryDate = _startDate.AddYears(years).AddDays(-1);

        var request = new AccountPerformanceRequest(AccountCode, queryDate);

        // act
        var result = await _queryHandler.Handle(request);

        // assert
        using var _ = new AssertionScope();
        result.Items.Count.Should().Be(years);
        
        // items should have a Period property which should contain a year, eg. "2020", "2021", "2022"
        for (var year = _startDate.Year; year < _startDate.Year + years; year++)
        {
            result.Items.Should().ContainSingle(i => i.Period == year.ToString());
        }
    }

    [Fact]
    public async Task Handle_WhenInflowsOccur_ReturnsInflows()
    {
        // arrange
        var years = 3;
        var queryDate = _startDate.AddYears(years).AddDays(-1);

        var cashStatementItems = new List<CashStatementItem>
        {
            new(AccountCode, _startDate.AddDays(100), "contribute £75", 75m, 0) { CashStatementItemType = CashStatementItemTypes.Contribution }, 
            new(AccountCode, _startDate.AddDays(300), "contribute £25", 25m, 0) { CashStatementItemType = CashStatementItemTypes.Contribution },
            new(AccountCode, _startDate.AddYears(1).AddDays(100), "contribute £125", 125m, 0) { CashStatementItemType = CashStatementItemTypes.Contribution }, 
            new(AccountCode, _startDate.AddYears(1).AddDays(200), "contribute £75", 75m, 0) { CashStatementItemType = CashStatementItemTypes.Contribution }, 
            new(AccountCode, _startDate.AddYears(2).AddDays(150), "Withdraw £50", 0, -50m) { CashStatementItemType = CashStatementItemTypes.Withdrawal },
        };
    
        _mockCashStatementItemFetcher.GetCashStatementItems(AccountCode).Returns(cashStatementItems);
        
        var request = new AccountPerformanceRequest(AccountCode, queryDate);

        // act
        var result = await _queryHandler.Handle(request);
        
        // assert
        using var _ = new AssertionScope();
        result.Items.Count.Should().Be(years);
        
        result.Items[0].Should().BeEquivalentTo(new { Inflows = 100m });
        result.Items[1].Should().BeEquivalentTo(new { Inflows = 200m });
        result.Items[2].Should().BeEquivalentTo(new { Inflows = -50m });
    }
    
    [Fact]
    public async Task Handle_WhenGrowthOccurs_ReturnsGrowth()
    {
        // arrange
        var years = 3;
        var queryDate = _startDate.AddYears(years).AddDays(-1);
        
        var cashStatementItems = new List<CashStatementItem>
        {
            new(AccountCode, _startDate.AddDays(100), "contribute £100", 100m, 0) { CashStatementItemType = CashStatementItemTypes.Contribution },
            new(AccountCode, _startDate.AddDays(101), "buy stock", 0, -100m) { CashStatementItemType = CashStatementItemTypes.Purchase },
            new(AccountCode, _startDate.AddYears(1).AddDays(100), "contribute £100", 100m, 0) { CashStatementItemType = CashStatementItemTypes.Contribution },
            new(AccountCode, _startDate.AddYears(2).AddDays(100), "contribute £100", 100m, 0) { CashStatementItemType = CashStatementItemTypes.Contribution },
        };

        _mockCashStatementItemFetcher.GetCashStatementItems(AccountCode).Returns(cashStatementItems);

        var stockTransactions = new List<StockTransaction>
        {
            new (AccountCode, _startDate.AddDays(101), "Purchase", "buy some stock with cash balance", 1, 100m, "reference", 1.50m, 0, "SMT.L") { TransactionType = StockTransactionTypes.Purchase}
        };
        
        _mockStockTransactionFetcher.GetStockTransactions(AccountCode).Returns(stockTransactions);
        
        _mockStockPriceFetcher.GetStockPrice("SMT.L", Arg.Is<DateOnly>(d => d.Year == _startDate.Year && d.Month != 12))
            .Returns(new StockPriceResult(100m, "GBP", "GBP", 0));
        
        // Value increases in the last month of the first year
        _mockStockPriceFetcher.GetStockPrice("SMT.L", Arg.Is<DateOnly>(d => d.Year == _startDate.Year && d.Month == 12))
            .Returns(new StockPriceResult(150m, "GBP", "GBP", 0));
        
        _mockStockPriceFetcher.GetStockPrice("SMT.L", Arg.Is<DateOnly>(d => d.Year == _startDate.Year + 1))
            .Returns(new StockPriceResult(200m, "GBP", "GBP", 0));
        
        _mockStockPriceFetcher.GetStockPrice("SMT.L", Arg.Is<DateOnly>(d => d.Year == _startDate.Year + 2))
            .Returns(new StockPriceResult(400m, "GBP", "GBP", 0));
        
        _mockCashStatementItemFetcher.GetCashStatementItems(AccountCode).Returns(cashStatementItems);
        
        var request = new AccountPerformanceRequest(AccountCode, queryDate);

        // act
        var result = await _queryHandler.Handle(request);
        
        // assert
        using var _ = new AssertionScope();
        result.Items.Count.Should().Be(years);
        
        // Growth in the first year as the value has increased, disregarding inflows.
        result.Items[0].Should().BeEquivalentTo(new { Growth = 50 });
        
        // Growth in the second year as the value has increased compared with the value of the previous year
        // when disregarding the inflows
        result.Items[1].Should().BeEquivalentTo(new { Growth = 50 });
        
        // Growth in the third year as the value has increased compared with the value of the previous year
        // when disregarding the inflows
        result.Items[2].Should().BeEquivalentTo(new { Growth = 200m });
    }
    
      [Fact]
    public async Task Handle_WhenGrowthOccurs_ReturnsPercentageUnitPrice()
    {
        // arrange
        var years = 3;
        var queryDate = _startDate.AddYears(years).AddDays(-1);
        
        var cashStatementItems = new List<CashStatementItem>
        {
            new(AccountCode, _startDate.AddDays(100), "contribute £100", 100m, 0) { CashStatementItemType = CashStatementItemTypes.Contribution },
            new(AccountCode, _startDate.AddDays(101), "buy stock", 0, -100m) { CashStatementItemType = CashStatementItemTypes.Purchase },
            new(AccountCode, _startDate.AddYears(1).AddDays(100), "contribute £100", 100m, 0) { CashStatementItemType = CashStatementItemTypes.Contribution },
            new(AccountCode, _startDate.AddYears(2).AddDays(100), "contribute £100", 100m, 0) { CashStatementItemType = CashStatementItemTypes.Contribution },
        };

        _mockCashStatementItemFetcher.GetCashStatementItems(AccountCode).Returns(cashStatementItems);

        var stockTransactions = new List<StockTransaction>
        {
            new (AccountCode, _startDate.AddDays(101), "Purchase", "buy some stock with cash balance", 1, 100m, "reference", 1.50m, 0, "SMT.L") { TransactionType = StockTransactionTypes.Purchase}
        };
        
        _mockStockTransactionFetcher.GetStockTransactions(AccountCode).Returns(stockTransactions);
        
        _mockStockPriceFetcher.GetStockPrice("SMT.L", Arg.Is<DateOnly>(d => d.Year == _startDate.Year && d.Month != 12))
            .Returns(new StockPriceResult(100m, "GBP", "GBP", 0));
        
        // Value increases in the last month of the first year
        _mockStockPriceFetcher.GetStockPrice("SMT.L", Arg.Is<DateOnly>(d => d.Year == _startDate.Year && d.Month == 12))
            .Returns(new StockPriceResult(150m, "GBP", "GBP", 0));
        
        _mockStockPriceFetcher.GetStockPrice("SMT.L", Arg.Is<DateOnly>(d => d.Year == _startDate.Year + 1))
            .Returns(new StockPriceResult(200m, "GBP", "GBP", 0));
        
        _mockStockPriceFetcher.GetStockPrice("SMT.L", Arg.Is<DateOnly>(d => d.Year == _startDate.Year + 2))
            .Returns(new StockPriceResult(400m, "GBP", "GBP", 0));
        
        _mockCashStatementItemFetcher.GetCashStatementItems(AccountCode).Returns(cashStatementItems);
        
        var request = new AccountPerformanceRequest(AccountCode, queryDate);

        // act
        var result = await _queryHandler.Handle(request);
        
        // assert
        using var _ = new AssertionScope();
        result.Items.Count.Should().Be(years);
        
        // Growth in the first year as the value has increased, disregarding inflows.
        result.Items[0].Should().BeEquivalentTo(new { PercentageUnitChange = 0.5m });
        
        // Growth in the second year as the value has increased compared with the value of the previous year
        // when disregarding the inflows
        result.Items[1].Should().BeEquivalentTo(new { PercentageUnitChange = 0.3333m });
        
        result.Items[2].Should().BeEquivalentTo(new { PercentageUnitChange = 0.6667m });
    }
    
    //
    // [Fact]
    // public async Task Handle_WhenStockIsBought_ReturnsTotalValue()
    // {
    //     // arrange
    //     var days = 5;
    //     var queryDate = _startDate.AddDays(days - 1);
    //
    //     var request = new AccountValueHistoryRequest(AccountCode, queryDate);
    //
    //     var cashStatementItems = new List<CashStatementItem>
    //     {
    //         new(AccountCode, _startDate, "on the first day, contribute £100", 100m, 0) { CashStatementItemType = CashStatementItemTypes.Contribution }, 
    //         new(AccountCode, _startDate.AddDays(1), "on the second day, buy some stock with cash balance", 0, -87m) { CashStatementItemType = CashStatementItemTypes.Purchase },
    //         new(AccountCode, _startDate.AddDays(2), "on the third day, contribute another £100", 100m, 0) { CashStatementItemType = CashStatementItemTypes.Contribution },
    //         new(AccountCode, _startDate.AddDays(4), "on the fifth day, withdraw £50", 0, -50m) { CashStatementItemType = CashStatementItemTypes.Withdrawal },
    //     };
    //
    //     _mockCashStatementItemFetcher.GetCashStatementItems(AccountCode).Returns(cashStatementItems);
    //
    //     var stockTransactions = new List<StockTransaction>
    //     {
    //         new (AccountCode, _startDate.AddDays(1), "Purchase", "buy some stock with cash balance", 1, 87m, "reference", 1.50m, 0, "SMT.L") { TransactionType = StockTransactionTypes.Purchase}
    //     };
    //     
    //     _mockStockTransactionFetcher.GetStockTransactions(AccountCode).Returns(stockTransactions);
    //     
    //     _mockStockPriceFetcher.GetStockPrice("SMT.L", Arg.Any<DateOnly>())
    //         .Returns(new StockPriceResult(87m, "GBP", "GBP", 0));
    //
    //     // act
    //     var result = await _queryHandler.Handle(request);
    //
    //     // assert
    //     using var _ = new AssertionScope();
    //     result.Items.Count.Should().Be(days);
    //     
    //     result.Items[0].Should().BeEquivalentTo(new { ValueInGbp = 100m, Inflows = 100m });
    //     result.Items[1].Should().BeEquivalentTo(new { ValueInGbp = 100m, Inflows = 0m });
    //     result.Items[2].Should().BeEquivalentTo(new { ValueInGbp = 200m, Inflows = 100m });
    //     result.Items[3].Should().BeEquivalentTo(new { ValueInGbp = 200m, Inflows = 0m });
    //     result.Items[4].Should().BeEquivalentTo(new { ValueInGbp = 150m, Inflows = -50m });
    // }
    //
    // [Fact]
    // public async Task Handle_WhenRecordedTotalValueExists_ReturnsDiscrepancy()
    // {
    //     // arrange
    //     var days = 2;
    //     var queryDate = _startDate.AddDays(days - 1);
    //
    //     var request = new AccountValueHistoryRequest(AccountCode, queryDate);
    //
    //     var cashStatementItems = new List<CashStatementItem>
    //     {
    //         new(AccountCode, _startDate, "on the first day, contribute £100", 100m, 0) { CashStatementItemType = CashStatementItemTypes.Contribution },
    //         
    //     };
    //
    //     _mockCashStatementItemFetcher.GetCashStatementItems(AccountCode).Returns(cashStatementItems);
    //     
    //     var recordedTotalValues = new List<RecordedTotalValue>
    //     {
    //         new(_startDate.AddDays(1), AccountCode, 110m, "test")
    //     };
    //
    //     _mockRecordedTotalValueFetcher.GetRecordedTotalValues(AccountCode).Returns(recordedTotalValues);
    //
    //     // act
    //     var result = await _queryHandler.Handle(request);
    //
    //     // assert
    //     using var _ = new AssertionScope();
    //     result.Items.Count.Should().Be(days);
    //     
    //     result.Items[0].Should().BeEquivalentTo(new { ValueInGbp = 100m, Inflows = 100m, RecordedTotalValueInGbp = (decimal?)null, RecordedTotalValueSource = default(string), DiscrepancyRatio = (decimal?)null });
    //     result.Items[1].Should().BeEquivalentTo(new { ValueInGbp = 100m, Inflows = 0m, RecordedTotalValueInGbp = 110, RecordedTotalValueSource = "test", DiscrepancyRatio = -0.1m });
    // }
    //
    // [Fact]
    // public async Task Handle_WhenTotalValueDiffersFromPreviousDay_ReturnsDifference()
    // {
    //     // arrange
    //     var days = 3;
    //     var queryDate = _startDate.AddDays(days - 1);
    //
    //     var request = new AccountValueHistoryRequest(AccountCode, queryDate);
    //
    //     var cashStatementItems = new List<CashStatementItem>
    //     {
    //         new(AccountCode, _startDate, "on the first day, contribute £100", 100m, 0) { CashStatementItemType = CashStatementItemTypes.Contribution }, 
    //         new(AccountCode, _startDate.AddDays(1), "on the second day, buy some stock with cash balance", 0, -100m) { CashStatementItemType = CashStatementItemTypes.Purchase },
    //     };
    //
    //     _mockCashStatementItemFetcher.GetCashStatementItems(AccountCode).Returns(cashStatementItems);
    //
    //     var stockTransactions = new List<StockTransaction>
    //     {
    //         new (AccountCode, _startDate.AddDays(1), "Purchase", "buy some stock with cash balance", 1, 100m, "reference", 1.50m, 0, "SMT.L") { TransactionType = StockTransactionTypes.Purchase}
    //     };
    //     
    //     _mockStockTransactionFetcher.GetStockTransactions(AccountCode).Returns(stockTransactions);
    //     
    //     _mockStockPriceFetcher.GetStockPrice("SMT.L", _startDate.AddDays(1))
    //         .Returns(new StockPriceResult(100m, "GBP", "GBP", 0));
    //     
    //     _mockStockPriceFetcher.GetStockPrice("SMT.L", _startDate.AddDays(2))
    //         .Returns(new StockPriceResult(200m, "GBP", "GBP", 0));
    //
    //     // act
    //     var result = await _queryHandler.Handle(request);
    //
    //     // assert
    //     using var _ = new AssertionScope();
    //     result.Items.Count.Should().Be(days);
    //     
    //     result.Items[0].Should().BeEquivalentTo(new { ValueInGbp = 100m, Inflows = 100m });
    //     result.Items[1].Should().BeEquivalentTo(new { ValueInGbp = 100m, Inflows = 0m, DifferenceToPreviousDay = 0, DifferenceRatio = 0 });
    //     result.Items[2].Should().BeEquivalentTo(new { ValueInGbp = 200m, Inflows = 0m,  DifferenceToPreviousDay = 100m, DifferenceRatio = 1m});
    // }
    //
    // [Fact]
    // public async Task Handle_WithContributions_CalculatesNumberOfUnits()
    // {
    //     // arrange
    //     var days = 4;
    //     var queryDate = _startDate.AddDays(days - 1);
    //
    //     var request = new AccountValueHistoryRequest(AccountCode, queryDate);
    //     
    //     var cashStatementItems = new List<CashStatementItem>
    //     {
    //         new(AccountCode, _startDate.AddDays(1), "on the second day, contribute £100", 100m, 0) { CashStatementItemType = CashStatementItemTypes.Contribution }, 
    //         new(AccountCode, _startDate.AddDays(2), "on the third day, contribute £100", 100m, 0) { CashStatementItemType = CashStatementItemTypes.Contribution }, 
    //         new(AccountCode, _startDate.AddDays(3), "on the fourth day, withdraw £50", 0, -50m) { CashStatementItemType = CashStatementItemTypes.Withdrawal }, 
    //     };
    //
    //     _mockCashStatementItemFetcher.GetCashStatementItems(AccountCode).Returns(cashStatementItems);
    //   
    //     // act
    //     var result = await _queryHandler.Handle(request);
    //  
    //     // assert
    //     using var _ = new AssertionScope();
    //
    //     result.Items[0].Units.Should().BeEquivalentTo(new { NumberOfUnits = 0, ValueInGbpPerUnit = 100 });
    //     result.Items[1].Units.Should().BeEquivalentTo(new { NumberOfUnits = 1, ValueInGbpPerUnit = 100 });
    //     result.Items[2].Units.Should().BeEquivalentTo(new { NumberOfUnits = 2, ValueInGbpPerUnit = 100 });
    //     result.Items[3].Units.Should().BeEquivalentTo(new { NumberOfUnits = 1.5m, ValueInGbpPerUnit = 100 });
    // }
    //
    // [Fact]
    // public async Task Handle_WithStockPriceIncrease_CalculatesValueOfUnits()
    // {
    //     // arrange
    //     var days = 4;
    //     var queryDate = _startDate.AddDays(days - 1);
    //
    //     var request = new AccountValueHistoryRequest(AccountCode, queryDate);
    //     
    //     var cashStatementItems = new List<CashStatementItem>
    //     {
    //         new(AccountCode, _startDate.AddDays(1), "on the second day, contribute £100", 100m, 0) { CashStatementItemType = CashStatementItemTypes.Contribution }, 
    //         new(AccountCode, _startDate.AddDays(2), "on the third day, buy some stock with cash balance", 0, -100m) { CashStatementItemType = CashStatementItemTypes.Purchase },
    //     };
    //
    //     _mockCashStatementItemFetcher.GetCashStatementItems(AccountCode).Returns(cashStatementItems);
    //
    //     var stockTransactions = new List<StockTransaction>
    //     {
    //         new (AccountCode, _startDate.AddDays(2), "Purchase", "buy some stock with cash balance", 1, 100m, "reference", 1.50m, 0, "SMT.L") { TransactionType = StockTransactionTypes.Purchase}
    //     };
    //     
    //     _mockStockTransactionFetcher.GetStockTransactions(AccountCode).Returns(stockTransactions);
    //     
    //     _mockStockPriceFetcher.GetStockPrice("SMT.L", _startDate)
    //         .Returns(new StockPriceResult(100m, "GBP", "GBP", 0));
    //     
    //     _mockStockPriceFetcher.GetStockPrice("SMT.L", _startDate.AddDays(2))
    //         .Returns(new StockPriceResult(100m, "GBP", "GBP", 0));
    //     
    //     _mockStockPriceFetcher.GetStockPrice("SMT.L", _startDate.AddDays(3))
    //         .Returns(new StockPriceResult(150m, "GBP", "GBP", 0));
    //   
    //     // act
    //     var result = await _queryHandler.Handle(request);
    //  
    //     // assert
    //     using var _ = new AssertionScope();
    //
    //     result.Items[0].Units.Should().BeEquivalentTo(new { NumberOfUnits = 0, ValueInGbpPerUnit = 100 });
    //     result.Items[1].Units.Should().BeEquivalentTo(new { NumberOfUnits = 1, ValueInGbpPerUnit = 100 });
    //     result.Items[2].Units.Should().BeEquivalentTo(new { NumberOfUnits = 1, ValueInGbpPerUnit = 100 });
    //     result.Items[3].Units.Should().BeEquivalentTo(new { NumberOfUnits = 1, ValueInGbpPerUnit = 150 });
    // }
}
