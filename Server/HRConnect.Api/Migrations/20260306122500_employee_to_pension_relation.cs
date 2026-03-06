using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class employee_to_pension_relation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PensionOptionId",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PayrollPeriods",
                columns: table => new
                {
                    PayrollPeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollPeriods", x => x.PayrollPeriodId);
                });

            migrationBuilder.CreateTable(
                name: "PensionOptions",
                columns: table => new
                {
                    PensionOptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContributionPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PensionOptions", x => x.PensionOptionId);
                });

            migrationBuilder.CreateTable(
                name: "PayrollRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayrollRunId = table.Column<int>(type: "int", nullable: false),
                    PeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodPayrollPeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PeriodDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsFinalised = table.Column<bool>(type: "bit", nullable: false),
                    FinalisedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollRuns_PayrollPeriods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "PayrollPeriods",
                        principalColumn: "PayrollPeriodId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PayrollRuns_PayrollPeriods_PeriodPayrollPeriodId",
                        column: x => x.PeriodPayrollPeriodId,
                        principalTable: "PayrollPeriods",
                        principalColumn: "PayrollPeriodId");
                });

            migrationBuilder.CreateTable(
                name: "EmployeePensionEnrollments",
                columns: table => new
                {
                    EmployeePensionEnrollmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PensionOptionId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PayrollRunId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeePensionEnrollments", x => x.EmployeePensionEnrollmentId);
                    table.ForeignKey(
                        name: "FK_EmployeePensionEnrollments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeePensionEnrollments_PayrollRuns_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalTable: "PayrollRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeePensionEnrollments_PensionOptions_PensionOptionId",
                        column: x => x.PensionOptionId,
                        principalTable: "PensionOptions",
                        principalColumn: "PensionOptionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayrollRecords",
                columns: table => new
                {
                    PayrollRecordId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayrollRunId = table.Column<int>(type: "int", nullable: false),
                    PayrollRunId1 = table.Column<int>(type: "int", nullable: true),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollRecords", x => x.PayrollRecordId);
                    table.ForeignKey(
                        name: "FK_PayrollRecords_PayrollRuns_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalTable: "PayrollRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PayrollRecords_PayrollRuns_PayrollRunId1",
                        column: x => x.PayrollRunId1,
                        principalTable: "PayrollRuns",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PensionDeductions",
                columns: table => new
                {
                    PensionDeductionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeID = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateJoinedCompany = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IDNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Passport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PensionableSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PensionOptionId = table.Column<int>(type: "int", nullable: false),
                    PendsionCategoryPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PensionContribution = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VoluntaryContribution = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhyscialAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayrollRunId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PensionDeductions", x => x.PensionDeductionId);
                    table.ForeignKey(
                        name: "FK_PensionDeductions_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PensionDeductions_PayrollRuns_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalTable: "PayrollRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PensionDeductions_PensionOptions_PensionOptionId",
                        column: x => x.PensionOptionId,
                        principalTable: "PensionOptions",
                        principalColumn: "PensionOptionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PensionOptionId",
                table: "Employees",
                column: "PensionOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePensionEnrollments_EmployeeId",
                table: "EmployeePensionEnrollments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePensionEnrollments_PayrollRunId",
                table: "EmployeePensionEnrollments",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePensionEnrollments_PensionOptionId",
                table: "EmployeePensionEnrollments",
                column: "PensionOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRecords_PayrollRunId",
                table: "PayrollRecords",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRecords_PayrollRunId1",
                table: "PayrollRecords",
                column: "PayrollRunId1");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRuns_PeriodId",
                table: "PayrollRuns",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRuns_PeriodPayrollPeriodId",
                table: "PayrollRuns",
                column: "PeriodPayrollPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_PensionDeductions_EmployeeId",
                table: "PensionDeductions",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PensionDeductions_PayrollRunId",
                table: "PensionDeductions",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_PensionDeductions_PensionOptionId",
                table: "PensionDeductions",
                column: "PensionOptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_PensionOptions_PensionOptionId",
                table: "Employees",
                column: "PensionOptionId",
                principalTable: "PensionOptions",
                principalColumn: "PensionOptionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_PensionOptions_PensionOptionId",
                table: "Employees");

            migrationBuilder.DropTable(
                name: "EmployeePensionEnrollments");

            migrationBuilder.DropTable(
                name: "PayrollRecords");

            migrationBuilder.DropTable(
                name: "PensionDeductions");

            migrationBuilder.DropTable(
                name: "PayrollRuns");

            migrationBuilder.DropTable(
                name: "PensionOptions");

            migrationBuilder.DropTable(
                name: "PayrollPeriods");

            migrationBuilder.DropIndex(
                name: "IX_Employees_PensionOptionId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PensionOptionId",
                table: "Employees");
        }
    }
}
