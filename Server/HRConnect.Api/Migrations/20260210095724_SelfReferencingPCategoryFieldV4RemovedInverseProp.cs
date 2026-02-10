using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class SelfReferencingPCategoryFieldV4RemovedInverseProp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalOptionCategories_MedicalOptionCategories_ParentCategoryMedicalOptionCategoryId",
                table: "MedicalOptionCategories");

            migrationBuilder.DropColumn(
                name: "MedicalOptionParentCategoryId",
                table: "MedicalOptionCategories");

            migrationBuilder.RenameColumn(
                name: "ParentCategoryMedicalOptionCategoryId",
                table: "MedicalOptionCategories",
                newName: "ParentCategoryIdMedicalOptionCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalOptionCategories_ParentCategoryMedicalOptionCategoryId",
                table: "MedicalOptionCategories",
                newName: "IX_MedicalOptionCategories_ParentCategoryIdMedicalOptionCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalOptionCategories_MedicalOptionCategories_ParentCategoryIdMedicalOptionCategoryId",
                table: "MedicalOptionCategories",
                column: "ParentCategoryIdMedicalOptionCategoryId",
                principalTable: "MedicalOptionCategories",
                principalColumn: "MedicalOptionCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalOptionCategories_MedicalOptionCategories_ParentCategoryIdMedicalOptionCategoryId",
                table: "MedicalOptionCategories");

            migrationBuilder.RenameColumn(
                name: "ParentCategoryIdMedicalOptionCategoryId",
                table: "MedicalOptionCategories",
                newName: "ParentCategoryMedicalOptionCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalOptionCategories_ParentCategoryIdMedicalOptionCategoryId",
                table: "MedicalOptionCategories",
                newName: "IX_MedicalOptionCategories_ParentCategoryMedicalOptionCategoryId");

            migrationBuilder.AddColumn<int>(
                name: "MedicalOptionParentCategoryId",
                table: "MedicalOptionCategories",
                type: "int",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalOptionCategories_MedicalOptionCategories_ParentCategoryMedicalOptionCategoryId",
                table: "MedicalOptionCategories",
                column: "ParentCategoryMedicalOptionCategoryId",
                principalTable: "MedicalOptionCategories",
                principalColumn: "MedicalOptionCategoryId");
        }
    }
}
