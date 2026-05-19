using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class EmployeePayrollEarningRelationsAndEmployeeDeductionRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeDeductions");

            migrationBuilder.DropTable(
                name: "EmployeePayrollEarnings");

            migrationBuilder.DropTable(
                name: "Deductions");

            migrationBuilder.DeleteData(
                table: "PayrollEarnings",
                keyColumn: "PayrollEarningId",
                keyValue: "PRE001");
        }
    }
}
