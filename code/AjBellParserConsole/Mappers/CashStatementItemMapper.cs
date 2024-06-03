using System.Globalization;
using AjBellParserConsole.InputModels;
using AjBellParserConsole.OutputModels;

namespace AjBellParserConsole.Mappers;

public class CashStatementItemMapper
{
    private readonly string _accountCode;

    public CashStatementItemMapper(string accountCode)
    {
        _accountCode = accountCode;
    }
    
    public IList<CashStatementItem> Map(IEnumerable<AjBellCashStatementItem> inputCashStatementItems)
    {
        var outputCashStatementItems = new List<CashStatementItem>();

        foreach (var inputCashStatementItem in inputCashStatementItems)
        {
            var date = DateOnly.ParseExact(inputCashStatementItem.Date, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            var outputCashStatementItem = new CashStatementItem
            {
                AccountCode = _accountCode,
                Date = date.ToString("yyyy-MM-dd"),
                Description = inputCashStatementItem.Description,
                ReceiptAmountGbp = Decimal.Parse(inputCashStatementItem.ReceiptAmountGbp),
                PaymentAmountGbp = Decimal.Parse(inputCashStatementItem.PaymentAmountGbp)
            };

            outputCashStatementItems.Add(outputCashStatementItem);
        }

        return outputCashStatementItems;
    }
}
