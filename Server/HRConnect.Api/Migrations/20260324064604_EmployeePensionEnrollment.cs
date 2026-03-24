using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class EmployeePensionEnrollment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PayrollRun_PayrollRunNumber",
                table: "PayrollRuns");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DateJoinedCompany",
                table: "PensionDeductions",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "CreatedDate",
                table: "PensionDeductions",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPensionContribution",
                table: "PensionDeductions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PensionOptionId",
                table: "Employees",
                type: "int",
                nullable: true);

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
                name: "EmployeePensionEnrollments",
                columns: table => new
                {
                    EmployeePensionEnrollmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PensionOptionId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    VoluntaryContribution = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsVoluntaryContributionPermament = table.Column<bool>(type: "bit", nullable: true),
                    PayrollRunId = table.Column<int>(type: "int", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false)
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
                        principalColumn: "PayrollRunId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeePensionEnrollments_PensionOptions_PensionOptionId",
                        column: x => x.PensionOptionId,
                        principalTable: "PensionOptions",
                        principalColumn: "PensionOptionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PensionDeductions_PensionOptionId",
                table: "PensionDeductions",
                column: "PensionOptionId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_PensionOptions_PensionOptionId",
                table: "Employees",
                column: "PensionOptionId",
                principalTable: "PensionOptions",
                principalColumn: "PensionOptionId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PensionDeductions_PensionOptions_PensionOptionId",
                table: "PensionDeductions",
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

            migrationBuilder.DropForeignKey(
                name: "FK_PensionDeductions_PensionOptions_PensionOptionId",
                table: "PensionDeductions");

            migrationBuilder.DropTable(
                name: "EmployeePensionEnrollments");

            migrationBuilder.DropTable(
                name: "PensionOptions");

            migrationBuilder.DropIndex(
                name: "IX_PensionDeductions_PensionOptionId",
                table: "PensionDeductions");

            migrationBuilder.DropIndex(
                name: "IX_Employees_PensionOptionId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "TotalPensionContribution",
                table: "PensionDeductions");

            migrationBuilder.DropColumn(
                name: "PensionOptionId",
                table: "Employees");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateJoinedCompany",
                table: "PensionDeductions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "PensionDeductions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PayrollRun_PayrollRunNumber",
                table: "PayrollRuns",
                sql: "[PayrollRunNumber] BETWEEN 1 AND 12");
        }
    }
}
