using Common;
using Database.Entities;

namespace LoaderConsole.StockTransactionEnrichers;

public class StockTransactionFeeEnricher : IStockTransactionEnricher
{
    public void Enrich(StockTransaction stockTransaction)
    {
        // Make some assumptions here about the fees......

        var date = DateOnly.ParseExact(stockTransaction.Date, "yyyy-MM-dd");

        decimal fee = 0;
        
        if (stockTransaction.TransactionType == "Purchase")
        {
            if (RegularInvestmentDayCalculator.IsRegularInvestmentDay(date))
            {
                fee = 1.5m;
            }
            else if (date >= new DateOnly(2024, 4, 1))
            {
                fee = 5m;
            }
            else
            {
                fee = 9.95m;
            }
        }
        else if (stockTransaction.TransactionType == "Sale")
        {
            if (date >= new DateOnly(2024, 4, 1))
            {
                fee = 5m;
            }
            else
            {
                fee = 9.95m;
            }
        }
        
        stockTransaction.Fee = fee;
    }
}
