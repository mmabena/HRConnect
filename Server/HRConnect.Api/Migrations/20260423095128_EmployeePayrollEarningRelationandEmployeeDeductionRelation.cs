using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class EmployeePayrollEarningRelationandEmployeeDeductionRelation : Migration
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

            //migrationBuilder.CreateTable(
            //    name: "CompanyContributions",
            //    columns: table => new
            //    {
            //        CompanyContributionId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        ShortDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        LongDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        TaxCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Percentage = table.Column<decimal>(type: "decimal(10,6)", nullable: false),
            //        IsActive = table.Column<bool>(type: "bit", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CompanyContributions", x => x.CompanyContributionId);
            //    });

            migrationBuilder.CreateTable(
                name: "Deductions",
                columns: table => new
                {
                    DeductionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CompanyId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaxCode = table.Column<int>(type: "int", nullable: true),
                    ShortDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LongDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeductionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InputType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinimumValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaximumValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EmployerContributed = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deductions", x => x.DeductionId);
                });

            //migrationBuilder.CreateTable(
            //    name: "EmployeeCompanyContributions",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [PayrollRecordSequence]"),
            //        PayrollRunId = table.Column<int>(type: "int", nullable: false),
            //        IsLocked = table.Column<bool>(type: "bit", nullable: false),
            //        EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        IdNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        PassportNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Age = table.Column<int>(type: "int", nullable: false),
            //        Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        DeathAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        DeathPercentage = table.Column<decimal>(type: "decimal(10,6)", nullable: false),
            //        DisabilityAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        DisabilityPercentage = table.Column<decimal>(type: "decimal(10,6)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_EmployeeCompanyContributions", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_EmployeeCompanyContributions_PayrollRuns_PayrollRunId",
            //            column: x => x.PayrollRunId,
            //            principalTable: "PayrollRuns",
            //            principalColumn: "PayrollRunId",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            migrationBuilder.CreateTable(
                name: "EmployeePayrollEarnings",
                columns: table => new
                {
                    EmployeePayrollEarningId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PayrollEarningId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TaxCode = table.Column<int>(type: "int", nullable: false),
                    OverTimeHoursWorked = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CalculatedAmountAfterTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PayrollRunId = table.Column<int>(type: "int", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeePayrollEarnings", x => x.EmployeePayrollEarningId);
                    table.ForeignKey(
                        name: "FK_EmployeePayrollEarnings_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId");
                    table.ForeignKey(
                        name: "FK_EmployeePayrollEarnings_PayrollEarnings_PayrollEarningId",
                        column: x => x.PayrollEarningId,
                        principalTable: "PayrollEarnings",
                        principalColumn: "PayrollEarningId");
                    table.ForeignKey(
                        name: "FK_EmployeePayrollEarnings_PayrollRuns_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalTable: "PayrollRuns",
                        principalColumn: "PayrollRunId",
                        onDelete: ReferentialAction.Cascade);
                });

            //migrationBuilder.CreateTable(
            //    name: "PensionFunds",
            //    columns: table => new
            //    {
            //        PensionFundId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        EmployeeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        MonthlySalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        ContributionPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        ContributionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        TaxCode = table.Column<int>(type: "int", nullable: false),
            //        PensionOptionId = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_PensionFunds", x => x.PensionFundId);
            //        table.ForeignKey(
            //            name: "FK_PensionFunds_Employees_EmployeeId",
            //            column: x => x.EmployeeId,
            //            principalTable: "Employees",
            //            principalColumn: "EmployeeId",
            //            onDelete: ReferentialAction.Restrict);
            //        table.ForeignKey(
            //            name: "FK_PensionFunds_PensionOptions_PensionOptionId",
            //            column: x => x.PensionOptionId,
            //            principalTable: "PensionOptions",
            //            principalColumn: "PensionOptionId",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            migrationBuilder.CreateTable(
                name: "EmployeeDeductions",
                columns: table => new
                {
                    EmployeeDeductionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DeductionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DeductionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeductionInputType = table.Column<int>(type: "int", nullable: false),
                    AmountOrPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CalculatedDeductionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PayrollRunId = table.Column<int>(type: "int", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeDeductions", x => x.EmployeeDeductionId);
                    table.ForeignKey(
                        name: "FK_EmployeeDeductions_Deductions_DeductionId",
                        column: x => x.DeductionId,
                        principalTable: "Deductions",
                        principalColumn: "DeductionId");
                    table.ForeignKey(
                        name: "FK_EmployeeDeductions_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId");
                    table.ForeignKey(
                        name: "FK_EmployeeDeductions_PayrollRuns_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalTable: "PayrollRuns",
                        principalColumn: "PayrollRunId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PayrollEarnings",
                columns: new[] { "PayrollEarningId", "CanProRata", "IsActive", "IsOnGoing", "LongDescription", "OvertimeHourMultiplier", "ShortDescription", "TaxCode", "TaxPercentage", "Taxable" },
                values: new object[] { "PRE001", true, true, true, "Employee monthly salary", null, "Basic salary", 3601, 100m, true });

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

            //migrationBuilder.CreateIndex(
            //    name: "IX_EmployeeCompanyContributions_PayrollRunId_EmployeeId",
            //    table: "EmployeeCompanyContributions",
            //    columns: new[] { "PayrollRunId", "EmployeeId" },
            //    unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDeductions_DeductionId",
                table: "EmployeeDeductions",
                column: "DeductionId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDeductions_EmployeeId",
                table: "EmployeeDeductions",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDeductions_PayrollRunId",
                table: "EmployeeDeductions",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePayrollEarnings_EmployeeId",
                table: "EmployeePayrollEarnings",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePayrollEarnings_PayrollEarningId",
                table: "EmployeePayrollEarnings",
                column: "PayrollEarningId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePayrollEarnings_PayrollRunId",
                table: "EmployeePayrollEarnings",
                column: "PayrollRunId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_PensionFunds_EmployeeId",
            //    table: "PensionFunds",
            //    column: "EmployeeId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_PensionFunds_PensionOptionId",
            //    table: "PensionFunds",
            //    column: "PensionOptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "CompanyContributions");

            //migrationBuilder.DropTable(
            //    name: "EmployeeCompanyContributions");

            migrationBuilder.DropTable(
                name: "EmployeeDeductions");

            migrationBuilder.DropTable(
                name: "EmployeePayrollEarnings");

            //migrationBuilder.DropTable(
            //    name: "PensionFunds");

            migrationBuilder.DropTable(
                name: "Deductions");

            migrationBuilder.DropIndex(
                name: "IX_StatutoryContributions_PayrollRunId_EmployeeId",
                table: "StatutoryContributions");

            migrationBuilder.DropIndex(
                name: "IX_PensionDeductions_PayrollRunId_EmployeeId",
                table: "PensionDeductions");

            migrationBuilder.DropIndex(
                name: "IX_MedicalAidDeductions_PayrollRunId_EmployeeId",
                table: "MedicalAidDeductions");

            migrationBuilder.DeleteData(
                table: "PayrollEarnings",
                keyColumn: "PayrollEarningId",
                keyValue: "PRE001");

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
