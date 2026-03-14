using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class PayrollService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "PayrollRecordSequence");

            migrationBuilder.AddColumn<int>(
                name: "PensionOptionId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PayrollPeriods",
                columns: table => new
                {
                    PayrollPeriodId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollPeriods", x => x.PayrollPeriodId);
                });

            migrationBuilder.CreateTable(
                name: "PensionOptions",
                columns: table => new
                {
                    PensionOptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContributionPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PensionOptions", x => x.PensionOptionId);
                });

            migrationBuilder.CreateTable(
                name: "TestEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayrollRunId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestEntities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollRuns",
                columns: table => new
                {
                    PayrollRunId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayrollRunNumber = table.Column<int>(type: "int", nullable: false),
                    PeriodId = table.Column<int>(type: "int", nullable: false),
                    PeriodDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsFinalised = table.Column<bool>(type: "bit", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    FinalisedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollRuns", x => x.PayrollRunId);
                    table.ForeignKey(
                        name: "FK_PayrollRuns_PayrollPeriods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "PayrollPeriods",
                        principalColumn: "PayrollPeriodId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeePensionEnrollments",
                columns: table => new
                {
                    EmployeePensionEnrollmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PensionOptionId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PayrollRunId = table.Column<int>(type: "int", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeePensionEnrollments", x => x.EmployeePensionEnrollmentId);
                    table.ForeignKey(
                        name: "FK_EmployeePensionEnrollments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeePensionEnrollments_PayrollRuns_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalTable: "PayrollRuns",
                        principalColumn: "PayrollRunId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeePensionEnrollments_PensionOptions_PensionOptionId",
                        column: x => x.PensionOptionId,
                        principalTable: "PensionOptions",
                        principalColumn: "PensionOptionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalAidDeductions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [PayrollRecordSequence]"),
                    PayrollRunId = table.Column<int>(type: "int", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MedicalAidDeductionId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Branch = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MedicalOptionId = table.Column<int>(type: "int", nullable: false),
                    MedicalOptionCategoryId = table.Column<int>(type: "int", nullable: false),
                    EmployeeStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalDependentsPremium = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinalisedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PrincipalCount = table.Column<int>(type: "int", nullable: false),
                    AdultCount = table.Column<int>(type: "int", nullable: false),
                    ChildrenCount = table.Column<int>(type: "int", nullable: false),
                    PrincipalPremium = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    SpousePremium = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    ChildPremium = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    TotalDeductionAmount = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    Salary = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    MedicalCategoryId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalAidDeductions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalAidDeductions_MedicalOptionCategories_MedicalCategoryId",
                        column: x => x.MedicalCategoryId,
                        principalTable: "MedicalOptionCategories",
                        principalColumn: "MedicalOptionCategoryId");
                    table.ForeignKey(
                        name: "FK_MedicalAidDeductions_MedicalOptions_MedicalOptionId",
                        column: x => x.MedicalOptionId,
                        principalTable: "MedicalOptions",
                        principalColumn: "MedicalOptionId");
                    table.ForeignKey(
                        name: "FK_MedicalAidDeductions_PayrollRuns_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalTable: "PayrollRuns",
                        principalColumn: "PayrollRunId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PensionDeductions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [PayrollRecordSequence]"),
                    PayrollRunId = table.Column<int>(type: "int", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmployeePensionDeductionId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateJoinedCompany = table.Column<DateOnly>(type: "date", nullable: false),
                    IDNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Passport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PensionableSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PensionOptionId = table.Column<int>(type: "int", nullable: false),
                    PendsionCategoryPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PensionContribution = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VoluntaryContribution = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhyscialAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PensionDeductions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PensionDeductions_PayrollRuns_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalTable: "PayrollRuns",
                        principalColumn: "PayrollRunId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PensionDeductions_PensionOptions_PensionOptionId",
                        column: x => x.PensionOptionId,
                        principalTable: "PensionOptions",
                        principalColumn: "PensionOptionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PensionOptionId",
                table: "Employees",
                column: "PensionOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePensionEnrollments_EmployeeId",
                table: "EmployeePensionEnrollments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePensionEnrollments_PayrollRunId",
                table: "EmployeePensionEnrollments",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePensionEnrollments_PensionOptionId",
                table: "EmployeePensionEnrollments",
                column: "PensionOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalAidDeductions_MedicalCategoryId",
                table: "MedicalAidDeductions",
                column: "MedicalCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalAidDeductions_MedicalOptionId",
                table: "MedicalAidDeductions",
                column: "MedicalOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalAidDeductions_PayrollRunId",
                table: "MedicalAidDeductions",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRuns_PeriodId",
                table: "PayrollRuns",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_PensionDeductions_PayrollRunId",
                table: "PensionDeductions",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_PensionDeductions_PensionOptionId",
                table: "PensionDeductions",
                column: "PensionOptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_PensionOptions_PensionOptionId",
                table: "Employees",
                column: "PensionOptionId",
                principalTable: "PensionOptions",
                principalColumn: "PensionOptionId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_PensionOptions_PensionOptionId",
                table: "Employees");

            migrationBuilder.DropTable(
                name: "EmployeePensionEnrollments");

            migrationBuilder.DropTable(
                name: "MedicalAidDeductions");

            migrationBuilder.DropTable(
                name: "PensionDeductions");

            migrationBuilder.DropTable(
                name: "TestEntities");

            migrationBuilder.DropTable(
                name: "PayrollRuns");

            migrationBuilder.DropTable(
                name: "PensionOptions");

            migrationBuilder.DropTable(
                name: "PayrollPeriods");

            migrationBuilder.DropIndex(
                name: "IX_Employees_PensionOptionId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PensionOptionId",
                table: "Employees");

            migrationBuilder.DropSequence(
                name: "PayrollRecordSequence");
        }
    }
}
