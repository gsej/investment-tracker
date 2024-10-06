using Api.QueryHandlers;
using Api.QueryHandlers.Fetchers;
using Api.QueryHandlers.Portfolio;
using Common;
using Database.Entities;
using Database.ValueTypes;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace UnitTests.Api.QueryHandlers;

public class AccountPortfolioQueryHandlerTests
{
    private readonly IAccountPortfolioQueryHandler _queryHander;
    
    private readonly ICashStatementItemFetcher _cashStatementItemFetcher;
    private readonly IStockTransactionFetcher _stockTransactionFetcher;
    private readonly IStockPriceFetcher _stockPriceFetcher;

    private const string AccountCode = "Account";

    public AccountPortfolioQueryHandlerTests()
    {
        _cashStatementItemFetcher = Substitute.For<ICashStatementItemFetcher>();
        _stockTransactionFetcher = Substitute.For<IStockTransactionFetcher>();

        _stockPriceFetcher = Substitute.For<IStockPriceFetcher>();
        
        _stockPriceFetcher.GetStockPrice(Arg.Any<string>(), Arg.Any<DateOnly>())
            .Returns(callInfo => StockPriceResult.Missing(callInfo.Arg<string>()));
        
        _queryHander = new AccountPortfolioQueryHandler(
            Substitute.For<ILogger<AccountPortfolioQueryHandler>>(),
            _stockPriceFetcher,
            _cashStatementItemFetcher,
            _stockTransactionFetcher
            );
    }
    
    [Fact]
    public async Task Handle_WhenNoDataExists_ReturnsEmptyResult()
    {
        // arrange
        var request = new AccountPortfolioRequest(AccountCode, new DateOnly(2023, 12, 31));
        
        // act
        var result = await _queryHander.Handle(request);
        
        // assert
        using var _ = new AssertionScope();
        result.AccountCode.Should().Be(AccountCode);
        result.CashBalanceInGbp.Should().Be(0);
        result.Holdings.Should().BeEmpty();
        result.Allocations.Should().BeEmpty();
        result.TotalValue.ValueInGbp.Should().Be(0);
        result.TotalValue.TotalPriceAgeInDays.Should().Be(0);
    }
     
    [Fact]
    public async Task Handle_WhenCashStatementItemsExist_ReturnsCashBalance()
    {
        // arrange
        var cashStatementItems = new List<CashStatementItem>
        {
            new(AccountCode, new DateOnly(2023, 1, 1), "description", 100, 0) { CashStatementItemType = CashStatementItemTypes.Contribution }, 
            new(AccountCode, new DateOnly(2023, 5, 1), "description", 0, -50) { CashStatementItemType = CashStatementItemTypes.Purchase }, 
            new(AccountCode, new DateOnly(2023, 12, 31), "description", 0, -1) { CashStatementItemType = CashStatementItemTypes.Charge }, 
            new(AccountCode, new DateOnly(2024, 1, 1), "description", 1000, 0) { CashStatementItemType = CashStatementItemTypes.Contribution },
        };
        
        _cashStatementItemFetcher.GetCashStatementItems(AccountCode).Returns(cashStatementItems);
        
        var request = new AccountPortfolioRequest("Account", new DateOnly(2023, 12, 31));
        
        // act
        var result = await _queryHander.Handle(request);
        
        // assert
        result.CashBalanceInGbp.Should().Be(49);
    }
    
    [Fact]
    public async Task Handle_WhenStockTransactionsExist_ReturnsHolding()
    {
        // arrange
        var stock = new Stock.StockBuilder("SMT.L", "Scottish Mortgage Trust", StockTypes.Share, "Growth")
            .Build();

        var stockTransactions = new List<StockTransaction>
        {
            new(AccountCode,
                new DateOnly(2023, 1, 1),
                transaction: "Purchase",
                description: "Scottish Mortgage Trust",
                quantity: 100,
                amountGbp: 1020m,
                reference: "Reference",
                fee: 5m,
                stampDuty: 5.1m,
                stockSymbol: "SMT.L") { TransactionType = StockTransactionTypes.Purchase, Stock = stock },
            new(AccountCode,
                new DateOnly(2023, 12, 31),
                transaction: "Sale",
                description: "Scottish Mortgage Trust",
                quantity: 50,
                amountGbp: 510m,
                reference: "Reference",
                fee: 5m,
                stampDuty: 0,
                stockSymbol: "SMT.L") { TransactionType = StockTransactionTypes.Sale, Stock = stock },
            new(AccountCode,
                new DateOnly(2024, 1, 1),
                transaction: "Purchase",
                description: "Scottish Mortgage Trust",
                quantity: 1000,
                amountGbp: 10200m,
                reference: "Reference",
                fee: 5m,
                stampDuty: 51m,
                stockSymbol: "SMT.L") { TransactionType = StockTransactionTypes.Purchase, Stock = stock }
        };

        _stockTransactionFetcher.GetStockTransactions(AccountCode).Returns(stockTransactions);
        
        var request = new AccountPortfolioRequest("Account", new DateOnly(2023, 12, 31));
        
        // act
        var result = await _queryHander.Handle(request);
        
        // assert
        using var _ = new AssertionScope();
        result.Holdings.Count.Should().Be(1);
        result.Holdings[0].StockSymbol.Should().Be("SMT.L");
        result.Holdings[0].Quantity.Should().Be(50);
    }
    
