using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HRConnect.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataPositionAndLeave : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "quartz");

            migrationBuilder.CreateSequence(
                name: "PayrollRecordSequence");

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PassportNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MonthlySalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProjectedSalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UifEmployeeAmount = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    UifEmployerAmount = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    EmployerSdlContribution = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AuditAction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TabelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuditedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditId);
                });

            migrationBuilder.CreateTable(
                name: "JobGrades",
                columns: table => new
                {
                    JobGradeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobGrades", x => x.JobGradeId);
                });

            migrationBuilder.CreateTable(
                name: "LeaveTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResetMonth = table.Column<int>(type: "int", nullable: true),
                    ResetDay = table.Column<int>(type: "int", nullable: true),
                    MaxCarryoverDays = table.Column<int>(type: "int", nullable: true),
                    CarryoverExpiryMonth = table.Column<int>(type: "int", nullable: true),
                    CarryoverExpiryDay = table.Column<int>(type: "int", nullable: true),
                    CarryoverNotificationMonth = table.Column<int>(type: "int", nullable: true),
                    CarryoverNotificationDay = table.Column<int>(type: "int", nullable: true),
                    IsRollingWindow = table.Column<bool>(type: "bit", nullable: false),
                    RollingMonths = table.Column<int>(type: "int", nullable: true),
                    FemaleOnly = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MedicalOptionCategories",
                columns: table => new
                {
                    MedicalOptionCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicalOptionCategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalOptionCategories", x => x.MedicalOptionCategoryId);
                });

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
                name: "OccupationalLevels",
                columns: table => new
                {
                    OccupationalLevelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OccupationalLevels", x => x.OccupationalLevelId);
                });

            migrationBuilder.CreateTable(
                name: "PasswordHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetPins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetPins", x => x.Id);
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
                name: "QRTZ_CALENDARS",
                schema: "quartz",
                columns: table => new
                {
                    SCHED_NAME = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    CALENDAR_NAME = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CALENDAR = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRTZ_CALENDARS", x => new { x.SCHED_NAME, x.CALENDAR_NAME });
                });

            migrationBuilder.CreateTable(
                name: "QRTZ_FIRED_TRIGGERS",
                schema: "quartz",
                columns: table => new
                {
                    SCHED_NAME = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ENTRY_ID = table.Column<string>(type: "nvarchar(140)", maxLength: 140, nullable: false),
                    TRIGGER_NAME = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TRIGGER_GROUP = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    INSTANCE_NAME = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FIRED_TIME = table.Column<long>(type: "bigint", nullable: false),
                    SCHED_TIME = table.Column<long>(type: "bigint", nullable: false),
                    PRIORITY = table.Column<int>(type: "int", nullable: false),
                    STATE = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    JOB_NAME = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    JOB_GROUP = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IS_NONCONCURRENT = table.Column<bool>(type: "bit", nullable: false),
                    REQUESTS_RECOVERY = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRTZ_FIRED_TRIGGERS", x => new { x.SCHED_NAME, x.ENTRY_ID });
                });

            migrationBuilder.CreateTable(
                name: "QRTZ_JOB_DETAILS",
                schema: "quartz",
                columns: table => new
                {
                    SCHED_NAME = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    JOB_NAME = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    JOB_GROUP = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    JOB_CLASS_NAME = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    IS_DURABLE = table.Column<bool>(type: "bit", nullable: false),
                    IS_NONCONCURRENT = table.Column<bool>(type: "bit", nullable: false),
                    IS_UPDATE_DATA = table.Column<bool>(type: "bit", nullable: false),
                    REQUESTS_RECOVERY = table.Column<bool>(type: "bit", nullable: false),
                    JOB_DATA = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRTZ_JOB_DETAILS", x => new { x.SCHED_NAME, x.JOB_NAME, x.JOB_GROUP });
                });

            migrationBuilder.CreateTable(
                name: "QRTZ_LOCKS",
                schema: "quartz",
                columns: table => new
                {
                    SCHED_NAME = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LOCK_NAME = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRTZ_LOCKS", x => new { x.SCHED_NAME, x.LOCK_NAME });
                });

            migrationBuilder.CreateTable(
                name: "QRTZ_PAUSED_TRIGGER_GRPS",
                schema: "quartz",
                columns: table => new
                {
                    SCHED_NAME = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    TRIGGER_GROUP = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRTZ_PAUSED_TRIGGER_GRPS", x => new { x.SCHED_NAME, x.TRIGGER_GROUP });
                });

            migrationBuilder.CreateTable(
                name: "QRTZ_SCHEDULER_STATE",
                schema: "quartz",
                columns: table => new
                {
                    SCHED_NAME = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    INSTANCE_NAME = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LAST_CHECKIN_TIME = table.Column<long>(type: "bigint", nullable: false),
                    CHECKIN_INTERVAL = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRTZ_SCHEDULER_STATE", x => new { x.SCHED_NAME, x.INSTANCE_NAME });
                });

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

            migrationBuilder.CreateTable(
                name: "TaxDeduction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaxYear = table.Column<int>(type: "int", nullable: false),
                    Remuneration = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    AnnualEquivalent = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    TaxUnder65 = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Tax65To74 = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    TaxOver75 = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxDeduction", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaxTableUpload",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaxYear = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxTableUpload", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "LeaveEntitlementRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeaveTypeId = table.Column<int>(type: "int", nullable: false),
                    JobGradeId = table.Column<int>(type: "int", nullable: false),
                    MinYearsService = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxYearsService = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DaysAllocated = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveEntitlementRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveEntitlementRules_JobGrades_JobGradeId",
                        column: x => x.JobGradeId,
                        principalTable: "JobGrades",
                        principalColumn: "JobGradeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveEntitlementRules_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalOptions",
                columns: table => new
                {
                    MedicalOptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicalOptionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MedicalOptionCategoryId = table.Column<int>(type: "int", nullable: false),
                    SalaryBracketMin = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    SalaryBracketMax = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    MonthlyRiskContributionPrincipal = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    MonthlyRiskContributionAdult = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    MonthlyRiskContributionChild = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    MonthlyRiskContributionChild2 = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    MonthlyMsaContributionPrincipal = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    MonthlyMsaContributionAdult = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    MonthlyMsaContributionChild = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    TotalMonthlyContributionsPrincipal = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    TotalMonthlyContributionsAdult = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    TotalMonthlyContributionsChild = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    TotalMonthlyContributionsChild2 = table.Column<decimal>(type: "decimal(15,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalOptions", x => x.MedicalOptionId);
                    table.ForeignKey(
                        name: "FK_MedicalOptions_MedicalOptionCategories_MedicalOptionCategoryId",
                        column: x => x.MedicalOptionCategoryId,
                        principalTable: "MedicalOptionCategories",
                        principalColumn: "MedicalOptionCategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    PositionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PositionTitle = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    JobGradeId = table.Column<int>(type: "int", nullable: false),
                    OccupationalLevelId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.PositionId);
                    table.ForeignKey(
                        name: "FK_Positions_JobGrades_JobGradeId",
                        column: x => x.JobGradeId,
                        principalTable: "JobGrades",
                        principalColumn: "JobGradeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Positions_OccupationalLevels_OccupationalLevelId",
                        column: x => x.OccupationalLevelId,
                        principalTable: "OccupationalLevels",
                        principalColumn: "OccupationalLevelId",
                        onDelete: ReferentialAction.Restrict);
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
                name: "QRTZ_TRIGGERS",
                schema: "quartz",
                columns: table => new
                {
                    SCHED_NAME = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    TRIGGER_NAME = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TRIGGER_GROUP = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    JOB_NAME = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    JOB_GROUP = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    NEXT_FIRE_TIME = table.Column<long>(type: "bigint", nullable: true),
                    PREV_FIRE_TIME = table.Column<long>(type: "bigint", nullable: true),
                    PRIORITY = table.Column<int>(type: "int", nullable: true),
                    TRIGGER_STATE = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    TRIGGER_TYPE = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    START_TIME = table.Column<long>(type: "bigint", nullable: false),
                    END_TIME = table.Column<long>(type: "bigint", nullable: true),
                    CALENDAR_NAME = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MISFIRE_INSTR = table.Column<short>(type: "smallint", nullable: true),
                    JOB_DATA = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRTZ_TRIGGERS", x => new { x.SCHED_NAME, x.TRIGGER_NAME, x.TRIGGER_GROUP });
                    table.ForeignKey(
                        name: "FK_QRTZ_TRIGGERS_QRTZ_JOB_DETAILS_SCHED_NAME_JOB_NAME_JOB_GROUP",
                        columns: x => new { x.SCHED_NAME, x.JOB_NAME, x.JOB_GROUP },
                        principalSchema: "quartz",
                        principalTable: "QRTZ_JOB_DETAILS",
                        principalColumns: new[] { "SCHED_NAME", "JOB_NAME", "JOB_GROUP" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdNumber = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    PassportNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhysicalAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ZipCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HasDisability = table.Column<bool>(type: "bit", nullable: false),
                    DisabilityDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Branch = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MonthlySalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    EmploymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CareerManagerID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ProfileImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PensionOptionId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeId);
                    table.ForeignKey(
                        name: "FK_Employees_Employees_CareerManagerID",
                        column: x => x.CareerManagerID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_PensionOptions_PensionOptionId",
                        column: x => x.PensionOptionId,
                        principalTable: "PensionOptions",
                        principalColumn: "PensionOptionId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Employees_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "PositionId",
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

            migrationBuilder.CreateTable(
                name: "StatutoryContributions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [PayrollRecordSequence]"),
                    PayrollRunId = table.Column<int>(type: "int", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PassportNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MonthlySalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UifEmployeeAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UifEmployerAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EmployerSdlContribution = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DeductedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrentMonth = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatutoryContributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatutoryContributions_PayrollRuns_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalTable: "PayrollRuns",
                        principalColumn: "PayrollRunId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QRTZ_BLOB_TRIGGERS",
                schema: "quartz",
                columns: table => new
                {
                    SCHED_NAME = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    TRIGGER_NAME = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TRIGGER_GROUP = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    BLOB_DATA = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRTZ_BLOB_TRIGGERS", x => new { x.SCHED_NAME, x.TRIGGER_NAME, x.TRIGGER_GROUP });
                    table.ForeignKey(
                        name: "FK_QRTZ_BLOB_TRIGGERS_QRTZ_TRIGGERS_SCHED_NAME_TRIGGER_NAME_TRIGGER_GROUP",
                        columns: x => new { x.SCHED_NAME, x.TRIGGER_NAME, x.TRIGGER_GROUP },
                        principalSchema: "quartz",
                        principalTable: "QRTZ_TRIGGERS",
                        principalColumns: new[] { "SCHED_NAME", "TRIGGER_NAME", "TRIGGER_GROUP" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QRTZ_CRON_TRIGGERS",
                schema: "quartz",
                columns: table => new
                {
                    SCHED_NAME = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    TRIGGER_NAME = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TRIGGER_GROUP = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CRON_EXPRESSION = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    TIME_ZONE_ID = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRTZ_CRON_TRIGGERS", x => new { x.SCHED_NAME, x.TRIGGER_NAME, x.TRIGGER_GROUP });
                    table.ForeignKey(
                        name: "FK_QRTZ_CRON_TRIGGERS_QRTZ_TRIGGERS_SCHED_NAME_TRIGGER_NAME_TRIGGER_GROUP",
                        columns: x => new { x.SCHED_NAME, x.TRIGGER_NAME, x.TRIGGER_GROUP },
                        principalSchema: "quartz",
                        principalTable: "QRTZ_TRIGGERS",
                        principalColumns: new[] { "SCHED_NAME", "TRIGGER_NAME", "TRIGGER_GROUP" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QRTZ_SIMPLE_TRIGGERS",
                schema: "quartz",
                columns: table => new
                {
                    SCHED_NAME = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    TRIGGER_NAME = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TRIGGER_GROUP = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    REPEAT_COUNT = table.Column<long>(type: "bigint", nullable: false),
                    REPEAT_INTERVAL = table.Column<long>(type: "bigint", nullable: false),
                    TIMES_TRIGGERED = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRTZ_SIMPLE_TRIGGERS", x => new { x.SCHED_NAME, x.TRIGGER_NAME, x.TRIGGER_GROUP });
                    table.ForeignKey(
                        name: "FK_QRTZ_SIMPLE_TRIGGERS_QRTZ_TRIGGERS_SCHED_NAME_TRIGGER_NAME_TRIGGER_GROUP",
                        columns: x => new { x.SCHED_NAME, x.TRIGGER_NAME, x.TRIGGER_GROUP },
                        principalSchema: "quartz",
                        principalTable: "QRTZ_TRIGGERS",
                        principalColumns: new[] { "SCHED_NAME", "TRIGGER_NAME", "TRIGGER_GROUP" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QRTZ_SIMPROP_TRIGGERS",
                schema: "quartz",
                columns: table => new
                {
                    SCHED_NAME = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    TRIGGER_NAME = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TRIGGER_GROUP = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    STR_PROP_1 = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    STR_PROP_2 = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    STR_PROP_3 = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    INT_PROP_1 = table.Column<int>(type: "int", nullable: true),
                    INT_PROP_2 = table.Column<int>(type: "int", nullable: true),
                    LONG_PROP_1 = table.Column<long>(type: "bigint", nullable: true),
                    LONG_PROP_2 = table.Column<long>(type: "bigint", nullable: true),
                    DEC_PROP_1 = table.Column<decimal>(type: "numeric(13,4)", nullable: true),
                    DEC_PROP_2 = table.Column<decimal>(type: "numeric(13,4)", nullable: true),
                    BOOL_PROP_1 = table.Column<bool>(type: "bit", nullable: true),
                    BOOL_PROP_2 = table.Column<bool>(type: "bit", nullable: true),
                    TIME_ZONE_ID = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRTZ_SIMPROP_TRIGGERS", x => new { x.SCHED_NAME, x.TRIGGER_NAME, x.TRIGGER_GROUP });
                    table.ForeignKey(
                        name: "FK_QRTZ_SIMPROP_TRIGGERS_QRTZ_TRIGGERS_SCHED_NAME_TRIGGER_NAME_TRIGGER_GROUP",
                        columns: x => new { x.SCHED_NAME, x.TRIGGER_NAME, x.TRIGGER_GROUP },
                        principalSchema: "quartz",
                        principalTable: "QRTZ_TRIGGERS",
                        principalColumns: new[] { "SCHED_NAME", "TRIGGER_NAME", "TRIGGER_GROUP" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnnualLeaveAccrualHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Accrued = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Used = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Forfeited = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnnualLeaveAccrualHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnnualLeaveAccrualHistories_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeAccrualRateHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    PositionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnnualEntitlement = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DailyRate = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeAccrualRateHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeAccrualRateHistories_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeAccrualRateHistories_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "PositionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeLeaveBalances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LeaveTypeId = table.Column<int>(type: "int", nullable: false),
                    AccruedDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LastAccrualDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TakenDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AvailableDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CarryoverDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ForfeitedDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LastResetYear = table.Column<int>(type: "int", nullable: true),
                    LastCalculatedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeLeaveBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeLeaveBalances_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeLeaveBalances_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "LeaveApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LeaveTypeId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DaysRequested = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AppliedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DecisionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DecisionBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovalToken = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveApplications_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeaveApplications_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "JobGrades",
                columns: new[] { "JobGradeId", "CreatedDate", "IsActive", "Name", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 2, 11, 14, 33, 32, 0, DateTimeKind.Unspecified), true, "Executive Director", new DateTime(2026, 2, 27, 10, 15, 8, 0, DateTimeKind.Unspecified) },
                    { 2, new DateTime(2026, 2, 12, 7, 14, 45, 0, DateTimeKind.Unspecified), true, "Junior Management", new DateTime(2026, 2, 27, 6, 41, 39, 0, DateTimeKind.Unspecified) },
                    { 3, new DateTime(2026, 2, 12, 7, 15, 20, 0, DateTimeKind.Unspecified), true, "Middle Management", new DateTime(2026, 2, 12, 7, 15, 20, 0, DateTimeKind.Unspecified) },
                    { 4, new DateTime(2026, 2, 12, 7, 15, 39, 0, DateTimeKind.Unspecified), true, "Skilled/Semi Skilled", new DateTime(2026, 2, 12, 7, 15, 39, 0, DateTimeKind.Unspecified) },
                    { 5, new DateTime(2026, 2, 12, 7, 15, 54, 0, DateTimeKind.Unspecified), true, "Top/Senior Management", new DateTime(2026, 2, 12, 7, 15, 54, 0, DateTimeKind.Unspecified) },
                    { 6, new DateTime(2026, 2, 12, 7, 16, 8, 0, DateTimeKind.Unspecified), true, "Unskilled", new DateTime(2026, 2, 12, 7, 16, 8, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "LeaveTypes",
                columns: new[] { "Id", "CarryoverExpiryDay", "CarryoverExpiryMonth", "CarryoverNotificationDay", "CarryoverNotificationMonth", "Code", "Description", "FemaleOnly", "IsActive", "IsRollingWindow", "MaxCarryoverDays", "Name", "ResetDay", "ResetMonth", "RollingMonths" },
                values: new object[,]
                {
                    { 1, 1, 1, 1, 12, "AL", "Annual Leave Policy", false, true, false, 5, "Annual Leave", 1, 1, null },
                    { 2, null, null, null, null, "SL", "Sick Leave Policy", false, true, true, null, "Sick Leave", null, null, 36 },
                    { 3, null, null, null, null, "ML", "Maternity Leave Policy", true, true, false, null, "Maternity Leave", null, null, null },
                    { 4, null, null, null, null, "FRL", "Family Responsibility Policy", false, true, true, null, "Family Responsibility Leave", null, null, 12 }
                });

            migrationBuilder.InsertData(
                table: "OccupationalLevels",
                columns: new[] { "OccupationalLevelId", "CreatedDate", "Description", "IsActive", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 2, 12, 12, 26, 22, 0, DateTimeKind.Unspecified), "Top management", true, new DateTime(2026, 2, 23, 13, 45, 35, 0, DateTimeKind.Unspecified) },
                    { 2, new DateTime(2026, 2, 12, 12, 31, 7, 0, DateTimeKind.Unspecified), "Skilled technical and academically qualified workers, junior management, supervisors, foremen and superintendents", true, new DateTime(2026, 2, 23, 13, 51, 57, 0, DateTimeKind.Unspecified) },
                    { 3, new DateTime(2026, 2, 12, 12, 26, 59, 0, DateTimeKind.Unspecified), "Semi-skilled and discretionary decision making", true, new DateTime(2026, 2, 23, 13, 52, 19, 0, DateTimeKind.Unspecified) },
                    { 4, new DateTime(2026, 2, 13, 12, 54, 57, 0, DateTimeKind.Unspecified), "Professionally qualified, experienced specialists, and mid-management", true, new DateTime(2026, 2, 23, 13, 52, 42, 0, DateTimeKind.Unspecified) },
                    { 5, new DateTime(2026, 2, 12, 12, 29, 24, 0, DateTimeKind.Unspecified), "Senior management", true, new DateTime(2026, 2, 23, 13, 53, 2, 0, DateTimeKind.Unspecified) },
                    { 6, new DateTime(2026, 2, 12, 12, 30, 2, 0, DateTimeKind.Unspecified), "Unskilled and defined decision making", true, new DateTime(2026, 2, 23, 13, 53, 25, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "LeaveEntitlementRules",
                columns: new[] { "Id", "DaysAllocated", "IsActive", "JobGradeId", "LeaveTypeId", "MaxYearsService", "MinYearsService" },
                values: new object[,]
                {
                    { 1, 15m, true, 2, 1, 2.99m, 0m },
                    { 2, 15m, true, 3, 1, 2.99m, 0m },
                    { 3, 15m, true, 4, 1, 2.99m, 0m },
                    { 4, 15m, true, 6, 1, 2.99m, 0m },
                    { 5, 18m, true, 2, 1, 5m, 3m },
                    { 6, 18m, true, 3, 1, 5m, 3m },
                    { 7, 18m, true, 4, 1, 5m, 3m },
                    { 8, 18m, true, 6, 1, 5m, 3m },
                    { 9, 20m, true, 2, 1, null, 5.01m },
                    { 10, 20m, true, 3, 1, null, 5.01m },
                    { 11, 20m, true, 4, 1, null, 5.01m },
                    { 12, 20m, true, 6, 1, null, 5.01m },
                    { 13, 18m, true, 5, 1, 2.99m, 0m },
                    { 14, 21m, true, 5, 1, 5m, 3m },
                    { 15, 23m, true, 5, 1, null, 5.01m },
                    { 16, 22m, true, 1, 1, 2.99m, 0m },
                    { 17, 25m, true, 1, 1, 5m, 3m },
                    { 18, 27m, true, 1, 1, null, 5.01m }
                });

            migrationBuilder.InsertData(
                table: "Positions",
                columns: new[] { "PositionId", "CreatedDate", "IsActive", "JobGradeId", "OccupationalLevelId", "PositionTitle", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 1, 1, "Chief Financial Officer", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 1, 1, "Chief Operating Officer", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 1, 1, "Officer", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 1, 1, "Executive", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 1, 1, "Founder", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 1, 1, "Head: Financial Services", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 2, 2, "Accountant", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 2, 2, "Admin Manager", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 2, 2, "Analyst", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 2, 2, "Associate Data Analyst", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 11, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 2, 2, "Associate Project Analyst", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 12, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 2, 2, "Associate Software Engineer", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 13, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 2, 2, "Bookkeeper", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 14, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 2, 3, "Client Liason Manager", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 17, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 2, 2, "Data Analyst", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 18, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 2, 4, "Finance Administrator", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 19, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 2, 2, "Finance Agent 1", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 20, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 2, 2, "Finance Agent 2", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 21, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 2, 2, "Finance Agent 3", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 22, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 2, 2, "Finance Supervisor", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 23, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 2, 2, "INACTIVE Software Developer", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnnualLeaveAccrualHistories_EmployeeId",
                table: "AnnualLeaveAccrualHistories",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAccrualRateHistories_EmployeeId",
                table: "EmployeeAccrualRateHistories",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAccrualRateHistories_PositionId",
                table: "EmployeeAccrualRateHistories",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaveBalances_EmployeeId",
                table: "EmployeeLeaveBalances",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaveBalances_LeaveTypeId",
                table: "EmployeeLeaveBalances",
                column: "LeaveTypeId");

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
                name: "IX_Employees_CareerManagerID",
                table: "Employees",
                column: "CareerManagerID");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PensionOptionId",
                table: "Employees",
                column: "PensionOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PositionId",
                table: "Employees",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveApplications_EmployeeId",
                table: "LeaveApplications",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveApplications_LeaveTypeId",
                table: "LeaveApplications",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveEntitlementRules_JobGradeId",
                table: "LeaveEntitlementRules",
                column: "JobGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveEntitlementRules_LeaveTypeId",
                table: "LeaveEntitlementRules",
                column: "LeaveTypeId");

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
                name: "IX_MedicalOptions_MedicalOptionCategoryId",
                table: "MedicalOptions",
                column: "MedicalOptionCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_OccupationalLevels_Description",
                table: "OccupationalLevels",
                column: "Description",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_Positions_JobGradeId",
                table: "Positions",
                column: "JobGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_OccupationalLevelId",
                table: "Positions",
                column: "OccupationalLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_PositionTitle",
                table: "Positions",
                column: "PositionTitle",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_FT_JOB_GROUP",
                schema: "quartz",
                table: "QRTZ_FIRED_TRIGGERS",
                column: "JOB_GROUP");

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_FT_JOB_NAME",
                schema: "quartz",
                table: "QRTZ_FIRED_TRIGGERS",
                column: "JOB_NAME");

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_FT_JOB_REQ_RECOVERY",
                schema: "quartz",
                table: "QRTZ_FIRED_TRIGGERS",
                column: "REQUESTS_RECOVERY");

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_FT_TRIG_GROUP",
                schema: "quartz",
                table: "QRTZ_FIRED_TRIGGERS",
                column: "TRIGGER_GROUP");

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_FT_TRIG_INST_NAME",
                schema: "quartz",
                table: "QRTZ_FIRED_TRIGGERS",
                column: "INSTANCE_NAME");

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_FT_TRIG_NAME",
                schema: "quartz",
                table: "QRTZ_FIRED_TRIGGERS",
                column: "TRIGGER_NAME");

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_FT_TRIG_NM_GP",
                schema: "quartz",
                table: "QRTZ_FIRED_TRIGGERS",
                columns: new[] { "SCHED_NAME", "TRIGGER_NAME", "TRIGGER_GROUP" });

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_J_REQ_RECOVERY",
                schema: "quartz",
                table: "QRTZ_JOB_DETAILS",
                column: "REQUESTS_RECOVERY");

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_T_NEXT_FIRE_TIME",
                schema: "quartz",
                table: "QRTZ_TRIGGERS",
                column: "NEXT_FIRE_TIME");

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_T_NFT_ST",
                schema: "quartz",
                table: "QRTZ_TRIGGERS",
                columns: new[] { "NEXT_FIRE_TIME", "TRIGGER_STATE" });

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_T_STATE",
                schema: "quartz",
                table: "QRTZ_TRIGGERS",
                column: "TRIGGER_STATE");

            migrationBuilder.CreateIndex(
                name: "IX_QRTZ_TRIGGERS_SCHED_NAME_JOB_NAME_JOB_GROUP",
                schema: "quartz",
                table: "QRTZ_TRIGGERS",
                columns: new[] { "SCHED_NAME", "JOB_NAME", "JOB_GROUP" });

            migrationBuilder.CreateIndex(
                name: "IX_StatutoryContributions_PayrollRunId",
                table: "StatutoryContributions",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxDeduction_TaxYear_Remuneration",
                table: "TaxDeduction",
                columns: new[] { "TaxYear", "Remuneration" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnnualLeaveAccrualHistories");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "EmployeeAccrualRateHistories");

            migrationBuilder.DropTable(
                name: "EmployeeLeaveBalances");

            migrationBuilder.DropTable(
                name: "EmployeePensionEnrollments");

            migrationBuilder.DropTable(
                name: "LeaveApplications");

            migrationBuilder.DropTable(
                name: "LeaveEntitlementRules");

            migrationBuilder.DropTable(
                name: "MedicalAidDeductions");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PasswordHistories");

            migrationBuilder.DropTable(
                name: "PasswordResetPins");

            migrationBuilder.DropTable(
                name: "PensionDeductions");

            migrationBuilder.DropTable(
                name: "QRTZ_BLOB_TRIGGERS",
                schema: "quartz");

            migrationBuilder.DropTable(
                name: "QRTZ_CALENDARS",
                schema: "quartz");

            migrationBuilder.DropTable(
                name: "QRTZ_CRON_TRIGGERS",
                schema: "quartz");

            migrationBuilder.DropTable(
                name: "QRTZ_FIRED_TRIGGERS",
                schema: "quartz");

            migrationBuilder.DropTable(
                name: "QRTZ_LOCKS",
                schema: "quartz");

            migrationBuilder.DropTable(
                name: "QRTZ_PAUSED_TRIGGER_GRPS",
                schema: "quartz");

            migrationBuilder.DropTable(
                name: "QRTZ_SCHEDULER_STATE",
                schema: "quartz");

            migrationBuilder.DropTable(
                name: "QRTZ_SIMPLE_TRIGGERS",
                schema: "quartz");

            migrationBuilder.DropTable(
                name: "QRTZ_SIMPROP_TRIGGERS",
                schema: "quartz");

            migrationBuilder.DropTable(
                name: "StatutoryContributions");

            migrationBuilder.DropTable(
                name: "StatutoryContributionTypes");

            migrationBuilder.DropTable(
                name: "TaxDeduction");

            migrationBuilder.DropTable(
                name: "TaxTableUpload");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "LeaveTypes");

            migrationBuilder.DropTable(
                name: "MedicalOptions");

            migrationBuilder.DropTable(
                name: "QRTZ_TRIGGERS",
                schema: "quartz");

            migrationBuilder.DropTable(
                name: "PayrollRuns");

            migrationBuilder.DropTable(
                name: "PensionOptions");

            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.DropTable(
                name: "MedicalOptionCategories");

            migrationBuilder.DropTable(
                name: "QRTZ_JOB_DETAILS",
                schema: "quartz");

            migrationBuilder.DropTable(
                name: "PayrollPeriods");

            migrationBuilder.DropTable(
                name: "JobGrades");

            migrationBuilder.DropTable(
                name: "OccupationalLevels");

            migrationBuilder.DropSequence(
                name: "PayrollRecordSequence");
        }
    }
}
