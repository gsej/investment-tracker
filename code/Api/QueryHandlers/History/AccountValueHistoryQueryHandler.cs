using Api.QueryHandlers.Fetchers;
using Api.QueryHandlers.Portfolio;

namespace Api.QueryHandlers.History;

public class AccountValueHistoryQueryHandler : IAccountValueHistoryQueryHandler
{
    private readonly IAccountFetcher _accountFetcher;
    private readonly IAccountPortfolioQueryHandler _accountPortfolioQueryHandler;
    private readonly IRecordedTotalValueFetcher _recordedTotalValueFetcher;
    private readonly ILogger<AccountValueHistoryQueryHandler> _logger;

    public AccountValueHistoryQueryHandler(
        IAccountFetcher accountFetcher, 
        IAccountPortfolioQueryHandler accountPortfolioQueryHandler,
        IRecordedTotalValueFetcher recordedTotalValueFetcher, 
        ILogger<AccountValueHistoryQueryHandler> logger)
    {
        _accountFetcher = accountFetcher;
        _accountPortfolioQueryHandler = accountPortfolioQueryHandler;
        _recordedTotalValueFetcher = recordedTotalValueFetcher;
        _logger = logger;
    }
    
    // Returns the total value of the account at the end of each day in the date range.
    public async Task<AccountValueHistoryResult> Handle(AccountValueHistoryRequest request)
    {
        var currentDate = await GetStartDate(request.AccountCode);
        var endDate = request.QueryDate;
        
        var results = new List<AccountHistoricalValue>();
        
        var recordedTotalValues = await _recordedTotalValueFetcher.GetRecordedTotalValues(request.AccountCode);

        decimal? previousDayTotal = null;
        
        while (currentDate <= endDate)
        {
            var historicalValue = await GetPortfolioStateForDate(request.AccountCode, currentDate);
            
            // TODO: here.

            var matchingRecordedTotalValue = recordedTotalValues.SingleOrDefault(r => r.Date == currentDate);
            
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

    private async Task<DateOnly> GetStartDate(string accountCode)
    {
        var account = (await _accountFetcher.GetAccounts()).SingleOrDefault(account => account.AccountCode == accountCode);

        if (account == null)
        {
            _logger.LogError("The account with code {AccountCode} does not exist", accountCode);
            throw new InvalidOperationException();
        }
        
        var currentDate = account.OpeningDate;
        return currentDate;
    }

    private async Task<AccountHistoricalValue> GetPortfolioStateForDate(string accountCode, DateOnly date)
    {
        var daysResult = await _accountPortfolioQueryHandler.Handle(new AccountPortfolioRequest { AccountCode = accountCode, Date = date });

        var comment = string.Join(", ", daysResult.Holdings.Select(h => h.Comment).Where(c => !string.IsNullOrWhiteSpace(c)));
            
        var historicalValue = new AccountHistoricalValue(date, 
            accountCode, 
            daysResult.TotalValue.ValueInGbp,
            daysResult.Contributions,
            daysResult.TotalValue.TotalPriceAgeInDays, 
            comment);

        return historicalValue;
    }
}
