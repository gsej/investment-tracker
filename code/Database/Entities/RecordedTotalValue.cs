using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Database.Entities;

[Table("RecordedTotalValue")]
public class RecordedTotalValue
{
    public RecordedTotalValue(
        string accountCode,
        string date,
        decimal totalValueInGbp)
    {
        AccountCode = accountCode;;
        Date = date;
        TotalValueInGbp = totalValueInGbp;
    }

    [Key]
    public Guid RecordedTotalValueId { get; set; }
    
    [MaxLength(20)]
    [Required]
    [ForeignKey(nameof(Account))]
    public string AccountCode { get; private set; }
    
    public Account Account { get; private set; }
   
    [MaxLength(10)]
    [Required]
    public string Date { get; set; }
    
    [Precision(19,5)]
    [Required]
    public decimal TotalValueInGbp { get; set; }
}
