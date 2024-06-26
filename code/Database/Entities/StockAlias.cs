using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Entities;

[Table("StockAlias")]
public class StockAlias
{
    public StockAlias(string description) 
    {
        Description = description;
    }

    [Key]
    [Required]
    [MaxLength(50)]
    public string Description { get; set; }
    
    [Required]
    public Stock Stock { get; set; }
}
