using CsvHelper.Configuration;

namespace AjBellParserConsole.InputModels;

public class AjBellCashStatementItemMap : ClassMap<AjBellCashStatementItem>
{
    public AjBellCashStatementItemMap()
    {
        Map(m => m.Date).Name("Date");
        Map(m => m.Description).Name("Description");
        Map(m => m.ReceiptAmountGbp).Name("Receipt (GBP)");
        Map(m => m.PaymentAmountGbp).Name("Payment (GBP)");
    }
}
