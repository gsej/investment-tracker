using Common.Extensions;
using Database;
using FileReaders;
using FileReaders.Comments;
using Microsoft.Extensions.Logging;
using Comment = Database.Entities.Comment;

namespace DataLoaders;

public class CommentLoader
{
    private readonly IReader<CommentFileItem> _reader;
    private readonly InvestmentsDbContext _context;
    private readonly ILogger<CommentLoader> _logger;

    public CommentLoader(ILogger<CommentLoader> logger, IReader<CommentFileItem> reader, InvestmentsDbContext context)
    {
        _reader = reader;
        _context = context;
        _logger = logger;
    }

    public async Task Load(string fileName)
    {
        if (!File.Exists(fileName))
        {
            _logger.LogWarning("File {fileName} does not exist", fileName);
            return;
        }

        var fileItems = (await _reader.Read(fileName)).ToList();

        foreach (var fileItem in fileItems)
        {
            var comment = new Comment(
                date: fileItem.Date.ToDateOnly(),
                text: fileItem.Text,
                accountCodes: string.Join(",", fileItem.AccountCodes));

            _context.Comments.Add(comment);
        }

        await _context.SaveChangesAsync();
    }
}
