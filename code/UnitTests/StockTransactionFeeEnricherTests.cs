using Common.Extensions;
using FluentAssertions;
using LoaderConsole.StockTransactionEnrichers;
using UnitTests.Builder;
using Xunit.Sdk;

namespace UnitTests;

public class StockTransactionFeeEnricherTests
{
    private StockTransactionFeeEnricher _stockTransactionFeeEnricher;
    public StockTransactionFeeEnricherTests()
    {
        _stockTransactionFeeEnricher = new StockTransactionFeeEnricher();
    }
    
    [Theory]
    [InlineData("Purchase", "2023-03-31", 9.95)]
    [InlineData("Purchase", "2024-03-31", 9.95)]
    [InlineData("Purchase", "2024-04-01", 5)] // Fee changed to £5 from 1st April 2024
    [InlineData("Purchase", "2025-04-30", 5)]
    public void Enrich_WithPurchase_SetsFee(string transaction, string date, decimal expectedFee)
    {
        var stockTransaction = new StockTransactionBuilder()
            .WithDate(date.ToDateOnly())
            .WithTransaction(transaction)
            .WithTransactionType(transaction)
            .Build();
      
        _stockTransactionFeeEnricher.Enrich(stockTransaction);

        stockTransaction.Fee.Should().Be(expectedFee);
    }
    
    [Theory]
    [InlineData("Sale", "2023-03-31", 9.95)]
    [InlineData("Sale", "2024-03-31", 9.95)]
    [InlineData("Sale", "2024-04-01", 5)] // Fee changed to £5 from 1st April 2024
    [InlineData("Sale", "2025-04-30", 5)]
    public void Enrich_WithSale_SetsFee(string transaction, string date, decimal expectedFee)
    {
        var stockTransaction = new StockTransactionBuilder()
            .WithDate(date.ToDateOnly())
            .WithTransaction(transaction)
            .WithTransactionType(transaction)
            .Build();
      
        _stockTransactionFeeEnricher.Enrich(stockTransaction);

        stockTransaction.Fee.Should().Be(expectedFee);
    }
    
    [Theory]
    [InlineData("Purchase", "2023-03-10", 1.5)]
    [InlineData("Purchase", "2023-09-11", 1.5)]
    [InlineData("Purchase", "2024-03-11", 1.5)]
    [InlineData("Purchase", "2025-05-12", 1.5)]
    public void Enrich_WithRegularPurchase_SetsFee(string transaction, string date, decimal expectedFee)
    {
        var stockTransaction = new StockTransactionBuilder()
            .WithDate(date.ToDateOnly())
            .WithTransaction(transaction)
            .WithTransactionType(transaction)
            .Build();
      
        _stockTransactionFeeEnricher.Enrich(stockTransaction);

        stockTransaction.Fee.Should().Be(expectedFee);
    }
}
