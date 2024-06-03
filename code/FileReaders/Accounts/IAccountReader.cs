namespace FileReaders.Accounts;

public interface IAccountReader
{
    Task<IList<Account>> ReadFile(string fileName);
}

