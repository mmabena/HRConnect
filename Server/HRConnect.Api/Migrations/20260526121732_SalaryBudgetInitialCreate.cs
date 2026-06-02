using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class SalaryBudgetInitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalaryBudgets",
                columns: table => new
                {
                    SalaryBudgetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EmployeeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Branch = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JobGradeId = table.Column<int>(type: "int", nullable: false),
                    JobGradeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProposedPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NewAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CarAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BonusApril = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BonusOctober = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DeathBenefit = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DisabilityBenefit = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GrossSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCostToCompany = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryBudgets", x => x.SalaryBudgetId);
                    table.ForeignKey(
                        name: "FK_SalaryBudgets_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId");
                    table.ForeignKey(
                        name: "FK_SalaryBudgets_JobGrades_JobGradeId",
                        column: x => x.JobGradeId,
                        principalTable: "JobGrades",
                        principalColumn: "JobGradeId");
                    table.ForeignKey(
                        name: "FK_SalaryBudgets_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "PositionId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalaryBudgets_EmployeeId",
                table: "SalaryBudgets",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryBudgets_JobGradeId",
                table: "SalaryBudgets",
                column: "JobGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryBudgets_PositionId",
                table: "SalaryBudgets",
                column: "PositionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalaryBudgets");
        }
    }
}
