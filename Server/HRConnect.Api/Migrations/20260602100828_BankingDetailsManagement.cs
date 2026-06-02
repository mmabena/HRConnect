using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class BankingDetailsManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropForeignKey(
            //     name: "FK_EmployeePensionEnrollments_PayrollRuns_PayrollRunId",
            //     table: "EmployeePensionEnrollments");

            // migrationBuilder.DropForeignKey(
            //     name: "FK_Employees_PensionOptions_PensionOptionId",
            //     table: "Employees");

            // migrationBuilder.DropForeignKey(
            //     name: "FK_LeaveEntitlementRules_JobGrades_JobGradeId",
            //     table: "LeaveEntitlementRules");

            // migrationBuilder.DropIndex(
            //     name: "IX_LeaveEntitlementRules_JobGradeId",
            //     table: "LeaveEntitlementRules");

            // migrationBuilder.DropIndex(
            //     name: "IX_EmployeePensionEnrollments_PayrollRunId",
            //     table: "EmployeePensionEnrollments");

            // migrationBuilder.DropPrimaryKey(
            //     name: "PK_TaxTableUpload",
            //     table: "TaxTableUpload");

            // migrationBuilder.DropPrimaryKey(
            //     name: "PK_TaxDeduction",
            //     table: "TaxDeduction");

            // migrationBuilder.DropIndex(
            //     name: "IX_TaxDeduction_TaxYear_Remuneration",
            //     table: "TaxDeduction");

            // migrationBuilder.DropColumn(
            //     name: "JobGradeId",
            //     table: "LeaveEntitlementRules");

            // migrationBuilder.RenameTable(
            //     name: "TaxTableUpload",
            //     newName: "TaxTableUploads");

            // migrationBuilder.RenameTable(
            //     name: "TaxDeduction",
            //     newName: "TaxDeductions");

            // migrationBuilder.AddColumn<string>(
            //     name: "GroupKey",
            //     table: "LeaveEntitlementRules",
            //     type: "nvarchar(max)",
            //     nullable: false,
            //     defaultValue: "");

            // migrationBuilder.AddColumn<int>(
            //     name: "BankingDetailsId",
            //     table: "Employees",
            //     type: "int",
            //     nullable: false,
            //     defaultValue: 0);

            // migrationBuilder.AlterColumn<decimal>(
            //     name: "TaxUnder65",
            //     table: "TaxDeductions",
            //     type: "decimal(18,2)",
            //     nullable: false,
            //     oldClrType: typeof(decimal),
            //     oldType: "decimal(12,2)",
            //     oldPrecision: 12,
            //     oldScale: 2);

            // migrationBuilder.AlterColumn<decimal>(
            //     name: "TaxOver75",
            //     table: "TaxDeductions",
            //     type: "decimal(18,2)",
            //     nullable: false,
            //     oldClrType: typeof(decimal),
            //     oldType: "decimal(12,2)",
            //     oldPrecision: 12,
            //     oldScale: 2);

            // migrationBuilder.AlterColumn<decimal>(
            //     name: "Tax65To74",
            //     table: "TaxDeductions",
            //     type: "decimal(18,2)",
            //     nullable: false,
            //     oldClrType: typeof(decimal),
            //     oldType: "decimal(12,2)",
            //     oldPrecision: 12,
            //     oldScale: 2);

            // migrationBuilder.AlterColumn<decimal>(
            //     name: "AnnualEquivalent",
            //     table: "TaxDeductions",
            //     type: "decimal(18,2)",
            //     nullable: false,
            //     oldClrType: typeof(decimal),
            //     oldType: "decimal(12,2)",
            //     oldPrecision: 12,
            //     oldScale: 2);

            // migrationBuilder.AddPrimaryKey(
            //     name: "PK_TaxTableUploads",
            //     table: "TaxTableUploads",
            //     column: "Id");

            // migrationBuilder.AddPrimaryKey(
            //     name: "PK_TaxDeductions",
            //     table: "TaxDeductions",
            //     column: "Id");

            migrationBuilder.CreateTable(
                name: "BankBranchCodes",
                columns: table => new
                {
                    BankBranchCodeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UniversalCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankBranchCodes", x => x.BankBranchCodeId);
                });

            // migrationBuilder.CreateTable(
            //     name: "JobGradeGroupMaps",
            //     columns: table => new
            //     {
            //         Id = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         JobGradeId = table.Column<int>(type: "int", nullable: false),
            //         GroupKey = table.Column<string>(type: "nvarchar(450)", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_JobGradeGroupMaps", x => x.Id);
            //         table.ForeignKey(
            //             name: "FK_JobGradeGroupMaps_JobGrades_JobGradeId",
            //             column: x => x.JobGradeId,
            //             principalTable: "JobGrades",
            //             principalColumn: "JobGradeId",
            //             onDelete: ReferentialAction.Cascade);
            //     });

            migrationBuilder.CreateTable(
                name: "BankingDetails",
                columns: table => new
                {
                    BankingDetailsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdNumber = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    PassportNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountNumberEncrypted = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountNumberSearchHash = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AccountNumberLast4Digits = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    PayFrequency = table.Column<int>(type: "int", nullable: false),
                    ReferenceType = table.Column<int>(type: "int", nullable: false),
                    BankBranchCodeId = table.Column<int>(type: "int", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    LockedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NetSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankingDetails", x => x.BankingDetailsId);
                    table.ForeignKey(
                        name: "FK_BankingDetails_BankBranchCodes_BankBranchCodeId",
                        column: x => x.BankBranchCodeId,
                        principalTable: "BankBranchCodes",
                        principalColumn: "BankBranchCodeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BankingDetails_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankingDetails_AccountNumberSearchHash",
                table: "BankingDetails",
                column: "AccountNumberSearchHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankingDetails_BankBranchCodeId",
                table: "BankingDetails",
                column: "BankBranchCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_BankingDetails_EmployeeId",
                table: "BankingDetails",
                column: "EmployeeId",
                unique: true);

            // migrationBuilder.CreateIndex(
            //     name: "IX_JobGradeGroupMaps_JobGradeId_GroupKey",
            //     table: "JobGradeGroupMaps",
            //     columns: new[] { "JobGradeId", "GroupKey" },
            //     unique: true);

            // migrationBuilder.AddForeignKey(
            //     name: "FK_Employees_PensionOptions_PensionOptionId",
            //     table: "Employees",
            //     column: "PensionOptionId",
            //     principalTable: "PensionOptions",
            //     principalColumn: "PensionOptionId",
            //     onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropForeignKey(
            //     name: "FK_Employees_PensionOptions_PensionOptionId",
            //     table: "Employees");

            migrationBuilder.DropTable(
                name: "BankingDetails");

            // migrationBuilder.DropTable(
            //     name: "JobGradeGroupMaps");

            migrationBuilder.DropTable(
                name: "BankBranchCodes");

            // migrationBuilder.DropPrimaryKey(
            //     name: "PK_TaxTableUploads",
            //     table: "TaxTableUploads");

            // migrationBuilder.DropPrimaryKey(
            //     name: "PK_TaxDeductions",
            //     table: "TaxDeductions");

            // migrationBuilder.DropColumn(
            //     name: "GroupKey",
            //     table: "LeaveEntitlementRules");

            // migrationBuilder.DropColumn(
            //     name: "BankingDetailsId",
            //     table: "Employees");

            // migrationBuilder.RenameTable(
            //     name: "TaxTableUploads",
            //     newName: "TaxTableUpload");

            // migrationBuilder.RenameTable(
            //     name: "TaxDeductions",
            //     newName: "TaxDeduction");

            // migrationBuilder.AddColumn<int>(
            //     name: "JobGradeId",
            //     table: "LeaveEntitlementRules",
            //     type: "int",
            //     nullable: false,
            //     defaultValue: 0);

            // migrationBuilder.AlterColumn<decimal>(
            //     name: "TaxUnder65",
            //     table: "TaxDeduction",
            //     type: "decimal(12,2)",
            //     precision: 12,
            //     scale: 2,
            //     nullable: false,
            //     oldClrType: typeof(decimal),
            //     oldType: "decimal(18,2)");

            // migrationBuilder.AlterColumn<decimal>(
            //     name: "TaxOver75",
            //     table: "TaxDeduction",
            //     type: "decimal(12,2)",
            //     precision: 12,
            //     scale: 2,
            //     nullable: false,
            //     oldClrType: typeof(decimal),
            //     oldType: "decimal(18,2)");

            // migrationBuilder.AlterColumn<decimal>(
            //     name: "Tax65To74",
            //     table: "TaxDeduction",
            //     type: "decimal(12,2)",
            //     precision: 12,
            //     scale: 2,
            //     nullable: false,
            //     oldClrType: typeof(decimal),
            //     oldType: "decimal(18,2)");

            // migrationBuilder.AlterColumn<decimal>(
            //     name: "AnnualEquivalent",
            //     table: "TaxDeduction",
            //     type: "decimal(12,2)",
            //     precision: 12,
            //     scale: 2,
            //     nullable: false,
            //     oldClrType: typeof(decimal),
            //     oldType: "decimal(18,2)");

            // migrationBuilder.AddPrimaryKey(
            //     name: "PK_TaxTableUpload",
            //     table: "TaxTableUpload",
            //     column: "Id");

            // migrationBuilder.AddPrimaryKey(
            //     name: "PK_TaxDeduction",
            //     table: "TaxDeduction",
            //     column: "Id");

            // migrationBuilder.CreateIndex(
            //     name: "IX_LeaveEntitlementRules_JobGradeId",
            //     table: "LeaveEntitlementRules",
            //     column: "JobGradeId");

            // migrationBuilder.CreateIndex(
            //     name: "IX_EmployeePensionEnrollments_PayrollRunId",
            //     table: "EmployeePensionEnrollments",
            //     column: "PayrollRunId");

            // migrationBuilder.CreateIndex(
            //     name: "IX_TaxDeduction_TaxYear_Remuneration",
            //     table: "TaxDeduction",
            //     columns: new[] { "TaxYear", "Remuneration" },
            //     unique: true);

            // migrationBuilder.AddForeignKey(
            //     name: "FK_EmployeePensionEnrollments_PayrollRuns_PayrollRunId",
            //     table: "EmployeePensionEnrollments",
            //     column: "PayrollRunId",
            //     principalTable: "PayrollRuns",
            //     principalColumn: "PayrollRunId",
            //     onDelete: ReferentialAction.Cascade);

            // migrationBuilder.AddForeignKey(
            //     name: "FK_Employees_PensionOptions_PensionOptionId",
            //     table: "Employees",
            //     column: "PensionOptionId",
            //     principalTable: "PensionOptions",
            //     principalColumn: "PensionOptionId",
            //     onDelete: ReferentialAction.SetNull);

            // migrationBuilder.AddForeignKey(
            //     name: "FK_LeaveEntitlementRules_JobGrades_JobGradeId",
            //     table: "LeaveEntitlementRules",
            //     column: "JobGradeId",
            //     principalTable: "JobGrades",
            //     principalColumn: "JobGradeId",
            //     onDelete: ReferentialAction.Restrict);
        }
    }
}
