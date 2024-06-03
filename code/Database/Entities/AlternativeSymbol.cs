using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Entities;

[Table("AlternativeSymbol")]
public class AlternativeSymbol
{
    public AlternativeSymbol(string alternative)
    {
        Alternative = alternative;
    }

    [MaxLength(15)]
    [Required]
    [Key]
    public string Alternative { get; set; }
    
    [Required]
    public Stock Stock { get; set; }
}
