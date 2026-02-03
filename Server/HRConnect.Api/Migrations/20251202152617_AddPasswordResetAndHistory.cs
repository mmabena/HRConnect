using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
  /// <inheritdoc />
  public partial class AddPasswordResetAndHistory : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "PasswordHistories",
          columns: table => new
          {
            Id = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            UserId = table.Column<int>(type: "int", nullable: false),
            PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
            ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_PasswordHistories", x => x.Id);
          });

      migrationBuilder.CreateTable(
          name: "PasswordResetPins",
          columns: table => new
          {
            Id = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            UserId = table.Column<int>(type: "int", nullable: false),
            Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Pin = table.Column<string>(type: "nvarchar(max)", nullable: false),
            ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            IsUsed = table.Column<bool>(type: "bit", nullable: false),
            CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_PasswordResetPins", x => x.Id);
          });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "PasswordHistories");

      migrationBuilder.DropTable(
          name: "PasswordResetPins");
    }
  }
}
