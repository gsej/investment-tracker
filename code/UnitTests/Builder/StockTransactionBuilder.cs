using Database.Entities;

namespace UnitTests.Builder;

public class StockTransactionBuilder
{
    private string _accountCode = "ACCOUNT-CODE";
    private DateOnly _date = new(2023, 3, 3);
    private string _transaction = "Purchase";
    private string _transactionType = "Purchase";
    private string _description = "National Grid Plc";
    private decimal _quantity = 100m;
    private decimal _amountGbp = 120.11m;
    private string _reference = "REFERENCE";
    private decimal _fee = 0m;
    private decimal _stampDuty = 0m;
    private string _stockSymbol = "NG.L";
    
    public StockTransactionBuilder WithDate(DateOnly date)
    {
        _date = date;
        return this;
    }
    
    public StockTransactionBuilder WithTransaction(string transaction)
    {
        _transaction = transaction;
        return this;
    }
    
    public StockTransactionBuilder WithTransactionType(string transaction)
    {
        _transactionType = transaction;
        return this;
    }
    
    public StockTransactionBuilder WithAmountGbp(decimal amountGbp)
    {
        _amountGbp = amountGbp;
        return this;
    }

    public StockTransaction Build()
    {
        var stockTransaction = new StockTransaction(
            _accountCode,
            _date,
            _transaction,
            _description, 
            _quantity, 
            _amountGbp,
            _reference,
            _fee,
            _stampDuty,
            _stockSymbol);
        
        stockTransaction.TransactionType = _transactionType;

        return stockTransaction;
    }
}
