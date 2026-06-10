using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class MedicalAidDeductionFeatureUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "MedicalAidDeductions");

            migrationBuilder.DropColumn(
                name: "FinalisedDate",
                table: "MedicalAidDeductions");

            migrationBuilder.DropColumn(
                name: "MedicalAidDeductionId",
                table: "MedicalAidDeductions");

            migrationBuilder.DropColumn(
                name: "MedicalOptionCategoryId",
                table: "MedicalAidDeductions");

            migrationBuilder.DropColumn(
                name: "OptionCategory",
                table: "MedicalAidDeductions");

            migrationBuilder.DropColumn(
                name: "TotalDependentsPremium",
                table: "MedicalAidDeductions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "MedicalAidDeductions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FinalisedDate",
                table: "MedicalAidDeductions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "MedicalAidDeductionId",
                table: "MedicalAidDeductions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MedicalOptionCategoryId",
                table: "MedicalAidDeductions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OptionCategory",
                table: "MedicalAidDeductions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDependentsPremium",
                table: "MedicalAidDeductions",
                type: "decimal(15,2)",
                nullable: true);
        }
    }
}
