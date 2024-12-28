using Api.QueryHandlers.Account;
using Api.QueryHandlers.History;
using Api.QueryHandlers.Performance;
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
    private readonly IAccountValueHistoryQueryHandler _accountValueHistoryQueryHandler;

    public AccountController(ILogger<AccountController> logger,
        IAccountPortfolioQueryHandler accountPortfolioQueryHandler,
        IAccountQueryHandler accountQueryHandler,
        IAccountValueHistoryQueryHandler accountValueHistoryQueryHandler
        )
    {
        _logger = logger;
        _accountPortfolioQueryHandler = accountPortfolioQueryHandler;
        _accountQueryHandler = accountQueryHandler;
        _accountValueHistoryQueryHandler = accountValueHistoryQueryHandler;
    }

    
    /// <summary>
    /// Returns a list of accounts
    /// </summary>
    [HttpGet("/accounts")]
    public async Task<AccountResponse> GetAccounts()
    {
        var accounts = await _accountQueryHandler.Handle(new AccountRequest());
        return new AccountResponse(accounts);
    }

    /// <summary>
    /// Returns the state of the portfolio at a given point in time
    /// </summary>
    [HttpPost("/account/portfolio")]
    public async Task<AccountPortfolioResult> GetPortfolio([FromBody] AccountPortfolioRequest request)
    {
        using var activity = InvestmentTrackerActivitySource.Instance.StartActivity();
        return await _accountPortfolioQueryHandler.Handle(request);
    }
    
    /// <summary>
    /// Returns an entry for each date in the portfolios history showing key data for each day
    /// </summary>
    [HttpPost("/account/history")]
    public async Task<AccountValueHistoryResult> GetHistory([FromBody] AccountValueHistoryRequest request)
    {
        using var activity = InvestmentTrackerActivitySource.Instance.StartActivity();
        return await _accountValueHistoryQueryHandler.Handle(request);
    }
    
    // [HttpPost("/account/performance")]
    // public async Task<AccountPerformanceResult> GetAccountAnnualPerformance([FromBody] AccountPerformanceRequest request)
    // {
    //     return await _accountPerformanceQueryHandler.Handle(request);
    // }
}
