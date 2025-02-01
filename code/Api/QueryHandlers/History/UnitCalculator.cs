using Api.QueryHandlers.Common;

namespace Api.QueryHandlers.History;

public class UnitCalculator
{
    // Take a list of value history results and calculate units for each date....

    public IList<UnitAccount> Calculate(IList<AccountHistoricalValue> historicalValues, decimal initialValue)
    {
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
                // we have previous units......

                // buy additional units at the previous price:
                var previousNumberOfUnits = previousUnit.NumberOfUnits;
                var boughtUnits = currentValue.Inflows / previousUnit.ValueInGbpPerUnit;
                var currentNumberOfUnits = previousNumberOfUnits + boughtUnits;
                
                var currentValueOfUnit = currentNumberOfUnits == 0 ? previousUnit.ValueInGbpPerUnit : currentValue.ValueInGbp / currentNumberOfUnits;
                
                
                var unitAccount = new UnitAccount(currentValue.Date, currentNumberOfUnits, currentValueOfUnit);
                
                
                // buy additional units at the current price:
                // var previousNumberOfUnits = previousUnit.NumberOfUnits;
                // var previousValue = previousUnit.ValueInGbpPerUnit;
                // var boughtUnits = currentValue.Contributions / previousValue;
                // var currentNumberOfUnits = previousNumberOfUnits + boughtUnits;
                //
                // // TODO: does this really make sense? what should happen if number of units becomes 0?
                // var currentValueOfUnit = currentNumberOfUnits == 0 ? previousUnit.ValueInGbpPerUnit : currentValue.ValueInGbp / currentNumberOfUnits;
                //
                // var unitAccount = new UnitAccount(currentValue.Date, currentNumberOfUnits, currentValueOfUnit);
                //
                // TODO: if the contribution is not 0, and the total value has changed due to market fluctuations, the order of calculation here matters.
                // i.e. if the number of bought units is worked out before the new unit value or after. 
                // not sure how much it matters.
                
                units.Add(unitAccount);
                previousUnit = unitAccount;
            }
        }

        return units;
    }
}
