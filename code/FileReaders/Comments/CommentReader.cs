using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace FileReaders.Comments;

public class CommentReader : IReader<CommentFileItem>
{
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    private readonly ILogger<CommentReader> _logger;

    public CommentReader(ILogger<CommentReader> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<CommentFileItem>> Read(string fileName)
    {
        if (!File.Exists(fileName))
        {
            _logger.LogWarning("File {FileName} does not exist", fileName);
            return Array.Empty<CommentFileItem>();
        }

        var jsonString = await File.ReadAllTextAsync(fileName);
        var items = JsonSerializer.Deserialize<IList<CommentFileItem>>(jsonString, _options);
        return items;
    }
}
