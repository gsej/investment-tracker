namespace Common;

public class AccountType : Enumeration
{
    public static readonly AccountType Isa = new AccountType(1, "ISA");
    public static readonly AccountType Sipp = new AccountType(2, "SIPP");
    
    
    public AccountType() { }
    private AccountType(int value, string displayName) : base(value, displayName) { }
}
