using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class AccountHistoricalValueTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountHistoricalValue",
                columns: table => new
                {
                    AccountHistoricalValueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    AccountCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ValueInGbp = table.Column<decimal>(type: "decimal(19,5)", precision: 19, scale: 5, nullable: false),
                    Contributions = table.Column<decimal>(type: "decimal(19,5)", precision: 19, scale: 5, nullable: false),
                    TotalPriceAgeInDays = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RecordedTotalValueInGbp = table.Column<decimal>(type: "decimal(19,5)", precision: 19, scale: 5, nullable: true),
                    RecordedTotalValueSource = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DiscrepancyRatio = table.Column<decimal>(type: "decimal(19,5)", precision: 19, scale: 5, nullable: true),
                    DifferenceToPreviousDay = table.Column<decimal>(type: "decimal(19,5)", precision: 19, scale: 5, nullable: true),
                    DifferenceRatio = table.Column<decimal>(type: "decimal(19,5)", precision: 19, scale: 5, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountHistoricalValue", x => x.AccountHistoricalValueId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountHistoricalValue");
        }
    }
}
