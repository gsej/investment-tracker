using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Database.Entities;

[Table("StockPrice")]
public class StockPrice
{
    public StockPrice(string stockSymbol,
        DateOnly date,
        decimal price,
        string currency,
        string source,
        string originalCurrency,
        int? exchangeRateAgeInDays = null,
        string comment = null)
    {
        StockSymbol = stockSymbol;
        Date = date;
        Price = price;
        Currency = currency;
        Source = source;
        OriginalCurrency = originalCurrency;
        ExchangeRateAgeInDays = exchangeRateAgeInDays;
        Comment = comment;
    }

    [Key]
    public Guid StockPriceId { get; set; }
    
    [ForeignKey(nameof(Stock))]
    [Required]
    [MaxLength(15)]
    public string StockSymbol { get; set; }
    
    public Stock Stock { get; set; }
  
    [Required]
    public DateOnly Date { get; set; }
    
    [Required]
    [Precision(19,5)]
    public decimal Price { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string Currency { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Source { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string OriginalCurrency { get; set; }
    
    public int? ExchangeRateAgeInDays { get; set; }
    
    [MaxLength(200)]
    public string Comment { get; set; }
}
