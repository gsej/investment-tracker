using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDerivedColumnsFromAccountHistoricalValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DifferenceRatio",
                table: "AccountHistoricalValue");

            migrationBuilder.DropColumn(
                name: "DifferenceToPreviousDay",
                table: "AccountHistoricalValue");

            migrationBuilder.DropColumn(
                name: "DiscrepancyRatio",
                table: "AccountHistoricalValue");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DifferenceRatio",
                table: "AccountHistoricalValue",
                type: "decimal(19,5)",
                precision: 19,
                scale: 5,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DifferenceToPreviousDay",
                table: "AccountHistoricalValue",
                type: "decimal(19,5)",
                precision: 19,
                scale: 5,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscrepancyRatio",
                table: "AccountHistoricalValue",
                type: "decimal(19,5)",
                precision: 19,
                scale: 5,
                nullable: true);
        }
    }
}
