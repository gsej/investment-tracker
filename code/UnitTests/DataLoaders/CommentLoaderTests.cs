using Database;
using DataLoaders;
using FileReaders;
using FileReaders.Comments;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace UnitTests.DataLoaders;

public class CommentLoaderTests
{
    private static InvestmentsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<InvestmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new InvestmentsDbContext(options);
    }

    [Fact]
    public async Task Load_JoinsAccountCodesWithCommas()
    {
        using var context = CreateContext();
        var reader = Substitute.For<IReader<CommentFileItem>>();
        var fileName = Path.GetTempFileName();

        try
        {
            reader.Read(fileName).Returns(new List<CommentFileItem>
            {
                new() { Date = "2024-03-15", Text = "Withdrew funds", AccountCodes = new List<string> { "ISA123", "SIPP456" } }
            });

            var loader = new CommentLoader(
                Substitute.For<ILogger<CommentLoader>>(),
                reader,
                context);

            await loader.Load(fileName);

            var saved = await context.Comments.SingleAsync();

            using var _ = new AssertionScope();
            saved.Date.Should().Be(new DateOnly(2024, 3, 15));
            saved.Text.Should().Be("Withdrew funds");
            saved.AccountCodes.Should().Be("ISA123,SIPP456");
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Fact]
    public async Task Load_WhenFileDoesNotExist_LoadsNothing()
    {
        using var context = CreateContext();
        var reader = Substitute.For<IReader<CommentFileItem>>();

        var loader = new CommentLoader(
            Substitute.For<ILogger<CommentLoader>>(),
            reader,
            context);

        await loader.Load("does-not-exist.json");

        (await context.Comments.CountAsync()).Should().Be(0);
        await reader.DidNotReceive().Read(Arg.Any<string>());
    }
}
