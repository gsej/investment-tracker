using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Database.Entities;

[Table(nameof(CashStatementItem))]
public class CashStatementItem
{
    public CashStatementItem(string accountCode,
        DateOnly date,
        string description,
        decimal receiptAmountGbp,
        decimal paymentAmountGbp)
    {
        if (receiptAmountGbp < 0)
            throw new ArgumentOutOfRangeException(nameof(receiptAmountGbp));

        if (paymentAmountGbp > 0)
            throw new ArgumentOutOfRangeException(nameof(paymentAmountGbp));
        
        
        AccountCode = accountCode;
        Date = date;
        Description = description;
        PaymentAmountGbp = paymentAmountGbp;
        ReceiptAmountGbp = receiptAmountGbp;
    }

    [Key]
    public Guid CashStatementItemId { get; set; }
    
    [ForeignKey(nameof(Account))]
    [Required]
    [MaxLength(20)]
    public string AccountCode { get; }
    
    public Account Account { get; private set; }
   
    [Required]
    public DateOnly Date { get; private set; }
    
    [Required]
    [MaxLength(200)]
    public string Description { get; private set; }
    
    [Precision(19, 5)]
    public decimal ReceiptAmountGbp { get; private set; }
    
    [Precision(19, 5)]
    public decimal PaymentAmountGbp { get; private set; }
    
    [Required]
    [MaxLength(100)]
    public string CashStatementItemType { get;  set; }
}
