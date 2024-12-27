using Api.QueryHandlers.Fetchers;
using Api.QueryHandlers.Portfolio;

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
                var boughtUnits = currentValue.Contributions / previousUnit.ValueInGbpPerUnit;
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



public class AccountValueHistoryQueryHandler : IAccountValueHistoryQueryHandler
{
    private readonly IAccountFetcher _accountFetcher;
    private readonly IAccountPortfolioQueryHandler _accountPortfolioQueryHandler;
    private readonly IRecordedTotalValueQueryHandler _recordedTotalValueQueryHandler;
    private readonly ILogger<AccountValueHistoryQueryHandler> _logger;

    public AccountValueHistoryQueryHandler(
        IAccountFetcher accountFetcher, 
        IAccountPortfolioQueryHandler accountPortfolioQueryHandler,
        IRecordedTotalValueQueryHandler recordedTotalValueQueryHandler, 
        ILogger<AccountValueHistoryQueryHandler> logger)
    {
        _accountFetcher = accountFetcher;
        _accountPortfolioQueryHandler = accountPortfolioQueryHandler;
        _recordedTotalValueQueryHandler = recordedTotalValueQueryHandler;
        _logger = logger;
    }
    
    // Returns the total value of the account at the end of each day in the date range.
    public async Task<AccountValueHistoryResult> Handle(AccountValueHistoryRequest request)
    {
        var account = (await _accountFetcher.GetAccounts()).SingleOrDefault(account => account.AccountCode == request.AccountCode);

        if (account == null)
        {
            _logger.LogError("The account with code {AccountCode} does not exist", request.AccountCode);
            throw new InvalidOperationException(); // TODO: perhaps return something better?
        }
       
        // iterate over each day in the date range
        var results = new List<AccountHistoricalValue>();

        var currentDate = account.OpeningDate;

        var accountCode = request.AccountCode;
        
        // get recorded total values
        var recordedTotalValues = await _recordedTotalValueQueryHandler.Handle(new RecordedTotalValuesRequest(accountCode));

        decimal? previousDayTotal = null;
        
        while (currentDate <= request.QueryDate)
        {
            var daysResult = await _accountPortfolioQueryHandler.Handle(new AccountPortfolioRequest { AccountCode = request.AccountCode, Date = currentDate });

            var comment = string.Join(", ", daysResult.Holdings.Select(h => h.Comment).Where(c => !string.IsNullOrWhiteSpace(c)));

            var matchingRecordedTotalValue = recordedTotalValues.RecordedTotalValues.SingleOrDefault(r => r.Date == currentDate);
            
            var historicalValue = new AccountHistoricalValue(currentDate, 
                accountCode, 
                daysResult.TotalValue.ValueInGbp,
                daysResult.Contributions,
                daysResult.TotalValue.TotalPriceAgeInDays, 
                comment);

            if (matchingRecordedTotalValue != null)
            {
                historicalValue.RecordedTotalValueInGbp = matchingRecordedTotalValue.TotalValueInGbp;
                historicalValue.RecordedTotalValueSource = matchingRecordedTotalValue.Source;

                if (historicalValue.ValueInGbp != 0)
                {
                    historicalValue.DiscrepancyRatio =  (historicalValue.ValueInGbp - historicalValue.RecordedTotalValueInGbp) / historicalValue.ValueInGbp;
                }
            }

            if (previousDayTotal.HasValue)
            {
                // calculate difference to previous day to see if there are big leaps. Remove any contribution which might affect the result
                historicalValue.DifferenceToPreviousDay = historicalValue.ValueInGbp - historicalValue.Contributions - previousDayTotal.Value;

                if (previousDayTotal.Value != 0)
                {
                    historicalValue.DifferenceRatio = historicalValue.DifferenceToPreviousDay / previousDayTotal.Value;
                }
            }
            
            results.Add(historicalValue);

            currentDate = currentDate.AddDays(1);
            previousDayTotal = historicalValue.ValueInGbp;
        }

        results = results.OrderBy(r => r.Date).ToList();
        
        var unitValues = new UnitCalculator().Calculate(results, 100);

        foreach (var result in results)
        {
            var matchingUnitValues = unitValues.SingleOrDefault(u => u.Date == result.Date);

             if (matchingUnitValues == null)
             {
                 result.Units = new UnitAccount(result.Date, null, null);
             }
             else
             {
                 result.Units = matchingUnitValues;
             }
        }
        
        return new AccountValueHistoryResult(results);
    }
}
