using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
  /// <inheritdoc />
  public partial class UserMFA : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AlterColumn<int>(
          name: "DeliveryChannel",
          table: "Notifications",
          type: "int",
          nullable: false,
          oldClrType: typeof(string),
          oldType: "nvarchar(max)");

      // migrationBuilder.AddColumn<string>(
      //     name: "IdempotencyKey",
      //     table: "Notifications",
      //     type: "nvarchar(max)",
      //     nullable: false,
      //     defaultValue: "");
      //
      migrationBuilder.Sql(@"
        IF NOT EXISTS (
            SELECT 1
            FROM sys.columns
            WHERE Name = N'IdempotencyKey'
            AND Object_ID = Object_ID(N'Notifications')
        )
        BEGIN
            ALTER TABLE [Notifications]
            ADD [IdempotencyKey] NVARCHAR(MAX) NOT NULL DEFAULT N''
        END
    ");

      migrationBuilder.CreateTable(
          name: "TOTPStates",
          columns: table => new
          {
            Id = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            UserId = table.Column<int>(type: "int", nullable: false),
            LastUsedTimeStamp = table.Column<long>(type: "bigint", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_TOTPStates", x => x.Id);
          });

      migrationBuilder.CreateTable(
          name: "UserSecrets",
          columns: table => new
          {
            SecretId = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            UserId = table.Column<int>(type: "int", nullable: false),
            EncryptedUserSecret = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
            KeyVersion = table.Column<int>(type: "int", nullable: false),
            CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_UserSecrets", x => x.SecretId);
            table.ForeignKey(
                      name: "FK_UserSecrets_Users_UserId",
                      column: x => x.UserId,
                      principalTable: "Users",
                      principalColumn: "UserId",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateIndex(
          name: "IX_UserSecrets_UserId",
          table: "UserSecrets",
          column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "TOTPStates");

      migrationBuilder.DropTable(
          name: "UserSecrets");

      // migrationBuilder.DropColumn(
      //     name: "IdempotencyKey",
      //     table: "Notifications");

      migrationBuilder.Sql(@"
        IF NOT EXISTS (
            SELECT 1
            FROM sys.columns
            WHERE Name = N'IdempotencyKey'
            AND Object_ID = Object_ID(N'Notifications')
        )
        BEGIN
            ALTER TABLE [Notifications]
            DROP COLUMN [IdempotencyKey] 
      END
    ");
      migrationBuilder.AlterColumn<string>(
          name: "DeliveryChannel",
          table: "Notifications",
          type: "nvarchar(max)",
          nullable: false,
          oldClrType: typeof(int),
          oldType: "int");
    }
  }
}