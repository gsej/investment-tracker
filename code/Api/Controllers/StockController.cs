using Api.QueryHandlers.StockHistory;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class StockController : ControllerBase
{
    private readonly IStockHistoryQueryHandler _stockHistoryQueryHandler;

    public StockController(IStockHistoryQueryHandler stockHistoryQueryHandler)
    {
        _stockHistoryQueryHandler = stockHistoryQueryHandler;
    }

    [HttpPost("/stock/history")]
    public async Task<StockHistoryResult> GetHistory([FromBody] StockHistoryRequest request)
    {
        return await _stockHistoryQueryHandler.Handle(request);
    }
}
