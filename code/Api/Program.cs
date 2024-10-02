using Api.Controllers;
using Api.Correlation;
using Api.QueryHandlers.Account;
using Api.QueryHandlers.History;
using Api.QueryHandlers.Quality;
using Api.QueryHandlers.Summary;
using Database;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;

namespace Api;

public static class Program
{
    public static void Main(params string[]  args)
    {

        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();
        
        builder.Services.AddMemoryCache();

        var connectionString = builder.Configuration["SqlConnectionString"];

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
                .AddOtlpExporter();
        });
        
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(s =>
        {
            s.SchemaFilter<ExampleSchemaFilter>();
            s.CustomSchemaIds(x => x.FullName);
        });

        builder.Services.AddDbContext<InvestmentsDbContext>(
            opts => opts.UseSqlServer(connectionString)
        );

        builder.Services.AddScoped<IAccountSummaryQueryHandler, AccountSummaryQueryHandler>();
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

        app.Run();
        
    }
}
