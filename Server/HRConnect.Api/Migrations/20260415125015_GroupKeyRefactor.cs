using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class GroupKeyRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveEntitlementRules_JobGrades_JobGradeId",
                table: "LeaveEntitlementRules");

            migrationBuilder.AddColumn<string>(
                name: "GroupKey",
                table: "LeaveEntitlementRules",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveEntitlementRules_JobGrades_JobGradeId",
                table: "LeaveEntitlementRules",
                column: "JobGradeId",
                principalTable: "JobGrades",
                principalColumn: "JobGradeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveEntitlementRules_JobGrades_JobGradeId",
                table: "LeaveEntitlementRules");

            migrationBuilder.DropColumn(
                name: "GroupKey",
                table: "LeaveEntitlementRules");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveEntitlementRules_JobGrades_JobGradeId",
                table: "LeaveEntitlementRules",
                column: "JobGradeId",
                principalTable: "JobGrades",
                principalColumn: "JobGradeId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
