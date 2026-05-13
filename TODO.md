# TODO

## AccountValueHistoryQueryHandler2

`AccountValueHistoryQueryHandler2` uses the pre-calculated `AccountHistoricalValue` table and supports multiple accounts, but has several gaps compared to the original `AccountValueHistoryQueryHandler`.

### Other gaps

**Loop breaks on missing dates**  
If there is a gap in the pre-calculated data (a date with no rows), the loop breaks rather than skipping or continuing. The old handler always walked every calendar day from the account opening date to the query date. Decide whether a gap should break, skip, or be treated as an error.

## TENTATIVE: drop the pre-calculation path entirely

Now that the live `AccountValueHistoryQueryHandler` is fast enough (single date-walk with running state + per-stock price cursors), the `AccountValueHistoryQueryHandler2` / `AccountHistoricalValue` table / `HistoryCalculatorConsole` pipeline may be redundant. Keeping it costs:

- A second handler that must stay equivalent to the live path
- A pre-calc table that goes stale between `HistoryCalculatorConsole` runs (already caused real test failures)
- A manual refresh step in the data-loading workflow

To drop it cleanly:

1. Add multi-account support to the live handler. Two options:
   - Simple: run the per-account handler N times, aggregate results day-by-day.
   - Optimal: pre-fetch cash items / transactions / prices for all requested accounts, walk dates once maintaining per-account state.
2. Switch the Angular frontend from `/account/history2` to `/account/history`.
3. Delete `AccountValueHistoryQueryHandler2`, its interface, the `/account/history2` endpoint, and its tests.
4. Delete `HistoryCalculatorConsole`, the `AccountHistoricalValue` table, and add a migration to drop the table.
5. Delete `HistoryEquivalenceTests` (between new and v2) — the orig equivalence test is the meaningful one.

Defer until there's a reason to revisit.

