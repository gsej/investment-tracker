using Database.Entities;

namespace DataLoaders;

public class ExchangeRateFetcher
{
    // assume USD / GBP as that's all we have right now.....
    //
    // Expects exchangeRates pre-sorted ascending by (Date, ExchangeRateId). The
    // caller is responsible for sorting once so per-record lookups stay cheap.
    public ExchangeRateResult GetExchangeRate(
        IReadOnlyList<ExchangeRate> exchangeRates,
        DateOnly requestDate)
    {
        if (exchangeRates.Count == 0)
        {
            return ExchangeRateResult.Missing();
        }

        // Binary search for the highest index whose Date is <= requestDate.
        // Tie-breaker on ExchangeRateId is baked into the input ordering.
        var lo = 0;
        var hi = exchangeRates.Count - 1;
        var idx = -1;

        while (lo <= hi)
        {
            var mid = lo + ((hi - lo) >> 1);
            if (exchangeRates[mid].Date.CompareTo(requestDate) <= 0)
            {
                idx = mid;
                lo = mid + 1;
            }
            else
            {
                hi = mid - 1;
            }
        }

        if (idx < 0)
        {
            return ExchangeRateResult.Missing();
        }

        var exchangeRate = exchangeRates[idx];
        var ageInDays = requestDate.DayNumber - exchangeRate.Date.DayNumber;

        return new ExchangeRateResult(exchangeRate.Rate, ageInDays);
    }
}
