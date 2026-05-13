using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

public class StockRepository : IStockRepository
{
    private readonly InvestmentsDbContext _context;

    public StockRepository(InvestmentsDbContext context)
    {
        _context = context;
    }
    
    public async Task<IList<Stock>> GetStocks()
    {
        var stocks = await _context
            .Stocks
            .Include(stock => stock.Aliases)
            .Include(stock => stock.AlternativeSymbols)
            .AsSingleQuery()
            .ToListAsync();
        
        return stocks;
    }

    public void Add(Stock stock)
    {
        _context.Add(stock);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
