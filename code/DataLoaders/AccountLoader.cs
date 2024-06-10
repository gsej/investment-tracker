using Common.Tracing;
using Database.Converters;
using Database.Repositories;
using FileReaders;
using FileReaders.Accounts;
using Microsoft.Extensions.Logging;
using Account = Database.Entities.Account;

namespace DataLoaders;

public class AccountLoader
{
    private readonly ILogger<AccountLoader> _logger;
    private readonly IAccountRepository _repository;
    private readonly IReader<FileReaders.Accounts.Account> _reader;
    private readonly DateOnlyConverter _dateOnlyConverter;

    public AccountLoader(
        ILogger<AccountLoader> logger,
        IAccountRepository repository,
        IReader<FileReaders.Accounts.Account> reader,
        DateOnlyConverter dateOnlyConverter)
    {
        _logger = logger;
        _reader = reader;
        _dateOnlyConverter = dateOnlyConverter;
        _repository = repository;
    }

    public AccountLoader(IReader<FileReaders.Accounts.Account> reader, DateOnlyConverter dateOnlyConverter)
    {
        _reader = reader;
        _dateOnlyConverter = dateOnlyConverter;
    }

    public async Task LoadFile(string fileName)
    {
        using (InvestmentTrackerActivitySource.Instance.StartActivity($"File: {fileName}"))
        using (_logger.BeginScope(new Dictionary<string, string> { ["File"] = fileName, ["Contents"] = "Accounts" }))
        {
            _logger.LogInformation("Loading accounts from {fileName}", fileName);

            var accounts = (await _reader.Read(fileName)).ToList();

            foreach (var accountDto in accounts)
            {
                _logger.LogInformation("beginning to process account {accountDto}", accountDto);

                var account = new Account(accountDto.AccountCode, (DateOnly?)_dateOnlyConverter.ConvertFromProvider(accountDto.OpeningDate));

                _repository.Add(account);
            }

            await _repository.SaveChangesAsync();
        }
    }
}
