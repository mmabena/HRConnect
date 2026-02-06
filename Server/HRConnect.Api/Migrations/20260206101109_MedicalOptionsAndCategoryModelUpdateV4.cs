using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class MedicalOptionsAndCategoryModelUpdateV4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdultAmount",
                table: "MedicalOptions");

            migrationBuilder.DropColumn(
                name: "Child1Amount",
                table: "MedicalOptions");

            migrationBuilder.DropColumn(
                name: "Child2Amount",
                table: "MedicalOptions");

            migrationBuilder.DropColumn(
                name: "PrincipalAmount",
                table: "MedicalOptions");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalMonthlyContributionsAdult",
                table: "MedicalOptionCategories",
                type: "decimal(15,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalMonthlyContributionsChild",
                table: "MedicalOptionCategories",
                type: "decimal(15,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalMonthlyContributionsChild2",
                table: "MedicalOptionCategories",
                type: "decimal(15,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalMonthlyContributionsPrincipal",
                table: "MedicalOptionCategories",
                type: "decimal(15,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalMonthlyContributionsAdult",
                table: "MedicalOptionCategories");

            migrationBuilder.DropColumn(
                name: "TotalMonthlyContributionsChild",
                table: "MedicalOptionCategories");

            migrationBuilder.DropColumn(
                name: "TotalMonthlyContributionsChild2",
                table: "MedicalOptionCategories");

            migrationBuilder.DropColumn(
                name: "TotalMonthlyContributionsPrincipal",
                table: "MedicalOptionCategories");

            migrationBuilder.AddColumn<decimal>(
                name: "AdultAmount",
                table: "MedicalOptions",
                type: "decimal(15,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Child1Amount",
                table: "MedicalOptions",
                type: "decimal(15,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Child2Amount",
                table: "MedicalOptions",
                type: "decimal(15,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrincipalAmount",
                table: "MedicalOptions",
                type: "decimal(15,2)",
                nullable: true);
        }
    }
}
