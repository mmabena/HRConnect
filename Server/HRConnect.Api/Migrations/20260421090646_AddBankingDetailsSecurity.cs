using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBankingDetailsSecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AccountNumber",
                table: "BankingDetails",
                newName: "AccountNumberLast4Digits");

            migrationBuilder.AlterColumn<int>(
                name: "BankingDetailsId",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PassportNumber",
                table: "BankingDetails",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "AccountNumberEncrypted",
                table: "BankingDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AccountNumberSearchHash",
                table: "BankingDetails",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsLocked",
                table: "BankingDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockedAt",
                table: "BankingDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankingDetails_AccountNumberSearchHash",
                table: "BankingDetails",
                column: "AccountNumberSearchHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BankingDetails_AccountNumberSearchHash",
                table: "BankingDetails");

            migrationBuilder.DropColumn(
                name: "AccountNumberEncrypted",
                table: "BankingDetails");

            migrationBuilder.DropColumn(
                name: "AccountNumberSearchHash",
                table: "BankingDetails");

            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "BankingDetails");

            migrationBuilder.DropColumn(
                name: "LockedAt",
                table: "BankingDetails");

            migrationBuilder.RenameColumn(
                name: "AccountNumberLast4Digits",
                table: "BankingDetails",
                newName: "AccountNumber");

            migrationBuilder.AlterColumn<int>(
                name: "BankingDetailsId",
                table: "Employees",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "PassportNumber",
                table: "BankingDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
