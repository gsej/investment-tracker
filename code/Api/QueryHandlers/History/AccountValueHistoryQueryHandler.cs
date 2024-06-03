using Api.QueryHandlers.Summary;
using Database;
using Microsoft.EntityFrameworkCore;

namespace Api.QueryHandlers.History;

public class AccountValueHistoryQueryHandler : IAccountValueHistoryQueryHandler
{
    private readonly InvestmentsDbContext _context;
    private readonly ISummaryQueryHandler _summaryQueryHandler;
    private readonly IRecordedTotalValueQueryHandler _recordedTotalValueQueryHandler;
    private readonly ILogger<AccountValueHistoryQueryHandler> _logger;

    public AccountValueHistoryQueryHandler(InvestmentsDbContext context,
        ISummaryQueryHandler summaryQueryHandler,
        IRecordedTotalValueQueryHandler recordedTotalValueQueryHandler, 
        ILogger<AccountValueHistoryQueryHandler> logger)
    {
        _context = context;
        _summaryQueryHandler = summaryQueryHandler;
        _recordedTotalValueQueryHandler = recordedTotalValueQueryHandler;
        _logger = logger;
    }
    
    // Returns the total value of the account at the end of each day in the date range.
    public async Task<AccountValueHistoryResult> Handle(AccountValueHistoryRequest request)
    {
        var account = await _context.Accounts.SingleOrDefaultAsync(account => account.AccountCode == request.AccountCode);

        if (account == null)
        {
            _logger.LogError("The account with code {AccountCode} does not exist", request.AccountCode);
            throw new InvalidOperationException(); // TODO: perhaps return something better?
        }
        
        var startDate = account.OpeningDate ?? new DateOnly(2020, 1, 1);
        var now = DateTime.UtcNow;

        var endDate = new DateOnly(now.Year, now.Month, now.Day);
       
        // iterate over each day in the date range
        var results = new List<AccountHistoricalValue>();

        var currentDate = startDate;

        var accountCode = request.AccountCode; //GSEJ
        
        // get record total values
        var recordedTotalValues = await _recordedTotalValueQueryHandler.Handle(new RecordedTotalValuesRequest(accountCode));

        while (currentDate <= endDate)
        {
            var dateAsString = currentDate.ToString("yyyy-MM-dd");
            var daysResult = await _summaryQueryHandler.Handle(new SummaryRequest { AccountCode = request.AccountCode, Date = dateAsString });

            var comment = string.Join(", ", daysResult.Holdings.Select(h => h.Comment).Where(c => !string.IsNullOrWhiteSpace(c)));

            var matchingRecordedTotalValue = recordedTotalValues.RecordedTotalValues.SingleOrDefault(r => r.Date == dateAsString);
            
            var historicalValue = new AccountHistoricalValue(dateAsString, 
                accountCode, 
                daysResult.TotalValue.ValueInGbp,
                daysResult.TotalValue.TotalPriceAgeInDays, 
                comment);

            if (matchingRecordedTotalValue != null)
            {
                historicalValue.RecordedTotalValueInGbp = matchingRecordedTotalValue.TotalValueInGbp;
                historicalValue.DiscrepancyPercentage = 100 * (historicalValue.ValueInGbp - historicalValue.RecordedTotalValueInGbp) / historicalValue.ValueInGbp;
            }
            
            results.Add(historicalValue);

            currentDate = currentDate.AddDays(1);
        }

        results = results.OrderBy(r => r.Date).ToList();
        return new AccountValueHistoryResult(results);
    }
}
