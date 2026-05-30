using Api.QueryHandlers.StockHistory;
using Api.QueryHandlers.Stocks;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class StockController : ControllerBase
{
    private readonly IStockHistoryQueryHandler _stockHistoryQueryHandler;
    private readonly IStocksQueryHandler _stocksQueryHandler;

    public StockController(
        IStockHistoryQueryHandler stockHistoryQueryHandler,
        IStocksQueryHandler stocksQueryHandler)
    {
        _stockHistoryQueryHandler = stockHistoryQueryHandler;
        _stocksQueryHandler = stocksQueryHandler;
    }

    [HttpPost("/stock/history")]
    public async Task<StockHistoryResult> GetHistory([FromBody] StockHistoryRequest request)
    {
        return await _stockHistoryQueryHandler.Handle(request);
    }

    [HttpGet("/stocks")]
    public async Task<IReadOnlyList<StockResult>> GetStocks()
    {
        return await _stocksQueryHandler.Handle();
    }
}
