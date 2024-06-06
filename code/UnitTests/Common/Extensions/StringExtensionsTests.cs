using Common.Extensions;
using FluentAssertions;

namespace UnitTests.Common.Extensions;

public class StringExtensionsTests
{
    [Fact]
    public void ToDateOnly_WhenCalledWithValidLongDate_ReturnsDateOnly()
    {
        // Arrange
        var dateAsLongString = "2021-01-01T00:00:00Z";
        
        // Act
        var dateOnly = dateAsLongString.ToDateOnly();
        
        // Assert
        dateOnly.Should().Be(new DateOnly(2021, 1, 1));
    }
    
    [Fact]
    public void ToDateOnly_WhenCalledWithValidShortDate_ReturnsDateOnly()
    {
        // Arrange
        var dateAsShortString = "2021-01-01";
        
        // Act
        var dateOnly = dateAsShortString.ToDateOnly();
        
        // Assert
        dateOnly.Should().Be(new DateOnly(2021, 1, 1));
    }
    
    [Fact]
    public void ToDateOnly_WhenCalledWithInvalidDate_Throws()
    {
        // Arrange
        var invalidDate = "202-01-01";
        
        // Act
        Action act = () => invalidDate.ToDateOnly();
        
        
        // Assert
        act.Should().Throw<FormatException>();
    }
}
