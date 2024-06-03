using Database;
using Microsoft.EntityFrameworkCore;

namespace Api.QueryHandlers.Quality;

public class QualityQueryHandler : IQualityQueryHandler
{
    private readonly InvestmentsDbContext _context;

    public QualityQueryHandler(InvestmentsDbContext context)
    {
        _context = context;
    }
    
    public async Task<QualityReport> Handle()
    {
        var stocksWithTransactions = await _context
            .StockTransactions
            .AsNoTracking()
            .Select(s => s.StockSymbol)
            .Distinct()
            .ToListAsync();
        
        var stocksWithPrices =  await _context
            .StockPrices
            .AsNoTracking()
            .Select(s => s.StockSymbol)
            .Distinct()
            .ToListAsync();

        var stocksWithoutAnyPrices = stocksWithTransactions.Except(stocksWithPrices).ToList();
        
        return new QualityReport(stocksWithoutAnyPrices);
    }
}
