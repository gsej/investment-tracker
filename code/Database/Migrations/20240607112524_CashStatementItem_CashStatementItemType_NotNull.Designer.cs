﻿// <auto-generated />
using System;
using Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Database.Migrations
{
    [DbContext(typeof(InvestmentsDbContext))]
    [Migration("20240607112524_CashStatementItem_CashStatementItemType_NotNull")]
    partial class CashStatementItem_CashStatementItemType_NotNull
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Database.Entities.Account", b =>
                {
                    b.Property<string>("AccountCode")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("OpeningDate")
                        .HasColumnType("nvarchar(10)");

                    b.HasKey("AccountCode");

                    b.ToTable("Account");
                });

            modelBuilder.Entity("Database.Entities.AlternativeSymbol", b =>
                {
                    b.Property<string>("Alternative")
                        .HasMaxLength(15)
                        .HasColumnType("nvarchar(15)");

                    b.Property<string>("StockSymbol")
                        .IsRequired()
                        .HasColumnType("nvarchar(15)");

                    b.HasKey("Alternative");

                    b.HasIndex("StockSymbol");

                    b.ToTable("AlternativeSymbol");
                });

            modelBuilder.Entity("Database.Entities.CashStatementItem", b =>
                {
                    b.Property<Guid>("CashStatementItemId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newid()");

                    b.Property<string>("AccountCode")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("CashStatementItemType")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Date")
                        .IsRequired()
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<decimal>("PaymentAmountGbp")
                        .HasPrecision(19, 5)
                        .HasColumnType("decimal(19,5)");

                    b.Property<decimal>("ReceiptAmountGbp")
                        .HasPrecision(19, 5)
                        .HasColumnType("decimal(19,5)");

                    b.HasKey("CashStatementItemId");

                    b.HasIndex("AccountCode");

                    b.ToTable("CashStatementItem");
                });

            modelBuilder.Entity("Database.Entities.ExchangeRate", b =>
                {
                    b.Property<Guid>("ExchangeRateId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newid()");

                    b.Property<string>("AlternateCurrency")
                        .IsRequired()
                        .HasMaxLength(3)
                        .HasColumnType("nvarchar(3)")
                        .UseCollation("SQL_Latin1_General_CP1_CS_AS");

                    b.Property<string>("BaseCurrency")
                        .IsRequired()
                        .HasMaxLength(3)
                        .HasColumnType("nvarchar(3)")
                        .UseCollation("SQL_Latin1_General_CP1_CS_AS");

                    b.Property<string>("Date")
                        .IsRequired()
                        .HasColumnType("nvarchar(10)");

                    b.Property<decimal>("Rate")
                        .HasPrecision(19, 5)
                        .HasColumnType("decimal(19,5)");

                    b.Property<string>("Source")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("ExchangeRateId");

                    b.ToTable("ExchangeRate");
                });

            modelBuilder.Entity("Database.Entities.RecordedTotalValue", b =>
                {
                    b.Property<Guid>("RecordedTotalValueId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newid()");

                    b.Property<string>("AccountCode")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("Date")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<decimal>("TotalValueInGbp")
                        .HasPrecision(19, 5)
                        .HasColumnType("decimal(19,5)");

                    b.HasKey("RecordedTotalValueId");

                    b.HasIndex("AccountCode");

                    b.ToTable("RecordedTotalValue");
                });

            modelBuilder.Entity("Database.Entities.Stock", b =>
                {
                    b.Property<string>("StockSymbol")
                        .HasMaxLength(15)
                        .HasColumnType("nvarchar(15)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Isin")
                        .HasMaxLength(12)
                        .HasColumnType("nvarchar(12)");

                    b.Property<string>("Notes")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("StockType")
                        .HasMaxLength(15)
                        .HasColumnType("nvarchar(15)");

                    b.Property<bool>("SubjectToStampDuty")
                        .HasColumnType("bit");

                    b.HasKey("StockSymbol");

                    b.ToTable("Stock");
                });

            modelBuilder.Entity("Database.Entities.StockAlias", b =>
                {
                    b.Property<string>("Description")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("StockSymbol")
                        .IsRequired()
                        .HasColumnType("nvarchar(15)");

                    b.HasKey("Description");

                    b.HasIndex("StockSymbol");

                    b.ToTable("StockAlias");
                });

            modelBuilder.Entity("Database.Entities.StockPrice", b =>
                {
                    b.Property<Guid>("StockPriceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Comment")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)")
                        .UseCollation("SQL_Latin1_General_CP1_CS_AS");

                    b.Property<string>("Date")
                        .IsRequired()
                        .HasColumnType("nvarchar(10)");

                    b.Property<int?>("ExchangeRateAgeInDays")
                        .HasColumnType("int");

                    b.Property<string>("OriginalCurrency")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)")
                        .UseCollation("SQL_Latin1_General_CP1_CS_AS");

                    b.Property<decimal>("Price")
                        .HasPrecision(19, 5)
                        .HasColumnType("decimal(19,5)");

                    b.Property<string>("Source")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("StockSymbol")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("nvarchar(15)");

                    b.HasKey("StockPriceId");

                    b.HasIndex("StockSymbol");

                    b.ToTable("StockPrice");
                });

            modelBuilder.Entity("Database.Entities.StockTransaction", b =>
                {
                    b.Property<Guid>("StockTransactionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newid()");

                    b.Property<string>("AccountCode")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<decimal>("AmountGbp")
                        .HasPrecision(19, 5)
                        .HasColumnType("decimal(19,5)");

                    b.Property<string>("Date")
                        .IsRequired()
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<decimal>("Fee")
                        .HasPrecision(19, 5)
                        .HasColumnType("decimal(19,5)");

                    b.Property<decimal>("Quantity")
                        .HasPrecision(19, 5)
                        .HasColumnType("decimal(19,5)");

                    b.Property<string>("Reference")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<decimal>("StampDuty")
                        .HasPrecision(19, 5)
                        .HasColumnType("decimal(19,5)");

                    b.Property<string>("StockSymbol")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("nvarchar(15)");

                    b.Property<string>("Transaction")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("TransactionType")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("StockTransactionId");

                    b.HasIndex("AccountCode");

                    b.HasIndex("StockSymbol");

                    b.ToTable("StockTransaction");
                });

            modelBuilder.Entity("Database.Entities.AlternativeSymbol", b =>
                {
                    b.HasOne("Database.Entities.Stock", "Stock")
                        .WithMany("AlternativeSymbols")
                        .HasForeignKey("StockSymbol")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Stock");
                });

            modelBuilder.Entity("Database.Entities.CashStatementItem", b =>
                {
                    b.HasOne("Database.Entities.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountCode")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });

            modelBuilder.Entity("Database.Entities.RecordedTotalValue", b =>
                {
                    b.HasOne("Database.Entities.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountCode")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });

            modelBuilder.Entity("Database.Entities.StockAlias", b =>
                {
                    b.HasOne("Database.Entities.Stock", "Stock")
                        .WithMany("Aliases")
                        .HasForeignKey("StockSymbol")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Stock");
                });

            modelBuilder.Entity("Database.Entities.StockPrice", b =>
                {
                    b.HasOne("Database.Entities.Stock", "Stock")
                        .WithMany()
                        .HasForeignKey("StockSymbol")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Stock");
                });

            modelBuilder.Entity("Database.Entities.StockTransaction", b =>
                {
                    b.HasOne("Database.Entities.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountCode")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Database.Entities.Stock", "Stock")
                        .WithMany()
                        .HasForeignKey("StockSymbol")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("Stock");
                });

            modelBuilder.Entity("Database.Entities.Stock", b =>
                {
                    b.Navigation("Aliases");

                    b.Navigation("AlternativeSymbols");
                });
#pragma warning restore 612, 618
        }
    }
}
