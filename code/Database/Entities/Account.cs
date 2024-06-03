using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Entities;

[Table("Account")]
public class Account
{
    public Account(string accountCode)
    {
        AccountCode = accountCode;
    }
    
    public Account(string accountCode, DateOnly? openingDate)
    {
        AccountCode = accountCode;
        OpeningDate = openingDate;
    }

    [MaxLength(20)]
    [Required]
    [Key]
    public string AccountCode { get; init; }
    
    public DateOnly? OpeningDate { get; init; }
}
