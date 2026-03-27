using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
  /// <inheritdoc />
  public partial class PayrollWithDeductions : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateSequence(
          name: "PayrollRecordSequence");

      //Drop the old StatutoryContributions table and add to the PayrollRecordSequence
      migrationBuilder.Sql(@"
          IF OBJECT_ID('dbo.StatutoryContributions','U') IS NOT NULL
          BEGIN
            DROP TABLE dbo.StatutoryContributions;
          END
          ");

      //Create the table instead of just Altering a column
            migrationBuilder.CreateTable(
                name: "StatutoryContributions",
                columns: table => new
                {
                    // Id = table.Column<int>(type: "int", nullable: false)
                    //     .Annotation("SqlServer:Identity", "1, 1"),
                    //
                    Id = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [PayrollRecordSequence]"),
                    EmployeeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PassportNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MonthlySalary = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UifEmployeeAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UifEmployerAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    EmployerSdlContribution = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DeductedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrentMonth = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatutoryContributions", x => x.Id);
                });
      // migrationBuilder.AlterColumn<int>(
      //     name: "Id",
      //     table: "StatutoryContributions",
      //     type: "int",
      //     nullable: false,
      //     defaultValueSql: "NEXT VALUE FOR [PayrollRecordSequence]",
      //     oldClrType: typeof(int),
      //     oldType: "int")
      //     .OldAnnotation("SqlServer:Identity", "1, 1");
      //
      migrationBuilder.AddColumn<bool>(
          name: "IsLocked",
          table: "StatutoryContributions",
          type: "bit",
          nullable: false,
          defaultValue: false);

      migrationBuilder.AddColumn<int>(
          name: "PayrollRunId",
          table: "StatutoryContributions",
          type: "int",
          nullable: false,
          defaultValue: 0);

      migrationBuilder.AddColumn<bool>(
          name: "IsActive",
          table: "Employees",
          type: "bit",
          nullable: false,
          defaultValue: false);

      migrationBuilder.AddColumn<int>(
          name: "PensionOptionId",
          table: "Employees",
          type: "int",
          nullable: true);

      migrationBuilder.CreateTable(
          name: "Notifications",
          columns: table => new
          {
            NotificationId = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
            IsRead = table.Column<bool>(type: "bit", nullable: false),
            Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Severity = table.Column<string>(type: "nvarchar(max)", nullable: false),
            EmployeeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
            DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
            CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            DeliveryChannel = table.Column<string>(type: "nvarchar(max)", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_Notifications", x => x.NotificationId);
          });

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
            VoluntaryContribution = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            IsVoluntaryContributionPermament = table.Column<bool>(type: "bit", nullable: true),
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
            OptionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            MedicalOptionCategoryId = table.Column<int>(type: "int", nullable: false),
            EmployeeStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            TotalDependentsPremium = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
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
            OptionCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
            CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            TerminationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
            OptionCategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            TerminationReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
            PeriodId = table.Column<int>(type: "int", nullable: false),
            FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            DateJoinedCompany = table.Column<DateOnly>(type: "date", nullable: false),
            IdNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Passport = table.Column<string>(type: "nvarchar(max)", nullable: true),
            TaxNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            PensionableSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            PensionOptionId = table.Column<int>(type: "int", nullable: false),
            PendsionCategoryPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            PensionContribution = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            VoluntaryContribution = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            TotalPensionContribution = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
            EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
            PhysicalAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
          name: "IX_StatutoryContributions_PayrollRunId",
          table: "StatutoryContributions",
          column: "PayrollRunId");

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

      migrationBuilder.AddForeignKey(
          name: "FK_StatutoryContributions_PayrollRuns_PayrollRunId",
          table: "StatutoryContributions",
          column: "PayrollRunId",
          principalTable: "PayrollRuns",
          principalColumn: "PayrollRunId",
          onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropForeignKey(
          name: "FK_Employees_PensionOptions_PensionOptionId",
          table: "Employees");

      migrationBuilder.DropForeignKey(
          name: "FK_StatutoryContributions_PayrollRuns_PayrollRunId",
          table: "StatutoryContributions");

      migrationBuilder.DropTable(
          name: "EmployeePensionEnrollments");

      migrationBuilder.DropTable(
          name: "MedicalAidDeductions");

      migrationBuilder.DropTable(
          name: "Notifications");

      migrationBuilder.DropTable(
          name: "PensionDeductions");

      migrationBuilder.DropTable(
          name: "PayrollRuns");

      migrationBuilder.DropTable(
          name: "PensionOptions");

      migrationBuilder.DropTable(
          name: "PayrollPeriods");

      migrationBuilder.DropIndex(
          name: "IX_StatutoryContributions_PayrollRunId",
          table: "StatutoryContributions");

      migrationBuilder.DropIndex(
          name: "IX_Employees_PensionOptionId",
          table: "Employees");

      migrationBuilder.DropColumn(
          name: "IsLocked",
          table: "StatutoryContributions");

      migrationBuilder.DropColumn(
          name: "PayrollRunId",
          table: "StatutoryContributions");

      migrationBuilder.DropColumn(
          name: "IsActive",
          table: "Employees");

      migrationBuilder.DropColumn(
          name: "PensionOptionId",
          table: "Employees");

      migrationBuilder.DropSequence(
          name: "PayrollRecordSequence");

      migrationBuilder.AlterColumn<int>(
          name: "Id",
          table: "StatutoryContributions",
          type: "int",
          nullable: false,
          oldClrType: typeof(int),
          oldType: "int",
          oldDefaultValueSql: "NEXT VALUE FOR [PayrollRecordSequence]")
          .Annotation("SqlServer:Identity", "1, 1");
    }
  }
}