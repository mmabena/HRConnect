using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixJobGradeRelationshipMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveEntitlementRules_JobGrades_JobGradeId1",
                table: "LeaveEntitlementRules");

            migrationBuilder.DropIndex(
                name: "IX_LeaveEntitlementRules_JobGradeId1",
                table: "LeaveEntitlementRules");

            migrationBuilder.DropColumn(
                name: "JobGradeId1",
                table: "LeaveEntitlementRules");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "JobGradeId1",
                table: "LeaveEntitlementRules",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 1,
                column: "JobGradeId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 2,
                column: "JobGradeId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 3,
                column: "JobGradeId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 4,
                column: "JobGradeId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 5,
                column: "JobGradeId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 6,
                column: "JobGradeId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 7,
                column: "JobGradeId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 8,
                column: "JobGradeId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 9,
                column: "JobGradeId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 10,
                column: "JobGradeId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 11,
                column: "JobGradeId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 12,
                column: "JobGradeId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 13,
                column: "JobGradeId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 14,
                column: "JobGradeId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 15,
                column: "JobGradeId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 16,
                column: "JobGradeId1",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveEntitlementRules_JobGradeId1",
                table: "LeaveEntitlementRules",
                column: "JobGradeId1");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveEntitlementRules_JobGrades_JobGradeId1",
                table: "LeaveEntitlementRules",
                column: "JobGradeId1",
                principalTable: "JobGrades",
                principalColumn: "JobGradeId");
        }
    }
}
