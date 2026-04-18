using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class FinalTaxDeductionwithUIFandSDL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

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


            migrationBuilder.CreateTable(
                name: "FinalTaxDeductions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [PayrollRecordSequence]"),
                    PayrollRunId = table.Column<int>(type: "int", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PassportNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxYear = table.Column<int>(type: "int", nullable: false),
                    MonthlySalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MedicalAidMembers = table.Column<int>(type: "int", nullable: false),
                    MedicalAidDependants = table.Column<int>(type: "int", nullable: false),
                    MedicalAidChildren = table.Column<int>(type: "int", nullable: false),
                    MedicalTaxCredit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PensionContribution = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PensionableIncome = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxDeductionAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NetSalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinalTaxDeductions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinalTaxDeductions_PayrollRuns_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalTable: "PayrollRuns",
                        principalColumn: "PayrollRunId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinalTaxDeductions_PayrollRunId_EmployeeId",
                table: "FinalTaxDeductions",
                columns: new[] { "PayrollRunId", "EmployeeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyContributions");

            migrationBuilder.DropTable(
                name: "EmployeeCompanyContributions");

            migrationBuilder.DropTable(
                name: "FinalTaxDeductions");

            migrationBuilder.DropTable(
                name: "PensionFunds");

            migrationBuilder.DropIndex(
                name: "IX_StatutoryContributions_PayrollRunId_EmployeeId",
                table: "StatutoryContributions");

            migrationBuilder.DropIndex(
                name: "IX_PensionDeductions_PayrollRunId_EmployeeId",
                table: "PensionDeductions");

            migrationBuilder.DropIndex(
                name: "IX_MedicalAidDeductions_PayrollRunId_EmployeeId",
                table: "MedicalAidDeductions");

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
        }
    }
}
