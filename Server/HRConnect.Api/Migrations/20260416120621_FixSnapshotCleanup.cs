using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixSnapshotCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobGradeGroupMaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobGradeId = table.Column<int>(type: "int", nullable: false),
                    GroupKey = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobGradeGroupMaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobGradeGroupMaps_JobGrades_JobGradeId",
                        column: x => x.JobGradeId,
                        principalTable: "JobGrades",
                        principalColumn: "JobGradeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobGradeGroupMaps_JobGradeId_GroupKey",
                table: "JobGradeGroupMaps",
                columns: new[] { "JobGradeId", "GroupKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobGradeGroupMaps");
        }
    }
}
