using Api.QueryHandlers.History;
using Database;
using Microsoft.EntityFrameworkCore;

namespace Api.QueryHandlers.Fetchers;

public class RecordedTotalValueFetcher : IRecordedTotalValueFetcher
{
    private readonly InvestmentsDbContext _context;

    public RecordedTotalValueFetcher(InvestmentsDbContext context)
    {
        _context = context;
    }

    public async Task<IList<RecordedTotalValue>> GetRecordedTotalValues(string accountCode)
    {
        var recordedTotalValues = await _context.RecordedTotalValues
            .Where(v => v.AccountCode == accountCode)
            .OrderBy(v => v.Date)
            .AsNoTracking()
            .Select(v => new RecordedTotalValue(v.Date, v.AccountCode, v.TotalValueInGbp, v.Source))
            .ToListAsync();

        return recordedTotalValues;
    }
}
