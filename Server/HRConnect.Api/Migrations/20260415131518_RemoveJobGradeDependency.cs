using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveJobGradeDependency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveEntitlementRules_JobGrades_JobGradeId",
                table: "LeaveEntitlementRules");

            migrationBuilder.DropIndex(
                name: "IX_LeaveEntitlementRules_JobGradeId",
                table: "LeaveEntitlementRules");

            migrationBuilder.DropColumn(
                name: "JobGradeId",
                table: "LeaveEntitlementRules");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "JobGradeId",
                table: "LeaveEntitlementRules",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveEntitlementRules_JobGradeId",
                table: "LeaveEntitlementRules",
                column: "JobGradeId");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveEntitlementRules_JobGrades_JobGradeId",
                table: "LeaveEntitlementRules",
                column: "JobGradeId",
                principalTable: "JobGrades",
                principalColumn: "JobGradeId");
        }
    }
}
