using System.Net.Http.Json;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace IntegrationTests;

public class HistoryEquivalenceTests : IClassFixture<WebApplicationFactory<Api.Program>>
{
    private readonly HttpClient _client;

    public HistoryEquivalenceTests(WebApplicationFactory<Api.Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task History_AndHistory2_ReturnEquivalentResultsForEachAccount()
    {
        var queryDate = DateOnly.FromDateTime(DateTime.Now).ToString("yyyy-MM-dd");

        var accountsResult = await _client.GetFromJsonAsync<AccountsResponse>("/accounts");
        accountsResult.Should().NotBeNull();
        accountsResult!.Accounts.Should().NotBeEmpty("the database must be populated before running integration tests");

        foreach (var account in accountsResult.Accounts)
        {
            var history = await PostAsync<HistoryResponse>("/account/history",
                new { accountCode = account.AccountCode, queryDate });

            var history2 = await PostAsync<HistoryResponse>("/account/history2",
                new { accountCodes = new[] { account.AccountCode }, queryDate });

            using var scope = new AssertionScope($"account {account.AccountCode}");

            history2!.Items.Should().HaveSameCount(history!.Items);
         
            
            var index = 0;
            foreach (var (h1, h2) in history.Items.Zip(history2.Items))
            {
                h2.Date.Should().Be(h1.Date, $"index {index}");
                h2.ValueInGbp.Should().BeApproximately(h1.ValueInGbp, 0.0001m, $"index {index}");
                index++;
                h2.NetInflows.Should().Be(h1.NetInflows);
                h2.TotalPriceAgeInDays.Should().Be(h1.TotalPriceAgeInDays);
                h2.RecordedTotalValueInGbp.Should().Be(h1.RecordedTotalValueInGbp);
                h2.DiscrepancyRatio.Should().BeApproximately(h1.DiscrepancyRatio, 0.000001m);
                h2.DifferenceToPreviousDay.Should().BeApproximately(h1.DifferenceToPreviousDay, 0.0001m);
                h2.DifferenceRatio.Should().BeApproximately(h1.DifferenceRatio, 0.000001m);
            }
        }
    }

    private async Task<T?> PostAsync<T>(string url, object body)
    {
        var response = await _client.PostAsJsonAsync(url, body);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    private record AccountsResponse(List<AccountItem> Accounts);
    private record AccountItem(string AccountCode);
    private record HistoryResponse(List<HistoryItem> Items);
    private record HistoryItem(
        string Date,
        decimal ValueInGbp,
        decimal NetInflows,
        int TotalPriceAgeInDays,
        decimal? RecordedTotalValueInGbp,
        decimal? DiscrepancyRatio,
        decimal? DifferenceToPreviousDay,
        decimal? DifferenceRatio);
}
