using Common;
using Database.Entities;
using Microsoft.Extensions.Logging;

namespace LoaderConsole.StockTransactionEnrichers;

public class StockTransactionTypeEnricher : IStockTransactionEnricher
{
    private readonly ILogger<StockTransactionTypeEnricher> _logger;

    public StockTransactionTypeEnricher(ILogger<StockTransactionTypeEnricher> logger)
    {
        _logger = logger;
    }

    public void Enrich(StockTransaction stockTransaction)
    {
        if (stockTransaction.Transaction.Equals("Purchase"))
        {
            stockTransaction.TransactionType = StockTransactionTypes.Purchase;
        }
        else if (stockTransaction.Transaction.Equals("Sale"))
        {
            stockTransaction.TransactionType = StockTransactionTypes.Sale;
        }
        else if (stockTransaction.Transaction.Equals("Transfer In"))
        {
            stockTransaction.TransactionType = StockTransactionTypes.TransferIn;
        }
        else if (stockTransaction.Transaction.Contains("Removal"))
        {
            stockTransaction.TransactionType = StockTransactionTypes.Removal;
        }
        else if (stockTransaction.Transaction.Contains("Receipt"))
        {
            stockTransaction.TransactionType = StockTransactionTypes.Receipt;
        }
        else
        {
            _logger.LogWarning("Unknown transaction type: {TransactionType}", stockTransaction.Transaction);
            stockTransaction.TransactionType = string.Empty;
        }
    }
}
