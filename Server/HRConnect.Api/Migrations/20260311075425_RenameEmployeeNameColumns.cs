using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameEmployeeNameColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Employees",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Employees",
                newName: "Surname");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Employees",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "Surname",
                table: "Employees",
                newName: "LastName");
        }
    }
}
