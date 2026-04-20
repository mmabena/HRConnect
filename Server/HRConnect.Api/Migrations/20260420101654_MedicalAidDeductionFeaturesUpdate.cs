using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class MedicalAidDeductionFeaturesUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop old indexes conditionally
            migrationBuilder.Sql(
                @"IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StatutoryContributions_PayrollRunId' AND object_id = OBJECT_ID('StatutoryContributions'))
                  DROP INDEX [IX_StatutoryContributions_PayrollRunId] ON [StatutoryContributions]");

            migrationBuilder.Sql(
                @"IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PensionDeductions_PayrollRunId' AND object_id = OBJECT_ID('PensionDeductions'))
                  DROP INDEX [IX_PensionDeductions_PayrollRunId] ON [PensionDeductions]");

            migrationBuilder.Sql(
                @"IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MedicalAidDeductions_PayrollRunId' AND object_id = OBJECT_ID('MedicalAidDeductions'))
                  DROP INDEX [IX_MedicalAidDeductions_PayrollRunId] ON [MedicalAidDeductions]");

            // Drop columns conditionally
            migrationBuilder.Sql(
                @"IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MedicalAidDeductions') AND name = 'CreateDate')
                  ALTER TABLE [MedicalAidDeductions] DROP COLUMN [CreateDate]");

            migrationBuilder.Sql(
                @"IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MedicalAidDeductions') AND name = 'FinalisedDate')
                  ALTER TABLE [MedicalAidDeductions] DROP COLUMN [FinalisedDate]");

            migrationBuilder.Sql(
                @"IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MedicalAidDeductions') AND name = 'MedicalAidDeductionId')
                  ALTER TABLE [MedicalAidDeductions] DROP COLUMN [MedicalAidDeductionId]");

            migrationBuilder.Sql(
                @"IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MedicalAidDeductions') AND name = 'MedicalOptionCategoryId')
                  ALTER TABLE [MedicalAidDeductions] DROP COLUMN [MedicalOptionCategoryId]");

            migrationBuilder.Sql(
                @"IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MedicalAidDeductions') AND name = 'OptionCategory')
                  ALTER TABLE [MedicalAidDeductions] DROP COLUMN [OptionCategory]");

            migrationBuilder.Sql(
                @"IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MedicalAidDeductions') AND name = 'TotalDependentsPremium')
                  ALTER TABLE [MedicalAidDeductions] DROP COLUMN [TotalDependentsPremium]");

            // Alter columns conditionally
            migrationBuilder.Sql(
                @"IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('StatutoryContributions') AND name = 'EmployeeId' AND max_length = -1)
                  ALTER TABLE [StatutoryContributions] ALTER COLUMN [EmployeeId] nvarchar(450) NOT NULL");

            migrationBuilder.Sql(
                @"IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PensionDeductions') AND name = 'EmployeeId' AND max_length = -1)
                  ALTER TABLE [PensionDeductions] ALTER COLUMN [EmployeeId] nvarchar(450) NOT NULL");

            migrationBuilder.Sql(
                @"IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MedicalAidDeductions') AND name = 'EmployeeId' AND max_length = -1)
                  ALTER TABLE [MedicalAidDeductions] ALTER COLUMN [EmployeeId] nvarchar(450) NOT NULL");

            // Create tables conditionally
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CompanyContributions')
                BEGIN
                    CREATE TABLE [CompanyContributions] (
                        [CompanyContributionId] int NOT NULL IDENTITY,
                        [Code] nvarchar(max) NOT NULL,
                        [ShortDescription] nvarchar(max) NOT NULL,
                        [LongDescription] nvarchar(max) NOT NULL,
                        [TaxCode] nvarchar(max) NOT NULL,
                        [Percentage] decimal(10,6) NOT NULL,
                        [IsActive] bit NOT NULL,
                        CONSTRAINT [PK_CompanyContributions] PRIMARY KEY ([CompanyContributionId])
                    )
                END");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EmployeeCompanyContributions')
                BEGIN
                    CREATE TABLE [EmployeeCompanyContributions] (
                        [Id] int NOT NULL DEFAULT (NEXT VALUE FOR [PayrollRecordSequence]),
                        [PayrollRunId] int NOT NULL,
                        [IsLocked] bit NOT NULL,
                        [EmployeeId] nvarchar(450) NOT NULL,
                        [Name] nvarchar(max) NOT NULL,
                        [Surname] nvarchar(max) NOT NULL,
                        [IdNumber] nvarchar(max) NOT NULL,
                        [PassportNumber] nvarchar(max) NOT NULL,
                        [Age] int NOT NULL,
                        [Salary] decimal(18,2) NOT NULL,
                        [DeathAmount] decimal(18,2) NOT NULL,
                        [DeathPercentage] decimal(10,6) NOT NULL,
                        [DisabilityAmount] decimal(18,2) NOT NULL,
                        [DisabilityPercentage] decimal(10,6) NOT NULL,
                        CONSTRAINT [PK_EmployeeCompanyContributions] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_EmployeeCompanyContributions_PayrollRuns_PayrollRunId] FOREIGN KEY ([PayrollRunId]) REFERENCES [PayrollRuns] ([PayrollRunId]) ON DELETE CASCADE
                    )
                END");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PensionFunds')
                BEGIN
                    CREATE TABLE [PensionFunds] (
                        [PensionFundId] int NOT NULL IDENTITY,
                        [EmployeeId] nvarchar(450) NOT NULL,
                        [EmployeeName] nvarchar(max) NOT NULL,
                        [MonthlySalary] decimal(18,2) NOT NULL,
                        [ContributionPercentage] decimal(18,2) NOT NULL,
                        [ContributionAmount] decimal(18,2) NOT NULL,
                        [TaxCode] int NOT NULL,
                        [PensionOptionId] int NOT NULL,
                        CONSTRAINT [PK_PensionFunds] PRIMARY KEY ([PensionFundId]),
                        CONSTRAINT [FK_PensionFunds_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([EmployeeId]) ON DELETE NO ACTION,
                        CONSTRAINT [FK_PensionFunds_PensionOptions_PensionOptionId] FOREIGN KEY ([PensionOptionId]) REFERENCES [PensionOptions] ([PensionOptionId]) ON DELETE CASCADE
                    )
                END");

            // Create indexes conditionally
            migrationBuilder.Sql(
                @"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StatutoryContributions_PayrollRunId_EmployeeId' AND object_id = OBJECT_ID('StatutoryContributions'))
                  CREATE UNIQUE INDEX [IX_StatutoryContributions_PayrollRunId_EmployeeId] ON [StatutoryContributions] ([PayrollRunId], [EmployeeId])");

            migrationBuilder.Sql(
                @"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PensionDeductions_PayrollRunId_EmployeeId' AND object_id = OBJECT_ID('PensionDeductions'))
                  CREATE UNIQUE INDEX [IX_PensionDeductions_PayrollRunId_EmployeeId] ON [PensionDeductions] ([PayrollRunId], [EmployeeId])");

            migrationBuilder.Sql(
                @"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MedicalAidDeductions_PayrollRunId_EmployeeId' AND object_id = OBJECT_ID('MedicalAidDeductions'))
                  CREATE UNIQUE INDEX [IX_MedicalAidDeductions_PayrollRunId_EmployeeId] ON [MedicalAidDeductions] ([PayrollRunId], [EmployeeId])");

            migrationBuilder.Sql(
                @"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmployeeCompanyContributions_PayrollRunId_EmployeeId' AND object_id = OBJECT_ID('EmployeeCompanyContributions'))
                  CREATE UNIQUE INDEX [IX_EmployeeCompanyContributions_PayrollRunId_EmployeeId] ON [EmployeeCompanyContributions] ([PayrollRunId], [EmployeeId])");

            migrationBuilder.Sql(
                @"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PensionFunds_EmployeeId' AND object_id = OBJECT_ID('PensionFunds'))
                  CREATE INDEX [IX_PensionFunds_EmployeeId] ON [PensionFunds] ([EmployeeId])");

            migrationBuilder.Sql(
                @"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PensionFunds_PensionOptionId' AND object_id = OBJECT_ID('PensionFunds'))
                  CREATE INDEX [IX_PensionFunds_PensionOptionId] ON [PensionFunds] ([PensionOptionId])");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyContributions");

            migrationBuilder.DropTable(
                name: "EmployeeCompanyContributions");

            migrationBuilder.DropTable(
                name: "PensionFunds");

            migrationBuilder.DropIndex(
                name: "IX_StatutoryContributions_PayrollRunId_EmployeeId",
                table: "StatutoryContributions");

            migrationBuilder.DropIndex(
                name: "IX_PensionDeductions_PayrollRunId_EmployeeId",
                table: "PensionDeductions");

            migrationBuilder.DropIndex(
                name: "IX_MedicalAidDeductions_PayrollRunId_EmployeeId",
                table: "MedicalAidDeductions");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "StatutoryContributions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "PensionDeductions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "MedicalAidDeductions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "MedicalAidDeductions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FinalisedDate",
                table: "MedicalAidDeductions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "MedicalAidDeductionId",
                table: "MedicalAidDeductions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MedicalOptionCategoryId",
                table: "MedicalAidDeductions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OptionCategory",
                table: "MedicalAidDeductions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDependentsPremium",
                table: "MedicalAidDeductions",
                type: "decimal(15,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatutoryContributions_PayrollRunId",
                table: "StatutoryContributions",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_PensionDeductions_PayrollRunId",
                table: "PensionDeductions",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalAidDeductions_PayrollRunId",
                table: "MedicalAidDeductions",
                column: "PayrollRunId");
        }
    }
}
