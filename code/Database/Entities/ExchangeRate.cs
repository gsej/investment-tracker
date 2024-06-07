using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Database.Entities;

[Table("ExchangeRate")]
public class ExchangeRate
{
    [Key]
    public Guid ExchangeRateId { get; init; }
    
    [Required]
    [MaxLength(3)]
    public string BaseCurrency { get; init; }
    
    [Required]
    [MaxLength(3)]
    public string AlternateCurrency { get; init; }
    
    [Required]
    public DateOnly Date { get; init; }
    
    [Required]
    [Precision(19,5)]
    public decimal Rate { get; init; }
    
    [Required]
    [MaxLength(100)]
    public string Source { get; init; }
}
