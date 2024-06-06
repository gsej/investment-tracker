using Database.Entities;

namespace DataLoaders;

public class ExchangeRateFetcher
{
    // assume USD / GBP as that's all we have right now.....
    public ExchangeRateResult GetExchangeRate(
        IList<ExchangeRate> exchangeRates,
        DateOnly requestDate)
    {
        
        // TODO: what if there are multiple values fo rthe same date? 
        // TODO: does this work when there is no value? 
        var exchangeRate = exchangeRates
            .Where(s =>
                s.Date.CompareTo(requestDate) <= 0)
            .MaxBy(s => s.Date);

        if (exchangeRate != null)
        {
            var ageInDays = requestDate.DayNumber - exchangeRate.Date.DayNumber;
            
            return new ExchangeRateResult(exchangeRate.Rate, ageInDays);
        }

        return ExchangeRateResult.Missing();
    }
}
