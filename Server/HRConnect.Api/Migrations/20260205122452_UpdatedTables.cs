using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Positions_OccuptionalLevels_OccuptionalLevelId",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "JobGrades",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "OcuptionalLevels",
                table: "Positions");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Positions",
                newName: "UpdatedDate");

            migrationBuilder.RenameColumn(
                name: "OccuptionalLevelId",
                table: "Positions",
                newName: "OccupationalLevelId");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Positions",
                newName: "CreatedDate");

            migrationBuilder.RenameIndex(
                name: "IX_Positions_OccuptionalLevelId",
                table: "Positions",
                newName: "IX_Positions_OccupationalLevelId");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "OccuptionalLevels",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "OccuptionalLevelId",
                table: "OccuptionalLevels",
                newName: "OccupationalLevelId");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "JobGrades",
                newName: "UpdatedDate");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "JobGrades",
                newName: "CreatedDate");

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_OccuptionalLevels_OccupationalLevelId",
                table: "Positions",
                column: "OccupationalLevelId",
                principalTable: "OccuptionalLevels",
                principalColumn: "OccupationalLevelId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Positions_OccuptionalLevels_OccupationalLevelId",
                table: "Positions");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "Positions",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "OccupationalLevelId",
                table: "Positions",
                newName: "OccuptionalLevelId");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Positions",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Positions_OccupationalLevelId",
                table: "Positions",
                newName: "IX_Positions_OccuptionalLevelId");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "OccuptionalLevels",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "OccupationalLevelId",
                table: "OccuptionalLevels",
                newName: "OccuptionalLevelId");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "JobGrades",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "JobGrades",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<string>(
                name: "JobGrades",
                table: "Positions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OcuptionalLevels",
                table: "Positions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_OccuptionalLevels_OccuptionalLevelId",
                table: "Positions",
                column: "OccuptionalLevelId",
                principalTable: "OccuptionalLevels",
                principalColumn: "OccuptionalLevelId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
