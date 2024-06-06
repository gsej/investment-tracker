using Common;
using Database.Entities;

namespace LoaderConsole.StockTransactionEnrichers;

public class StockTransactionFeeEnricher : IStockTransactionEnricher
{
    public void Enrich(StockTransaction stockTransaction)
    {
        // Make some assumptions here about the fees......

        decimal fee = 0;

        // AjBell reduced their regular trade price on 2024-04-01
        var isAfterPriceReduction = stockTransaction.Date.DayNumber >= (new DateOnly(2024, 4, 1)).DayNumber;
        
        if (stockTransaction.TransactionType == "Purchase")
        {
            if (RegularInvestmentDayCalculator.IsRegularInvestmentDay(stockTransaction.Date))
            {
                fee = 1.5m;
            }
            else if (isAfterPriceReduction)
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
            if (isAfterPriceReduction)
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
