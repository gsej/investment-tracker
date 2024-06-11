using System.Text.Json;
using Common;
using Database.Entities;

namespace DataLoaders.CashStatementItemEnrichers;

public class CashStatementItemTypeEnricher : ICashStatementItemEnricher
{
    public void Enrich(CashStatementItem cashStatementItem)
    {
        if (cashStatementItem.Description.StartsWith("Purchase"))
        {
            if (RegularInvestmentDayCalculator.IsRegularInvestmentDay(cashStatementItem.Date))
            {
                cashStatementItem.CashStatementItemType = CashStatementItemTypes.RegularInvestmentPurchase;
            }
            else
            {
                cashStatementItem.CashStatementItemType = CashStatementItemTypes.Purchase;
            }
        }
        else if (cashStatementItem.Description.StartsWith("Sale"))
        {
            cashStatementItem.CashStatementItemType = CashStatementItemTypes.Sale;
        }
        else if (cashStatementItem.Description.StartsWith("Div") ||
                 cashStatementItem.Description.StartsWith("DIVIDEND"))
        {
            cashStatementItem.CashStatementItemType = CashStatementItemTypes.Dividend;
        }
        else if (cashStatementItem.Description.StartsWith("Contribution") ||
                 cashStatementItem.Description.Contains("Deposit", StringComparison.InvariantCultureIgnoreCase) ||
                 cashStatementItem.Description.Contains("Subscription", StringComparison.InvariantCultureIgnoreCase) ||
                 cashStatementItem.Description.Equals("Direct debit payment") ||
                 cashStatementItem.Description.Equals("Debit card contribution") ||
                 cashStatementItem.Description.Equals("Debit card payment", StringComparison.InvariantCultureIgnoreCase) ||
                 cashStatementItem.Description.Equals("Transfer From SIPP Cash Account", StringComparison.InvariantCultureIgnoreCase))
        {
            cashStatementItem.CashStatementItemType = CashStatementItemTypes.Contribution;
        }
        else if (cashStatementItem.Description.Equals("Tax Relief"))
        {
            cashStatementItem.CashStatementItemType = CashStatementItemTypes.TaxRelief;
        }
        else if (cashStatementItem.Description.Contains("Charge", StringComparison.InvariantCultureIgnoreCase))
        {
            cashStatementItem.CashStatementItemType = CashStatementItemTypes.Charge;
        }
        else if (cashStatementItem.Description.Contains("Interest", StringComparison.InvariantCultureIgnoreCase))
        {
            cashStatementItem.CashStatementItemType = CashStatementItemTypes.Interest;
        }
        else if (cashStatementItem.Description.Contains("Transfer Value"))
        {
            cashStatementItem.CashStatementItemType = CashStatementItemTypes.TransferIn;
        }
        else if (cashStatementItem.Description.Contains("Cash Withdrawal"))
        {
            cashStatementItem.CashStatementItemType = CashStatementItemTypes.Withdrawal;
        }
        else if (cashStatementItem.Description.Contains("Balance", StringComparison.InvariantCultureIgnoreCase))
        {
            cashStatementItem.CashStatementItemType = CashStatementItemTypes.Balance;
        }
        else if (
            cashStatementItem.Description.Equals("National Grid  cash fractions", StringComparison.InvariantCultureIgnoreCase) ||
            cashStatementItem.Description.Equals("CAPITA18            Rights issue", StringComparison.InvariantCultureIgnoreCase) ||
            cashStatementItem.Description.Equals("PROV CAP RET SHS    Return of Capital", StringComparison.InvariantCultureIgnoreCase) ||
            cashStatementItem.Description.Equals("Microfocus Cash Fractions", StringComparison.InvariantCultureIgnoreCase))
        {
            // this is an odd one
            cashStatementItem.CashStatementItemType = CashStatementItemTypes.OtherReceipt;
        }
        else if (cashStatementItem.Description.StartsWith("Equalisation", StringComparison.InvariantCultureIgnoreCase))
        {
            cashStatementItem.CashStatementItemType = CashStatementItemTypes.Dividend;
            // only seems to apply to money market fund
        }
        else
        {
            var json = JsonSerializer.Serialize(cashStatementItem, new JsonSerializerOptions { WriteIndented = true });
            throw new Exception($"couldn't identify type for description {json}");
        }
    }
}
