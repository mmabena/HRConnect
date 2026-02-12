using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedFamilyResponsibilityLeave : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "LeaveEntitlementRules",
                columns: new[] { "Id", "DaysAllocated", "IsActive", "JobGradeId", "JobGradeId1", "LeaveTypeId", "MaxYearsService", "MinYearsService" },
                values: new object[,]
                {
                    { 15, 3, true, 2, null, 4, null, 0m },
                    { 16, 3, true, 3, null, 4, null, 0m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 16);
        }
    }
}
