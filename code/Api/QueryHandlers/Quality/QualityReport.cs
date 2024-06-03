namespace Api.QueryHandlers.Quality;

public record QualityReport(IList<string> StocksWithoutAnyPrices);

// TODO: 

// prices:

// spot big gaps in stock price records which fall inside the range in which we care...


// Compare incoming stock prices with previous price and report big differences.

// Calculate price on transactions and compare with stock price on that day.

// Report big changes in account total balances from one day to the next.
