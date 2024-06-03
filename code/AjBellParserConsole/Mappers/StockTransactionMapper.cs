using System.Globalization;
using AjBellParserConsole.InputModels;
using AjBellParserConsole.OutputModels;

namespace AjBellParserConsole.Mappers;

public class StockTransactionMapper
{
    private readonly string _accountCode;

    public StockTransactionMapper(string accountCode)
    {
        _accountCode = accountCode;
    }

    public IList<StockTransaction> Map(IEnumerable<AjBellTransaction> inputStockTransactions)
    {
        var outputStockTransactions = new List<StockTransaction>();

        foreach (var inputStockTransaction in inputStockTransactions)
        {
            var date = DateOnly.ParseExact(inputStockTransaction.Date, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            try
            {
                var outputStockTransaction = new StockTransaction
                {
                    AccountCode = _accountCode,
                    Date = date.ToString("yyyy-MM-dd"),
                    Transaction = inputStockTransaction.Transaction,
                    Description = inputStockTransaction.Description,
                    Quantity = Decimal.Parse(inputStockTransaction.Quantity),
                    AmountGbp = Decimal.Parse(inputStockTransaction.AmountGbp),
                    Reference = inputStockTransaction.Reference
                };
                outputStockTransactions.Add(outputStockTransaction);
            }
            catch (Exception e)
            {
                
            }

            
        }

        return outputStockTransactions;
    }
}
