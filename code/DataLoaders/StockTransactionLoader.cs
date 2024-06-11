using Common.Extensions;
using Common.Tracing;
using Database;
using DataLoaders.StockTransactionEnrichers;
using FileReaders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StockTransaction = Database.Entities.StockTransaction;

namespace DataLoaders;

public class StockTransactionLoader
{
    private readonly ILogger<StockTransactionLoader> _logger;
    private readonly StockTransactionTypeEnricher _stockTransactionTypeEnricher;
    private readonly StockTransactionFeeEnricher _stockTransactionFeeEnricher;
    private readonly StockTransactionStampDutyEnricher _stockTransactionStampDutyEnricher;
    private readonly IReader<FileReaders.AccountStatements.StockTransaction> _reader;
    private readonly InvestmentsDbContext _context;

    public StockTransactionLoader(
        ILogger<StockTransactionLoader> logger,
        StockTransactionTypeEnricher stockTransactionTypeEnricher,
        StockTransactionFeeEnricher stockTransactionFeeEnricher,
        StockTransactionStampDutyEnricher stockTransactionStampDutyEnricher,
        IReader<FileReaders.AccountStatements.StockTransaction> reader,
        InvestmentsDbContext context)
    {
        _logger = logger;
        _stockTransactionTypeEnricher = stockTransactionTypeEnricher;
        _stockTransactionFeeEnricher = stockTransactionFeeEnricher;
        _stockTransactionStampDutyEnricher = stockTransactionStampDutyEnricher;
        _reader = reader;
        _context = context;
    }

    public async Task Load(string fileName)
    {
        using var _ = InvestmentTrackerActivitySource.Instance.StartActivity();
        
        if (!File.Exists(fileName))
        {
            _logger.LogError("File {fileName} does not exist", fileName);
            return;
        }
        
        var stocks = await _context
            .Stocks
            .Include(stock => stock.Aliases)
            .Include(stock => stock.AlternativeSymbols)
            .ToListAsync();
        
        var ajBellStockTransactions = (await _reader.Read(fileName)).ToList();
        
        foreach (var ajBellStockTransaction in ajBellStockTransactions)
        {
            var matchingStock = stocks.SingleOrDefault(s =>
                s.Description.Equals(ajBellStockTransaction.Description, StringComparison.InvariantCultureIgnoreCase) ||
                s.Aliases.Any(alias =>
                    alias.Description.Equals(ajBellStockTransaction.Description, StringComparison.InvariantCultureIgnoreCase)));

            if (matchingStock == null)
            {
                _logger.LogError("No stock found for transaction {ajBellStockTransaction}", ajBellStockTransaction);
                continue;
            }
            
            var quantity = ajBellStockTransaction.Quantity;

            // TODO: fix this hack:
            // Is this actually for all GILTs? 
            if (matchingStock?.StockSymbol == "T26A" || matchingStock?.StockSymbol == "T25" || matchingStock?.StockSymbol == "T27A")
            {
                // quantities for some GILTs from AJBell are out by a factor of 100
                quantity = quantity / 100;
            }
            
            var stockTransaction = new StockTransaction(
                accountCode: ajBellStockTransaction.AccountCode,
                date: ajBellStockTransaction.Date.ToDateOnly(),
                transaction: ajBellStockTransaction.Transaction,
                description: ajBellStockTransaction.Description,
                quantity: quantity,
                amountGbp: ajBellStockTransaction.AmountGbp,
                reference: ajBellStockTransaction.Reference,
                fee: -1, //todo: remove this default
                stampDuty: 11111, // todo: enrich,
                stockSymbol: matchingStock != null ? matchingStock.StockSymbol : null // is this possible to be null?
             );

            _stockTransactionTypeEnricher.Enrich(stockTransaction);
            _stockTransactionFeeEnricher.Enrich(stockTransaction);
            _stockTransactionStampDutyEnricher.Enrich(stockTransaction, matchingStock);

            _context.StockTransactions.Add(stockTransaction);
            await _context.SaveChangesAsync();

        }
    }
}
