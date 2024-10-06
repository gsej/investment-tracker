using Api.QueryHandlers.Portfolio;
using Common;
using Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using DateOnly = System.DateOnly;

namespace Api.QueryHandlers.History;

public interface IAnnualPerformanceQueryHandler
{
    Task<AnnualPerformanceResult> Handle(AnnualPerformanceRequest request);
}

public class AnnualPerformanceQueryHandler : IAnnualPerformanceQueryHandler
{
    private readonly InvestmentsDbContext _context;
    private readonly IAccountPortfolioQueryHandler _accountPortfolioQueryHandler;
    private readonly ILogger<AnnualPerformanceQueryHandler> _logger;

    public AnnualPerformanceQueryHandler(InvestmentsDbContext context,
        IAccountPortfolioQueryHandler accountPortfolioQueryHandler,
        ILogger<AnnualPerformanceQueryHandler> logger)
    {
        _context = context;
        _accountPortfolioQueryHandler = accountPortfolioQueryHandler;
        _logger = logger;
    }
    
    public async Task<AnnualPerformanceResult> Handle(AnnualPerformanceRequest request)
    {
        var accounts = await _context.Accounts.Where(account => request.AccountCodes.Contains(account.AccountCode)).ToListAsync();
        
         if (accounts.IsNullOrEmpty())
         {
             _logger.LogError("The accounts could not be found");
             throw new InvalidOperationException($"The accounts could not be found");
         }

         var startYear = accounts.Select(a => a.OpeningDate).Min().Year;
         var endYear = request.AsOfDate.Year;

         var years = Enumerable.Range(startYear, endYear - startYear + 1);

         var result = new AnnualPerformanceResult
         {
             Years = years.Select(year => new PerformanceResult(year)).ToList()
         };

         var inflowTypes = new[]
         {
             CashStatementItemTypes.Contribution,
             CashStatementItemTypes.TaxRelief,
             CashStatementItemTypes.TransferIn,
             CashStatementItemTypes.Withdrawal
         };

         foreach (var performanceResult in result.Years)
         {
             var startDate = new DateOnly(performanceResult.Year, 1, 1);
             var endDate = new DateOnly(performanceResult.Year, 12, 31);

             var cashInflow = await _context.CashStatementItems
                 .Where(c =>
                     request.AccountCodes.Contains(c.AccountCode) &&
                     inflowTypes.Contains(c.CashStatementItemType) &&
                     c.Date >= startDate &&
                     c.Date <= endDate)
                 .SumAsync(pr => pr.ReceiptAmountGbp + pr.PaymentAmountGbp);
             performanceResult.NetInflowsInGbp = cashInflow;

             performanceResult.ValueAtStart = new TotalValue(0, 0);
             performanceResult.ValueAtEnd = new TotalValue(0, 0);
             foreach (var accountCode in request.AccountCodes)
             {
                 var summaryAtStart = await _accountPortfolioQueryHandler.Handle(new AccountPortfolioRequest(accountCode, startDate));
                 var summaryAtEnd = await _accountPortfolioQueryHandler.Handle(new AccountPortfolioRequest(accountCode, endDate));

                 performanceResult.ValueAtStart = new TotalValue(ValueInGbp: performanceResult.ValueAtStart.ValueInGbp + summaryAtStart.TotalValue.ValueInGbp, TotalPriceAgeInDays: performanceResult.ValueAtStart.TotalPriceAgeInDays + summaryAtStart.TotalValue.TotalPriceAgeInDays);
                 performanceResult.ValueAtEnd = new TotalValue(ValueInGbp: performanceResult.ValueAtEnd.ValueInGbp + summaryAtEnd.TotalValue.ValueInGbp, TotalPriceAgeInDays: performanceResult.ValueAtEnd.TotalPriceAgeInDays + summaryAtEnd.TotalValue.TotalPriceAgeInDays);
             }
         }
         
         return result;
    }
}
