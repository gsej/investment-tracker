using System.Text.Json;
using Common.Tracing;
using FileReaders.JsonConverters;
using Microsoft.Extensions.Logging;

namespace FileReaders.Accounts;

public class AccountReader : IAccountReader
{
    private readonly ILogger<AccountReader> _logger;
    private readonly JsonSerializerOptions _options;

    public AccountReader(ILogger<AccountReader> logger)
    {
        _logger = logger;
        _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public async Task<IList<Account>> ReadFile(string fileName)
    {
        using var _ = InvestmentTrackerActivitySource.Instance.StartActivity($"Reading account file  {fileName}");
        _logger.LogInformation("Reading account file {fileName}", fileName);
        
        await using var stream = File.OpenRead(fileName);

        try
        {
            var items = await JsonSerializer.DeserializeAsync<IList<Account>>(stream, _options);
            return items;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to read account file {fileName}", fileName);
            throw;
        }
    }
}
