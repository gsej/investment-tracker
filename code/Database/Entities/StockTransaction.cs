using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Database.Entities;

[Table(nameof(StockTransaction))]
public class StockTransaction
{
    public StockTransaction(
        string accountCode,
        string date,
        string transaction,
        string description,
        decimal quantity,
        decimal amountGbp,
        string reference,
        decimal fee,
        decimal stampDuty,
        string stockSymbol)
    {
        AccountCode = accountCode;
        Date = date;
        Transaction = transaction;
        Description = description;
        Quantity = quantity;
        AmountGbp = amountGbp;
        Reference = reference;
        Fee = fee;
        StampDuty = stampDuty;
        StockSymbol = stockSymbol;
    }
 
    [Key]
    public Guid StockTransactionId { get; }
    
    [MaxLength(20)]
    [Required]
    [ForeignKey(nameof(Account))]
    public string AccountCode { get; private set; }
    
    public Account Account { get; private set; }
    
    [MaxLength(15)]
    [ForeignKey(nameof(Stock))]
    [Required]
    public string StockSymbol { get; private set; }
    
    public Stock Stock { get; private set; }
    
    [MaxLength(10)]
    [Required]
    public string Date { get; private set; }
    
    [MaxLength(200)]
    [Required]
    public string Transaction { get; private set; }
    
    [MaxLength(200)]
    [Required]
    public string Description { get; private set; }
    
    [Precision(19,5)]
    [Required]
    public decimal Quantity { get; private set; }
    
    [Precision(19,5)]
    [Required]
    public decimal AmountGbp { get; set; }
    
    [MaxLength(20)]
    [Required]
    public string Reference { get; set; }
    
    [Precision(19,5)]
    [Required]
    public decimal Fee { get; set; }
    
    [Precision(19,5)]
    [Required]
    public decimal StampDuty { get; set; }
    
    [MaxLength(100)]
    [Required]
    // TODO: use Enumeration
    public string TransactionType { get; set; }
}
