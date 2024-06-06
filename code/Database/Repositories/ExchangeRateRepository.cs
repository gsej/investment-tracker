using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

public class ExchangeRateRepository : IExchangeRateRepository
{
    private readonly InvestmentsDbContext _context;

    public ExchangeRateRepository(InvestmentsDbContext context)
    {
        _context = context;
    }

    public async Task<IList<ExchangeRate>> GetAll()
    {
        return await _context.ExchangeRates
            .AsNoTracking()
            .ToListAsync();
    }
}
