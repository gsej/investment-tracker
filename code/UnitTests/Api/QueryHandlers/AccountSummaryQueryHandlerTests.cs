using Api.QueryHandlers.Summary;
using Common;
using Database;
using Database.Entities;
using Database.ValueTypes;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace UnitTests.Api.QueryHandlers;

public class AccountSummaryQueryHandlerTests : IDisposable, IAsyncDisposable
{
    private readonly InvestmentsDbContext _context;
    private readonly IAccountSummaryQueryHandler _queryHander;

    private const string AccountCode = "Account";

    public AccountSummaryQueryHandlerTests()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider();

        var builder = new DbContextOptionsBuilder<InvestmentsDbContext>();
        builder.UseInMemoryDatabase(databaseName:"InvestmentsDb")
            .UseInternalServiceProvider(serviceProvider);
        
        _context = new InvestmentsDbContext(builder.Options);
        
        _queryHander = new AccountSummaryQueryHandler(
            Substitute.For<ILogger<AccountSummaryQueryHandler>>(),
            _context,
            Substitute.For<IMemoryCache>());
    }
    
    [Fact]
    public async Task Handler_WhenNoCashStatementItemsExist_ReturnsZeroCashBalance()
    {
        // arrange
        var request = new AccountSummaryRequest(AccountCode, new DateOnly(2023, 12, 31));
        
        // act
        var result = await _queryHander.Handle(request);
        
        // assert
        result.CashBalanceInGbp.Should().Be(0);
    }
     
    [Fact]
    public async Task Handler_WhenCashStatementItemsExist_ReturnsCashBalance()
    {
        // arrange
        var cashStatementItems = new List<CashStatementItem>
        {
            new(AccountCode, new DateOnly(2023, 1, 1), "description", 100, 0) { CashStatementItemType = CashStatementItemTypes.Contribution }, 
            new(AccountCode, new DateOnly(2023, 5, 1), "description", 0, -50) { CashStatementItemType = CashStatementItemTypes.Purchase }, 
            new(AccountCode, new DateOnly(2023, 12, 31), "description", 0, -1) { CashStatementItemType = CashStatementItemTypes.Charge }, 
            new(AccountCode, new DateOnly(2024, 1, 1), "description", 1000, 0) { CashStatementItemType = CashStatementItemTypes.Contribution },
        };
        
        _context.CashStatementItems.AddRange(cashStatementItems);
        await _context.SaveChangesAsync();
        
        var request = new AccountSummaryRequest("Account", new DateOnly(2023, 12, 31));
        
        // act
        var result = await _queryHander.Handle(request);
        
        // assert
        result.CashBalanceInGbp.Should().Be(49);
    }
    
    [Fact]
    public async Task Handler_WhenNoStockTransactionsExist_ReturnsNoHoldings()
    {
        // arrange
        var request = new AccountSummaryRequest(AccountCode, new DateOnly(2023, 12, 31));
        
        // act
        var result = await _queryHander.Handle(request);
        
        // assert
        result.Holdings.Should().BeEmpty();
    }
    
    [Fact]
    public async Task Handler_WhenStockTransacftionsExist_ReturnsHolding()
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
                stockSymbol: "SMT.L")
            {
                TransactionType = StockTransactionTypes.Purchase,
                Stock = stock
            },
            new(AccountCode,
                new DateOnly(2023, 12, 31),
                transaction: "Sale",
                description: "Scottish Mortgage Trust",
                quantity: 50,
                amountGbp: 510m,
                reference: "Reference",
                fee: 5m,
                stampDuty: 0,
                stockSymbol: "SMT.L")
            {
                TransactionType = StockTransactionTypes.Sale,
                Stock = stock
            },
            new(AccountCode,
                new DateOnly(2024, 1, 1),
                transaction: "Purchase",
                description: "Scottish Mortgage Trust",
                quantity: 1000,
                amountGbp: 10200m,
                reference: "Reference",
                fee: 5m,
                stampDuty: 51m,
                stockSymbol: "SMT.L")
            {
                TransactionType = StockTransactionTypes.Purchase,
                Stock = stock
            }
        };
        
        _context.StockTransactions.AddRange(stockTransactions);
        await _context.SaveChangesAsync();
        
        var request = new AccountSummaryRequest("Account", new DateOnly(2023, 12, 31));
        
        // act
        var result = await _queryHander.Handle(request);
        
        // assert
        using var _ = new AssertionScope();
        result.Holdings.Count.Should().Be(1);
        result.Holdings[0].StockSymbol.Should().Be("SMT.L");
        result.Holdings[0].Quantity.Should().Be(50);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}
