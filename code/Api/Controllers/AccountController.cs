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
    private readonly IAccountValueHistoryQueryHandler _accountValueHistoryQueryHandler;
    private readonly IAccountValueHistoryQueryHandler2 _accountValueHistoryQueryHandler2;

    public AccountController(ILogger<AccountController> logger,
        IAccountPortfolioQueryHandler accountPortfolioQueryHandler,
        IAccountQueryHandler accountQueryHandler,
        IAccountValueHistoryQueryHandler accountValueHistoryQueryHandler,
        IAccountValueHistoryQueryHandler2 accountValueHistoryQueryHandler2)
    {
        _logger = logger;
        _accountPortfolioQueryHandler = accountPortfolioQueryHandler;
        _accountQueryHandler = accountQueryHandler;
        _accountValueHistoryQueryHandler = accountValueHistoryQueryHandler;
        _accountValueHistoryQueryHandler2 = accountValueHistoryQueryHandler2;
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

        request.AccountCodes ??= [];
        return await _accountPortfolioQueryHandler.Handle(request);
    }
    
    [HttpPost("/account/history")]
    public async Task<AccountValueHistoryResult> GetHistory([FromBody] AccountValueHistoryRequest request)
    {
        // TODO: this needs to be removed
        using var activity = InvestmentTrackerActivitySource.Instance.StartActivity();
        return await _accountValueHistoryQueryHandler.Handle(request);
    }
    
    [HttpPost("/account/history2")]
    public async Task<AccountValueHistoryResult> GetHistory2([FromBody] AccountValueHistoryRequest2 request)
    {
        using var activity = InvestmentTrackerActivitySource.Instance.StartActivity();
        
        request.AccountCodes ??= [];
        return await _accountValueHistoryQueryHandler2.Handle(request);
    }
    
    // [HttpPost("/account/annual-performance")]
    // public async Task<AnnualPerformanceResult> GetAccountAnnualPerformance([FromBody] AnnualPerformanceRequest request)
    // {
    //     return await _annualPerformanceQueryHandler.Handle(request);
    // }
}
