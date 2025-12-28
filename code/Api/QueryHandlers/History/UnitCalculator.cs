namespace Api.QueryHandlers.History;

public class UnitCalculator
{
    public IList<UnitAccount> Calculate(IList<AccountHistoricalValue> historicalValues, decimal initialValue)
    {
        // Take a list of value history results and calculate units for each date....
        var units = new List<UnitAccount>(historicalValues.Count);
        UnitAccount previousUnit = null;

        for (int i = 0; i < historicalValues.Count; i++)
        {
            AccountHistoricalValue currentValue = historicalValues[i];
           
            if (previousUnit is null)
            {
                var unitAccount = new UnitAccount(currentValue.Date, currentValue.ValueInGbp / initialValue, initialValue);
                units.Add(unitAccount);
                previousUnit = unitAccount;
            }
            else
            {
                // buy additional units at the previous price:
                var previousNumberOfUnits = previousUnit.NumberOfUnits;
                var boughtUnits = currentValue.NetInflows / previousUnit.ValueInGbpPerUnit;
                var currentNumberOfUnits = previousNumberOfUnits + boughtUnits;
                
                // if the value is now zero, number of units should be zero too, but rounding errors
                // cause issues, so:

                if (currentValue.ValueInGbp == 0)
                {
                    currentNumberOfUnits = 0;
                }
                
                var currentValueOfUnit = currentNumberOfUnits == 0 ? previousUnit.ValueInGbpPerUnit : currentValue.ValueInGbp / currentNumberOfUnits;
                
                var unitAccount = new UnitAccount(currentValue.Date, currentNumberOfUnits, currentValueOfUnit);
                
                units.Add(unitAccount);
                previousUnit = unitAccount;
            }
        }

        return units;
    }
}
