using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Database.Entities;

[Table("AccountHistoricalValue")]
public class AccountHistoricalValue
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AccountHistoricalValueId { get; set; }
    
    public DateOnly Date { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string AccountCode { get; set; }
    
    [Precision(19,5)]
    public decimal ValueInGbp { get; set; }
    
    [Precision(19,5)]
    public decimal NetInflows { get; set; }
    
    public int TotalPriceAgeInDays { get; set; }
    
    [MaxLength(200)]
    public string Comment { get; set; }
    
    [Precision(19,5)]
    public decimal? RecordedTotalValueInGbp { get; set; }
    
    [MaxLength(50)] 
    public string RecordedTotalValueSource { get; set; }
    
    [Precision(19,5)]
    public decimal? DiscrepancyRatio { get; set; }
    
    [Precision(19,5)]
    public decimal? DifferenceToPreviousDay { get; set; }
    
    [Precision(19,5)]
    public decimal? DifferenceRatio { get; set; }
}
