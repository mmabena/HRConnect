using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
  /// <inheritdoc />
  public partial class AddUserCompanyTable : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {

      migrationBuilder.CreateTable(
          name: "UserCompanies",
          columns: table => new
          {
            UserId = table.Column<int>(type: "int", nullable: false),
            CompanyId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            IsDefault = table.Column<bool>(type: "bit", nullable: false),
            CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_UserCompanies", x => new { x.UserId, x.CompanyId });
            table.ForeignKey(
                      name: "FK_UserCompanies_Companies_CompanyId",
                      column: x => x.CompanyId,
                      principalTable: "Companies",
                      principalColumn: "CompanyId",
                      onDelete: ReferentialAction.Cascade);
            table.ForeignKey(
                      name: "FK_UserCompanies_Users_UserId",
                      column: x => x.UserId,
                      principalTable: "Users",
                      principalColumn: "UserId",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateIndex(
          name: "IX_UserCompanies_CompanyId",
          table: "UserCompanies",
          column: "CompanyId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {

      migrationBuilder.DropIndex(
name: "IX_Employees_CompanyId",
table: "Employees");

      migrationBuilder.DropTable(
          name: "UserCompanies");
    }
  }
}
