using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
  /// <inheritdoc />
  public partial class DBContextUpdate : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      // migrationBuilder.CreateTable(
      //     name: "JobGrades",
      //     columns: table => new
      //     {
      //         JobGradeId = table.Column<int>(type: "int", nullable: false)
      //             .Annotation("SqlServer:Identity", "1, 1"),
      //         Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
      //         Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
      //         IsActive = table.Column<bool>(type: "bit", nullable: false),
      //         CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
      //         UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
      //     },
      //     constraints: table =>
      //     {
      //         table.PrimaryKey("PK_JobGrades", x => x.JobGradeId);
      //     });

      // migrationBuilder.CreateTable(
      //     name: "OccuptionalLevels",
      //     columns: table => new
      //     {
      //         OccuptionalLevelId = table.Column<int>(type: "int", nullable: false)
      //             .Annotation("SqlServer:Identity", "1, 1"),
      //         Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
      //         Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
      //         CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
      //     },
      //     constraints: table =>
      //     {
      //         table.PrimaryKey("PK_OccuptionalLevels", x => x.OccuptionalLevelId);
      //     });

      // migrationBuilder.CreateIndex(
      //     name: "IX_Positions_JobGradeId",
      //     table: "Positions",
      //     column: "JobGradeId");

      // migrationBuilder.CreateIndex(
      //     name: "IX_Positions_OccuptionalLevelId",
      //     table: "Positions",
      //     column: "OccuptionalLevelId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      // migrationBuilder.DropTable(
      //     name: "Positions");

      // migrationBuilder.DropTable(
      //     name: "JobGrades");

      // migrationBuilder.DropTable(
      //     name: "OccuptionalLevels");
    }
  }
}
