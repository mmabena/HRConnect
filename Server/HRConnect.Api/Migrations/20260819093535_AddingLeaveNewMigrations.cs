using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddingLeaveNewMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeePensionEnrollments_PayrollRuns_PayrollRunId",
                table: "EmployeePensionEnrollments");

            migrationBuilder.DropIndex(
                name: "IX_EmployeePensionEnrollments_PayrollRunId",
                table: "EmployeePensionEnrollments");

            migrationBuilder.AlterColumn<string>(
                name: "TempRole",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TOTPStates_UserId",
                table: "TOTPStates",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TOTPStates_UserId",
                table: "TOTPStates");

            migrationBuilder.AlterColumn<int>(
                name: "TempRole",
                table: "Users",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePensionEnrollments_PayrollRunId",
                table: "EmployeePensionEnrollments",
                column: "PayrollRunId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeePensionEnrollments_PayrollRuns_PayrollRunId",
                table: "EmployeePensionEnrollments",
                column: "PayrollRunId",
                principalTable: "PayrollRuns",
                principalColumn: "PayrollRunId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
