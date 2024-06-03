using Database.Entities;

namespace LoaderConsole.StockTransactionEnrichers;

public class StockTransactionStampDutyEnricher
{
    public void Enrich(StockTransaction stockTransaction, Stock stock)
    {
        decimal stampDuty = 0;
        if (stockTransaction.TransactionType == "Purchase" && stock.SubjectToStampDuty)
        {
            var costOfShares = (stockTransaction.AmountGbp - stockTransaction.Fee) / 1.005m;

            stampDuty = costOfShares * 0.005m;

        }

        stockTransaction.StampDuty = Math.Round(stampDuty, 2);

    }
}
