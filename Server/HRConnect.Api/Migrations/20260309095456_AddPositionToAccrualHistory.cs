using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPositionToAccrualHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PositionId",
                table: "EmployeeAccrualRateHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAccrualRateHistories_PositionId",
                table: "EmployeeAccrualRateHistories",
                column: "PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAccrualRateHistories_Positions_PositionId",
                table: "EmployeeAccrualRateHistories",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "PositionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAccrualRateHistories_Positions_PositionId",
                table: "EmployeeAccrualRateHistories");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeAccrualRateHistories_PositionId",
                table: "EmployeeAccrualRateHistories");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "EmployeeAccrualRateHistories");
        }
    }
}
