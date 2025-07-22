using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Database.Converters;
using Database.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

public class StockPriceRepository : IStockPriceRepository
{
    private readonly InvestmentsDbContext _context;
    private readonly string _connectionString;
    private readonly DateOnlyConverter _dateOnlyConverter = new ();

    public StockPriceRepository(InvestmentsDbContext context, LoaderConfiguration configuration)
    {
        _context = context;
        _connectionString = configuration.SqlConnectionString;
    }

    public async Task BulkAdd(IEnumerable<StockPrice> stockPrices)
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("StockPriceId", typeof(Guid));
        dataTable.Columns.Add("StockSymbol", typeof(string));
        dataTable.Columns.Add("Date", typeof(string));
        dataTable.Columns.Add("Price", typeof(decimal));
        dataTable.Columns.Add("Currency", typeof(string));
        dataTable.Columns.Add("Source", typeof(string));
        dataTable.Columns.Add("OriginalCurrency", typeof(string));
        dataTable.Columns.Add("ExchangeRateAgeInDays", typeof(int));
        dataTable.Columns.Add("Comment", typeof(string));
        
        foreach (var price in stockPrices)
        {
            var dateString = _dateOnlyConverter.ConvertToProvider(price.Date);
            dataTable.Rows.Add(
                Guid.NewGuid(),
                price.StockSymbol,
                dateString,
                price.Price,
                price.Currency,
                price.Source,
                price.OriginalCurrency,
                price.ExchangeRateAgeInDays,
                price.Comment);
        }

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        using var bulkCopy = new SqlBulkCopy(connection);
        bulkCopy.DestinationTableName = "StockPrice";
        bulkCopy.ColumnMappings.Add("StockPriceId", "StockPriceId");
        bulkCopy.ColumnMappings.Add("StockSymbol", "StockSymbol");
        bulkCopy.ColumnMappings.Add("Date", "Date");
        bulkCopy.ColumnMappings.Add("Price", "Price");
        bulkCopy.ColumnMappings.Add("Currency", "Currency");
        bulkCopy.ColumnMappings.Add("Source", "Source");
        bulkCopy.ColumnMappings.Add("OriginalCurrency", "OriginalCurrency");
        bulkCopy.ColumnMappings.Add("ExchangeRateAgeInDays", "ExchangeRateAgeInDays");
        bulkCopy.ColumnMappings.Add("Comment", "Comment");
        await bulkCopy.WriteToServerAsync(dataTable);
    }

    public async Task<IList<StockPrice>> GetAll(string stockSymbol)
    {
        return await _context.StockPrices
            .Where(c => c.StockSymbol == stockSymbol)
            .AsNoTracking()
            .ToListAsync();
    }
}
