using System.Globalization;
using Database;
using Microsoft.EntityFrameworkCore;

namespace Api.QueryHandlers;

public static class InvestmentsDbContextExtensions
{
    public static async Task<StockPriceResult> GetStockPrice(
        this InvestmentsDbContext context,
        string stockSymbol,
        string date)
    {
        var stockPrice = await context.StockPrices
            .Where(s =>
                s.StockSymbol == stockSymbol &&
                s.Date.CompareTo(date) <= 0)
            .OrderByDescending(s => s.Date)
            .FirstOrDefaultAsync();

        if (stockPrice != null)
        {
            var requestDate = DateTime.ParseExact(date, "yyyy-MM-dd", null, DateTimeStyles.AssumeUniversal);
            var priceDate = DateTime.ParseExact(stockPrice.Date, "yyyy-MM-dd", null, DateTimeStyles.AssumeUniversal);

            var ageInDays = (requestDate - priceDate).Days;
            
            return new StockPriceResult(stockPrice.Price, stockPrice.Currency, ageInDays);
        }

        return StockPriceResult.Missing(stockSymbol);
    }
}
