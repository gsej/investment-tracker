using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateInitialDataModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Account",
                columns: table => new
                {
                    AccountCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OpeningDate = table.Column<string>(type: "nvarchar(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account", x => x.AccountCode);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeRate",
                columns: table => new
                {
                    ExchangeRateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newid()"),
                    BaseCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, collation: "SQL_Latin1_General_CP1_CS_AS"),
                    AlternateCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, collation: "SQL_Latin1_General_CP1_CS_AS"),
                    Date = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(19,5)", precision: 19, scale: 5, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRate", x => x.ExchangeRateId);
                });

            migrationBuilder.CreateTable(
                name: "Stock",
                columns: table => new
                {
                    StockSymbol = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Isin = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StockType = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    SubjectToStampDuty = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stock", x => x.StockSymbol);
                });

            migrationBuilder.CreateTable(
                name: "StockPrice",
                columns: table => new
                {
                    StockPriceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockSymbol = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Date = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(19,5)", precision: 19, scale: 5, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, collation: "SQL_Latin1_General_CP1_CS_AS"),
                    Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockPrice", x => x.StockPriceId);
                });

            migrationBuilder.CreateTable(
                name: "CashStatementItem",
                columns: table => new
                {
                    CashStatementItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newid()"),
                    AccountCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Date = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReceiptAmountGbp = table.Column<decimal>(type: "decimal(19,5)", precision: 19, scale: 5, nullable: false),
                    PaymentAmountGbp = table.Column<decimal>(type: "decimal(19,5)", precision: 19, scale: 5, nullable: false),
                    CashStatementItemType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashStatementItem", x => x.CashStatementItemId);
                    table.ForeignKey(
                        name: "FK_CashStatementItem_Account_AccountCode",
                        column: x => x.AccountCode,
                        principalTable: "Account",
                        principalColumn: "AccountCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecordedTotalValue",
                columns: table => new
                {
                    RecordedTotalValueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newid()"),
                    AccountCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Date = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TotalValueInGbp = table.Column<decimal>(type: "decimal(19,5)", precision: 19, scale: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordedTotalValue", x => x.RecordedTotalValueId);
                    table.ForeignKey(
                        name: "FK_RecordedTotalValue_Account_AccountCode",
                        column: x => x.AccountCode,
                        principalTable: "Account",
                        principalColumn: "AccountCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlternativeSymbol",
                columns: table => new
                {
                    Alternative = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    StockSymbol = table.Column<string>(type: "nvarchar(15)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlternativeSymbol", x => x.Alternative);
                    table.ForeignKey(
                        name: "FK_AlternativeSymbol_Stock_StockSymbol",
                        column: x => x.StockSymbol,
                        principalTable: "Stock",
                        principalColumn: "StockSymbol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockAlias",
                columns: table => new
                {
                    Description = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StockSymbol = table.Column<string>(type: "nvarchar(15)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockAlias", x => x.Description);
                    table.ForeignKey(
                        name: "FK_StockAlias_Stock_StockSymbol",
                        column: x => x.StockSymbol,
                        principalTable: "Stock",
                        principalColumn: "StockSymbol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockTransaction",
                columns: table => new
                {
                    StockTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newid()"),
                    AccountCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StockSymbol = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Date = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Transaction = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(19,5)", precision: 19, scale: 5, nullable: false),
                    AmountGbp = table.Column<decimal>(type: "decimal(19,5)", precision: 19, scale: 5, nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(19,5)", precision: 19, scale: 5, nullable: false),
                    StampDuty = table.Column<decimal>(type: "decimal(19,5)", precision: 19, scale: 5, nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransaction", x => x.StockTransactionId);
                    table.ForeignKey(
                        name: "FK_StockTransaction_Account_AccountCode",
                        column: x => x.AccountCode,
                        principalTable: "Account",
                        principalColumn: "AccountCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockTransaction_Stock_StockSymbol",
                        column: x => x.StockSymbol,
                        principalTable: "Stock",
                        principalColumn: "StockSymbol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlternativeSymbol_StockSymbol",
                table: "AlternativeSymbol",
                column: "StockSymbol");

            migrationBuilder.CreateIndex(
                name: "IX_CashStatementItem_AccountCode",
                table: "CashStatementItem",
                column: "AccountCode");

            migrationBuilder.CreateIndex(
                name: "IX_RecordedTotalValue_AccountCode",
                table: "RecordedTotalValue",
                column: "AccountCode");

            migrationBuilder.CreateIndex(
                name: "IX_StockAlias_StockSymbol",
                table: "StockAlias",
                column: "StockSymbol");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransaction_AccountCode",
                table: "StockTransaction",
                column: "AccountCode");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransaction_StockSymbol",
                table: "StockTransaction",
                column: "StockSymbol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlternativeSymbol");

            migrationBuilder.DropTable(
                name: "CashStatementItem");

            migrationBuilder.DropTable(
                name: "ExchangeRate");

            migrationBuilder.DropTable(
                name: "RecordedTotalValue");

            migrationBuilder.DropTable(
                name: "StockAlias");

            migrationBuilder.DropTable(
                name: "StockPrice");

            migrationBuilder.DropTable(
                name: "StockTransaction");

            migrationBuilder.DropTable(
                name: "Account");

            migrationBuilder.DropTable(
                name: "Stock");
        }
    }
}
