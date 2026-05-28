using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
  /// <inheritdoc />
  public partial class AddCompanyContributions : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "CompanyContributions",
          columns: table => new
          {
            CompanyContributionId = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
            ShortDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
            LongDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
            TaxCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Percentage = table.Column<decimal>(type: "decimal(10,6)", nullable: false),
            IsActive = table.Column<bool>(type: "bit", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_CompanyContributions", x => x.CompanyContributionId);
          });

      migrationBuilder.CreateTable(
          name: "EmployeeCompanyContributions",
          columns: table => new
          {
            Id = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [PayrollRecordSequence]"),
            PayrollRunId = table.Column<int>(type: "int", nullable: false),
            IsLocked = table.Column<bool>(type: "bit", nullable: false),
            EmployeeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
            IdNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            PassportNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Age = table.Column<int>(type: "int", nullable: false),
            Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            DeathAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            DeathPercentage = table.Column<decimal>(type: "decimal(10,6)", nullable: false),
            DisabilityAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            DisabilityPercentage = table.Column<decimal>(type: "decimal(10,6)", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_EmployeeCompanyContributions", x => x.Id);
            table.ForeignKey(
                      name: "FK_EmployeeCompanyContributions_PayrollRuns_PayrollRunId",
                      column: x => x.PayrollRunId,
                      principalTable: "PayrollRuns",
                      principalColumn: "PayrollRunId",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateIndex(
          name: "IX_EmployeeCompanyContributions_PayrollRunId",
          table: "EmployeeCompanyContributions",
          column: "PayrollRunId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "CompanyContributions");

      migrationBuilder.DropTable(
          name: "EmployeeCompanyContributions");
    }
  }
}
