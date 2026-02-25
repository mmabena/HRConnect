using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class StatutoryContributionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ContributionTypeId",
                table: "StatutoryContributions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StatutoryContributionTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmployeeRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0.01m),
                    EmployerRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0.01m),
                    CapAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatutoryContributionTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StatutoryContributions_ContributionTypeId",
                table: "StatutoryContributions",
                column: "ContributionTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_StatutoryContributions_StatutoryContributionTypes_ContributionTypeId",
                table: "StatutoryContributions",
                column: "ContributionTypeId",
                principalTable: "StatutoryContributionTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StatutoryContributions_StatutoryContributionTypes_ContributionTypeId",
                table: "StatutoryContributions");

            migrationBuilder.DropTable(
                name: "StatutoryContributionTypes");

            migrationBuilder.DropIndex(
                name: "IX_StatutoryContributions_ContributionTypeId",
                table: "StatutoryContributions");

            migrationBuilder.DropColumn(
                name: "ContributionTypeId",
                table: "StatutoryContributions");
        }
    }
}
