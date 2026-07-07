using Common;
using Common.Extensions;
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
    private readonly CashStatementItemDividendStockEnricher _dividendStockEnricher;

    public CashStatementItemLoader(
        ILogger<CashStatementItemLoader> logger,
        IReader<FileReaders.AccountStatements.CashStatementItem> cashStatementReader,
        InvestmentsDbContext context,
        CashStatementItemDividendStockEnricher dividendStockEnricher)
    {
        _cashStatementReader = cashStatementReader;
        _context = context;
        _logger = logger;
        _dividendStockEnricher = dividendStockEnricher;
    }

    public async Task Load(string fileName)
    {
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

            if (cashStatementItem.CashStatementItemType == CashStatementItemTypes.Dividend)
            {
                await _dividendStockEnricher.Enrich(cashStatementItem);
            }

            _context.CashStatementItems.Add(cashStatementItem);
        }

        await _context.SaveChangesAsync();
    }
}
