using Api.QueryHandlers.Fetchers;
using Api.QueryHandlers.Portfolio;

namespace Api.QueryHandlers.History;

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
        
        var now = DateTime.UtcNow;
        var endDate = new DateOnly(now.Year, now.Month, now.Day);
       
        // iterate over each day in the date range
        var results = new List<AccountHistoricalValue>();

        var currentDate = account.OpeningDate;

        var accountCode = request.AccountCode;
        
        // get recorded total values
        var recordedTotalValues = await _recordedTotalValueQueryHandler.Handle(new RecordedTotalValuesRequest(accountCode));

        decimal? previousDayTotal = null;
        
        while (currentDate <= endDate)
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
        return new AccountValueHistoryResult(results);
    }
}
