using Database.Entities;
using Database.Repositories;

namespace UnitTests.Fakes;

public class FakeStockPriceRepository : IStockPriceRepository
{
    public List<StockPrice> StockPrices { get; } = new ();

    public void Add(StockPrice stockPrice)
    {
        StockPrices.Add(stockPrice);
    }

    public Task BulkAdd(IEnumerable<StockPrice> stockPrices)
    {
        StockPrices.AddRange(stockPrices);
        return Task.CompletedTask;
    }

    public Task<IList<StockPrice>> GetAll(string stockSymbol)
    {
        return Task.FromResult<IList<StockPrice>>(StockPrices.Where(c => c.StockSymbol == stockSymbol).ToList());
    }
}
