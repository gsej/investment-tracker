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
    
    public Account(string accountCode, DateOnly openingDate)
    {
        AccountCode = accountCode;
        OpeningDate = openingDate;
    }

    
    [Key]
    [Required]
    [MaxLength(20)]
    public string AccountCode { get; init; }
    
    public DateOnly OpeningDate { get; init; }
}
