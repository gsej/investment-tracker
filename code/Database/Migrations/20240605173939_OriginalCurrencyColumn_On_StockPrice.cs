using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class OriginalCurrencyColumn_On_StockPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "StockPrice",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                collation: "SQL_Latin1_General_CP1_CS_AS",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true,
                oldCollation: "SQL_Latin1_General_CP1_CS_AS");

            migrationBuilder.AddColumn<string>(
                name: "OriginalCurrency",
                table: "StockPrice",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                collation: "SQL_Latin1_General_CP1_CS_AS");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalCurrency",
                table: "StockPrice");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "StockPrice",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                collation: "SQL_Latin1_General_CP1_CS_AS",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldCollation: "SQL_Latin1_General_CP1_CS_AS");
        }
    }
}
