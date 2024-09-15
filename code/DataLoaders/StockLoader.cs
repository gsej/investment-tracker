using Common.Tracing;
using Database.Converters;
using Database.Entities;
using Database.Repositories;
using FileReaders.Stocks;
using Microsoft.Extensions.Logging;
using Stock = Database.Entities.Stock;

namespace DataLoaders;

public class StockLoader
{
    private readonly ILogger<StockLoader> _logger;
    private readonly IStockRepository _repository;
    private readonly IStockReader _reader;
    private readonly DateOnlyConverter _dateOnlyConverter;

    public StockLoader(
        ILogger<StockLoader> logger,
        IStockRepository repository,
        IStockReader reader,
        DateOnlyConverter dateOnlyConverter)
    {
        _logger = logger;
        _reader = reader;
        _dateOnlyConverter = dateOnlyConverter;
        _repository = repository;
    }

    public StockLoader(IStockReader reader, DateOnlyConverter dateOnlyConverter)
    {
        _reader = reader;
        _dateOnlyConverter = dateOnlyConverter;
    }

    public async Task LoadFile(string fileName)
    {
        using (InvestmentTrackerActivitySource.Instance.StartActivity($"File: {fileName}"))
        using (_logger.BeginScope(new Dictionary<string, string> { ["File"] = fileName, ["Contents"] = "Stocks" }))
        {
            _logger.LogInformation("Loading Stocks from {fileName}", fileName);

            var stocks = (await _reader.ReadFile(fileName)).ToList();

            foreach (var stockDto in stocks)
            {
                _logger.LogInformation("beginning to process Stock {stockDto}", stockDto);

                var stock = new Stock.StockBuilder(
                        stockDto.StockSymbol,
                        stockDto.Description,
                        stockDto.StockType,
                        stockDto.Allocation)
                        .WithAliases(stockDto.Aliases.Select(a => new StockAlias(a.Description)).ToList())
                        .WithAlternativeSymbols(stockDto.AlternativeSymbols.Select(a => new AlternativeSymbol(a.Alternative)).ToList())
                        .Build();
              
                _repository.Add(stock);
                await _repository.SaveChangesAsync();
            }

            await _repository.SaveChangesAsync();
        }
    }
}
