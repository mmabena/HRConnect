using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameTitleToPositionTitleInPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Positions",
                newName: "PositionTitle");

            migrationBuilder.RenameIndex(
                name: "IX_Positions_Title",
                table: "Positions",
                newName: "IX_Positions_PositionTitle");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PositionTitle",
                table: "Positions",
                newName: "Title");

            migrationBuilder.RenameIndex(
                name: "IX_Positions_PositionTitle",
                table: "Positions",
                newName: "IX_Positions_Title");
        }
    }
}
