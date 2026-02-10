using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class SelfReferencingPCategoryFieldV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalOptionCategories_MedicalOptionCategories_ParentCategoryMedicalOptionCategoryId",
                table: "MedicalOptionCategories");

            migrationBuilder.DropIndex(
                name: "IX_MedicalOptionCategories_ParentCategoryMedicalOptionCategoryId",
                table: "MedicalOptionCategories");

            migrationBuilder.DropColumn(
                name: "ParentCategoryMedicalOptionCategoryId",
                table: "MedicalOptionCategories");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalOptionCategories_MedicalOptionParentCategoryId",
                table: "MedicalOptionCategories",
                column: "MedicalOptionParentCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalOptionCategories_MedicalOptionCategories_MedicalOptionParentCategoryId",
                table: "MedicalOptionCategories",
                column: "MedicalOptionParentCategoryId",
                principalTable: "MedicalOptionCategories",
                principalColumn: "MedicalOptionCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalOptionCategories_MedicalOptionCategories_MedicalOptionParentCategoryId",
                table: "MedicalOptionCategories");

            migrationBuilder.DropIndex(
                name: "IX_MedicalOptionCategories_MedicalOptionParentCategoryId",
                table: "MedicalOptionCategories");

            migrationBuilder.AddColumn<int>(
                name: "ParentCategoryMedicalOptionCategoryId",
                table: "MedicalOptionCategories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalOptionCategories_ParentCategoryMedicalOptionCategoryId",
                table: "MedicalOptionCategories",
                column: "ParentCategoryMedicalOptionCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalOptionCategories_MedicalOptionCategories_ParentCategoryMedicalOptionCategoryId",
                table: "MedicalOptionCategories",
                column: "ParentCategoryMedicalOptionCategoryId",
                principalTable: "MedicalOptionCategories",
                principalColumn: "MedicalOptionCategoryId");
        }
    }
}
