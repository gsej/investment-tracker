using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class AddDividendTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dividend",
                columns: table => new
                {
                    DividendId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockSymbol = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Date = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(19,5)", precision: 19, scale: 5, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, collation: "SQL_Latin1_General_CP1_CS_AS"),
                    Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginalCurrency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, collation: "SQL_Latin1_General_CP1_CS_AS"),
                    ExchangeRateAgeInDays = table.Column<int>(type: "int", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dividend", x => x.DividendId);
                    table.ForeignKey(
                        name: "FK_Dividend_Stock_StockSymbol",
                        column: x => x.StockSymbol,
                        principalTable: "Stock",
                        principalColumn: "StockSymbol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dividend_StockSymbol",
                table: "Dividend",
                column: "StockSymbol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dividend");
        }
    }
}
