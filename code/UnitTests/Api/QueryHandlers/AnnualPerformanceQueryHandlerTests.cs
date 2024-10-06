using Api.QueryHandlers.History;
using Api.QueryHandlers.Portfolio;
using Common;
using Database;
using Database.Entities;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace UnitTests.Api.QueryHandlers;

public class AnnualPerformanceQueryHandlerTests
{
    private readonly InvestmentsDbContext _context;
    
    private const string AccountCode = "Account";
    private readonly DateOnly _accountOpeningDate = new(2020, 1, 1);

    private readonly IAnnualPerformanceQueryHandler _queryHandler;
    private readonly IAccountPortfolioQueryHandler _accountPortfolioQueryHandler;
    
    public AnnualPerformanceQueryHandlerTests()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider();

        var builder = new DbContextOptionsBuilder<InvestmentsDbContext>();
        builder.UseInMemoryDatabase(databaseName:"InvestmentsDb")
            .UseInternalServiceProvider(serviceProvider);
        
        _context = new InvestmentsDbContext(builder.Options);
        
        _accountPortfolioQueryHandler = Substitute.For<IAccountPortfolioQueryHandler>();

        _accountPortfolioQueryHandler.Handle(Arg.Any<AccountPortfolioRequest>())
            .Returns(new AccountPortfolioResult(AccountCode, new List<Holding>(), 0, new TotalValue(0, 0), new List<Allocation>()));
        
        _queryHandler = new AnnualPerformanceQueryHandler(
            _context,
            _accountPortfolioQueryHandler,
            Substitute.For<ILogger<AnnualPerformanceQueryHandler>>());
        
        var account = new Account(AccountCode, _accountOpeningDate);

        _context.Accounts.Add(account);
        _context.SaveChanges();
    }
    
    [Fact]
    public async Task Handle_Returns_OneResultPerYear()
    {
        // arrange
        var request = new AnnualPerformanceRequest([AccountCode], new DateOnly(2024, 6, 6));

        // act
        var result = await _queryHandler.Handle(request);
        
        // assert
        using var _ = new AssertionScope();
        
        result.Years.Should().HaveCount(5);
        result.Years.Select(r => r.Year).Should().BeEquivalentTo(new[] { 2020, 2021, 2022, 2023, 2024 });
    }
    
    [Fact]
    public async Task Handle_Returns_CashInflows()
    {
        // arrange
        var request = new AnnualPerformanceRequest([AccountCode], new DateOnly(2024, 6, 6));

        var cashStatementItems = new List<CashStatementItem>
        {
            new(AccountCode, _accountOpeningDate.AddDays(1), "TransferIn", 5000, 0)
            {
                CashStatementItemType = CashStatementItemTypes.TransferIn
            },
            new(AccountCode, _accountOpeningDate.AddDays(2), "Subscription", 7500, 0)
            {
                CashStatementItemType = CashStatementItemTypes.Contribution
            },
            new(AccountCode, _accountOpeningDate.AddDays(3), "TaxRelief", 2500, 0)
            {
                CashStatementItemType = CashStatementItemTypes.TaxRelief
            },
            new(AccountCode, _accountOpeningDate.AddDays(4), "Withdrawal", 0, -500)
            {
                CashStatementItemType = CashStatementItemTypes.Withdrawal
            },
            new("DifferentAccountCode", _accountOpeningDate.AddDays(5), "Withdrawal", 0, -5000)
            {
                CashStatementItemType = CashStatementItemTypes.Withdrawal
            },

        };
        
        _context.CashStatementItems.AddRange(cashStatementItems);
        await _context.SaveChangesAsync();

        // act
        var result = await _queryHandler.Handle(request);
        
        // assert
        var year = result.Years.First();
        
        using var _ = new AssertionScope();
        year.Year.Should().Be(2020);
        year.NetInflowsInGbp.Should().Be(14500);
    }
}
