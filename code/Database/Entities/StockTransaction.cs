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
        DateOnly date,
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
   
    [ForeignKey(nameof(Account))]
    [Required]
    [MaxLength(20)]
    public string AccountCode { get; private set; }
    
    public Account Account { get; private set; }
   
    [ForeignKey(nameof(Stock))]
    [Required]
    [MaxLength(15)]
    public string StockSymbol { get; private set; }
    
    public Stock Stock { get; private set; }
  
    [Required]
    public DateOnly Date { get; private set; }
    
    [Required]
    [MaxLength(200)]
    public string Transaction { get; private set; }
    
    [Required]
    [MaxLength(200)]
    public string Description { get; private set; }
    
    [Required]
    [Precision(19,5)]
    public decimal Quantity { get; private set; }
    
    [Required]
    [Precision(19,5)]
    public decimal AmountGbp { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Reference { get; set; }
    
    [Required]
    [Precision(19,5)]
    public decimal Fee { get; set; }
    
    [Required]
    [Precision(19,5)]
    public decimal StampDuty { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string TransactionType { get; set; }
}
