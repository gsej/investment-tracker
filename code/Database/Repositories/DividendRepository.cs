using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Database.Converters;
using Database.Entities;
using Microsoft.Data.SqlClient;

namespace Database.Repositories;

public class DividendRepository : IDividendRepository
{
    private readonly string _connectionString;
    private readonly DateOnlyConverter _dateOnlyConverter = new();

    public DividendRepository(string sqlConnectionString)
    {
        _connectionString = sqlConnectionString;
    }

    public async Task BulkAdd(IEnumerable<Dividend> dividends)
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("DividendId", typeof(Guid));
        dataTable.Columns.Add("StockSymbol", typeof(string));
        dataTable.Columns.Add("Date", typeof(string));
        dataTable.Columns.Add("Amount", typeof(decimal));
        dataTable.Columns.Add("Currency", typeof(string));
        dataTable.Columns.Add("Source", typeof(string));
        dataTable.Columns.Add("OriginalCurrency", typeof(string));
        dataTable.Columns.Add("ExchangeRateAgeInDays", typeof(int));
        dataTable.Columns.Add("Comment", typeof(string));

        foreach (var dividend in dividends)
        {
            var dateString = _dateOnlyConverter.ConvertToProvider(dividend.Date);
            dataTable.Rows.Add(
                Guid.NewGuid(),
                dividend.StockSymbol,
                dateString,
                dividend.Amount,
                dividend.Currency,
                dividend.Source,
                dividend.OriginalCurrency,
                dividend.ExchangeRateAgeInDays,
                dividend.Comment);
        }

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var bulkCopy = new SqlBulkCopy(connection);
        bulkCopy.DestinationTableName = "Dividend";
        bulkCopy.ColumnMappings.Add("DividendId", "DividendId");
        bulkCopy.ColumnMappings.Add("StockSymbol", "StockSymbol");
        bulkCopy.ColumnMappings.Add("Date", "Date");
        bulkCopy.ColumnMappings.Add("Amount", "Amount");
        bulkCopy.ColumnMappings.Add("Currency", "Currency");
        bulkCopy.ColumnMappings.Add("Source", "Source");
        bulkCopy.ColumnMappings.Add("OriginalCurrency", "OriginalCurrency");
        bulkCopy.ColumnMappings.Add("ExchangeRateAgeInDays", "ExchangeRateAgeInDays");
        bulkCopy.ColumnMappings.Add("Comment", "Comment");
        await bulkCopy.WriteToServerAsync(dataTable);
    }
}
