using Database.Entities;

namespace DataLoaders;

public class ExchangeRateFetcher
{
    // assume USD / GBP as that's all we have right now.....
    public ExchangeRateResult GetExchangeRate(
        IList<ExchangeRate> exchangeRates,
        DateOnly requestDate)
    {
        var exchangeRate = exchangeRates
            .Where(s =>
                s.Date.CompareTo(requestDate) <= 0)
            .OrderByDescending(s => s.Date)
            // secondary sort ensures deterministic selection when multiple rates exist for the same date
            .ThenByDescending(s => s.ExchangeRateId)
            .FirstOrDefault();

        if (exchangeRate != null)
        {
            var ageInDays = requestDate.DayNumber - exchangeRate.Date.DayNumber;
            
            return new ExchangeRateResult(exchangeRate.Rate, ageInDays);
        }

        return ExchangeRateResult.Missing();
    }
}
