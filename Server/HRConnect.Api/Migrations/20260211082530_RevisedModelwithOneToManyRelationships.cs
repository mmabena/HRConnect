using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class RevisedModelwithOneToManyRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalOptionCategories_MedicalOptionCategories_MedicalOptionParentCategoryId",
                table: "MedicalOptionCategories");

            migrationBuilder.DropIndex(
                name: "IX_MedicalOptions_MedicalOptionCategoryId",
                table: "MedicalOptions");

            migrationBuilder.DropIndex(
                name: "IX_MedicalOptionCategories_MedicalOptionParentCategoryId",
                table: "MedicalOptionCategories");

            migrationBuilder.DropColumn(
                name: "MedicalOptionParentCategoryId",
                table: "MedicalOptionCategories");

            migrationBuilder.DropColumn(
                name: "MonthlyMsaContributionAdult",
                table: "MedicalOptionCategories");

            migrationBuilder.DropColumn(
                name: "MonthlyMsaContributionChild",
                table: "MedicalOptionCategories");

            migrationBuilder.DropColumn(
                name: "MonthlyMsaContributionPrincipal",
                table: "MedicalOptionCategories");

            migrationBuilder.DropColumn(
                name: "MonthlyRiskContributionAdult",
                table: "MedicalOptionCategories");

            migrationBuilder.DropColumn(
                name: "MonthlyRiskContributionChild",
                table: "MedicalOptionCategories");

            migrationBuilder.DropColumn(
                name: "MonthlyRiskContributionChild2",
                table: "MedicalOptionCategories");

            migrationBuilder.DropColumn(
                name: "MonthlyRiskContributionPrincipal",
                table: "MedicalOptionCategories");

            migrationBuilder.DropColumn(
                name: "TotalMonthlyContributionsAdult",
                table: "MedicalOptionCategories");

            migrationBuilder.DropColumn(
                name: "TotalMonthlyContributionsChild",
                table: "MedicalOptionCategories");

            migrationBuilder.DropColumn(
                name: "TotalMonthlyContributionsChild2",
                table: "MedicalOptionCategories");

            migrationBuilder.DropColumn(
                name: "TotalMonthlyContributionsPrincipal",
                table: "MedicalOptionCategories");

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyMsaContributionAdult",
                table: "MedicalOptions",
                type: "decimal(15,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyMsaContributionChild",
                table: "MedicalOptions",
                type: "decimal(15,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyMsaContributionPrincipal",
                table: "MedicalOptions",
                type: "decimal(15,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyRiskContributionAdult",
                table: "MedicalOptions",
                type: "decimal(15,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyRiskContributionChild",
                table: "MedicalOptions",
                type: "decimal(15,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyRiskContributionChild2",
                table: "MedicalOptions",
                type: "decimal(15,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyRiskContributionPrincipal",
                table: "MedicalOptions",
                type: "decimal(15,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalMonthlyContributionsAdult",
                table: "MedicalOptions",
                type: "decimal(15,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalMonthlyContributionsChild",
                table: "MedicalOptions",
                type: "decimal(15,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalMonthlyContributionsChild2",
                table: "MedicalOptions",
                type: "decimal(15,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalMonthlyContributionsPrincipal",
                table: "MedicalOptions",
                type: "decimal(15,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalOptions_MedicalOptionCategoryId",
                table: "MedicalOptions",
                column: "MedicalOptionCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MedicalOptions_MedicalOptionCategoryId",
                table: "MedicalOptions");

            migrationBuilder.DropColumn(
                name: "MonthlyMsaContributionAdult",
                table: "MedicalOptions");

            migrationBuilder.DropColumn(
                name: "MonthlyMsaContributionChild",
                table: "MedicalOptions");

            migrationBuilder.DropColumn(
                name: "MonthlyMsaContributionPrincipal",
                table: "MedicalOptions");

            migrationBuilder.DropColumn(
                name: "MonthlyRiskContributionAdult",
                table: "MedicalOptions");

            migrationBuilder.DropColumn(
                name: "MonthlyRiskContributionChild",
                table: "MedicalOptions");

            migrationBuilder.DropColumn(
                name: "MonthlyRiskContributionChild2",
                table: "MedicalOptions");

            migrationBuilder.DropColumn(
                name: "MonthlyRiskContributionPrincipal",
                table: "MedicalOptions");

            migrationBuilder.DropColumn(
                name: "TotalMonthlyContributionsAdult",
                table: "MedicalOptions");

            migrationBuilder.DropColumn(
                name: "TotalMonthlyContributionsChild",
                table: "MedicalOptions");

            migrationBuilder.DropColumn(
                name: "TotalMonthlyContributionsChild2",
                table: "MedicalOptions");

            migrationBuilder.DropColumn(
                name: "TotalMonthlyContributionsPrincipal",
                table: "MedicalOptions");

            migrationBuilder.AddColumn<int>(
                name: "MedicalOptionParentCategoryId",
                table: "MedicalOptionCategories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyMsaContributionAdult",
                table: "MedicalOptionCategories",
                type: "decimal(15,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyMsaContributionChild",
                table: "MedicalOptionCategories",
                type: "decimal(15,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyMsaContributionPrincipal",
                table: "MedicalOptionCategories",
                type: "decimal(15,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyRiskContributionAdult",
                table: "MedicalOptionCategories",
                type: "decimal(15,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyRiskContributionChild",
                table: "MedicalOptionCategories",
                type: "decimal(15,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyRiskContributionChild2",
                table: "MedicalOptionCategories",
                type: "decimal(15,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyRiskContributionPrincipal",
                table: "MedicalOptionCategories",
                type: "decimal(15,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalMonthlyContributionsAdult",
                table: "MedicalOptionCategories",
                type: "decimal(15,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalMonthlyContributionsChild",
                table: "MedicalOptionCategories",
                type: "decimal(15,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalMonthlyContributionsChild2",
                table: "MedicalOptionCategories",
                type: "decimal(15,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalMonthlyContributionsPrincipal",
                table: "MedicalOptionCategories",
                type: "decimal(15,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalOptions_MedicalOptionCategoryId",
                table: "MedicalOptions",
                column: "MedicalOptionCategoryId",
                unique: true);

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
    }
}
