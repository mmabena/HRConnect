using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenamingFinalTaxDeduction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TaxableIncome",
                table: "FinalTaxDeductions",
                newName: "PensionableIncome");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PensionableIncome",
                table: "FinalTaxDeductions",
                newName: "TaxableIncome");
        }
    }
}
