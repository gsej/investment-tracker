using Common.Extensions;
using Database.Repositories;
using DataLoaders;
using FileReaders;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RecordedTotalValue = FileReaders.RecordedTotalValue;

namespace UnitTests.DataLoaders;

public class RecordedTotalValueLoaderTests
{
    private readonly RecordedTotalValueLoader _loader;
    private readonly IReader<RecordedTotalValue> _reader;
    private readonly IRecordedTotalValueRepository _recordedTotalValueRepository = Substitute.For<IRecordedTotalValueRepository>();

    public RecordedTotalValueLoaderTests()
    {
        _reader = Substitute.For<IReader<RecordedTotalValue>>();

        _loader = new RecordedTotalValueLoader(
            Substitute.For<ILogger<RecordedTotalValueLoader>>(),
            _recordedTotalValueRepository,
            _reader);
    }

    [Fact]
    public async Task LoadFile_SavesRecordedTotalValues()
    {
        // arrange
        global::Database.Entities.RecordedTotalValue savedRecordedTotalValue = null;
        
        _recordedTotalValueRepository.Add(Arg.Do<global::Database.Entities.RecordedTotalValue>(recordedTotalValue => savedRecordedTotalValue = recordedTotalValue));

        var accountCode = "AccountCode";
        var fileName = "test.json";
        var totalValue = 102.07m;
        var date = "2022-05-20";
        
        var readRecordedTotalValue = new RecordedTotalValue{
            AccountCode = accountCode,
            Date = date,
            TotalValueInGbp = totalValue.ToString("F2")
        };
        
        _reader.Read(fileName).Returns(new List<RecordedTotalValue> { readRecordedTotalValue });
        
        // act
        await _loader.LoadFile(fileName, source: "Test");
        
        // assert
        using var _ = new AssertionScope();
        
        savedRecordedTotalValue.Should().NotBeNull();
        savedRecordedTotalValue.AccountCode.Should().Be(accountCode);
        savedRecordedTotalValue.Date.Should().Be(date.ToDateOnly());
        savedRecordedTotalValue.TotalValueInGbp.Should().Be(totalValue);
    }
}
