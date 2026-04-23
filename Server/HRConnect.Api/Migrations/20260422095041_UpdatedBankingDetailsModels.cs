using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedBankingDetailsModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BranchCode",
                table: "BankingDetails");

            migrationBuilder.AddColumn<int>(
                name: "BankBranchCodeId",
                table: "BankingDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "BankBranchCode",
                columns: table => new
                {
                    BankBranchCodeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UniversalCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankBranchCode", x => x.BankBranchCodeId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankingDetails_BankBranchCodeId",
                table: "BankingDetails",
                column: "BankBranchCodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_BankingDetails_BankBranchCode_BankBranchCodeId",
                table: "BankingDetails",
                column: "BankBranchCodeId",
                principalTable: "BankBranchCode",
                principalColumn: "BankBranchCodeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankingDetails_BankBranchCode_BankBranchCodeId",
                table: "BankingDetails");

            migrationBuilder.DropTable(
                name: "BankBranchCode");

            migrationBuilder.DropIndex(
                name: "IX_BankingDetails_BankBranchCodeId",
                table: "BankingDetails");

            migrationBuilder.DropColumn(
                name: "BankBranchCodeId",
                table: "BankingDetails");

            migrationBuilder.AddColumn<string>(
                name: "BranchCode",
                table: "BankingDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
