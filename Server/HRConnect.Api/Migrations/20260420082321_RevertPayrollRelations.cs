using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class RevertPayrollRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
      name: "IX_StatutoryContributions_PayrollRunId_EmployeeId",
      table: "StatutoryContributions");

            migrationBuilder.DropIndex(
                name: "IX_PensionDeductions_PayrollRunId_EmployeeId",
                table: "PensionDeductions");

            migrationBuilder.DropIndex(
                name: "IX_MedicalAidDeductions_PayrollRunId_EmployeeId",
                table: "MedicalAidDeductions");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeCompanyContributions_PayrollRunId_EmployeeId",
                table: "EmployeeCompanyContributions");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "StatutoryContributions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "PensionDeductions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "MedicalAidDeductions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "EmployeeCompanyContributions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_StatutoryContributions_PayrollRunId",
                table: "StatutoryContributions",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_PensionDeductions_PayrollRunId",
                table: "PensionDeductions",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalAidDeductions_PayrollRunId",
                table: "MedicalAidDeductions",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCompanyContributions_PayrollRunId",
                table: "EmployeeCompanyContributions",
                column: "PayrollRunId");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StatutoryContributions_PayrollRunId",
                table: "StatutoryContributions");

            migrationBuilder.DropIndex(
                name: "IX_PensionDeductions_PayrollRunId",
                table: "PensionDeductions");

            migrationBuilder.DropIndex(
                name: "IX_MedicalAidDeductions_PayrollRunId",
                table: "MedicalAidDeductions");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeCompanyContributions_PayrollRunId",
                table: "EmployeeCompanyContributions");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "StatutoryContributions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "PensionDeductions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "MedicalAidDeductions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "EmployeeCompanyContributions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}