using System.Text.Json.Serialization;

namespace FileReaders.Stocks;

public record Stock(
    [property: JsonPropertyName("stock_symbol")] string StockSymbol,
    string Isin,
    string Description,
    [property: JsonPropertyName("alternative_symbols")] IList<AlternativeSymbols> AlternativeSymbols,
    IList<Alias> Aliases,
    [property: JsonPropertyName("stock_type")] string StockType,
    [property: JsonPropertyName("subject_to_stamp_duty")] bool SubjectToStampDuty,
    string Notes,
    string Allocation);

public record Alias(string Description);

public record AlternativeSymbols(string Alternative);


