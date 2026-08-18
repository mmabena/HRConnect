using System.Text;
using Audit.Core;
using Audit.EntityFramework;
using HRConnect.Api.Data;
using HRConnect.Api.Interfaces;
using HRConnect.Api.Interfaces.TOTP;
using HRConnect.Api.Interfaces.Notification;
using HRConnect.Api.Interfaces.Payroll.Deduction;
using HRConnect.Api.Interfaces.Payroll.Earning;
using HRConnect.Api.Interfaces.Pension;
using HRConnect.Api.Middleware;
using HRConnect.Api.Models;
using HRConnect.Api.Repositories;
using HRConnect.Api.Repository;
using HRConnect.Api.Hubs;
using HRConnect.Api.Services;
using HRConnect.Api.Hubs;
using HRConnect.Api.Utils;
using HRConnect.Api.Utils.Security;
using HRConnect.Api.Utils.Factories;
using HRConnect.Api.Utils.Jobs;
using HRConnect.Api.Utils.Jobs.Notification;
using HRConnect.Api.Utils.Jobs.Pension;
using HRConnect.Api.Interfaces.Payroll.Earning;
using HRConnect.Api.Interfaces.Payroll.Deduction;
using HRConnect.Api.Utils.Jobs;
using HRConnect.Api.Utils.BankingDetailsValidation;
using HRConnect.Api.Utils.Settings;
using HRConnect.Api.Utils.Jobs.Payroll;
using HRConnect.Api.Utils.Notification;
using HRConnect.Api.Utils.Payroll;
using HRConnect.Api.Utils.Jobs.Payroll;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OfficeOpenXml;
using Quartz;
using HRConnect.Api.Interfaces.Notification;
using HRConnect.Api.Utils.Factories;
using HRConnect.Api.Utils.Notification;
using HRConnect.Api.Interfaces.Payroll.Earning;
using HRConnect.Api.Interfaces.Payroll.Deduction;
using System.Threading.RateLimiting;
using HRConnect.Api.Utils.Notification.Channels;

var builder = WebApplication.CreateBuilder(args);


//Audit configuration for custom audit capturing
Audit.Core.Configuration.Setup()
  .UseEntityFramework(config => config
      .AuditTypeExplicitMapper(map => map
        .Map<StatutoryContribution, AuditLogs>((entity, audit) =>
          {
            audit.EmployeeId = entity.EmployeeId;
            audit.IdNumber = entity.IdNumber;
            audit.PassportNumber = entity.PassportNumber;
            audit.MonthlySalary = entity.MonthlySalary;
            audit.ProjectedSalary = entity.MonthlySalary - entity.UifEmployeeAmount;
            audit.UifEmployeeAmount = entity.UifEmployeeAmount;
            audit.UifEmployerAmount = entity.UifEmployerAmount;
            audit.EmployerSdlContribution = entity.EmployerSdlContribution;
          })
        .AuditEntityAction<AuditLogs>((e, entry, audit) =>
        {
          audit.AuditedAt = DateTime.Now;
          audit.AuditAction = entry.Action;
          audit.TabelName = entry.Name;
        })));

ExcelPackage.License.SetNonCommercialPersonal("YourName");


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
      options.JsonSerializerOptions.Converters.Add(
          new System.Text.Json.Serialization.JsonStringEnumConverter()
      );
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo { Title = "HRConnect.Api", Version = "v1" });

  var securityScheme = new OpenApiSecurityScheme
  {
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    Scheme = "bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "Enter 'Bearer' [space] and then your JWT token.",
    Reference = new OpenApiReference
    {
      Type = ReferenceType.SecurityScheme,
      Id = "Bearer"
    }
  };

  c.AddSecurityDefinition("Bearer", securityScheme);

  c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
    { securityScheme, Array.Empty<string>() }
    });

});

builder.Services.AddOpenApi();
builder.Services.AddDbContext<ApplicationDBContext>(options =>
    {
      options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!);
      options.AddInterceptors(new AuditSaveChangesInterceptor());
    });

