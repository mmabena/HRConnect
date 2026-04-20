using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class PensionFund : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PensionFunds",
                columns: table => new
                {
                    PensionFundId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EmployeeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MonthlySalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ContributionPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ContributionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxCode = table.Column<int>(type: "int", nullable: false),
                    PensionOptionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PensionFunds", x => x.PensionFundId);
                    table.ForeignKey(
                        name: "FK_PensionFunds_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PensionFunds_PensionOptions_PensionOptionId",
                        column: x => x.PensionOptionId,
                        principalTable: "PensionOptions",
                        principalColumn: "PensionOptionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PensionFunds_EmployeeId",
                table: "PensionFunds",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PensionFunds_PensionOptionId",
                table: "PensionFunds",
                column: "PensionOptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PensionFunds");
        }
    }
}
