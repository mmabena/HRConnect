using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedModelToAllowNullsMOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "SalaryBracketMin",
                table: "MedicalOptions",
                type: "decimal(15,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "SalaryBracketMax",
                table: "MedicalOptions",
                type: "decimal(15,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "SalaryBracketMin",
                table: "MedicalOptions",
                type: "decimal(15,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SalaryBracketMax",
                table: "MedicalOptions",
                type: "decimal(15,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,2)",
                oldNullable: true);
        }
    }
}
