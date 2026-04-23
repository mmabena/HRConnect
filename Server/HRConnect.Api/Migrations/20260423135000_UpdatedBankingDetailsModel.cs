using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedBankingDetailsModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PayFrequency",
                table: "BankingDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "BankingDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReferenceType",
                table: "BankingDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayFrequency",
                table: "BankingDetails");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "BankingDetails");

            migrationBuilder.DropColumn(
                name: "ReferenceType",
                table: "BankingDetails");
        }
    }
}
