using AjBellParserConsole.InputModels;
using AjBellParserConsole.Mappers;
using FluentAssertions;
using FluentAssertions.Execution;

namespace UnitTests.AjBellParserConsole;

public class StockTransactionMapperTests
{
    private const string AccountCode = "AccountCode";
    private readonly StockTransactionMapper _mapper = new (AccountCode);
    
    [Fact]
    public void Can_Parse_StockTransactionItem()
    {
        // arrange
        var inputTransaction = new AjBellTransaction()
        {
            Date = "31/01/2022",
            Description = "Scottish Mortgage Ord",
            AmountGbp = "100.43",
            Reference = "SomeReference",
            Quantity = "12.34",
            Transaction = "Purchase"
        };
        
        // Act
        var outputTransactions = _mapper.Map(new List<AjBellTransaction> { inputTransaction });
        
        // Assert
        using var _ = new AssertionScope();
        outputTransactions.Single().AccountCode.Should().Be(AccountCode);
        outputTransactions.Single().Date.Should().Be("2022-01-31");
        outputTransactions.Single().Description.Should().Be(inputTransaction.Description);
        outputTransactions.Single().AmountGbp.Should().Be(Decimal.Parse(inputTransaction.AmountGbp));
        outputTransactions.Single().Reference.Should().Be(inputTransaction.Reference);
        outputTransactions.Single().Quantity.Should().Be(Decimal.Parse(inputTransaction.Quantity));
        outputTransactions.Single().Transaction.Should().Be(inputTransaction.Transaction);
    }
}
