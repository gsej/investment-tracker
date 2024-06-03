using Api.QueryHandlers.Account;
using Api.QueryHandlers.Quality;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class QualityController : ControllerBase
{
    public record AccountResponse(IList<Account> Accounts);

    private readonly ILogger<QualityController> _logger;
    private readonly IQualityQueryHandler _qualityQueryHandler;
    
    public QualityController(ILogger<QualityController> logger, 
        IQualityQueryHandler qualityQueryHandler)
    {
        _logger = logger;
        _qualityQueryHandler = qualityQueryHandler;
    }

    [HttpGet("/quality")]
    public async Task<QualityReport> Get()
    {
        return await _qualityQueryHandler.Handle();
    }
}
