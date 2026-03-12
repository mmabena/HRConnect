using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixEmployeePensionRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_PensionFunds_PensionFundId",
                table: "Employees");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "PensionFunds",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_PensionFunds_EmployeeId",
                table: "PensionFunds",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_PensionFunds_PensionFundId",
                table: "Employees",
                column: "PensionFundId",
                principalTable: "PensionFunds",
                principalColumn: "PensionFundId");

            migrationBuilder.AddForeignKey(
                name: "FK_PensionFunds_Employees_EmployeeId",
                table: "PensionFunds",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_PensionFunds_PensionFundId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_PensionFunds_Employees_EmployeeId",
                table: "PensionFunds");

            migrationBuilder.DropIndex(
                name: "IX_PensionFunds_EmployeeId",
                table: "PensionFunds");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "PensionFunds",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_PensionFunds_PensionFundId",
                table: "Employees",
                column: "PensionFundId",
                principalTable: "PensionFunds",
                principalColumn: "PensionFundId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
