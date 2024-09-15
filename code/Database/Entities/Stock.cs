using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace Database.Entities;

[Table("Stock")]
[DebuggerDisplay("{StockSymbol} - {Description}")]
public class Stock
{
    [Key]
    [Required]
    [MaxLength(15)]
    public string StockSymbol { get; init; }
    
    [MaxLength(12)]
    public string Isin { get; init; }

    [Required]
    [MaxLength(50)]
    public string Description { get; init; }

    public IEnumerable<AlternativeSymbol> AlternativeSymbols { get; init; } = new List<AlternativeSymbol>();// encapsulate this
    
    public IEnumerable<StockAlias> Aliases { get; set; } = new List<StockAlias>();// encapsulate this
    
    [Required]
    [MaxLength(15)]
    public string StockType { get; init; }
    
    [Required]
    public bool SubjectToStampDuty { get; init; }
    
    [MaxLength(500)]
    public string Notes { get; init; }
    
    [Required]
    [MaxLength(20)]
    public string Allocation { get; init; }

    public class StockBuilder 
    {
        private readonly string _stockSymbol;
        private string _isin;
        private readonly string _description;
        private string _stockType;
        private string _notes;
        private bool _subjectToStampDuty;
        private string _allocation;
     
        private IEnumerable<StockAlias> _aliases = new List<StockAlias>();
        private IEnumerable<AlternativeSymbol> _alternativeSymbols = new List<AlternativeSymbol>();
      
        public StockBuilder(string stockSymbol, string description, string stockType, string allocation)
        {
            _stockSymbol = stockSymbol;
            _description = description;
            _stockType = stockType;
            _allocation = allocation;
        }
        
        public StockBuilder WithStampDuty()
        {
            _subjectToStampDuty = true;
            return this;
        }
        
        public StockBuilder WithIsin(string isin)
        {
            _isin = isin;
            return this;
        }
        
        public StockBuilder WithStockType(string stockType)
        {
            _stockType = stockType;
            return this;
        }
        
        public StockBuilder WithNotes(string notes)
        {
            _notes = notes;
            return this;
        }
        
        public StockBuilder WithAliases(IList<StockAlias> aliases)
        {
            _aliases = aliases;
            return this;
        }
        
        public StockBuilder WithAlternativeSymbols(IList<AlternativeSymbol> alternativeSymbols)
        {
            _alternativeSymbols = alternativeSymbols;
            return this;
        }
        
        public Stock Build()
        {
            return new Stock
            {
                StockSymbol = _stockSymbol,
                Description = _description,
                StockType = _stockType,
                Notes = _notes,
                Aliases = _aliases, 
                AlternativeSymbols = _alternativeSymbols,
                Isin = _isin,
                SubjectToStampDuty = _subjectToStampDuty,
                Allocation = _allocation
            };
        }
    }
}
