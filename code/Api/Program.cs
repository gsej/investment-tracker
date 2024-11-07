using Api.Controllers;
using Api.Correlation;
using Api.QueryHandlers.Account;
using Api.QueryHandlers.Fetchers;
using Api.QueryHandlers.History;
using Api.QueryHandlers.Portfolio;
using Api.QueryHandlers.Quality;
using Common.Tracing;
using Database;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Api;

public static class Program
{
    public static void Main(params string[]  args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var configurationRoot = builder.Configuration
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();
        
        var configuration = configurationRoot.GetRequiredSection(nameof(ApiConfiguration)).Get<ApiConfiguration>();
        
        builder.Services.AddMemoryCache();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins",
                policy =>
                {
                    policy.AllowAnyOrigin();
                    policy.AllowAnyMethod();
                    policy.AllowAnyHeader();
                });
        });

        builder.Services.AddControllers();

        builder.Services.AddOpenTelemetry().WithTracing(builder =>
        {
            builder
                // Configure ASP.NET Core Instrumentation
                .AddAspNetCoreInstrumentation()
                // Configure OpenTelemetry Protocol (OTLP) Exporter

                .AddOtlpExporter()
                .ConfigureResource(r => r.AddService("InvestmentTracker"));

        });
        
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(s =>
        {
            s.SchemaFilter<ExampleSchemaFilter>();
            s.CustomSchemaIds(x => x.FullName);
        });

        builder.Services.AddDbContext<InvestmentsDbContext>(
            opts => opts.UseSqlServer(configuration.SqlConnectionString)
        );

        builder.Services.AddScoped<IAccountFetcher, AccountFetcher>();
        
        builder.Services.AddScoped<IStockFetcher, StockFetcher>();
        builder.Services.AddScoped<IStockPriceFetcher, StockPriceFetcher>();
        builder.Services.AddScoped<ICashStatementItemFetcher, CashStatementItemFetcher>();
        builder.Services.AddScoped<IStockTransactionFetcher, StockTransactionFetcher>();

        builder.Services.AddScoped<IAccountPortfolioQueryHandler, AccountPortfolioQueryHandler>();
        builder.Services.AddScoped<IAccountQueryHandler, AccountQueryHandler>();
        builder.Services.AddScoped<IRecordedTotalValueQueryHandler, RecordedTotalValueQueryHandler>();
        builder.Services.AddScoped<IAccountValueHistoryQueryHandler, AccountValueHistoryQueryHandler>();
        builder.Services.AddScoped<IAnnualPerformanceQueryHandler, AnnualPerformanceQueryHandler>();

        builder.Services.AddScoped<IQualityQueryHandler, QualityQueryHandler>();
        builder.Services.AddScoped<ICorrelationIdGenerator, CorrelationIdGenerator>();
        
        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        app.UseCors("AllowAllOrigins");

        app.MapGet("/", http =>
        {
            http.Response.Redirect("/swagger/index.html", false);
            return Task.CompletedTask;
        });

        app.MapControllers();

        app.UseMiddleware<CorrelationIdMiddleware>();
        
        using var traceProvider = TracerProviderFactory.GetTracerProvider("Api", configuration.AppInsightsConnectionString);

        app.Run();
        
    }
}
