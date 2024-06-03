using Database.Entities;

namespace Api.Calculators;

public  record Deposit(string Date, decimal Amount, decimal ContibutionDateValue);

public class ReturnCalculator
{
    public double CalculateReturn(RecordedTotalValue startValue, RecordedTotalValue endValue, IEnumerable<Deposit> deposits)
    {
        return 0;
    }
}