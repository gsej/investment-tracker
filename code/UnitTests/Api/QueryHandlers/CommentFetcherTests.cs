using Api.QueryHandlers.Fetchers;
using Database;
using Database.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Api.QueryHandlers;

public class CommentFetcherTests
{
    private static InvestmentsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<InvestmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new InvestmentsDbContext(options);
    }

    [Fact]
    public async Task Get_FiltersByAccountCode()
    {
        using var context = CreateContext();

        context.Comments.AddRange(
            new Comment(new DateOnly(2024, 1, 1), "For A", "AccountA"),
            new Comment(new DateOnly(2024, 1, 2), "For B", "AccountB"),
            new Comment(new DateOnly(2024, 1, 3), "For A and C", "AccountA,AccountC"));
        await context.SaveChangesAsync();

        var fetcher = new CommentFetcher(context);

        var results = await fetcher.Get(["AccountA"]);

        results.Should().HaveCount(2);
        results.Select(c => c.Text).Should().BeEquivalentTo(["For A", "For A and C"]);
    }

    [Fact]
    public async Task Get_ReturnsEmpty_WhenNoMatch()
    {
        using var context = CreateContext();

        context.Comments.Add(new Comment(new DateOnly(2024, 1, 1), "For B", "AccountB"));
        await context.SaveChangesAsync();

        var fetcher = new CommentFetcher(context);

        var results = await fetcher.Get(["AccountA"]);

        results.Should().BeEmpty();
    }
}
