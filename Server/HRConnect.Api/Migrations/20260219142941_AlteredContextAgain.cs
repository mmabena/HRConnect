using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class AlteredContextAgain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Positions_JobGrades_JobGradeId",
                table: "Positions");

            migrationBuilder.DropForeignKey(
                name: "FK_Positions_OccupationalLevels_OccupationalLevelId",
                table: "Positions");

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_JobGrades_JobGradeId",
                table: "Positions",
                column: "JobGradeId",
                principalTable: "JobGrades",
                principalColumn: "JobGradeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_OccupationalLevels_OccupationalLevelId",
                table: "Positions",
                column: "OccupationalLevelId",
                principalTable: "OccupationalLevels",
                principalColumn: "OccupationalLevelId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Positions_JobGrades_JobGradeId",
                table: "Positions");

            migrationBuilder.DropForeignKey(
                name: "FK_Positions_OccupationalLevels_OccupationalLevelId",
                table: "Positions");

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_JobGrades_JobGradeId",
                table: "Positions",
                column: "JobGradeId",
                principalTable: "JobGrades",
                principalColumn: "JobGradeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_OccupationalLevels_OccupationalLevelId",
                table: "Positions",
                column: "OccupationalLevelId",
                principalTable: "OccupationalLevels",
                principalColumn: "OccupationalLevelId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
