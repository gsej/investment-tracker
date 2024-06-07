using AjBellParserConsole.InputModels;
using AjBellParserConsole.Mappers;
using FluentAssertions;
using FluentAssertions.Execution;

namespace UnitTests.AjBellParserConsole;

public class CashStatementItemMapperTests
{
    private const string AccountCode = "AccountCode";
    private readonly CashStatementItemMapper _mapper = new (AccountCode);
    
    [Fact]
    public void Can_Parse_CashStatementItem()
    {
        // arrange
        var inputCashStatementItem = new AjBellCashStatementItem
        {
            Date = "31/01/2022",
            Description = "Test Description",
            ReceiptAmountGbp = "100.43",
            PaymentAmountGbp = "1.32"
        };
        
        // Act
        var outputCashStatementItems = _mapper.Map(new List<AjBellCashStatementItem> { inputCashStatementItem });
        
        // Assert
        using var _ = new AssertionScope();
        outputCashStatementItems.Single().AccountCode.Should().Be(AccountCode);
        outputCashStatementItems.Single().Description.Should().Be(inputCashStatementItem.Description);
        outputCashStatementItems.Single().ReceiptAmountGbp.Should().Be(Decimal.Parse(inputCashStatementItem.ReceiptAmountGbp));
        outputCashStatementItems.Single().PaymentAmountGbp.Should().Be(Decimal.Parse(inputCashStatementItem.PaymentAmountGbp));
        outputCashStatementItems.Single().Date.Should().Be("2022-01-31");
    }
}
