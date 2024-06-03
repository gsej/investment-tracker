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
            .Where(kb => request.AccountCode == kb.AccountCode)
            .OrderBy(kb => kb.Date)
            .AsNoTracking()
            .Select(kb => new RecordedTotalValue(kb.Date, kb.AccountCode, kb.TotalValueInGbp))
            .ToListAsync();

        return new RecordedTotalValuesResult(recordedTotalValues);
    }
}
