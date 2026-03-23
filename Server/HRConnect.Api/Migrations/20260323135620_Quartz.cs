using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class Quartz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "StatutoryContributions",
                type: "int",
                nullable: false,
                defaultValueSql: "NEXT VALUE FOR [PayrollRecordSequence]",
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<bool>(
                name: "IsLocked",
                table: "StatutoryContributions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PayrollRunId",
                table: "StatutoryContributions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_StatutoryContributions_PayrollRunId",
                table: "StatutoryContributions",
                column: "PayrollRunId");

            migrationBuilder.AddForeignKey(
                name: "FK_StatutoryContributions_PayrollRuns_PayrollRunId",
                table: "StatutoryContributions",
                column: "PayrollRunId",
                principalTable: "PayrollRuns",
                principalColumn: "PayrollRunId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StatutoryContributions_PayrollRuns_PayrollRunId",
                table: "StatutoryContributions");

            migrationBuilder.DropIndex(
                name: "IX_StatutoryContributions_PayrollRunId",
                table: "StatutoryContributions");

            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "StatutoryContributions");

            migrationBuilder.DropColumn(
                name: "PayrollRunId",
                table: "StatutoryContributions");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "StatutoryContributions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValueSql: "NEXT VALUE FOR [PayrollRecordSequence]")
                .Annotation("SqlServer:Identity", "1, 1");
        }
    }
}
