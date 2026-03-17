using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class MergeUpdateWithQuartNamespaceUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PayrollRun_PayrollRunNumber",
                table: "PayrollRuns");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "MedicalAidDeductions");

            migrationBuilder.DropColumn(
                name: "MedicalOptionCategoryId",
                table: "MedicalAidDeductions");

            migrationBuilder.DropColumn(
                name: "TotalDependentsPremium",
                table: "MedicalAidDeductions");

            migrationBuilder.RenameColumn(
                name: "OptionCategory",
                table: "MedicalAidDeductions",
                newName: "OptionCategoryName");

            migrationBuilder.RenameColumn(
                name: "FinalisedDate",
                table: "MedicalAidDeductions",
                newName: "UpdatedDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "MedicalAidDeductions",
                newName: "FinalisedDate");

            migrationBuilder.RenameColumn(
                name: "OptionCategoryName",
                table: "MedicalAidDeductions",
                newName: "OptionCategory");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "MedicalAidDeductions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "MedicalOptionCategoryId",
                table: "MedicalAidDeductions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDependentsPremium",
                table: "MedicalAidDeductions",
                type: "decimal(15,2)",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_PayrollRun_PayrollRunNumber",
                table: "PayrollRuns",
                sql: "[PayrollRunNumber] BETWEEN 1 AND 12");
        }
    }
}
