using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class OriginalPCMigratePensionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PensionFundId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PensionOptionID",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PensionOptions",
                columns: table => new
                {
                    PensionOptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContributionPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PensionOptions", x => x.PensionOptionId);
                });

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
                name: "IX_Employees_PensionFundId",
                table: "Employees",
                column: "PensionFundId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PensionOptionID",
                table: "Employees",
                column: "PensionOptionID");

            migrationBuilder.CreateIndex(
                name: "IX_PensionFunds_EmployeeId",
                table: "PensionFunds",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PensionFunds_PensionOptionId",
                table: "PensionFunds",
                column: "PensionOptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_PensionFunds_PensionFundId",
                table: "Employees",
                column: "PensionFundId",
                principalTable: "PensionFunds",
                principalColumn: "PensionFundId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_PensionOptions_PensionOptionID",
                table: "Employees",
                column: "PensionOptionID",
                principalTable: "PensionOptions",
                principalColumn: "PensionOptionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_PensionFunds_PensionFundId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_PensionOptions_PensionOptionID",
                table: "Employees");

            migrationBuilder.DropTable(
                name: "PensionFunds");

            migrationBuilder.DropTable(
                name: "PensionOptions");

            migrationBuilder.DropIndex(
                name: "IX_Employees_PensionFundId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_PensionOptionID",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PensionFundId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PensionOptionID",
                table: "Employees");
        }
    }
}
