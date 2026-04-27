using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class SalaryBenchmarksTq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnnualLeaveAccrualHistories_Employees_EmployeeId",
                table: "AnnualLeaveAccrualHistories");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "LeaveApplications",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "EmployeeLeaveBalances",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "EmployeeAccrualRateHistories",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "AnnualLeaveAccrualHistories",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            // migrationBuilder.CreateTable(
            //     name: "CompanyContributions",
            //     columns: table => new
            //     {
            //         CompanyContributionId = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         ShortDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         LongDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         TaxCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         Percentage = table.Column<decimal>(type: "decimal(10,6)", nullable: false),
            //         IsActive = table.Column<bool>(type: "bit", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_CompanyContributions", x => x.CompanyContributionId);
            //     });

            // migrationBuilder.CreateTable(
            //     name: "EmployeeCompanyContributions",
            //     columns: table => new
            //     {
            //         Id = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [PayrollRecordSequence]"),
            //         PayrollRunId = table.Column<int>(type: "int", nullable: false),
            //         IsLocked = table.Column<bool>(type: "bit", nullable: false),
            //         EmployeeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         Surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         IdNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         PassportNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         Age = table.Column<int>(type: "int", nullable: false),
            //         Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //         DeathAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //         DeathPercentage = table.Column<decimal>(type: "decimal(10,6)", nullable: false),
            //         DisabilityAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //         DisabilityPercentage = table.Column<decimal>(type: "decimal(10,6)", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_EmployeeCompanyContributions", x => x.Id);
            //         table.ForeignKey(
            //             name: "FK_EmployeeCompanyContributions_PayrollRuns_PayrollRunId",
            //             column: x => x.PayrollRunId,
            //             principalTable: "PayrollRuns",
            //             principalColumn: "PayrollRunId",
            //             onDelete: ReferentialAction.Cascade);
            //     });

            // migrationBuilder.CreateTable(
            //     name: "FinalTaxDeductions",
            //     columns: table => new
            //     {
            //         Id = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [PayrollRecordSequence]"),
            //         PayrollRunId = table.Column<int>(type: "int", nullable: false),
            //         IsLocked = table.Column<bool>(type: "bit", nullable: false),
            //         EmployeeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         Surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         IdNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         PassportNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //         TaxYear = table.Column<int>(type: "int", nullable: false),
            //         MonthlySalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
            //         MedicalAidMembers = table.Column<int>(type: "int", nullable: false),
            //         MedicalAidDependants = table.Column<int>(type: "int", nullable: false),
            //         MedicalAidChildren = table.Column<int>(type: "int", nullable: false),
            //         MedicalTaxCredit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
            //         PensionContribution = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
            //         PensionableIncome = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
            //         TaxDeductionAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
            //         NetSalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
            //         TaxCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_FinalTaxDeductions", x => x.Id);
            //         table.ForeignKey(
            //             name: "FK_FinalTaxDeductions_PayrollRuns_PayrollRunId",
            //             column: x => x.PayrollRunId,
            //             principalTable: "PayrollRuns",
            //             principalColumn: "PayrollRunId",
            //             onDelete: ReferentialAction.Cascade);
            //     });

            // migrationBuilder.CreateTable(
            //     name: "PensionFunds",
            //     columns: table => new
            //     {
            //         PensionFundId = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //         EmployeeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         MonthlySalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //         ContributionPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //         ContributionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //         TaxCode = table.Column<int>(type: "int", nullable: false),
            //         PensionOptionId = table.Column<int>(type: "int", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_PensionFunds", x => x.PensionFundId);
            //         table.ForeignKey(
            //             name: "FK_PensionFunds_Employees_EmployeeId",
            //             column: x => x.EmployeeId,
            //             principalTable: "Employees",
            //             principalColumn: "EmployeeId",
            //             onDelete: ReferentialAction.Restrict);
            //         table.ForeignKey(
            //             name: "FK_PensionFunds_PensionOptions_PensionOptionId",
            //             column: x => x.PensionOptionId,
            //             principalTable: "PensionOptions",
            //             principalColumn: "PensionOptionId",
            //             onDelete: ReferentialAction.Cascade);
            //     });

            migrationBuilder.CreateTable(
                name: "SalaryBenchmarks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Salary25th = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Salary50th = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Salary75th = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InternalJobGradeId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryBenchmarks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalaryBenchmarks_JobGrades_InternalJobGradeId",
                        column: x => x.InternalJobGradeId,
                        principalTable: "JobGrades",
                        principalColumn: "JobGradeId",
                        onDelete: ReferentialAction.Restrict);
                });

            // migrationBuilder.CreateIndex(
            //     name: "IX_EmployeeCompanyContributions_PayrollRunId",
            //     table: "EmployeeCompanyContributions",
            //     column: "PayrollRunId");

            // migrationBuilder.CreateIndex(
            //     name: "IX_FinalTaxDeductions_PayrollRunId",
            //     table: "FinalTaxDeductions",
            //     column: "PayrollRunId");

            // migrationBuilder.CreateIndex(
            //     name: "IX_PensionFunds_EmployeeId",
            //     table: "PensionFunds",
            //     column: "EmployeeId");

            // migrationBuilder.CreateIndex(
            //     name: "IX_PensionFunds_PensionOptionId",
            //     table: "PensionFunds",
            //     column: "PensionOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryBenchmarks_InternalJobGradeId",
                table: "SalaryBenchmarks",
                column: "InternalJobGradeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AnnualLeaveAccrualHistories_Employees_EmployeeId",
                table: "AnnualLeaveAccrualHistories",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnnualLeaveAccrualHistories_Employees_EmployeeId",
                table: "AnnualLeaveAccrualHistories");

            migrationBuilder.DropTable(
                name: "CompanyContributions");

            migrationBuilder.DropTable(
                name: "EmployeeCompanyContributions");

            migrationBuilder.DropTable(
                name: "FinalTaxDeductions");

            migrationBuilder.DropTable(
                name: "PensionFunds");

            migrationBuilder.DropTable(
                name: "SalaryBenchmarks");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "LeaveApplications",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "EmployeeLeaveBalances",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "EmployeeAccrualRateHistories",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "AnnualLeaveAccrualHistories",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AnnualLeaveAccrualHistories_Employees_EmployeeId",
                table: "AnnualLeaveAccrualHistories",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
