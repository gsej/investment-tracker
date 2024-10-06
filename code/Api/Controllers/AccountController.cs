using Api.QueryHandlers.Account;
using Api.QueryHandlers.History;
using Api.QueryHandlers.Portfolio;
using Common.Tracing;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    public record AccountResponse(IList<Account> Accounts);

    private readonly ILogger<AccountController> _logger;
    private readonly IAccountPortfolioQueryHandler _accountPortfolioQueryHandler;
    private readonly IAccountQueryHandler _accountQueryHandler;
    private readonly IRecordedTotalValueQueryHandler _recordedTotalValueQueryHandler;
    private readonly IAccountValueHistoryQueryHandler _accountValueHistoryQueryHandler;
    private readonly IAnnualPerformanceQueryHandler _annualPerformanceQueryHandler;

    public AccountController(ILogger<AccountController> logger,
        IAccountPortfolioQueryHandler accountPortfolioQueryHandler,
        IAccountQueryHandler accountQueryHandler,
        IRecordedTotalValueQueryHandler recordedTotalValueQueryHandler,
        IAccountValueHistoryQueryHandler accountValueHistoryQueryHandler,
        IAnnualPerformanceQueryHandler annualPerformanceQueryHandler
        )
    {
        _logger = logger;
        _accountPortfolioQueryHandler = accountPortfolioQueryHandler;
        _accountQueryHandler = accountQueryHandler;
        _recordedTotalValueQueryHandler = recordedTotalValueQueryHandler;
        _accountValueHistoryQueryHandler = accountValueHistoryQueryHandler;
        _annualPerformanceQueryHandler = annualPerformanceQueryHandler;
    }

    [HttpGet("/accounts")]
    public async Task<AccountResponse> GetAccounts()
    {
            var accounts = await _accountQueryHandler.Handle(new AccountRequest());
            return new AccountResponse(accounts);
    }

    [HttpPost("/account/portfolio")]
    public async Task<AccountPortfolioResult> GetPortfolio([FromBody] AccountPortfolioRequest request)
    {
        using var activity = InvestmentTrackerActivitySource.Instance.StartActivity();
        return await _accountPortfolioQueryHandler.Handle(request);
    }
    
    [HttpPost("/account/history")]
    public async Task<AccountValueHistoryResult> GetHistory([FromBody] AccountValueHistoryRequest request)
    {using var activity = InvestmentTrackerActivitySource.Instance.StartActivity();
        
        return await _accountValueHistoryQueryHandler.Handle(request);
    }
    
    // [HttpPost("/account/annual-performance")]
    // public async Task<AnnualPerformanceResult> GetAccountAnnualPerformance([FromBody] AnnualPerformanceRequest request)
    // {
    //     return await _annualPerformanceQueryHandler.Handle(request);
    // }
}
