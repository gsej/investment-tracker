using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Common.Tracing;

public static class TracerProviderFactory
{
    public static TracerProvider GetTracerProvider(string serviceName, string appInsightsConnectionString)
    {
        var builder = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
            .AddSource(InvestmentTrackerActivitySource.Instance.Name)
            .AddOtlpExporter(o =>
                o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf);

        if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
        {
            builder.AddAzureMonitorTraceExporter(o =>
            {
                o.ConnectionString = appInsightsConnectionString;
            });
        }

        return builder.Build();
    }
}
