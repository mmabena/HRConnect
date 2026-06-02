using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class SalaryBudgetEmployeeReModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "JobTitle",
                table: "SalaryBudgetEmployees",
                newName: "PositionTitle");

            migrationBuilder.RenameColumn(
                name: "SalaryBudgetEmployeeId",
                table: "SalaryBudgetEmployees",
                newName: "Id");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "SalaryBudgets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BudgetYear",
                table: "SalaryBudgets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "SalaryBudgets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "SalaryBudgets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "SalaryBudgets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "ProposedPercentage",
                table: "SalaryBudgetEmployees",
                type: "decimal(5,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "SalaryBudgets");

            migrationBuilder.DropColumn(
                name: "BudgetYear",
                table: "SalaryBudgets");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "SalaryBudgets");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "SalaryBudgets");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "SalaryBudgets");

            migrationBuilder.RenameColumn(
                name: "PositionTitle",
                table: "SalaryBudgetEmployees",
                newName: "JobTitle");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "SalaryBudgetEmployees",
                newName: "SalaryBudgetEmployeeId");

            migrationBuilder.AlterColumn<decimal>(
                name: "ProposedPercentage",
                table: "SalaryBudgetEmployees",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldNullable: true);
        }
    }
}
