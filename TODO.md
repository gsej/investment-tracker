# TODO

## AccountValueHistoryQueryHandler2

`AccountValueHistoryQueryHandler2` uses the pre-calculated `AccountHistoricalValue` table and supports multiple accounts, but has several gaps compared to the original `AccountValueHistoryQueryHandler`.

### Other gaps

**Loop breaks on missing dates**  
If there is a gap in the pre-calculated data (a date with no rows), the loop breaks rather than skipping or continuing. The old handler always walked every calendar day from the account opening date to the query date. Decide whether a gap should break, skip, or be treated as an error.

