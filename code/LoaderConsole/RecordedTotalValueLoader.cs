using Database;
using FileReaders;
using RecordedTotalValue = Database.Entities.RecordedTotalValue;

namespace LoaderConsole;

public class RecordedTotalValueLoader
{
    private readonly InvestmentsDbContext _context;
    private readonly IRecordedTotalValueReader _reader;

    public RecordedTotalValueLoader(InvestmentsDbContext context, IRecordedTotalValueReader reader)
    {
        _context = context;
        _reader = reader;
    }

    public async Task LoadFile(string fileName)
    {
        var recordedValues = _reader.Read(fileName).ToList();

        foreach (var recordedValueDto in recordedValues)
        {
            decimal totalValueInGbp;
            
            var totalValueParsable = decimal.TryParse(recordedValueDto.TotalValueInGbp, null, out totalValueInGbp);
            
            if (!totalValueParsable)
            {
                continue;
            }

            // var matchingAccount = accounts.SingleOrDefault(a =>
            //     a.AccountCode.Equals(balanceDto.AccountCode, StringComparison.InvariantCultureIgnoreCase));

            var date = DateTime.ParseExact(recordedValueDto.Date, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
       
            
            var dateOnly = new DateOnly(date.Year, date.Month, date.Day);

            var knownValue = new RecordedTotalValue(
                accountCode: recordedValueDto.AccountCode,
                date: dateOnly,
                totalValueInGbp: totalValueInGbp);

            try
            {
                _context.RecordedTotalValues.Add(knownValue);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                Console.WriteLine(knownValue.AccountCode);
                throw;
            }
        }
    }
}
