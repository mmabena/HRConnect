using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDaysAllocatedToDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "DaysAllocated",
                table: "LeaveEntitlementRules",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 1,
                column: "DaysAllocated",
                value: 15m);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 2,
                column: "DaysAllocated",
                value: 18m);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 3,
                column: "DaysAllocated",
                value: 22m);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 4,
                column: "DaysAllocated",
                value: 18m);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 5,
                column: "DaysAllocated",
                value: 21m);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 6,
                column: "DaysAllocated",
                value: 25m);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 7,
                column: "DaysAllocated",
                value: 20m);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 8,
                column: "DaysAllocated",
                value: 23m);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 9,
                column: "DaysAllocated",
                value: 27m);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 10,
                column: "DaysAllocated",
                value: 30m);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 11,
                column: "DaysAllocated",
                value: 30m);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 12,
                column: "DaysAllocated",
                value: 30m);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 13,
                column: "DaysAllocated",
                value: 120m);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 14,
                column: "DaysAllocated",
                value: 3m);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 15,
                column: "DaysAllocated",
                value: 3m);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 16,
                column: "DaysAllocated",
                value: 3m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "DaysAllocated",
                table: "LeaveEntitlementRules",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 1,
                column: "DaysAllocated",
                value: 15);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 2,
                column: "DaysAllocated",
                value: 18);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 3,
                column: "DaysAllocated",
                value: 22);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 4,
                column: "DaysAllocated",
                value: 18);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 5,
                column: "DaysAllocated",
                value: 21);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 6,
                column: "DaysAllocated",
                value: 25);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 7,
                column: "DaysAllocated",
                value: 20);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 8,
                column: "DaysAllocated",
                value: 23);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 9,
                column: "DaysAllocated",
                value: 27);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 10,
                column: "DaysAllocated",
                value: 30);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 11,
                column: "DaysAllocated",
                value: 30);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 12,
                column: "DaysAllocated",
                value: 30);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 13,
                column: "DaysAllocated",
                value: 120);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 14,
                column: "DaysAllocated",
                value: 3);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 15,
                column: "DaysAllocated",
                value: 3);

            migrationBuilder.UpdateData(
                table: "LeaveEntitlementRules",
                keyColumn: "Id",
                keyValue: 16,
                column: "DaysAllocated",
                value: 3);
        }
    }
}
