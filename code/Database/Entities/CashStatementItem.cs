using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Database.Entities;

[Table(nameof(CashStatementItem))]
public class CashStatementItem
{
    public CashStatementItem(string accountCode,
        string date,
        string description,
        decimal paymentAmountGbp,
        decimal receiptAmountGbp)
    {
        AccountCode = accountCode;
        Date = date;
        Description = description;
        PaymentAmountGbp = paymentAmountGbp;
        ReceiptAmountGbp = receiptAmountGbp;
    }

    [Key]
    public Guid CashStatementItemId { get; set; }
    
    [MaxLength(20)]
    [Required]
    [ForeignKey(nameof(Account))]
    public string AccountCode { get; }
    
    public Account Account { get; private set; }
    
    [MaxLength(10)]
    [Required]
    public string Date { get; private set; }
    
    [MaxLength(200)]
    [Required]
    public string Description { get; private set; }
    
    [Precision(19, 5)]
    public decimal ReceiptAmountGbp { get; private set; }
    
    [Precision(19, 5)]
    public decimal PaymentAmountGbp { get; private set; }
    
    [MaxLength(100)]
    public string CashStatementItemType { get;  set; }
}
