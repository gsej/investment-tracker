using Common.Extensions;
using Database.Entities;
using Database.ValueTypes;
using FluentAssertions;
using LoaderConsole.StockTransactionEnrichers;
using UnitTests.Builder;

namespace UnitTests;

public class StockTransactionStampDutyEnricherTests
{
    private StockTransactionStampDutyEnricher _stockTransactionStampDutyEnricher;
    public StockTransactionStampDutyEnricherTests()
    {
        _stockTransactionStampDutyEnricher = new StockTransactionStampDutyEnricher();
    }

    [Theory]
    [InlineData("Purchase", "2019-11-11", 748.51, 3.72)]
    public void Enrich_WithPurchaseAndSubjectToStampDuty_SetStampDuty(string transaction, string date, decimal amountGbp, decimal expectedDuty)
    {
        var stock = new Stock.StockBuilder("SMT.L", "Scottish Mortgage", StockTypes.Share)
            .WithStampDuty()
            .Build();

        var stockTransaction = new StockTransactionBuilder()
            .WithDate(date.ToDateOnly())
            .WithTransaction(transaction)
            .WithTransactionType(transaction)
            .WithAmountGbp(amountGbp)
            .Build();

        stockTransaction.Fee = 1.5m;

        _stockTransactionStampDutyEnricher.Enrich(stockTransaction, stock);

        stockTransaction.StampDuty.Should().Be(expectedDuty);
    }
}
