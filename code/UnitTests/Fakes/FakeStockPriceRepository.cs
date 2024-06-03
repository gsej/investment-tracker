using Database.Entities;
using Database.Repositories;

namespace UnitTests.Fakes;

public class FakeStockPriceRepository : IStockPriceRepository
{
    public IList<StockPrice> StockPrices { get; set; } = new List<StockPrice>();

    public void Add(StockPrice stockPrice)
    {
        StockPrices.Add(stockPrice);
    }

    public Task<int> SaveChangesAsync()
    {
        //   throw new NotImplementedException();
        return Task.FromResult(0);
    }

    public Task<IList<StockPrice>> GetAll(string stockSymbol)
    {
        return Task.FromResult<IList<StockPrice>>(StockPrices.Where(c => c.StockSymbol == stockSymbol).ToList());
    }
}
