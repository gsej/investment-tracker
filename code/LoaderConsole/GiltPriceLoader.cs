// using database;
// using database.Entities;
// using FileReaders;
// using FileReaders.Prices;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
// using StockPrice = database.Entities.StockPrice;
//
// namespace loader;
//
// public class GiltPriceLoader
// {
//     private readonly ILogger<GiltPriceLoader> _logger;
//     private readonly InvestmentsDbContext _context;
//     private readonly IStockPriceReader _reader;
//
   //private readonly Lazy<List<Stock>> _stocks;
//
//     public GiltPriceLoader(ILogger<GiltPriceLoader> logger, InvestmentsDbContext context, IStockPriceReader reader)
//     {
//         _logger = logger;
//         _context = context;
//         _reader = reader;
//
//         _stocks = new(() =>
//             _context
//                 .Stocks
//                 .Include(stock => stock.Aliases)
//                 .Include(stock => stock.AlternativeSymbols)
//                 .ToList());
//     }
//
//     
//     public async Task LoadFiles(string directoryName, string source, string defaultStockSymbol = null)
//     {
//         var fileNames = Directory.GetFiles(directoryName, "*.json");
//
//         foreach (var fileName in fileNames)
//         {
//             await LoadFile(fileName, source, defaultStockSymbol);
//         }
//     }
//     
//     private async Task LoadFile(string fileName, string source, string defaultStockSymbol = null)
//     {
//         await LoadFileInternal(fileName, source, defaultStockSymbol);
//     }
//
//     private async Task LoadFileInternal(string fileName, string source, string defaultStockSymbol = null)
//     {
//         var giltPrices = await _reader.ReadFile(fileName);
//
//         foreach (var giltPriceDto in giltPrices)
//         {
//             // hack. why do the files have a null in the first position?
//             if (giltPriceDto == null)
//             {
//                 continue;
//             }
//
//             string date = giltPriceDto.Date.Substring(0, 10);
//
//             var priceParsable = decimal.TryParse(giltPriceDto.Price, null, out var price);
//
//             if (!priceParsable)
//             {
//                 continue;
//             }
//
//             var stockSymbol = giltPriceDto.StockSymbol ?? defaultStockSymbol;
//
//             var matchingStock = _stocks.Value.SingleOrDefault(s =>
//                 s.StockSymbol.Equals(stockSymbol, StringComparison.InvariantCultureIgnoreCase) ||
//                 s.AlternativeSymbols.Any(a =>
//                     a.Alternative.Equals(stockSymbol, StringComparison.InvariantCultureIgnoreCase)));
//             
//             // just skip if we don't have a matching stock
//             if (matchingStock == null && defaultStockSymbol == null)
//             {
//                 continue;
//             }
//
//             var symbol = matchingStock != null ? matchingStock.StockSymbol : stockSymbol;
//
//              // deduplication
//             var existing = await _context.StockPrices.SingleOrDefaultAsync(s =>
//                 s.Date == date && s.StockSymbol == symbol);
//
//             if (existing != null)
//             {
//                 if (existing.Price != price)
//                 {
//                     var difference = (existing.Price - price) / existing.Price;
//                     
//                     var logLevel = (Math.Abs(difference) > 0.01m) ? LogLevel.Error : LogLevel.Warning;
//                     
//                     _logger.Log(logLevel, "Price discrepancy ({difference}) for {symbol} on {date}, preferring {price} from {source} over {newPrice} from {newSource}", 
//                         difference.ToString("P3"),
//                         symbol, 
//                         giltPriceDto.Date,
//                         existing.Price, 
//                         existing.Source, 
//                         price, 
//                         source);
//                 }
//             }
//             if (existing == null)
//             {
//                 
//                 var currency = matchingStock?.DefaultCurrency ?? "GBp";
//                 
//                 var stockPrice = new StockPrice(
//                     stockSymbol: symbol,
//                     date: date,
//                     price: price,
//                     currency: currency, 
//                     source: source);
//
//                 _context.StockPrices.Add(stockPrice);
//                 await _context.SaveChangesAsync();
//             }
//         }
//     }
// }
