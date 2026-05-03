using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class RefactorSalaryBenchmarkPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalaryBenchmarks_JobGrades_InternalJobGradeId",
                table: "SalaryBenchmarks");

            migrationBuilder.RenameColumn(
                name: "InternalJobGradeId",
                table: "SalaryBenchmarks",
                newName: "PositionId");

            migrationBuilder.RenameIndex(
                name: "IX_SalaryBenchmarks_InternalJobGradeId",
                table: "SalaryBenchmarks",
                newName: "IX_SalaryBenchmarks_PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalaryBenchmarks_Positions_PositionId",
                table: "SalaryBenchmarks",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "PositionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalaryBenchmarks_Positions_PositionId",
                table: "SalaryBenchmarks");

            migrationBuilder.RenameColumn(
                name: "PositionId",
                table: "SalaryBenchmarks",
                newName: "InternalJobGradeId");

            migrationBuilder.RenameIndex(
                name: "IX_SalaryBenchmarks_PositionId",
                table: "SalaryBenchmarks",
                newName: "IX_SalaryBenchmarks_InternalJobGradeId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalaryBenchmarks_JobGrades_InternalJobGradeId",
                table: "SalaryBenchmarks",
                column: "InternalJobGradeId",
                principalTable: "JobGrades",
                principalColumn: "JobGradeId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
