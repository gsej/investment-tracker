using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Database.Entities;

[Table("StockPrice")]
public class StockPrice
{
    public StockPrice(string stockSymbol, string date, decimal price, string currency, string source, string originalCurrency)
    {
        StockSymbol = stockSymbol;
        Date = date;
        Price = price;
        Currency = currency;
        Source = source;
        OriginalCurrency = originalCurrency;
    }

    [Key]
    public Guid StockPriceId { get; set; }
    
    [MaxLength(15)]
    //[ForeignKey(nameof(Stock))]
    public string StockSymbol { get; set; }
    
    //public Stock Stock { get; set; }
    
    [MaxLength(10)]
    [Required]
    public string Date { get; set; }
    
    [Precision(19,5)]
    [Required]
    public decimal Price { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string Currency { get; set; }
    
    //  [Required]
    [MaxLength(100)]
    public string Source { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string OriginalCurrency { get; set; }
}
