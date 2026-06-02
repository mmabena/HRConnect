using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class SalaryBudgetEmployeeModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalaryBudgets_Employees_EmployeeId",
                table: "SalaryBudgets");

            migrationBuilder.DropForeignKey(
                name: "FK_SalaryBudgets_JobGrades_JobGradeId",
                table: "SalaryBudgets");

            migrationBuilder.DropForeignKey(
                name: "FK_SalaryBudgets_Positions_PositionId",
                table: "SalaryBudgets");

            migrationBuilder.DropIndex(
                name: "IX_SalaryBudgets_EmployeeId",
                table: "SalaryBudgets");

            migrationBuilder.DropIndex(
                name: "IX_SalaryBudgets_JobGradeId",
                table: "SalaryBudgets");

            migrationBuilder.DropIndex(
                name: "IX_SalaryBudgets_PositionId",
                table: "SalaryBudgets");

            migrationBuilder.DropColumn(
                name: "BonusApril",
                table: "SalaryBudgets");

            migrationBuilder.DropColumn(
                name: "BonusOctober",
                table: "SalaryBudgets");

            migrationBuilder.DropColumn(
                name: "Branch",
                table: "SalaryBudgets");

            migrationBuilder.DropColumn(
                name: "CarAllowance",
                table: "SalaryBudgets");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "SalaryBudgets");

            migrationBuilder.DropColumn(
                name: "CurrentSalary",
                table: "SalaryBudgets");

            migrationBuilder.DropColumn(
                name: "DeathBenefit",
                table: "SalaryBudgets");

            migrationBuilder.DropColumn(
                name: "DisabilityBenefit",
                table: "SalaryBudgets");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "SalaryBudgets");

            migrationBuilder.DropColumn(
                name: "EmployeeName",
                table: "SalaryBudgets");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "SalaryBudgets");

            migrationBuilder.DropColumn(
                name: "GrossSalary",
                table: "SalaryBudgets");

            migrationBuilder.DropColumn(
                name: "JobGradeId",
                table: "SalaryBudgets");

            migrationBuilder.DropColumn(
                name: "JobGradeName",
                table: "SalaryBudgets");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "SalaryBudgets");

            migrationBuilder.DropColumn(
                name: "NewAmount",
                table: "SalaryBudgets");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "SalaryBudgets");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "SalaryBudgets");

            migrationBuilder.DropColumn(
                name: "ProposedPercentage",
                table: "SalaryBudgets");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "SalaryBudgets");

            migrationBuilder.DropColumn(
                name: "TotalCostToCompany",
                table: "SalaryBudgets");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "SalaryBudgets");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "SalaryBudgets",
                newName: "SalaryBudgetName");

            migrationBuilder.CreateTable(
                name: "SalaryBudgetEmployees",
                columns: table => new
                {
                    SalaryBudgetEmployeeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalaryBudgetId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_SalaryBudgetEmployees", x => x.SalaryBudgetEmployeeId);
                    table.ForeignKey(
                        name: "FK_SalaryBudgetEmployees_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId");
                    table.ForeignKey(
                        name: "FK_SalaryBudgetEmployees_JobGrades_JobGradeId",
                        column: x => x.JobGradeId,
                        principalTable: "JobGrades",
                        principalColumn: "JobGradeId");
                    table.ForeignKey(
                        name: "FK_SalaryBudgetEmployees_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "PositionId");
                    table.ForeignKey(
                        name: "FK_SalaryBudgetEmployees_SalaryBudgets_SalaryBudgetId",
                        column: x => x.SalaryBudgetId,
                        principalTable: "SalaryBudgets",
                        principalColumn: "SalaryBudgetId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalaryBudgetEmployees_EmployeeId",
                table: "SalaryBudgetEmployees",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryBudgetEmployees_JobGradeId",
                table: "SalaryBudgetEmployees",
                column: "JobGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryBudgetEmployees_PositionId",
                table: "SalaryBudgetEmployees",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryBudgetEmployees_SalaryBudgetId",
                table: "SalaryBudgetEmployees",
                column: "SalaryBudgetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalaryBudgetEmployees");

            migrationBuilder.RenameColumn(
                name: "SalaryBudgetName",
                table: "SalaryBudgets",
                newName: "Status");

            migrationBuilder.AddColumn<decimal>(
                name: "BonusApril",
                table: "SalaryBudgets",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BonusOctober",
                table: "SalaryBudgets",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Branch",
                table: "SalaryBudgets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "CarAllowance",
                table: "SalaryBudgets",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "SalaryBudgets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentSalary",
                table: "SalaryBudgets",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DeathBenefit",
                table: "SalaryBudgets",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DisabilityBenefit",
                table: "SalaryBudgets",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeId",
                table: "SalaryBudgets",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmployeeName",
                table: "SalaryBudgets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "SalaryBudgets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GrossSalary",
                table: "SalaryBudgets",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "JobGradeId",
                table: "SalaryBudgets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "JobGradeName",
                table: "SalaryBudgets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "SalaryBudgets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "NewAmount",
                table: "SalaryBudgets",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "SalaryBudgets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PositionId",
                table: "SalaryBudgets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ProposedPercentage",
                table: "SalaryBudgets",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "SalaryBudgets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCostToCompany",
                table: "SalaryBudgets",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "SalaryBudgets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

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

            migrationBuilder.AddForeignKey(
                name: "FK_SalaryBudgets_Employees_EmployeeId",
                table: "SalaryBudgets",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalaryBudgets_JobGrades_JobGradeId",
                table: "SalaryBudgets",
                column: "JobGradeId",
                principalTable: "JobGrades",
                principalColumn: "JobGradeId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalaryBudgets_Positions_PositionId",
                table: "SalaryBudgets",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "PositionId");
        }
    }
}
