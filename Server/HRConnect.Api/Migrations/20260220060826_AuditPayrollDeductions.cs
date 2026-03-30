using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
  /// <inheritdoc />
  public partial class AuditPayrollDeductions : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.RenameColumn(
          name: "SdlAmount",
          table: "PayrollDeductions",
          newName: "EmployerSdlContribution");

      migrationBuilder.CreateTable(
          name: "AuditPayrollDeductions",
          columns: table => new
          {
            AuditId = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            EmployeeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
            IdNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            PassportNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            MonthlySalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
            ProjectedSalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
            UifEmployeeAmount = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
            UifEmployerAmount = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
            EmployerSdlContribution = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
            AuditAction = table.Column<string>(type: "nvarchar(max)", nullable: false),
            TabelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            AuditedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_AuditPayrollDeductions", x => x.AuditId);
          });

    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "AuditPayrollDeductions");

      migrationBuilder.DropTable(
          name: "TaxDeduction");

      migrationBuilder.DropTable(
          name: "TaxTableUpload");

      migrationBuilder.RenameColumn(
          name: "EmployerSdlContribution",
          table: "PayrollDeductions",
          newName: "SdlAmount");
    }
  }
}
