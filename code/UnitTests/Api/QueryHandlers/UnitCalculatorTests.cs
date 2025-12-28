using Api.QueryHandlers.History;
using FluentAssertions;
using AccountHistoricalValue = Api.QueryHandlers.History.AccountHistoricalValue;

namespace UnitTests.Api.QueryHandlers;

public class UnitCalculatorTests
{
    [Fact]
    public void Handle_WhenNoDataExists_ReturnsEmptyItemsForEachDate()
    {
        // Test to verify specific situation when the calculator was throwing an exception
        // fixed with special handling for the case when the number of units is very close to zero but not 
        // zero because of rounding issues.
        var historicalValues = new List<AccountHistoricalValue>
        {
            new(new DateOnly(2024, 7, 18), "Account", 0, 0, 0, "comment"),
            new(new DateOnly(2024, 7, 19), "Account", 19000, 19000, 0, "deposit"),
            new(new DateOnly(2024, 7, 20), "Account", 19102.52000m, 0, 0, "interest received"),
            new(new DateOnly(2024, 7, 21), "Account", 19102.52000m, 0, 0, "nothing"),
            new(new DateOnly(2024, 7, 22), "Account", 19102.52000m, 0, 0, "nothing"),
            new(new DateOnly(2024, 7, 23), "Account", 20032.94000m, 0, 0, "interest received"),
            new(new DateOnly(2024, 7, 24), "Account", 0, -20032.94000m, 0, "withdraw"),
            new(new DateOnly(2024, 7, 25), "Account", 0, 0, 0, "nothing"),
        };

        var unitCalculator = new UnitCalculator();

        var results = unitCalculator.Calculate(historicalValues, 100);

        results.Count.Should().Be(8);
    }
}
