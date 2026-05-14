using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Entities;

[Table(nameof(Comment))]
public class Comment
{
    public Comment(DateOnly date, string text, string accountCodes)
    {
        Date = date;
        Text = text;
        AccountCodes = accountCodes;
    }

    [Key]
    public Guid CommentId { get; set; }

    [Required]
    public DateOnly Date { get; private set; }

    [Required]
    [MaxLength(500)]
    public string Text { get; private set; }

    [Required]
    [MaxLength(200)]
    public string AccountCodes { get; private set; }
}
