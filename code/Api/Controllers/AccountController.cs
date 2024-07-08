using Api.QueryHandlers.Account;
using Api.QueryHandlers.History;
using Api.QueryHandlers.Summary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Controllers;

public class ExampleSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(AccountSummaryRequest))
        {
            schema.Example = new OpenApiObject()
            {
                ["accountCodes"] = new OpenApiArray { new OpenApiString("SIPP")},
                ["date"] = new OpenApiString("2024-10-30"),
            };
        }
    }
}

[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    public record AccountResponse(IList<Account> Accounts);

    private readonly ILogger<AccountController> _logger;
    private readonly IAccountSummaryQueryHandler _accountSummaryQueryHandler;
    private readonly IAccountQueryHandler _accountQueryHandler;
    private readonly IRecordedTotalValueQueryHandler _recordedTotalValueQueryHandler;
    private readonly IAccountValueHistoryQueryHandler _accountValueHistoryQueryHandler;
    private readonly IAnnualPerformanceQueryHandler _annualPerformanceQueryHandler;

    public AccountController(ILogger<AccountController> logger,
        IAccountSummaryQueryHandler accountSummaryQueryHandler,
        IAccountQueryHandler accountQueryHandler,
        IRecordedTotalValueQueryHandler recordedTotalValueQueryHandler,
        IAccountValueHistoryQueryHandler accountValueHistoryQueryHandler,
        IAnnualPerformanceQueryHandler annualPerformanceQueryHandler)
    {
        _logger = logger;
        _accountSummaryQueryHandler = accountSummaryQueryHandler;
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

    [HttpPost("/account/summary")]
    public async Task<AccountSummaryResult> GetSummary([FromBody] AccountSummaryRequest request)
    {
        return await _accountSummaryQueryHandler.Handle(request);
    }
    
    [HttpPost("/account/account-value-history")]
    public async Task<AccountValueHistoryResult> GetAccountValueHistory([FromBody] AccountValueHistoryRequest request)
    {
        return await _accountValueHistoryQueryHandler.Handle(request);
    }
    
    [HttpPost("/account/annual-performance")]
    public async Task<AnnualPerformanceResult> GetAccountAnnualPerformance([FromBody] AnnualPerformanceRequest request)
    {
        return await _annualPerformanceQueryHandler.Handle(request);
    }
}
