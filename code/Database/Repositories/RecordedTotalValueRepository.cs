using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

public class RecordedTotalValueRepository : IRecordedTotalValueRepository
{
    private readonly InvestmentsDbContext _context;

    public RecordedTotalValueRepository(InvestmentsDbContext context)
    {
        _context = context;
    }
    
    public void Add(RecordedTotalValue recordedTotalValue)
    {
        _context.Add(recordedTotalValue);
    }

    public async Task<int> SaveChangesAsync()
    {
        // TODO: move to UoW
        return await _context.SaveChangesAsync();
    }
}
