using HRConnect.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
// using Resend;
using HRConnect.Api.Interfaces;
using HRConnect.Api.Middleware;
using HRConnect.Api.Repositories;
using HRConnect.Api.Services;
using HRConnect.Api.Repository;
using Microsoft.AspNetCore.Identity;
using HRConnect.Api.Models;
using HRConnect.Api.Utils;
using OfficeOpenXml;
using HRConnect.Api.Interfaces.PensionProjection;
using Audit.Core;
using Audit.EntityFramework;
using Quartz;
using System.Text.Json;

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
          audit.AuditedAt = DateTime.UtcNow;
          audit.AuditAction = entry.Action;
          audit.TabelName = entry.Name;
        })));

ExcelPackage.License.SetNonCommercialPersonal("YourName");


builder.Services.AddControllers();
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
      options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
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
  // Read secret and support base64-encoded secrets (recommended) or plain-text fallback
  var secretValue = jwt["Secret"] ?? string.Empty;
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
  var jobKey = new JobKey("PayrollRolloverJob");

  //Add a service for to run as a background job 
  q.AddJob<PayrollRolloverJob>(opts =>
  opts.WithIdentity(jobKey)
  .StoreDurably());

  //Triggers that will need to be fired to run background job
  // using Cron Schedule
  // Second, Minute, Hour, Day of The Month, Month, Day of The Week
  q.AddTrigger(opts => opts
  .ForJob(jobKey)
  .WithIdentity("PayrollRollover-Trigger")
  .WithCronSchedule("0 0 0 1 * ?", x =>
  x.WithMisfireHandlingInstructionFireAndProceed())); //when a job misfire happens. 
                                                      // Properly re-execute it and proceed as usual

  // 0 -> 0 seconds
  // 0 -> 0 minutes
  // 0 -> 0 hours
  // 1 -> first day of the year
  // * -> for any/every month 
  // ? -> for all days of the week

  //Adding persistence to quartz to be able to be run in the back
  q.UsePersistentStore(options =>
  {
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!);
    options.UseSerializer<Quartz.Simpl.SystemTextJsonObjectSerializer>();
    options.UseProperties = true;
  });
});

builder.Services.AddQuartzHostedService(q =>
{
  q.WaitForJobsToComplete = true;
});

//Register payroll stuff

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
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
builder.Services.AddScoped<IJobGradeRepository, JobGradeRepository>();
builder.Services.AddScoped<IJobGradeService, JobGradeService>();
builder.Services.AddScoped<IOccupationalLevelRepository, OccupationalLevelRepository>();
builder.Services.AddScoped<IOccupationalLevelService, OccupationalLevelService>();
builder.Services.AddScoped<HRConnect.Api.Interfaces.IAuthService, HRConnect.Api.Services.AuthService>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IStatutoryContributionRepository, StatutoryContributionRepository>();
builder.Services.AddScoped<IStatutoryContributionService, StatutoryContributionService>();
builder.Services.AddTransient<IPensionProjectionService, PensionProjectionService>();
builder.Services.AddScoped<IMedicalOptionRepository, MedicalOptionRepository>();
builder.Services.AddScoped<HRConnect.Api.Interfaces.IMedicalOptionService,
  HRConnect.Api.Services.MedicalOptionService>();
builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowReact",
      policy => policy
          .WithOrigins("http://localhost:3000", "http://localhost:5147")
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials());
});

var app = builder.Build();

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
app.MapControllers();
app.Run();