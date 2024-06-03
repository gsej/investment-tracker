namespace Api.QueryHandlers.Account;

public interface IAccountQueryHandler
{
    Task<IList<Account>> Handle(AccountRequest _);
}
