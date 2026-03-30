using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class NewPullmigration4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_PensionOptions_PensionOptionId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_PensionOptionID",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PensionOptionID",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "PensionOptionId",
                table: "Employees",
                newName: "PensionOptionID");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_PensionOptionId",
                table: "Employees",
                newName: "IX_Employees_PensionOptionID");

            migrationBuilder.AlterColumn<int>(
                name: "PensionOptionID",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_PensionOptions_PensionOptionID",
                table: "Employees",
                column: "PensionOptionID",
                principalTable: "PensionOptions",
                principalColumn: "PensionOptionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_PensionOptions_PensionOptionID",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "PensionOptionID",
                table: "Employees",
                newName: "PensionOptionId");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_PensionOptionID",
                table: "Employees",
                newName: "IX_Employees_PensionOptionId");

            migrationBuilder.AlterColumn<int>(
                name: "PensionOptionId",
                table: "Employees",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "PensionOptionID",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PensionOptionID",
                table: "Employees",
                column: "PensionOptionID");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_PensionOptions_PensionOptionId",
                table: "Employees",
                column: "PensionOptionId",
                principalTable: "PensionOptions",
                principalColumn: "PensionOptionId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
