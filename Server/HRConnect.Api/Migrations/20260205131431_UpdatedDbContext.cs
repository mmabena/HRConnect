using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Positions_OccuptionalLevels_OccupationalLevelId",
                table: "Positions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OccuptionalLevels",
                table: "OccuptionalLevels");

            migrationBuilder.RenameTable(
                name: "OccuptionalLevels",
                newName: "OccupationalLevels");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OccupationalLevels",
                table: "OccupationalLevels",
                column: "OccupationalLevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_OccupationalLevels_OccupationalLevelId",
                table: "Positions",
                column: "OccupationalLevelId",
                principalTable: "OccupationalLevels",
                principalColumn: "OccupationalLevelId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Positions_OccupationalLevels_OccupationalLevelId",
                table: "Positions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OccupationalLevels",
                table: "OccupationalLevels");

            migrationBuilder.RenameTable(
                name: "OccupationalLevels",
                newName: "OccuptionalLevels");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OccuptionalLevels",
                table: "OccuptionalLevels",
                column: "OccupationalLevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_OccuptionalLevels_OccupationalLevelId",
                table: "Positions",
                column: "OccupationalLevelId",
                principalTable: "OccuptionalLevels",
                principalColumn: "OccupationalLevelId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
