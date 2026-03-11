using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class MergeUpdatePayrollAndMedicalAid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "MedicalAidDeductions");

            migrationBuilder.DropColumn(
                name: "TotalDependentsPremium",
                table: "MedicalAidDeductions");

            migrationBuilder.RenameColumn(
                name: "MedicalOptionCategoryId",
                table: "MedicalAidDeductions",
                newName: "PrincipalCount");

            migrationBuilder.RenameColumn(
                name: "FinalisedDate",
                table: "MedicalAidDeductions",
                newName: "CreatedDate");

            migrationBuilder.AlterColumn<decimal>(
                name: "SpousePremium",
                table: "MedicalAidDeductions",
                type: "decimal(15,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PrincipalPremium",
                table: "MedicalAidDeductions",
                type: "decimal(15,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ChildPremium",
                table: "MedicalAidDeductions",
                type: "decimal(15,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AdultCount",
                table: "MedicalAidDeductions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Branch",
                table: "MedicalAidDeductions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ChildrenCount",
                table: "MedicalAidDeductions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MedicalAidDeductionId",
                table: "MedicalAidDeductions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MedicalCategoryId",
                table: "MedicalAidDeductions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "MedicalAidDeductions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Salary",
                table: "MedicalAidDeductions",
                type: "decimal(15,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Surname",
                table: "MedicalAidDeductions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDeductionAmount",
                table: "MedicalAidDeductions",
                type: "decimal(15,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalAidDeductions_MedicalCategoryId",
                table: "MedicalAidDeductions",
                column: "MedicalCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalAidDeductions_MedicalOptionId",
                table: "MedicalAidDeductions",
                column: "MedicalOptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalAidDeductions_MedicalOptionCategories_MedicalCategoryId",
                table: "MedicalAidDeductions",
                column: "MedicalCategoryId",
                principalTable: "MedicalOptionCategories",
                principalColumn: "MedicalOptionCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalAidDeductions_MedicalOptions_MedicalOptionId",
                table: "MedicalAidDeductions",
                column: "MedicalOptionId",
                principalTable: "MedicalOptions",
                principalColumn: "MedicalOptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalAidDeductions_MedicalOptionCategories_MedicalCategoryId",
                table: "MedicalAidDeductions");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalAidDeductions_MedicalOptions_MedicalOptionId",
                table: "MedicalAidDeductions");

            migrationBuilder.DropIndex(
                name: "IX_MedicalAidDeductions_MedicalCategoryId",
                table: "MedicalAidDeductions");

            migrationBuilder.DropIndex(
                name: "IX_MedicalAidDeductions_MedicalOptionId",
                table: "MedicalAidDeductions");

            migrationBuilder.DropColumn(
                name: "AdultCount",
                table: "MedicalAidDeductions");

            migrationBuilder.DropColumn(
                name: "Branch",
                table: "MedicalAidDeductions");

            migrationBuilder.DropColumn(
                name: "ChildrenCount",
                table: "MedicalAidDeductions");

            migrationBuilder.DropColumn(
                name: "MedicalAidDeductionId",
                table: "MedicalAidDeductions");

            migrationBuilder.DropColumn(
                name: "MedicalCategoryId",
                table: "MedicalAidDeductions");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "MedicalAidDeductions");

            migrationBuilder.DropColumn(
                name: "Salary",
                table: "MedicalAidDeductions");

            migrationBuilder.DropColumn(
                name: "Surname",
                table: "MedicalAidDeductions");

            migrationBuilder.DropColumn(
                name: "TotalDeductionAmount",
                table: "MedicalAidDeductions");

            migrationBuilder.RenameColumn(
                name: "PrincipalCount",
                table: "MedicalAidDeductions",
                newName: "MedicalOptionCategoryId");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "MedicalAidDeductions",
                newName: "FinalisedDate");

            migrationBuilder.AlterColumn<decimal>(
                name: "SpousePremium",
                table: "MedicalAidDeductions",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PrincipalPremium",
                table: "MedicalAidDeductions",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ChildPremium",
                table: "MedicalAidDeductions",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "MedicalAidDeductions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDependentsPremium",
                table: "MedicalAidDeductions",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
