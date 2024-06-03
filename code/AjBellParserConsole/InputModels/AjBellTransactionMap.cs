using CsvHelper.Configuration;

namespace AjBellParserConsole.InputModels;

public class AjBellTransactionMap : ClassMap<AjBellTransaction>
{
    public AjBellTransactionMap()
    {
        Map(m => m.Date).Name("Date");
        Map(m => m.Transaction).Name("Transaction");
        Map(m => m.Description).Name("Description");
        Map(m => m.Quantity).Name("Quantity");
        Map(m => m.AmountGbp).Name("Amount (GBP)");
        Map(m => m.Reference).Name("Reference");
    }
}
