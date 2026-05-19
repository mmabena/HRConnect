using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
  /// <inheritdoc />
  public partial class CleanPayrollFix : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
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

      migrationBuilder.CreateIndex(
          name: "IX_StatutoryContributions_PayrollRunId_EmployeeId",
          table: "StatutoryContributions",
          columns: new[] { "PayrollRunId", "EmployeeId" },
          unique: true);

      migrationBuilder.CreateIndex(
          name: "IX_PensionDeductions_PayrollRunId_EmployeeId",
          table: "PensionDeductions",
          columns: new[] { "PayrollRunId", "EmployeeId" },
          unique: true);

      migrationBuilder.CreateIndex(
          name: "IX_MedicalAidDeductions_PayrollRunId_EmployeeId",
          table: "MedicalAidDeductions",
          columns: new[] { "PayrollRunId", "EmployeeId" },
          unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCompanyContributions_PayrollRunId_EmployeeId",
                table: "EmployeeCompanyContributions",
                columns: new[] { "PayrollRunId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateTable(
                name: "PayrollEarnings",
                columns: table => new
                {
                    PayrollEarningId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ShortDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LongDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Taxable = table.Column<bool>(type: "bit", nullable: false),
                    TaxCode = table.Column<int>(type: "int", nullable: false),
                    TaxPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OvertimeHourMultiplier = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CanProRata = table.Column<bool>(type: "bit", nullable: false),
                    IsOnGoing = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollEarnings", x => x.PayrollEarningId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayrollEarnings");


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
    }
}
