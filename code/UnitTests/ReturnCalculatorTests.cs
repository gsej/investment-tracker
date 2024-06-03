// using api.Calculators;
// using database.Entities;
// using FluentAssertions;
//
// namespace unit_tests;
//
// public class ReturnCalculatorTests
// {
//     // https://www.rateofreturnexpert.com/time-weighted-return-calculator/
//     
//     
//     [Theory]
//     [InlineData(1000, 1000, 0)]
//     [InlineData(1000, 1500, 0.5)]
//     [InlineData(1000, 500, -0.5)]
//     public void CalculateReturnForOneYearWithNoContributions(decimal startValue, decimal endValue, double expectedRate)
//     {
//         var calculator = new ReturnCalculator();
//         var start = new RecordedTotalValue("Test", "2020-01-01", startValue);
//         var end = new RecordedTotalValue("Test", "2020-12-31", endValue);
//         var rate = calculator.CalculateReturn(start, end, new List<Deposit>());
//         rate.Should().Be(expectedRate);
//     }
//     
//     
//     [Theory]
//     [InlineData(1000, 1000, "2021-03-02", 500, 1508.21917808219, 0.05)]
//     //[InlineData(1000, 1500, 0.5)]
//     //[InlineData(1000, 500, -0.5)]
//     public void CalculateReturnForOneYearWithOneContribution(decimal startValue, decimal endValue, string contributionDate, decimal contributionAmount, decimal contibutionDateValue, double expectedRate)
//     {
//         var calculator = new ReturnCalculator();
//         var start = new RecordedTotalValue("Test", "2020-01-01", startValue);
//         var end = new RecordedTotalValue("Test", "2020-12-31", endValue);
//
//         var deposit = new Deposit(contributionDate, contributionAmount, contibutionDateValue);
//         
//         
//         var rate = calculator.CalculateReturn(start, end, new List<Deposit>());
//         rate.Should().Be(expectedRate);
//     }
// }
