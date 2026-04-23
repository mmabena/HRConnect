using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLeaveToMain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeePensionEnrollments_PayrollRuns_PayrollRunId",
                table: "EmployeePensionEnrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_PensionOptions_PensionOptionId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_EmployeePensionEnrollments_PayrollRunId",
                table: "EmployeePensionEnrollments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaxTableUpload",
                table: "TaxTableUpload");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaxDeduction",
                table: "TaxDeduction");

            migrationBuilder.DropIndex(
                name: "IX_TaxDeduction_TaxYear_Remuneration",
                table: "TaxDeduction");

            migrationBuilder.RenameTable(
                name: "TaxTableUpload",
                newName: "TaxTableUploads");

            migrationBuilder.RenameTable(
                name: "TaxDeduction",
                newName: "TaxDeductions");

            migrationBuilder.AlterColumn<decimal>(
                name: "EmployerRate",
                table: "StatutoryContributionTypes",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0.01m);

            migrationBuilder.AlterColumn<decimal>(
                name: "EmployeeRate",
                table: "StatutoryContributionTypes",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0.01m);

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

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxUnder65",
                table: "TaxDeductions",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)",
                oldPrecision: 12,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxOver75",
                table: "TaxDeductions",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)",
                oldPrecision: 12,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "Tax65To74",
                table: "TaxDeductions",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)",
                oldPrecision: 12,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "AnnualEquivalent",
                table: "TaxDeductions",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)",
                oldPrecision: 12,
                oldScale: 2);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaxTableUploads",
                table: "TaxTableUploads",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaxDeductions",
                table: "TaxDeductions",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCompanyContributions_PayrollRunId_EmployeeId",
                table: "EmployeeCompanyContributions",
                columns: new[] { "PayrollRunId", "EmployeeId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_PensionOptions_PensionOptionId",
                table: "Employees",
                column: "PensionOptionId",
                principalTable: "PensionOptions",
                principalColumn: "PensionOptionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_PensionOptions_PensionOptionId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeCompanyContributions_PayrollRunId_EmployeeId",
                table: "EmployeeCompanyContributions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaxTableUploads",
                table: "TaxTableUploads");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaxDeductions",
                table: "TaxDeductions");

            migrationBuilder.RenameTable(
                name: "TaxTableUploads",
                newName: "TaxTableUpload");

            migrationBuilder.RenameTable(
                name: "TaxDeductions",
                newName: "TaxDeduction");

            migrationBuilder.AlterColumn<decimal>(
                name: "EmployerRate",
                table: "StatutoryContributionTypes",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0.01m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "EmployeeRate",
                table: "StatutoryContributionTypes",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0.01m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

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

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxUnder65",
                table: "TaxDeduction",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxOver75",
                table: "TaxDeduction",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Tax65To74",
                table: "TaxDeduction",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "AnnualEquivalent",
                table: "TaxDeduction",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaxTableUpload",
                table: "TaxTableUpload",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaxDeduction",
                table: "TaxDeduction",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePensionEnrollments_PayrollRunId",
                table: "EmployeePensionEnrollments",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxDeduction_TaxYear_Remuneration",
                table: "TaxDeduction",
                columns: new[] { "TaxYear", "Remuneration" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeePensionEnrollments_PayrollRuns_PayrollRunId",
                table: "EmployeePensionEnrollments",
                column: "PayrollRunId",
                principalTable: "PayrollRuns",
                principalColumn: "PayrollRunId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_PensionOptions_PensionOptionId",
                table: "Employees",
                column: "PensionOptionId",
                principalTable: "PensionOptions",
                principalColumn: "PensionOptionId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
