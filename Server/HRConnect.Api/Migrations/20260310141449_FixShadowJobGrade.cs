using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixShadowJobGrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Positions_JobGrades_JobGradeId1",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_Positions_JobGradeId1",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "JobGradeId1",
                table: "Positions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "JobGradeId1",
                table: "Positions",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "PositionId",
                keyValue: 1,
                column: "JobGradeId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "PositionId",
                keyValue: 2,
                column: "JobGradeId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "PositionId",
                keyValue: 3,
                column: "JobGradeId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "PositionId",
                keyValue: 4,
                column: "JobGradeId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "PositionId",
                keyValue: 5,
                column: "JobGradeId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "Positions",
                keyColumn: "PositionId",
                keyValue: 6,
                column: "JobGradeId1",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Positions_JobGradeId1",
                table: "Positions",
                column: "JobGradeId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_JobGrades_JobGradeId1",
                table: "Positions",
                column: "JobGradeId1",
                principalTable: "JobGrades",
                principalColumn: "JobGradeId");
        }
    }
}
