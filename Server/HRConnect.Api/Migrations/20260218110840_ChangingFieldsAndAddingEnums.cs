using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangingFieldsAndAddingEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CareerManager",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "EmpPicture",
                table: "Employees",
                newName: "ProfileImage");

            migrationBuilder.AddColumn<string>(
                name: "CareerManagerID",
                table: "Employees",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_CareerManagerID",
                table: "Employees",
                column: "CareerManagerID");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Employees_CareerManagerID",
                table: "Employees",
                column: "CareerManagerID",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Employees_CareerManagerID",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_CareerManagerID",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CareerManagerID",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "ProfileImage",
                table: "Employees",
                newName: "EmpPicture");

            migrationBuilder.AddColumn<string>(
                name: "CareerManager",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
