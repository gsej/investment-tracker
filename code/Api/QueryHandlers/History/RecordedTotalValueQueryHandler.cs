using Database;
using Microsoft.EntityFrameworkCore;

namespace Api.QueryHandlers.History;

public class RecordedTotalValueQueryHandler : IRecordedTotalValueQueryHandler
{
    private readonly InvestmentsDbContext _context;

    public RecordedTotalValueQueryHandler(InvestmentsDbContext context)
    {
        _context = context;
    }
    
    public async Task<RecordedTotalValuesResult> Handle(RecordedTotalValuesRequest request)
    {
        var recordedTotalValues = await _context.RecordedTotalValues
            .Where(v => request.AccountCode == v.AccountCode)
            .OrderBy(v => v.Date)
            .AsNoTracking()
            .Select(v => new RecordedTotalValue(v.Date, v.AccountCode, v.TotalValueInGbp, v.Source))
            .ToListAsync();

        return new RecordedTotalValuesResult(recordedTotalValues);
    }
}
