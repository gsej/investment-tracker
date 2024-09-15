namespace FileReaders.Stocks;

public record Stock(string StockSymbol,
    string Isin,
    string Description,
    IList<AlternativeSymbols> AlternativeSymbols,
    IList<Alias> Aliases,
    string StockType,
    bool SubjectToStampDuty,
    string Notes, 
    string Allocation);

public record Alias(string Description);

public record AlternativeSymbols(string Alternative);


