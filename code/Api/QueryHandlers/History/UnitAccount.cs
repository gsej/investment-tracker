namespace Api.QueryHandlers.History;

public record UnitAccount(DateOnly Date, decimal? NumberOfUnits, decimal? ValueInGbpPerUnit);
