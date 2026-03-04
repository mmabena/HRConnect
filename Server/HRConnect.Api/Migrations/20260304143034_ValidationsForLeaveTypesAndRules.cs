using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class ValidationsForLeaveTypesAndRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LeaveEntitlementRules_LeaveTypeId",
                table: "LeaveEntitlementRules");

            migrationBuilder.DropIndex(
                name: "IX_LeaveApplications_EmployeeId",
                table: "LeaveApplications");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeLeaveBalances_EmployeeId",
                table: "EmployeeLeaveBalances");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "LeaveTypes",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveTypes_Name",
                table: "LeaveTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveEntitlementRules_LeaveTypeId_JobGradeId_MinYearsService",
                table: "LeaveEntitlementRules",
                columns: new[] { "LeaveTypeId", "JobGradeId", "MinYearsService" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveApplications_EmployeeId_StartDate_EndDate",
                table: "LeaveApplications",
                columns: new[] { "EmployeeId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaveBalances_EmployeeId_LeaveTypeId",
                table: "EmployeeLeaveBalances",
                columns: new[] { "EmployeeId", "LeaveTypeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LeaveTypes_Name",
                table: "LeaveTypes");

            migrationBuilder.DropIndex(
                name: "IX_LeaveEntitlementRules_LeaveTypeId_JobGradeId_MinYearsService",
                table: "LeaveEntitlementRules");

            migrationBuilder.DropIndex(
                name: "IX_LeaveApplications_EmployeeId_StartDate_EndDate",
                table: "LeaveApplications");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeLeaveBalances_EmployeeId_LeaveTypeId",
                table: "EmployeeLeaveBalances");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "LeaveTypes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveEntitlementRules_LeaveTypeId",
                table: "LeaveEntitlementRules",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveApplications_EmployeeId",
                table: "LeaveApplications",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaveBalances_EmployeeId",
                table: "EmployeeLeaveBalances",
                column: "EmployeeId");
        }
    }
}
