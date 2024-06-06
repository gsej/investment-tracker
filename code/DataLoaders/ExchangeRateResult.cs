namespace DataLoaders;

public record ExchangeRateResult
{
    public string Error { get; }
    public decimal? Rate { get; }
    public int? AgeInDays { get; }

    public bool HasRate => Rate.HasValue;

    public ExchangeRateResult(decimal rate, int ageInDays)
    {
        Rate = rate;
        AgeInDays = ageInDays;
    }

    private ExchangeRateResult(string error)
    {
        Error = error;
    }
    
    public static ExchangeRateResult Missing() => new($"Missing exchange rate");
}
