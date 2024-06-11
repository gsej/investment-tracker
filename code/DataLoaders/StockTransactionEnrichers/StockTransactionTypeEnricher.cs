using Common;
using Database.Entities;
using Microsoft.Extensions.Logging;

namespace DataLoaders.StockTransactionEnrichers;

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
        else if (stockTransaction.Transaction.Contains("Book cost adjustment"))
        {
            stockTransaction.TransactionType = StockTransactionTypes.BookCostAdjustment;
        }
        else if (stockTransaction.Transaction.Contains("Equalisation"))
        {
            /* Occurs on a money market fund and possibly some others.
             * There's a corresponding entry on the cash statement which contains the word
             * equalisation in the description. I record it there as a dividend, although
             * technically it isn't. See https://www.willisowen.co.uk/help/equalisation-explained 
             */
            
            stockTransaction.TransactionType = StockTransactionTypes.Equalisation;
        }
        else if (
            string.IsNullOrWhiteSpace(stockTransaction.Transaction) &&
            stockTransaction.Quantity == 0 &&
            stockTransaction.Reference.Contains("##") &&
            stockTransaction.AmountGbp > 0)
        {
            stockTransaction.TransactionType = StockTransactionTypes.AccumulatedDividend;
            
            /*
             * Secure message from AjBell, 07/06/2023 asking about entries with no Transaction
             * field and a double ## in the reference field, e.g. 44624##1932. There is
             * no corresponding entry in the cash statement.
             * 
             * Thank you for your message.
             * Please be advised that these transactions refer to dividends paid on accumulation version of your holdings which are not paid as cash but
             *  instead reinvested in the underlying assets of the fund.
             * 
             * If you have any further queries, please feel free to contact us.
             * 
             *  Yours sincerely
             *
             *  (name deleted)
             *  Dealing Services Team
             */
        }
        else
        {
            _logger.LogError("Unknown transaction type: {TransactionType}", stockTransaction.Transaction);
            throw new ArgumentException("Unknown transaction type: {stockTransaction.Transaction}");
        }
    }
}
