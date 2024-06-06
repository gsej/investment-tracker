using Common.Extensions;
using Common.Tracing;
using Database;
using Database.Entities;
using FileReaders;
using LoaderConsole.CashStatementItemEnrichers;
using Microsoft.Extensions.Logging;

namespace LoaderConsole;

public class CashStatementItemLoader
{
    private readonly ICashStatementReader _cashStatementReader;
    private readonly InvestmentsDbContext _context;
    private readonly ILogger<CashStatementItemLoader> _logger;

    public CashStatementItemLoader(ILogger<CashStatementItemLoader> logger, ICashStatementReader cashStatementReader, InvestmentsDbContext context)
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
        
        var ajBellCashStatementItems = _cashStatementReader.Read(fileName).ToList();
        var cashStatementItemTypeEnricher = new CashStatementItemTypeEnricher();
        
        foreach (var ajBellCashStatementItem in ajBellCashStatementItems)
        {
            var cashStatementItem = new CashStatementItem(
                accountCode: ajBellCashStatementItem.AccountCode,
                date: ajBellCashStatementItem.Date.ToDateOnly(), 
                description: ajBellCashStatementItem.Description,
                paymentAmountGbp: ajBellCashStatementItem.Payment_Amount_Gbp,
                receiptAmountGbp: ajBellCashStatementItem.ReceiptAmountGbp);

            cashStatementItemTypeEnricher.Enrich(cashStatementItem);

            _context.CashStatementItems.Add(cashStatementItem);
            await _context.SaveChangesAsync();
            
        }
    }
}
