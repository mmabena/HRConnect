using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenamePermamentToPermanent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsVoluntaryContributionPermament",
                table: "EmployeePensionEnrollments",
                newName: "IsVoluntaryContributionPermanent");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsVoluntaryContributionPermanent",
                table: "EmployeePensionEnrollments",
                newName: "IsVoluntaryContributionPermament");
        }
    }
}
