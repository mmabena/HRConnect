using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class ModifyLeaveApplicationModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "LeaveApplications",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "LeaveApplications",
                type: "rowversion",
                rowVersion: true,
                nullable: false);

            //QL Server cannot ALTER to rowversion → must drop + add
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "EmployeeLeaveBalances");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "EmployeeLeaveBalances",
                type: "rowversion",
                rowVersion: true,
                nullable: false);
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "LeaveApplications");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "LeaveApplications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "EmployeeLeaveBalances");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "EmployeeLeaveBalances",
                type: "varbinary(max)",
                nullable: false);
        }
    }
}
