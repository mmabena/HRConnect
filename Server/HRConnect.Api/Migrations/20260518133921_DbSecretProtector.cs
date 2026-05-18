using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
  /// <inheritdoc />
  public partial class DbSecretProtector : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AlterColumn<int>(
          name: "Severity",
          table: "Notifications",
          type: "int",
          nullable: false,
          oldClrType: typeof(string),
          oldType: "nvarchar(max)");

      migrationBuilder.AlterColumn<string>(
  name: "DeliveryChannel",
  table: "Notifications",
  type: "nvarchar(50)",
  nullable: false,
  oldClrType: typeof(int),
  oldType: "int");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AlterColumn<string>(
          name: "Severity",
          table: "Notifications",
          type: "nvarchar(max)",
          nullable: false,
          oldClrType: typeof(int),
          oldType: "int");

      migrationBuilder.AlterColumn<int>(
name: "DeliveryChannel",
table: "Notifications",
type: "int",
nullable: false,
oldClrType: typeof(string),
oldType: "nvarchar(50)");
    }
  }
}
