using Api.QueryHandlers.History;

namespace Api.QueryHandlers.StockHistory;

public record StockHistoryResult(
    string StockSymbol,
    IReadOnlyList<StockPriceResult> Prices,
    IReadOnlyList<TrailingReturnResult> TrailingReturns);

public record StockPriceResult(DateOnly Date, decimal Price);
