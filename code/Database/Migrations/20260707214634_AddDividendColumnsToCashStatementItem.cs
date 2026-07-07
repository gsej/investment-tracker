using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class AddDividendColumnsToCashStatementItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DividendQuantity",
                table: "CashStatementItem",
                type: "decimal(19,5)",
                precision: 19,
                scale: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StockSymbol",
                table: "CashStatementItem",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashStatementItem_StockSymbol",
                table: "CashStatementItem",
                column: "StockSymbol");

            migrationBuilder.AddForeignKey(
                name: "FK_CashStatementItem_Stock_StockSymbol",
                table: "CashStatementItem",
                column: "StockSymbol",
                principalTable: "Stock",
                principalColumn: "StockSymbol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CashStatementItem_Stock_StockSymbol",
                table: "CashStatementItem");

            migrationBuilder.DropIndex(
                name: "IX_CashStatementItem_StockSymbol",
                table: "CashStatementItem");

            migrationBuilder.DropColumn(
                name: "DividendQuantity",
                table: "CashStatementItem");

            migrationBuilder.DropColumn(
                name: "StockSymbol",
                table: "CashStatementItem");
        }
    }
}
