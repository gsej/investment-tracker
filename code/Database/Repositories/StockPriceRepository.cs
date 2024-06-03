using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

public class StockPriceRepository : IStockPriceRepository
{
    private readonly InvestmentsDbContext _context;

    public StockPriceRepository(InvestmentsDbContext context)
    {
        _context = context;
    }

    public void Add(StockPrice stockPrice)
    {
        _context.Add(stockPrice);
    }

    public async Task<int> SaveChangesAsync()
    {
        // TODO: move to UoW
        return await _context.SaveChangesAsync();
    }

    public async Task<IList<StockPrice>> GetAll(string stockSymbol)
    {
        return await _context.StockPrices
            .Where(c => c.StockSymbol == stockSymbol)
            .AsNoTracking()
            .ToListAsync();
    }
}
