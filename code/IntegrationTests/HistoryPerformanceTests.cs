using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests;

public class HistoryPerformanceTests : IClassFixture<WebApplicationFactory<Api.Program>>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public HistoryPerformanceTests(WebApplicationFactory<Api.Program> factory, ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _output = output;
    }

    [Fact]
    public async Task History_Performance() =>
        await MeasurePerAccount("/account/history", accountCode => new { accountCode, queryDate = QueryDate });

    [Fact]
    public async Task PrecalculatedHistory_Performance() =>
        await MeasurePerAccount("/account/precalculated-history", accountCode => new { accountCodes = new[] { accountCode }, queryDate = QueryDate });

    private static string QueryDate => DateOnly.FromDateTime(DateTime.Now).ToString("yyyy-MM-dd");

    private async Task MeasurePerAccount(string url, Func<string, object> buildBody)
    {
        var accountsResult = await _client.GetFromJsonAsync<AccountsResponse>("/accounts");

        var totalSw = Stopwatch.StartNew();
        foreach (var account in accountsResult!.Accounts)
        {
            var sw = Stopwatch.StartNew();
            var response = await _client.PostAsJsonAsync(url, buildBody(account.AccountCode));
            response.EnsureSuccessStatusCode();
            await response.Content.ReadAsStringAsync();
            sw.Stop();
            _output.WriteLine($"  {account.AccountCode}: {sw.ElapsedMilliseconds}ms");
        }
        totalSw.Stop();
        _output.WriteLine($"  Total:  {totalSw.ElapsedMilliseconds}ms");
    }

    private record AccountsResponse(List<AccountItem> Accounts);
    private record AccountItem(string AccountCode);
}
