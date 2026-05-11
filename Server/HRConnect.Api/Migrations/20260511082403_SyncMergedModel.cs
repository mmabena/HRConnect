using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class SyncMergedModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeePensionEnrollments_PayrollRuns_PayrollRunId",
                table: "EmployeePensionEnrollments");

            migrationBuilder.DropIndex(
                name: "IX_EmployeePensionEnrollments_PayrollRunId",
                table: "EmployeePensionEnrollments");
        }
    }
}
