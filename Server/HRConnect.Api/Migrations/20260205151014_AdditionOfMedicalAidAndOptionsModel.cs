using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdditionOfMedicalAidAndOptionsModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MedicalOptionCategories",
                columns: table => new
                {
                    MedicalOptionCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicalOptionCategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MonthlyRiskContributionPrincipal = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    MonthlyRiskContributionAdult = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    MonthlyRiskContributionChild = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    MonthlyRiskContributionChild2 = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    MonthlyMsaContributionPrincipal = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    MonthlyMsaContributionAdult = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    MonthlyMsaContributionChild = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    MonthlyMSAContributionChild2 = table.Column<decimal>(type: "decimal(15,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalOptionCategories", x => x.MedicalOptionCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "MedicalOptions",
                columns: table => new
                {
                    MedicalOptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicalOptionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MedicalOptionCategoryId = table.Column<int>(type: "int", nullable: false),
                    SalaryBracketMin = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    SalaryBracketMax = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    PrincipalAmount = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    Child1Amount = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    Child2Amount = table.Column<decimal>(type: "decimal(15,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalOptions", x => x.MedicalOptionId);
                    table.ForeignKey(
                        name: "FK_MedicalOptions_MedicalOptionCategories_MedicalOptionCategoryId",
                        column: x => x.MedicalOptionCategoryId,
                        principalTable: "MedicalOptionCategories",
                        principalColumn: "MedicalOptionCategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicalOptions_MedicalOptionCategoryId",
                table: "MedicalOptions",
                column: "MedicalOptionCategoryId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicalOptions");

            migrationBuilder.DropTable(
                name: "MedicalOptionCategories");
        }
    }
}
