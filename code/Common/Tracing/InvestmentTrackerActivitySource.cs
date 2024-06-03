using System.Diagnostics;

namespace Common.Tracing;

public class InvestmentTrackerActivitySource
{
    public static ActivitySource Instance { get; } = new(
        "InvestmentTracker.ActivitySource"
    );
}
