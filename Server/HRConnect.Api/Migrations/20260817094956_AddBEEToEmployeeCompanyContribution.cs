using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBEEToEmployeeCompanyContribution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BEEAmount",
                table: "EmployeeCompanyContributions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BEEPercentage",
                table: "EmployeeCompanyContributions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BEEAmount",
                table: "EmployeeCompanyContributions");

            migrationBuilder.DropColumn(
                name: "BEEPercentage",
                table: "EmployeeCompanyContributions");
        }
    }
}
