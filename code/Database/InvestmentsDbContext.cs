using System;
using Database.Converters;
using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Database;

public class InvestmentsDbContextFactory : IDesignTimeDbContextFactory<InvestmentsDbContext>
{
    public InvestmentsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InvestmentsDbContext>();
        optionsBuilder.UseSqlServer("Server=localhost;Initial Catalog=investments;Persist Security Info=False;User ID=sa;Password=Password123!;MultipleActiveResultSets=False;Encrypt=True;Trust Server Certificate=True;Connection Timeout=30;");
        
        return new InvestmentsDbContext(optionsBuilder.Options);
    }
}

public class InvestmentsDbContext : DbContext
{
    public InvestmentsDbContext(  DbContextOptions<InvestmentsDbContext> options) : base(options)
    {
    }
    
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Stock> Stocks { get; set; }
    public DbSet<CashStatementItem> CashStatementItems { get; set; }
    public DbSet<StockTransaction> StockTransactions { get; set; }
    public DbSet<StockPrice> StockPrices { get; set; }
    public DbSet<RecordedTotalValue> RecordedTotalValues { get; set; }
    public DbSet<ExchangeRate> ExchangeRates { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CashStatementItem>()
             .Property(s => s.CashStatementItemId).HasDefaultValueSql("newid()");

        modelBuilder.Entity<StockTransaction>()
            .Property(s => s.StockTransactionId).HasDefaultValueSql("newid()");
        
        modelBuilder.Entity<RecordedTotalValue>()
            .Property(s => s.RecordedTotalValueId).HasDefaultValueSql("newid()");
        
        modelBuilder.Entity<ExchangeRate>()
            .Property(e => e.ExchangeRateId).HasDefaultValueSql("newid()");
        
        // Make the currency column case sensitive
        modelBuilder.Entity<StockPrice>().Property(a => a.Currency)
            .UseCollation("SQL_Latin1_General_CP1_CS_AS");
        
        modelBuilder.Entity<ExchangeRate>().Property(a => a.BaseCurrency)
            .UseCollation("SQL_Latin1_General_CP1_CS_AS");
        
        modelBuilder.Entity<ExchangeRate>().Property(a => a.AlternateCurrency)
            .UseCollation("SQL_Latin1_General_CP1_CS_AS");
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.Properties<DateOnly>()
            .HaveConversion<DateOnlyConverter>()
            .HaveColumnType("nvarchar(10)");

        builder.Properties<DateOnly?>()
            .HaveConversion<DateOnlyConverter>()
            .HaveColumnType("nvarchar(10)");

    }
}

