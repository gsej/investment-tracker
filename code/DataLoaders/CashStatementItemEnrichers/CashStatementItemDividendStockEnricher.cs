using System.Text.RegularExpressions;
using Common;
using Database;
using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataLoaders.CashStatementItemEnrichers;

public class CashStatementItemDividendStockEnricher
{
    private readonly InvestmentsDbContext _context;
    private readonly ILogger<CashStatementItemDividendStockEnricher> _logger;
    private List<Stock> _stocks;

    // "Dividend Grp 1 12978.22890 COMPANY  FUND" — must be checked before the standard pattern
    private static readonly Regex GroupDividendRegex =
        new(@"^Dividend Grp \d+\s+([\d.]+)\s+(.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // "Dividend 19   COMPANY  FUND" or "Dividend 16047.72 COMPANY  FUND"
    private static readonly Regex StandardDividendRegex =
        new(@"^Dividend\s+([\d.]+)\s+(.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // "Div 19   COMPANY FUND" — abbreviated form used in older AjBell data
    private static readonly Regex ShortDividendRegex =
        new(@"^Div\s+([\d.]+)\s+(.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // "Equalisation COMPANY  FUND"
    private static readonly Regex EqualisationRegex =
        new(@"^Equalisation\s+(.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public CashStatementItemDividendStockEnricher(
        InvestmentsDbContext context,
        ILogger<CashStatementItemDividendStockEnricher> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Enrich(CashStatementItem cashStatementItem)
    {
        _stocks ??= await _context.Stocks
            .Include(s => s.Aliases)
            .AsSingleQuery()
            .ToListAsync();

        var description = cashStatementItem.Description;

        if (description.Equals("Distribution", StringComparison.InvariantCultureIgnoreCase))
        {
            if (cashStatementItem.AccountCode == "GSEJ-RTGS")
            {
                cashStatementItem.StockSymbol = "0P00018XAR.L";
            }
            return;
        }

        string stockDescription = null;
        decimal? quantity = null;

        var groupMatch = GroupDividendRegex.Match(description);
        if (groupMatch.Success)
        {
            quantity = decimal.Parse(groupMatch.Groups[1].Value);
            stockDescription = groupMatch.Groups[2].Value.Trim();
        }
        else
        {
            var standardMatch = StandardDividendRegex.Match(description);
            if (standardMatch.Success)
            {
                quantity = decimal.Parse(standardMatch.Groups[1].Value);
                stockDescription = standardMatch.Groups[2].Value.Trim();
            }
            else
            {
                var shortMatch = ShortDividendRegex.Match(description);
                if (shortMatch.Success)
                {
                    quantity = decimal.Parse(shortMatch.Groups[1].Value);
                    stockDescription = shortMatch.Groups[2].Value.Trim();
                }
                else
                {
                    var equalisationMatch = EqualisationRegex.Match(description);
                    if (equalisationMatch.Success)
                    {
                        stockDescription = equalisationMatch.Groups[1].Value.Trim();
                    }
                }
            }
        }

        if (stockDescription == null)
        {
            _logger.LogWarning("Could not parse dividend description: {Description}", description);
            return;
        }

        cashStatementItem.DividendQuantity = quantity;

        var matchingStock = _stocks.SingleOrDefault(s =>
            s.Description.Equals(stockDescription, StringComparison.InvariantCultureIgnoreCase) ||
            s.Aliases.Any(a => a.Description.Equals(stockDescription, StringComparison.InvariantCultureIgnoreCase)));

        if (matchingStock == null)
        {
            _logger.LogWarning("No stock found for dividend description: {StockDescription}", stockDescription);
            return;
        }

        cashStatementItem.StockSymbol = matchingStock.StockSymbol;
    }
}
