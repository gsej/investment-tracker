using Database.Converters;
using FluentAssertions;
using DateOnly = System.DateOnly;

namespace UnitTests.Database;

public class DateOnlyConverterTests
{
    [Fact]
    public void CanConvertDateOnlyToString()
    {
        var dateOnly = new DateOnly(2021, 1, 1);
        
        var converter = new DateOnlyConverter();
        
        var result = converter.ConvertToProvider(dateOnly);

        result.Should().Be("2021-01-01");
    }
    
    [Fact]
    public void CanConvertNullDateOnlyToNullString()
    {
        DateOnly? dateOnly = null;
        
        var converter = new DateOnlyConverter();
        
        var result = converter.ConvertToProvider(dateOnly);

        result.Should().BeNull();
    }
    
    [Fact]
    public void CanConvertStringToDateOnly()
    {
        var dateOnlyAsString = "2021-01-01";
        
        var converter = new DateOnlyConverter();
        
        var result = converter.ConvertFromProvider(dateOnlyAsString);

        result.Should().BeEquivalentTo(new DateOnly(2021, 1, 1));
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CanConvertNullOrEmptyStringToNullDateOnly(string dateOnlyAsString)
    {
        var converter = new DateOnlyConverter();
        
        var result = converter.ConvertFromProvider(dateOnlyAsString);

        result.Should().BeNull();
    }
}
