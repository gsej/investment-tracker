using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Database.Converters;
using Database.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

public class ExchangeRateRepository : IExchangeRateRepository
{
    private readonly InvestmentsDbContext _context;
    private readonly string _connectionString;
    private readonly DateOnlyConverter _dateOnlyConverter = new();

    public ExchangeRateRepository(InvestmentsDbContext context, string connectionString)
    {
        _context = context;
        _connectionString = connectionString;
    }

    public async Task<IList<ExchangeRate>> GetAll()
    {
        return await _context.ExchangeRates
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task BulkAdd(IEnumerable<ExchangeRate> exchangeRates)
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("ExchangeRateId", typeof(Guid));
        dataTable.Columns.Add("BaseCurrency", typeof(string));
        dataTable.Columns.Add("AlternateCurrency", typeof(string));
        dataTable.Columns.Add("Date", typeof(string));
        dataTable.Columns.Add("Rate", typeof(decimal));
        dataTable.Columns.Add("Source", typeof(string));

        foreach (var rate in exchangeRates)
        {
            dataTable.Rows.Add(
                Guid.NewGuid(),
                rate.BaseCurrency,
                rate.AlternateCurrency,
                _dateOnlyConverter.ConvertToProvider(rate.Date),
                rate.Rate,
                rate.Source);
        }

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var bulkCopy = new SqlBulkCopy(connection);
        bulkCopy.DestinationTableName = "ExchangeRate";
        bulkCopy.ColumnMappings.Add("ExchangeRateId", "ExchangeRateId");
        bulkCopy.ColumnMappings.Add("BaseCurrency", "BaseCurrency");
        bulkCopy.ColumnMappings.Add("AlternateCurrency", "AlternateCurrency");
        bulkCopy.ColumnMappings.Add("Date", "Date");
        bulkCopy.ColumnMappings.Add("Rate", "Rate");
        bulkCopy.ColumnMappings.Add("Source", "Source");

        await bulkCopy.WriteToServerAsync(dataTable);
    }
}
