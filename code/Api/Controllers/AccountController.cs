using Api.QueryHandlers.Account;
using Api.QueryHandlers.History;
using Api.QueryHandlers.Portfolio;
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
    private readonly IPrecalculatedAccountValueHistoryQueryHandler _precalculatedAccountValueHistoryQueryHandler;

    public AccountController(ILogger<AccountController> logger,
        IAccountPortfolioQueryHandler accountPortfolioQueryHandler,
        IAccountQueryHandler accountQueryHandler,
        IAccountValueHistoryQueryHandler accountValueHistoryQueryHandler,
        IPrecalculatedAccountValueHistoryQueryHandler precalculatedAccountValueHistoryQueryHandler)
    {
        _logger = logger;
        _accountPortfolioQueryHandler = accountPortfolioQueryHandler;
        _accountQueryHandler = accountQueryHandler;
        _accountValueHistoryQueryHandler = accountValueHistoryQueryHandler;
        _precalculatedAccountValueHistoryQueryHandler = precalculatedAccountValueHistoryQueryHandler;
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
        request.AccountCodes ??= [];
        return await _accountPortfolioQueryHandler.Handle(request);
    }

    [HttpPost("/account/history")]
    public async Task<AccountValueHistoryResult> GetHistory([FromBody] AccountValueHistoryRequest request)
    {
        return await _accountValueHistoryQueryHandler.Handle(request);
    }

    [HttpPost("/account/precalculated-history")]
    public async Task<AccountValueHistoryResult> GetPrecalculatedHistory([FromBody] PrecalculatedAccountValueHistoryRequest request)
    {
        request.AccountCodes ??= [];
        return await _precalculatedAccountValueHistoryQueryHandler.Handle(request);
    }
}
