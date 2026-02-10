using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class SelfReferencingPCategoryFieldV5RemoveSFKOnMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalOptionCategories_MedicalOptionCategories_ParentCategoryIdMedicalOptionCategoryId",
                table: "MedicalOptionCategories");

            migrationBuilder.RenameColumn(
                name: "ParentCategoryIdMedicalOptionCategoryId",
                table: "MedicalOptionCategories",
                newName: "MedicalOptionParentCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalOptionCategories_ParentCategoryIdMedicalOptionCategoryId",
                table: "MedicalOptionCategories",
                newName: "IX_MedicalOptionCategories_MedicalOptionParentCategoryId");

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

            migrationBuilder.RenameColumn(
                name: "MedicalOptionParentCategoryId",
                table: "MedicalOptionCategories",
                newName: "ParentCategoryIdMedicalOptionCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalOptionCategories_MedicalOptionParentCategoryId",
                table: "MedicalOptionCategories",
                newName: "IX_MedicalOptionCategories_ParentCategoryIdMedicalOptionCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalOptionCategories_MedicalOptionCategories_ParentCategoryIdMedicalOptionCategoryId",
                table: "MedicalOptionCategories",
                column: "ParentCategoryIdMedicalOptionCategoryId",
                principalTable: "MedicalOptionCategories",
                principalColumn: "MedicalOptionCategoryId");
        }
    }
}
