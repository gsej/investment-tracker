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
        DateOnly date,
        decimal totalValueInGbp)
    {
        AccountCode = accountCode;;
        Date = date;
        TotalValueInGbp = totalValueInGbp;
    }

    [Key]
    public Guid RecordedTotalValueId { get; set; }
    
    [ForeignKey(nameof(Account))]
    [Required]
    [MaxLength(20)]
    public string AccountCode { get; private set; }
    
    public Account Account { get; private set; }

    [Required]
    public DateOnly Date { get; set; }

    [Required]
    [Precision(19,5)]
    public decimal TotalValueInGbp { get; set; }
}
