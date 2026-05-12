# TODO

## AccountValueHistoryQueryHandler2

`AccountValueHistoryQueryHandler2` uses the pre-calculated `AccountHistoricalValue` table and supports multiple accounts, but has several gaps compared to the original `AccountValueHistoryQueryHandler`.

### Missing calculations

**DifferenceRatio** (hardcoded to `0`)  
Should be `DifferenceToPreviousDay / previousDayTotal`, guarded by `previousDayTotal != 0`. The `previousDayTotal` variable is declared but never assigned or used, so this calculation is entirely absent.

**DifferenceToPreviousDay** (summed from DB, likely wrong for multi-account)  
The DB stores a per-account `DifferenceToPreviousDay` (calculated relative to that account's previous day). Summing those values gives the sum of per-account differences, which is not the same as the combined portfolio's difference to the previous day. It should be recalculated here as `combinedValueInGbp - combinedNetInflows - previousDayCombinedTotal`, the same way the old handler computed it — tracking `previousDayTotal` across iterations.

### Other gaps

**`previousDayTotal` is dead code**  
Declared on line 32 but never updated within the loop and never read. It needs to be set to `historicalValue.ValueInGbp` at the end of each iteration (as the old handler does) once `DifferenceToPreviousDay` and `DifferenceRatio` are properly implemented.

**Loop breaks on missing dates**  
If there is a gap in the pre-calculated data (a date with no rows), the loop breaks rather than skipping or continuing. The old handler always walked every calendar day from the account opening date to the query date. Decide whether a gap should break, skip, or be treated as an error.

