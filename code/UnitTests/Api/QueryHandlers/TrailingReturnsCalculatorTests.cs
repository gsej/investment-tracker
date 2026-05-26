using Api.QueryHandlers.History;
using FluentAssertions;

namespace UnitTests.Api.QueryHandlers;

public class TrailingReturnsCalculatorTests
{
    private static UnitAccount Unit(int year, int month, int day, decimal unitValue) =>
        new(new DateOnly(year, month, day), 100m, unitValue);

    [Fact]
    public void Calculate_EmptyUnitValues_ReturnsSevenNullResults()
    {
        var result = TrailingReturnsCalculator.Calculate([], new DateOnly(2024, 12, 31));

        result.Should().HaveCount(7);
        result.Should().AllSatisfy(r => r.ReturnPercentage.Should().BeNull());
    }

    [Fact]
    public void Calculate_EndUnitValueIsZero_ReturnsSevenNullResults()
    {
        var units = new List<UnitAccount> { new(new DateOnly(2024, 12, 31), 0m, 0m) };

        var result = TrailingReturnsCalculator.Calculate(units, new DateOnly(2024, 12, 31));

        result.Should().HaveCount(7);
        result.Should().AllSatisfy(r => r.ReturnPercentage.Should().BeNull());
    }

    [Fact]
    public void Calculate_OneYearReturn_IsCorrect()
    {
        var units = new List<UnitAccount>
        {
            Unit(2023, 12, 31, 100m),
            Unit(2024, 12, 31, 110m),
        };

        var result = TrailingReturnsCalculator.Calculate(units, new DateOnly(2024, 12, 31));

        var oneYear = result.Single(r => r.PeriodLabel == "1 year");
        oneYear.ReturnPercentage.Should().BeApproximately(0.10m, 0.0001m);
        oneYear.IsAnnualised.Should().BeFalse();
    }

    [Fact]
    public void Calculate_ThreeMonthReturn_IsCorrect()
    {
        var units = new List<UnitAccount>
        {
            Unit(2024, 9, 30, 100m),
            Unit(2024, 12, 31, 105m),
        };

        var result = TrailingReturnsCalculator.Calculate(units, new DateOnly(2024, 12, 31));

        var threeMonth = result.Single(r => r.PeriodLabel == "3 months");
        threeMonth.ReturnPercentage.Should().BeApproximately(0.05m, 0.0001m);
        threeMonth.IsAnnualised.Should().BeFalse();
    }

    [Fact]
    public void Calculate_ThreeYearAnnualisedReturn_IsCorrect()
    {
        // 1.1^3 = 1.331 — so 10% annualised over 3 years gives 33.1% total
        var units = new List<UnitAccount>
        {
            Unit(2021, 12, 31, 100m),
            Unit(2024, 12, 31, 133.1m),
        };

        var result = TrailingReturnsCalculator.Calculate(units, new DateOnly(2024, 12, 31));

        var threeYear = result.Single(r => r.PeriodLabel == "3 years");
        threeYear.ReturnPercentage.Should().BeApproximately(0.10m, 0.001m);
        threeYear.IsAnnualised.Should().BeTrue();
    }

    [Fact]
    public void Calculate_FiveYearAnnualisedReturn_IsCorrect()
    {
        // 1.1^5 = 1.61051
        var units = new List<UnitAccount>
        {
            Unit(2019, 12, 31, 100m),
            Unit(2024, 12, 31, 161.051m),
        };

        var result = TrailingReturnsCalculator.Calculate(units, new DateOnly(2024, 12, 31));

        var fiveYear = result.Single(r => r.PeriodLabel == "5 years");
        fiveYear.ReturnPercentage.Should().BeApproximately(0.10m, 0.001m);
        fiveYear.IsAnnualised.Should().BeTrue();
    }

    [Fact]
    public void Calculate_InsufficientHistory_ReturnsNullForLongerPeriods()
    {
        // Only 2 years of data
        var units = new List<UnitAccount>
        {
            Unit(2022, 12, 31, 100m),
            Unit(2024, 12, 31, 120m),
        };

        var result = TrailingReturnsCalculator.Calculate(units, new DateOnly(2024, 12, 31));

        result.Single(r => r.PeriodLabel == "3 years").ReturnPercentage.Should().BeNull();
        result.Single(r => r.PeriodLabel == "5 years").ReturnPercentage.Should().BeNull();
        result.Single(r => r.PeriodLabel == "10 years").ReturnPercentage.Should().BeNull();
        result.Single(r => r.PeriodLabel == "1 year").ReturnPercentage.Should().NotBeNull();
    }

    [Fact]
    public void Calculate_YtdReturn_IsCorrect()
    {
        var units = new List<UnitAccount>
        {
            Unit(2024, 1, 1, 100m),
            Unit(2024, 6, 30, 108m),
        };

        var result = TrailingReturnsCalculator.Calculate(units, new DateOnly(2024, 6, 30));

        var ytd = result.Single(r => r.PeriodLabel == "YTD");
        ytd.ReturnPercentage.Should().BeApproximately(0.08m, 0.0001m);
        ytd.IsAnnualised.Should().BeFalse();
    }

    [Fact]
    public void Calculate_NearestDateUsedWhenExactStartDateNotAvailable()
    {
        // Data is on Dec 30 not Dec 31 — should still find it for 1-year lookback
        var units = new List<UnitAccount>
        {
            Unit(2023, 12, 30, 100m),
            Unit(2024, 12, 31, 110m),
        };

        var result = TrailingReturnsCalculator.Calculate(units, new DateOnly(2024, 12, 31));

        result.Single(r => r.PeriodLabel == "1 year").ReturnPercentage.Should().NotBeNull();
    }

    [Fact]
    public void Calculate_ReturnsCorrectPeriodLabelsInOrder()
    {
        var units = new List<UnitAccount> { Unit(2024, 12, 31, 100m) };

        var result = TrailingReturnsCalculator.Calculate(units, new DateOnly(2024, 12, 31));

        result.Select(r => r.PeriodLabel).Should().ContainInOrder(
            "3 months", "6 months", "1 year", "3 years", "5 years", "10 years", "YTD");
    }
}
