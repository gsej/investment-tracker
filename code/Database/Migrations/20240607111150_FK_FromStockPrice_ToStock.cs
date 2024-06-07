using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class FK_FromStockPrice_ToStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "StockSymbol",
                table: "StockPrice",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockPrice_StockSymbol",
                table: "StockPrice",
                column: "StockSymbol");

            migrationBuilder.AddForeignKey(
                name: "FK_StockPrice_Stock_StockSymbol",
                table: "StockPrice",
                column: "StockSymbol",
                principalTable: "Stock",
                principalColumn: "StockSymbol",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockPrice_Stock_StockSymbol",
                table: "StockPrice");

            migrationBuilder.DropIndex(
                name: "IX_StockPrice_StockSymbol",
                table: "StockPrice");

            migrationBuilder.AlterColumn<string>(
                name: "StockSymbol",
                table: "StockPrice",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15);
        }
    }
}
