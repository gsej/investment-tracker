# Dividend Stock Matching Plan

## Goal

Implement total return calculations for stock comparisons. Currently, stock comparison uses raw price only (notional quantity = 1, value = price). Total return requires reinvesting dividends: each time a dividend is paid, the notional unit count increases by `(price + dividend_per_unit) / price`.

## The naming mismatch problem

Dividend records in `cashstatement_items.json` use a completely different naming scheme from stock transactions. For the same stock (VUCP.L):

| Source | Description |
|---|---|
| `transactions.json` | `"Vanguard USD Corp Bd UCITS ETF GBP"` |
| `cashstatement_items.json` dividend | `"VANGUARD FUNDS PLC  VANGUARD USD CORP BOND UCITS ETF USD DIS"` |
| `stocks.json` description | `"Vanguard USD Corp Bd UCITS ETF GBP"` |

The dividend description format is: `"Dividend {units}   {company name}  {fund name}"`. The fund/company name portion is a third naming variant not covered by existing aliases.

### Real-data format variations (from `youinvest-csv-files`)

There are several format variants beyond the basic case:

| Variant | Example | Notes |
|---|---|---|
| Standard | `"Dividend 19   VANGUARD FUNDS PLC  VANGUARD USD CORP BOND UCITS ETF USD DIS"` | Integer units, two spaces between company and fund |
| Money market | `"Dividend Grp 1 12978.22890 VANGUARD INVESTMENTS MONEY MKT FDS  VANGUARD STG SHT T..."` | `Grp N` before the quantity; quantity is fractional; description truncated |
| Equalisation | `"Equalisation VANGUARD INVESTMENTS MONEY MKT FDS  VANGUARD STG SHT TR MNY MKT INV"` | No quantity at all; already classified as `Dividend` type |
| Bare distribution | `"Distribution"` | No stock info; cannot be linked to a stock |

Additional complications in the real data:
- **Truncation**: Some fund names are cut off mid-word (e.g. `"LYXOR CORE UK GOVNMENT BOND (DR) U"`), so alias matching must use `StartsWith` or a contains match rather than exact equality for some entries.
- **Company name changes**: The same stock appears under different company names over time (e.g. `"DB X-TRACKERS  FTSE ALL-SHARE UCITS ETF (DR)"` and `"XTRACKERS  FTSE ALL-SHARE UCITS ETF (DR)"`). Both variants need aliases.
- **Inconsistent suffixes**: The same fund appears with and without currency/dist suffix (e.g. `"ISHARES  FTSE 250 UCITS ETF"` vs `"ISHARES  FTSE 250 UCITS ETF GBP DIST"`).
- **Gilts**: Individual gilts appear with their own descriptions (e.g. `"HM TREASURY  GILT 4.125% (29/01/27)"`). These are not ETFs and may need separate handling.

---

## Phase 1 — Link dividends to stocks (data pipeline)

Follows the same pattern as `StockTransactionLoader`, which matches transaction descriptions against `Stock.Description` and `Stock.Aliases`.

### Steps

1. **New EF migration**: Add two nullable columns to the `CashStatementItem` entity:
   - `StockSymbol` (FK to `Stock`) — populated when a match is found
   - `DividendQuantity` (decimal) — the number extracted from the dividend description

   The `DividendQuantity` column is needed because this number represents the holding size at the **ex-dividend date**, which may differ from the holding at the payment date (shares could have been bought or sold between ex-dividend and payment). Storing it separately allows `dividend_per_unit = receipt_amount_gbp / DividendQuantity` to be calculated accurately at query time.

2. **Add dividend aliases to `stocks.json`**: For each distributing stock, add the text after the quantity prefix as a new alias entry. This is a manual step — requires going through the dividend cashstatement_items and mapping each description to the correct stock. Multiple aliases per stock will often be needed (due to name changes and truncation variants). For example, VUCP.L would need:
   ```json
   { "Description": "VANGUARD FUNDS PLC  VANGUARD USD CORP BOND UCITS ETF USD DIS" }
   ```
   **Scope**: Only stocks with `Benchmark = true` need to be matched for total return calculations (see Historical Data section below). Aliases for non-benchmark stocks can be added later if needed.

3. **`CashStatementItemDividendStockEnricher`**: New enricher — implemented simply and naively first, then refined based on observed mismatches. It should:
   - Strip the leading quantity prefix and extract the quantity. The prefix patterns are:
     - `"Dividend {integer}   "` — standard; quantity is the integer
     - `"Dividend Grp {integer} {decimal} "` — money market; quantity is the decimal
     - `"Equalisation "` — no quantity; set `DividendQuantity = null`
   - Match the remaining description text against `Stock.Description` and `Stock.Aliases` (case-insensitive, same logic as `StockTransactionLoader:60-63`)
   - Set `CashStatementItem.StockSymbol` and `CashStatementItem.DividendQuantity`
   - **Log a warning (not an error)** for unmatched rows — the initial run is expected to have mismatches, and the output is used to drive iterative alias additions

4. **`CashStatementItemLoader`**: Call the enricher for all items of type `Dividend`.

Note: The bare `"Distribution"` entry has no stock information and cannot be linked. Skip it silently.

---

## Phase 2 — Total return calculation (API)

Changes are localised to `StockHistoryQueryHandler.cs`.

Currently line 51 hardcodes `quantity = 1`:
```csharp
unitValues.Add(new UnitAccount(date, 1, currentPrice));
```

### Steps

1. Fetch all `CashStatementItem` rows where `StockSymbol = request.StockSymbol` and `CashStatementItemType = "Dividend"`.

2. Compute `dividend_per_unit` for each dividend date:
   ```
   dividend_per_unit = receipt_amount_gbp / DividendQuantity
   ```
   Deduplicate by date (the same dividend appears once per account holding the stock; the per-unit amount should be identical across accounts, but take the average or first to be safe).

3. Walk the price history, tracking accumulated units (starting at 1). On each dividend date:
   ```
   accumulated_units *= (current_price + dividend_per_unit) / current_price
   ```

4. Use `accumulated_units` in place of the hardcoded `1` when building `UnitAccount` objects.

---

## The historical data problem

Stock price downloads cover periods before the stocks were held in any account. For those periods, there are no dividend records in `cashstatement_items` — so a total return index cannot be computed from account data alone.

The scope of this problem is **smaller than it first appears**: total return is only needed for stocks with `Benchmark = true`, not all stocks in the database. The benchmark set is small and curated, so the number of stocks requiring historical dividend data is limited.

Options:

**A. Accept the limitation (simplest)**: Total return is calculated only from the date of the first dividend record for a stock. Before that date, show price return only. This is honest but potentially misleading for long lookbacks.

**B. Fetch historical dividend data from an external source**: Services like Yahoo Finance, Tiingo, and Quandl provide dividend history by stock ticker. This would require a new data pipeline step to download and store per-stock dividend history, separate from the account cashstatement_items.

**C. Hybrid display**: Show a clear marker on the chart at the point where total return data begins, so the user understands the pre-holding portion is price-only.

For now, Phase 1 + 2 are implemented using own account dividend data, with Option A or C as the limitation handling. Option B is deferred.
