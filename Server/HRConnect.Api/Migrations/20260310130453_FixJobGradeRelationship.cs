using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixJobGradeRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveEntitlementRules_LeaveTypes_LeaveTypeId",
                table: "LeaveEntitlementRules");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeAccrualRateHistories_EmployeeId",
                table: "EmployeeAccrualRateHistories");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Positions",
                newName: "PositionTitle");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "JobGrades",
                newName: "JobGradeId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Positions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Positions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "JobGradeId1",
                table: "Positions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OccupationalLevelId",
                table: "Positions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Positions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<decimal>(
                name: "MinYearsService",
                table: "LeaveEntitlementRules",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "MaxYearsService",
                table: "LeaveEntitlementRules",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DaysAllocated",
                table: "LeaveEntitlementRules",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveApplications_Employees_EmployeeId",
                table: "LeaveApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeLeaveBalances_Employees_EmployeeId",
                table: "EmployeeLeaveBalances");

            migrationBuilder.DropForeignKey(
                name: "FK_AnnualLeaveAccrualHistories_Employees_EmployeeId",
                table: "AnnualLeaveAccrualHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAccrualRateHistories_Employees_EmployeeId",
                table: "EmployeeAccrualRateHistories");

            migrationBuilder.DropIndex(
                name: "IX_LeaveApplications_EmployeeId_StartDate_EndDate",
                table: "LeaveApplications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Employees",
                table: "Employees");


            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "Employees",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");


            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "LeaveApplications",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "EmployeeLeaveBalances",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "EmployeeAccrualRateHistories",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "AnnualLeaveAccrualHistories",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Employees",
                table: "Employees",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveApplications_EmployeeId_StartDate_EndDate",
                table: "LeaveApplications",
                columns: new[] { "EmployeeId", "StartDate", "EndDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveApplications_Employees_EmployeeId",
                table: "LeaveApplications",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeLeaveBalances_Employees_EmployeeId",
                table: "EmployeeLeaveBalances",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AnnualLeaveAccrualHistories_Employees_EmployeeId",
                table: "AnnualLeaveAccrualHistories",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAccrualRateHistories_Employees_EmployeeId",
                table: "EmployeeAccrualRateHistories",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AlterColumn<decimal>(
                name: "TakenDays",
                table: "EmployeeLeaveBalances",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ForfeitedDays",
                table: "EmployeeLeaveBalances",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CarryoverDays",
                table: "EmployeeLeaveBalances",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "AvailableDays",
                table: "EmployeeLeaveBalances",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "AccruedDays",
                table: "EmployeeLeaveBalances",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");


            migrationBuilder.AlterColumn<decimal>(
                name: "AnnualEntitlement",
                table: "EmployeeAccrualRateHistories",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Used",
                table: "AnnualLeaveAccrualHistories",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "OpeningBalance",
                table: "AnnualLeaveAccrualHistories",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Forfeited",
                table: "AnnualLeaveAccrualHistories",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");


            migrationBuilder.AlterColumn<decimal>(
                name: "ClosingBalance",
                table: "AnnualLeaveAccrualHistories",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Accrued",
                table: "AnnualLeaveAccrualHistories",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.CreateTable(
                name: "OccupationalLevel",
                columns: table => new
                {
                    OccupationalLevelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OccupationalLevel", x => x.OccupationalLevelId);
                });

            migrationBuilder.InsertData(
                table: "OccupationalLevel",
                columns: new[] { "OccupationalLevelId", "Description" },
                values: new object[] { 0, "Unknown" });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "PositionId",
                keyValue: 1,
                columns: new[] { "CreatedDate", "IsActive", "JobGradeId1", "OccupationalLevelId", "UpdatedDate" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "PositionId",
                keyValue: 2,
                columns: new[] { "CreatedDate", "IsActive", "JobGradeId1", "OccupationalLevelId", "UpdatedDate" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "PositionId",
                keyValue: 3,
                columns: new[] { "CreatedDate", "IsActive", "JobGradeId1", "OccupationalLevelId", "UpdatedDate" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "PositionId",
                keyValue: 4,
                columns: new[] { "CreatedDate", "IsActive", "JobGradeId1", "OccupationalLevelId", "UpdatedDate" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "PositionId",
                keyValue: 5,
                columns: new[] { "CreatedDate", "IsActive", "JobGradeId1", "OccupationalLevelId", "UpdatedDate" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "PositionId",
                keyValue: 6,
                columns: new[] { "CreatedDate", "IsActive", "JobGradeId1", "OccupationalLevelId", "UpdatedDate" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.CreateIndex(
                name: "IX_Positions_JobGradeId1",
                table: "Positions",
                column: "JobGradeId1");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_OccupationalLevelId",
                table: "Positions",
                column: "OccupationalLevelId");

            migrationBuilder.RenameColumn(
                name: "ReportingManagerId",
                table: "Employees",
                newName: "CareerManagerID");

            migrationBuilder.AlterColumn<string>(
                name: "CareerManagerID",
                table: "Employees",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_CareerManagerID",
                table: "Employees",
                column: "CareerManagerID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAccrualRateHistories_EmployeeId_EffectiveFrom",
                table: "EmployeeAccrualRateHistories",
                columns: new[] { "EmployeeId", "EffectiveFrom" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Employees_CareerManagerID",
                table: "Employees",
                column: "CareerManagerID",
                principalTable: "Employees",
                principalColumn: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveEntitlementRules_LeaveTypes_LeaveTypeId",
                table: "LeaveEntitlementRules",
                column: "LeaveTypeId",
                principalTable: "LeaveTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_JobGrades_JobGradeId1",
                table: "Positions",
                column: "JobGradeId1",
                principalTable: "JobGrades",
                principalColumn: "JobGradeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_OccupationalLevel_OccupationalLevelId",
                table: "Positions",
                column: "OccupationalLevelId",
                principalTable: "OccupationalLevel",
                principalColumn: "OccupationalLevelId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Employees_CareerManagerID",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveEntitlementRules_LeaveTypes_LeaveTypeId",
                table: "LeaveEntitlementRules");

            migrationBuilder.DropForeignKey(
                name: "FK_Positions_JobGrades_JobGradeId1",
                table: "Positions");

            migrationBuilder.DropForeignKey(
                name: "FK_Positions_OccupationalLevel_OccupationalLevelId",
                table: "Positions");

            migrationBuilder.DropTable(
                name: "OccupationalLevel");

            migrationBuilder.DropIndex(
                name: "IX_Positions_JobGradeId1",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_Positions_OccupationalLevelId",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_Employees_CareerManagerID",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeAccrualRateHistories_EmployeeId_EffectiveFrom",
                table: "EmployeeAccrualRateHistories");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "JobGradeId1",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "OccupationalLevelId",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "JobGrades");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "JobGrades");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "JobGrades");

            migrationBuilder.RenameColumn(
                name: "PositionTitle",
                table: "Positions",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "JobGradeId",
                table: "JobGrades",
                newName: "Id");

            migrationBuilder.AlterColumn<decimal>(
                name: "MinYearsService",
                table: "LeaveEntitlementRules",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "MaxYearsService",
                table: "LeaveEntitlementRules",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DaysAllocated",
                table: "LeaveEntitlementRules",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<Guid>(
                name: "EmployeeId",
                table: "LeaveApplications",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<decimal>(
                name: "DaysRequested",
                table: "LeaveApplications",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "CareerManagerID",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "EmployeeId",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<decimal>(
                name: "TakenDays",
                table: "EmployeeLeaveBalances",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "ForfeitedDays",
                table: "EmployeeLeaveBalances",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<Guid>(
                name: "EmployeeId",
                table: "EmployeeLeaveBalances",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CarryoverDays",
                table: "EmployeeLeaveBalances",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "AvailableDays",
                table: "EmployeeLeaveBalances",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "AccruedDays",
                table: "EmployeeLeaveBalances",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<Guid>(
                name: "EmployeeId",
                table: "EmployeeAccrualRateHistories",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<decimal>(
                name: "AnnualEntitlement",
                table: "EmployeeAccrualRateHistories",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "Used",
                table: "AnnualLeaveAccrualHistories",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "OpeningBalance",
                table: "AnnualLeaveAccrualHistories",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "Forfeited",
                table: "AnnualLeaveAccrualHistories",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<Guid>(
                name: "EmployeeId",
                table: "AnnualLeaveAccrualHistories",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ClosingBalance",
                table: "AnnualLeaveAccrualHistories",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "Accrued",
                table: "AnnualLeaveAccrualHistories",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAccrualRateHistories_EmployeeId",
                table: "EmployeeAccrualRateHistories",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveEntitlementRules_LeaveTypes_LeaveTypeId",
                table: "LeaveEntitlementRules",
                column: "LeaveTypeId",
                principalTable: "LeaveTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
