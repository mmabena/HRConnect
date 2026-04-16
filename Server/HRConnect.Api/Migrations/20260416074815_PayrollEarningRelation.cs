using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class PayrollEarningRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PayrollEarnings",
                columns: table => new
                {
                    PayrollEarningId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ShortDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LongDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Taxable = table.Column<bool>(type: "bit", nullable: false),
                    TaxCode = table.Column<int>(type: "int", nullable: false),
                    TaxPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OvertimeHourMultiplier = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CanProRata = table.Column<bool>(type: "bit", nullable: false),
                    IsOnGoing = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollEarnings", x => x.PayrollEarningId);
                });

            migrationBuilder.CreateTable(
                name: "EmployeePayrollEarnings",
                columns: table => new
                {
                    EmployeePayrollEarningId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PayrollEarningId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TaxCode = table.Column<int>(type: "int", nullable: false),
                    OverTimeHoursWorked = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PayrollRunId = table.Column<int>(type: "int", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeePayrollEarnings", x => x.EmployeePayrollEarningId);
                    table.ForeignKey(
                        name: "FK_EmployeePayrollEarnings_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeePayrollEarnings_PayrollEarnings_PayrollEarningId",
                        column: x => x.PayrollEarningId,
                        principalTable: "PayrollEarnings",
                        principalColumn: "PayrollEarningId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeePayrollEarnings_PayrollRuns_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalTable: "PayrollRuns",
                        principalColumn: "PayrollRunId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePayrollEarnings_EmployeeId",
                table: "EmployeePayrollEarnings",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePayrollEarnings_PayrollEarningId",
                table: "EmployeePayrollEarnings",
                column: "PayrollEarningId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePayrollEarnings_PayrollRunId",
                table: "EmployeePayrollEarnings",
                column: "PayrollRunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeePayrollEarnings");

            migrationBuilder.DropTable(
                name: "PayrollEarnings");
        }
    }
}
