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

    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "PayrollEarnings");

    }
  }
}