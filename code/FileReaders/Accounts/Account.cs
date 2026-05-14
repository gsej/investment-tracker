using System.Text.Json.Serialization;

namespace FileReaders.Accounts;

public record Account(
    [property: JsonPropertyName("account_code")] string AccountCode,
    [property: JsonPropertyName("opening_date")] string OpeningDate);


