// using System.ComponentModel.DataAnnotations;
// using Microsoft.EntityFrameworkCore;
//
// namespace consumer.Entities;
//
// public class Dividends
// {
//     [MaxLength(20)]
//     [Required]
//     public string Account { get; set; }
//     
//     [Required]
//     public int Year { get; set; }
//     
//     [Precision(19, 5)]
//     public decimal TotalReceived { get; set; }
// }