builder.Services.AddAuthentication(options =>
{
  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
  var jwt = builder.Configuration.GetSection("JwtSettings");
  string secretValue = jwt["Secret"] ?? string.Empty;
  byte[] keyBytes;
  try
  {
    // Try to interpret as base64 first
    keyBytes = Convert.FromBase64String(secretValue);
  }
  catch (FormatException)
  {
    // Fallback to UTF8 bytes if not base64
    keyBytes = Encoding.UTF8.GetBytes(secretValue);
  }

  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = jwt["Issuer"],
    ValidAudience = jwt["Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
  };
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("SuperUserOnly", policy => policy.RequireRole("SuperUser"))
    .AddPolicy("NormalUserOnly", policy => policy.RequireRole("NormalUser"))
    .AddPolicy("SuperOrNormalUser", policy => policy.RequireRole("SuperUser", "NormalUser"));

builder.Services.AddQuartz(q =>
{
  var RolloverJobKey = new JobKey("PayrollRolloverJob");
 var NotificationJobKey = new JobKey("NotificationJob");
  //Add a service for to run as a background job 
  q.AddJob<PayrollRolloverJob>(opts =>
  opts.WithIdentity(RolloverJobKey)
  .StoreDurably());

  q.AddJob<NotificationJob>(opts =>
  opts.WithIdentity(NotificationJobKey)
  .StoreDurably());

  //Cron Schedule for Payroll Rollover Job
  // 0 -> 0 seconds
  // 0 -> 0 minutes
  // 0 -> 0 hours
  // 1 -> first day of the month 
  // * -> for any/every month 
  // ? -> for all days of the week
  q.AddTrigger(opts => opts
  .ForJob(RolloverJobKey)
  .WithIdentity("PayrollRollover-Trigger")
    .WithCronSchedule("0 0/20 * * * ?", x =>
    x.WithMisfireHandlingInstructionFireAndProceed()));

  q.AddTrigger(opts => opts
  .ForJob(NotificationJobKey)
  .WithIdentity("NotificationJob-Trigger")
  .WithCronSchedule("0 * * * * ?", x =>
  x.WithMisfireHandlingInstructionIgnoreMisfires()));
  //Cron Schedule for Payroll Notification Job
  // 0 -> 0 seconds
  // 0 -> 0 minutes
  // 0 -> 0 hours
  // 23-31 is the widest range of days to include February and longer months
  // * -> for any/every month 
  // ? -> for all days of the week

  JobKey employeePensionEnrollmentJob = new("EmployeeEnrollmentJob");
   q.AddJob<EmployeeEnrollmentJob>(opts =>
        opts.WithIdentity(employeePensionEnrollmentJob)
        .StoreDurably());

   q.AddTrigger(opts => opts
      .ForJob(employeePensionEnrollmentJob)
      .StartNow());

  q.UsePersistentStore(store =>
  {
    store.UseSqlServer(options =>
        {
          options.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
          options.TablePrefix = "quartz.QRTZ_";
        });
    store.UseSerializer<Quartz.Simpl.SystemTextJsonObjectSerializer>();
    store.UseProperties = true;
  });
});

builder.Services.AddQuartzHostedService(q =>
{
  q.WaitForJobsToComplete = true;
});

builder.Services.Configure<EncryptionSettings>(
  builder.Configuration.GetSection("EncryptionSettings"));

builder.Configuration.AddUserSecrets<Program>();
builder.Services.AddSingleton(provider =>
  provider.GetRequiredService<ISchedulerFactory>().GetScheduler().GetAwaiter().GetResult());

builder.Services.AddScoped<IPayrollPeriodRepository, PayrollPeriodRepository>();
builder.Services.AddScoped<IPayrollRunRepository, PayrollRunRepository>();
builder.Services.AddScoped<IPayrollRunService, PayrollRunService>();
builder.Services.AddScoped<IPayrollPeriodService, PayrollPeriodService>();
builder.Services.AddScoped<IReportsService, ReportsService>();
builder.Services.AddScoped<PayrollRolloverJob>();
builder.Services.AddScoped<PayrollInit>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<HRConnect.Api.Interfaces.IUserService, HRConnect.Api.Services.UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITaxTableUploadService, TaxTableUploadService>();
builder.Services.AddScoped<ITaxTableUploadRepository, TaxTableUploadRepository>();
builder.Services.AddScoped<ITaxDeductionService, TaxDeductionService>();
builder.Services.AddScoped<ITaxDeductionRepository, TaxDeductionRepository>();
builder.Services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();
builder.Services.AddScoped<IPositionRepository, PositionRepository>();
builder.Services.AddScoped<IPositionService, PositionService>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IActiveCompanyService, ActiveCompanyService>();
builder.Services.AddScoped<IUserCompanyService, UserCompanyService>();
builder.Services.AddScoped<IMedicalAidDependentService, MedicalAidDependentService>();
builder.Services.AddScoped<IMedicalAidDependentRepository, MedicalAidDependentRepository>();
builder.Services.AddScoped<IUserCompanyRepository, UserCompanyRepository>();
builder.Services.AddScoped<ICompanyContributionRepository, CompanyContributionRepository>();
builder.Services.AddScoped<IEmployeeCompanyContributionRepository, EmployeeCompanyContributionRepository>();
builder.Services.AddScoped<ICompanyContributionAllocationService, CompanyContributionAllocationService>();
builder.Services.AddScoped<ICompanyContributionRepository, CompanyContributionRepository>();
builder.Services.AddScoped<ICompanyContributionService, CompanyContributionService>();
builder.Services.AddScoped<IJobGradeRepository, JobGradeRepository>();
builder.Services.AddScoped<IJobGradeService, JobGradeService>();
builder.Services.AddScoped<IOccupationalLevelRepository, OccupationalLevelRepository>();
builder.Services.AddScoped<IOccupationalLevelService, OccupationalLevelService>();
builder.Services.AddScoped<HRConnect.Api.Interfaces.IAuthService, HRConnect.Api.Services.AuthService>();
builder.Services.AddScoped<IBankingDetailRepository, BankingDetailRepository>();
builder.Services.AddScoped<IBankingDetailService, BankingDetailService>();
// Register the encryption service as a singleton since it does not maintain any state and can be shared across the application.
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ILeaveBalanceService, LeaveBalanceService>();
builder.Services.AddScoped<ILeaveProcessingService, LeaveProcessingService>();
builder.Services.AddScoped<ILeaveRuleService, LeaveRuleService>();
builder.Services.AddScoped<IPensionFundService, PensionFundService>();
builder.Services.AddScoped<IEmployeePensionRepository, EmployeePensionRepository>();
builder.Services.AddScoped<IPensionFundService, PensionFundService>();
builder.Services.AddScoped<IPensionFundRepository, PensionFundRepository>();
builder.Services.AddScoped<ILeaveTypeManagementService, LeaveTypeManagementService>();
builder.Services.AddScoped<ILeaveApplicationService, LeaveApplicationService>();
builder.Services.AddScoped<IMedicalAidDependentNotificationService, MedicalAidDependentNotificationService>();
builder.Services.AddHostedService<LeaveAutomationBackgroundService>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeCompanyContributionService, EmployeeCompanyContributionService>();
builder.Services.AddScoped<IStatutoryContributionRepository, StatutoryContributionRepository>();
builder.Services.AddScoped<IStatutoryContributionService, StatutoryContributionService>();
builder.Services.AddTransient<IPensionProjectionService, PensionProjectionService>();
builder.Services.AddScoped<IMedicalOptionRepository, MedicalOptionRepository>();
builder.Services.AddScoped<IMedicalOptionService, MedicalOptionService>();
builder.Services.AddScoped<IPensionOptionRepository, PensionOptionRepository>();
builder.Services.AddScoped<IEmployeePensionEnrollmentRepository, EmployeePensionEnrollmentRepository>();
builder.Services.AddTransient<IEmployeePensionEnrollmentService, EmployeePensionEnrollmentService>();
builder.Services.AddScoped<IPensionDeductionRepository, PensionDeductionRepository>();
builder.Services.AddTransient<IPensionDeductionService, PensionDeductionService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationFactory, NotificationFactory>();
builder.Services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
builder.Services.AddScoped<INotificationDeliveryChannel, InAppDeliveryChannel>();
builder.Services.AddScoped<INotificationDeliveryChannel, EmailDeliveryChannel>();
builder.Services.AddScoped<IJobScheduleService, JobScheduleService>();
builder.Services.AddScoped<IPayrollEarningRepository, PayrollEarningRepository>();
builder.Services.AddScoped<IPayrollEarningService, PayrollEarningService>();
builder.Services.AddScoped<IEmployeePayrollEarningRepository, EmployeePayrollEarningRepository>();
builder.Services.AddScoped<IEmployeePayrollEarningService, EmployeePayrollEarningService>();
builder.Services.AddScoped<IDeductionRepository, DeductionRepository>();
builder.Services.AddScoped<IDeductionService, DeductionService>();
builder.Services.AddScoped<IEmployeeDeductionRepository, EmployeeDeductionRepository>();
builder.Services.AddScoped<IEmployeeDeductionService, EmployeeDeductionService>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<HashingHelper>();
builder.Services.AddScoped<ITOTPService, TOTPService>();
builder.Services.AddScoped<ITOTPRepository, TOTPRepository>();
builder.Services.AddScoped<IMFAUserSecretsService, MFAUserSecretsService>();
builder.Services.AddScoped<IMFAUserSecretsRepository, MFAUserSecretsRepository>();
builder.Services.AddScoped<IMedicalOptionService,
  MedicalOptionService>();
builder.Services.AddScoped<IMedicalAidEligibilityService, MedicalAidEligibilityService>();
builder.Services.AddScoped<IMedicalAidDeductionRepository, MedicalAidDeductionRepository>();
builder.Services.AddScoped<IMedicalAidDeductionService, MedicalAidDeductionService>();


builder.Services.AddHttpClient<IUserHttpClient, UserHttpClient>((provider, client) =>
{
  IConfiguration config = provider.GetRequiredService<IConfiguration>();
  client.BaseAddress = new Uri(config["Services:Api"]!);
});

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowReact",
      policy => policy
          .WithOrigins("http://localhost:3000", "http://localhost:5147")
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials());
});

builder.Services.AddRateLimiter(options =>
{
  options.AddPolicy("totp-policy", ctx =>
  {
    var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";

    return RateLimitPartition.GetFixedWindowLimiter(
     partitionKey: ip,
      factory: _ => new FixedWindowRateLimiterOptions
      {
        PermitLimit = 3,//3 attempts per time frame
        Window = TimeSpan.FromMinutes(1),
        QueueLimit = 0
      });
  });
});

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
  var initialiser = scope.ServiceProvider.GetRequiredService<PayrollInit>();
  var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

  //initialise a payperiod and payrun on app start up
  await initialiser.InitialisePayrollPeriod();
}


using (var scope = app.Services.CreateScope())
{
  var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
  await userService.SyncEmployeeUserAsync();
}


if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI(c =>
  {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "HRConnect.Api v1");
  });
}

// app.UseHttpsRedirection();
app.UseCors("AllowReact");
// Adding Global Exception Handler
app.UseGlobalExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionMiddleware>();
app.UseRateLimiter();
app.MapControllers();
app.MapHub<UserPositionHub>("/UserPositionHub");
app.MapHub<CompanyHub>("/companyHub");
app.Run();