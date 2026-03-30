using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEffectiveFrom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TaxTableUpload_TaxYear",
                table: "TaxTableUpload");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TaxTableUpload");

            migrationBuilder.RenameColumn(
                name: "EffectiveDate",
                table: "TaxTableUpload",
                newName: "EffectiveFrom");

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveTo",
                table: "TaxTableUpload",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EffectiveTo",
                table: "TaxTableUpload");

            migrationBuilder.RenameColumn(
                name: "EffectiveFrom",
                table: "TaxTableUpload",
                newName: "EffectiveDate");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TaxTableUpload",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxTableUpload_TaxYear",
                table: "TaxTableUpload",
                column: "TaxYear",
                unique: true,
                filter: "[IsActive] = 1");
        }
    }
}
