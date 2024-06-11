using Common.Extensions;
using Common.Tracing;
using Database;
using DataLoaders.CashStatementItemEnrichers;
using FileReaders;
using Microsoft.Extensions.Logging;
using CashStatementItem = Database.Entities.CashStatementItem;

namespace DataLoaders;

public class CashStatementItemLoader
{
    private readonly IReader<FileReaders.AccountStatements.CashStatementItem> _cashStatementReader;
    private readonly InvestmentsDbContext _context;
    private readonly ILogger<CashStatementItemLoader> _logger;

    public CashStatementItemLoader(ILogger<CashStatementItemLoader> logger, IReader<FileReaders.AccountStatements.CashStatementItem> cashStatementReader, InvestmentsDbContext context)
    {
        _cashStatementReader = cashStatementReader;
        _context = context;
        _logger = logger;
    }

    public async Task Load(string fileName)
    {
        using var _ = InvestmentTrackerActivitySource.Instance.StartActivity();

        if (!File.Exists(fileName))
        {
            _logger.LogError("File {fileName} does not exist", fileName);
            return;
        }
        
        var ajBellCashStatementItems = (await _cashStatementReader.Read(fileName)).ToList();
        var cashStatementItemTypeEnricher = new CashStatementItemTypeEnricher();
        
        foreach (var ajBellCashStatementItem in ajBellCashStatementItems)
        {
            var cashStatementItem = new CashStatementItem(
                accountCode: ajBellCashStatementItem.AccountCode,
                date: ajBellCashStatementItem.Date.ToDateOnly(), 
                description: ajBellCashStatementItem.Description,
                receiptAmountGbp: ajBellCashStatementItem.ReceiptAmountGbp, paymentAmountGbp: ajBellCashStatementItem.Payment_Amount_Gbp);

            cashStatementItemTypeEnricher.Enrich(cashStatementItem);

            _context.CashStatementItems.Add(cashStatementItem);
            await _context.SaveChangesAsync();
            
        }
    }
}
