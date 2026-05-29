using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class DeleteOptions1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PensionFunds_PensionOptions_PensionOptionId",
                table: "PensionFunds");

            migrationBuilder.AddForeignKey(
                name: "FK_PensionFunds_PensionOptions_PensionOptionId",
                table: "PensionFunds",
                column: "PensionOptionId",
                principalTable: "PensionOptions",
                principalColumn: "PensionOptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PensionFunds_PensionOptions_PensionOptionId",
                table: "PensionFunds");

            migrationBuilder.AddForeignKey(
                name: "FK_PensionFunds_PensionOptions_PensionOptionId",
                table: "PensionFunds",
                column: "PensionOptionId",
                principalTable: "PensionOptions",
                principalColumn: "PensionOptionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
