namespace Api.QueryHandlers.Common;

// Not sure why these are nullable
public record UnitAccount(DateOnly Date, decimal? NumberOfUnits, decimal? ValueInGbpPerUnit);