    [Fact]
    public async Task Handle_WithStockPrices_ReturnsValues()
    {
        // arrange
        var stock = new Stock.StockBuilder("SMT.L", "Scottish Mortgage Trust", StockTypes.Share, "Growth")
            .Build();

        var stockTransactions = new List<StockTransaction>
        {
            new(AccountCode,
                new DateOnly(2023, 1, 1),
                transaction: "Purchase",
                description: "Scottish Mortgage Trust",
                quantity: 100,
                amountGbp: 1020m,
                reference: "Reference",
                fee: 5m,
                stampDuty: 5.1m,
                stockSymbol: "SMT.L") { TransactionType = StockTransactionTypes.Purchase, Stock = stock },
        };

        _stockTransactionFetcher.GetStockTransactions(AccountCode).Returns(stockTransactions);
        
        var stockPrice = new StockPrice("SMT.L", new DateOnly(2023, 1, 2), 10m, "GBP", "Test", "GBP", null, null);
        
        _stockPriceFetcher.GetStockPrice("SMT.L", new DateOnly(2023, 12, 31)).Returns(new StockPriceResult(stockPrice.Price, stockPrice.Currency, stockPrice.OriginalCurrency, 364));
        
        var request = new AccountPortfolioRequest("Account", new DateOnly(2023, 12, 31));
        
        // act
        var result = await _queryHander.Handle(request);
        
        // assert
        using var _ = new AssertionScope();
        result.Holdings.Count.Should().Be(1);
        result.Holdings[0].StockSymbol.Should().Be("SMT.L");
        result.Holdings[0].Quantity.Should().Be(100);
        result.Holdings[0].ValueInGbp = 1000m;
        
        result.TotalValue.ValueInGbp.Should().Be(1000m);
    }
    
    [Fact]
    public async Task Handle_WithAllDate_ReturnsAllocations()
    {
        // arrange
        var stock = new Stock.StockBuilder("SMT.L", "Scottish Mortgage Trust", StockTypes.Share, "Growth")
            .Build();
        
        var cashStatementItems = new List<CashStatementItem>
        {
            new(AccountCode, new DateOnly(2023, 1, 1), "description", 100, 0) { CashStatementItemType = CashStatementItemTypes.Contribution },
        };
        
        _cashStatementItemFetcher.GetCashStatementItems(AccountCode).Returns(cashStatementItems);

        var stockTransactions = new List<StockTransaction>
        {
            new(AccountCode,
                new DateOnly(2023, 1, 1),
                transaction: "Purchase",
                description: "Scottish Mortgage Trust",
                quantity: 90,
                amountGbp: 1020m,
                reference: "Reference",
                fee: 5m,
                stampDuty: 5.1m,
                stockSymbol: "SMT.L") { TransactionType = StockTransactionTypes.Purchase, Stock = stock },
        };

        _stockTransactionFetcher.GetStockTransactions(AccountCode).Returns(stockTransactions);
        
        var stockPrice = new StockPrice("SMT.L", new DateOnly(2023, 1, 2), 10m, "GBP", "Test", "GBP", null, null);
        
        _stockPriceFetcher.GetStockPrice("SMT.L", new DateOnly(2023, 12, 31)).Returns(new StockPriceResult(stockPrice.Price, stockPrice.Currency, stockPrice.OriginalCurrency, 364));
        
        var request = new AccountPortfolioRequest("Account", new DateOnly(2023, 12, 31));
        
        // act
        var result = await _queryHander.Handle(request);
        
        // assert
        using var _ = new AssertionScope();
        result.Allocations.Should().HaveCount(2);
        result.Allocations.First().Name.Should().Be("Growth");
        result.Allocations.First().Value.Should().Be(900);
        result.Allocations.First().Percentage.Should().Be(0.9m);
        
        result.Allocations.Should().HaveCount(2);
        result.Allocations.Last().Name.Should().Be("Cash");
        result.Allocations.Last().Value.Should().Be(100);
        result.Allocations.Last().Percentage.Should().Be(0.1m);
        
    }
}
