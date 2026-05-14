using System.Text.Json.Serialization;

namespace FileReaders.Comments;

public record CommentFileItem
{    public string Date { get; init; }
    public string Text { get; init; }

    [JsonPropertyName("account_codes")]
    public IList<string> AccountCodes { get; init; }
}
