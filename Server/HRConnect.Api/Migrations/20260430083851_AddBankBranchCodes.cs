using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBankBranchCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankingDetails_BankBranchCode_BankBranchCodeId",
                table: "BankingDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BankBranchCode",
                table: "BankBranchCode");

            migrationBuilder.RenameTable(
                name: "BankBranchCode",
                newName: "BankBranchCodes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BankBranchCodes",
                table: "BankBranchCodes",
                column: "BankBranchCodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_BankingDetails_BankBranchCodes_BankBranchCodeId",
                table: "BankingDetails",
                column: "BankBranchCodeId",
                principalTable: "BankBranchCodes",
                principalColumn: "BankBranchCodeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankingDetails_BankBranchCodes_BankBranchCodeId",
                table: "BankingDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BankBranchCodes",
                table: "BankBranchCodes");

            migrationBuilder.RenameTable(
                name: "BankBranchCodes",
                newName: "BankBranchCode");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BankBranchCode",
                table: "BankBranchCode",
                column: "BankBranchCodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_BankingDetails_BankBranchCode_BankBranchCodeId",
                table: "BankingDetails",
                column: "BankBranchCodeId",
                principalTable: "BankBranchCode",
                principalColumn: "BankBranchCodeId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
