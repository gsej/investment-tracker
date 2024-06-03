using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Tracing;
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
        // TODO: how can I prevent this from going to the DB multiple times?
        using var _ = InvestmentTrackerActivitySource.Instance.StartActivity(); // TODO: do I need a name here? 
        
        var stocks = await _context
            .Stocks
            .Include(stock => stock.Aliases)
            .Include(stock => stock.AlternativeSymbols)
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
