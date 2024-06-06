// using System.Globalization;
// using Common.Extensions;
// using Database;
// using Microsoft.EntityFrameworkCore;
//
// namespace Api.QueryHandlers;
//
// public static class InvestmentsDbContextExtensions
// {
//     public static async Task<StockPriceResult> GetStockPrice(
//         this InvestmentsDbContext context,
//         string stockSymbol,
//         string date)
//     {
//         var stockPrice = await context.StockPrices
//             .Where(s =>
//                 s.StockSymbol == stockSymbol &&
//                 s.Date.CompareTo(date) <= 0)
//             .OrderByDescending(s => s.Date)
//             .FirstOrDefaultAsync();
//
//         if (stockPrice != null)
//         {
//             var requestDate = date.ToDateOnly();
//             var priceDate = stockPrice.Date;
//
//             var ageInDays = requestDate.DayNumber - priceDate.DayNumber;
//             
//             return new StockPriceResult(stockPrice.Price, stockPrice.Currency, ageInDays);
//         }
//
//         return StockPriceResult.Missing(stockSymbol);
//     }
// }
