using Database;
using Microsoft.EntityFrameworkCore;

namespace Api.QueryHandlers.Fetchers;

public class CommentFetcher : ICommentFetcher
{
    private readonly InvestmentsDbContext _context;

    public CommentFetcher(InvestmentsDbContext context)
    {
        _context = context;
    }

    public async Task<IList<Database.Entities.Comment>> Get(string[] accountCodes)
    {
        var allComments = await _context.Comments
            .AsNoTracking()
            .ToListAsync();

        return allComments
            .Where(c => c.AccountCodes
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(code => accountCodes.Contains(code)))
            .ToList();
    }
}
