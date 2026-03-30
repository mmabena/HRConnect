using System.Text;
using HRConnect.Api.Data;
using HRConnect.Api.Interfaces;
using HRConnect.Api.Interfaces.PensionProjection;
using HRConnect.Api.Models;
using HRConnect.Api.Repository;
using HRConnect.Api.Repositories;
using HRConnect.Api.Services;
using HRConnect.Api.Utils;
using HRConnect.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OfficeOpenXml;
using Audit.Core;
using Audit.EntityFramework;
using Quartz;
using Resend;

var builder = WebApplication.CreateBuilder(args);

// Audit configuration
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
  var secretValue = jwt["Secret"] ?? string.Empty;
  byte[] keyBytes;
  try
  {
    keyBytes = Convert.FromBase64String(secretValue);
  }
  catch (FormatException)
  {
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
  q.AddJob<PayrollRolloverJob>(opts => opts.WithIdentity(jobKey));
  q.AddTrigger(opts => opts
      .ForJob(jobKey)
      .WithIdentity("PayrollRolloverTrigger")
      .WithCronSchedule("0 0 0 1 * ?"));
});
builder.Services.AddQuartzHostedService(q =>
{
  q.WaitForJobsToComplete = true;
});

// Dependency injection registrations
builder.Services.AddScoped<IPensionFundRepository, PensionFundRepository>();
builder.Services.AddScoped<IPensionOptionRepository, PensionOptionRepository>();
builder.Services.AddScoped<IEmployeePensionRepository, EmployeePensionRepository>();
builder.Services.AddScoped<IPensionFundService, PensionFundService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IUserService, UserService>();
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
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStatutoryContributionRepository, StatutoryContributionRepository>();
builder.Services.AddScoped<IStatutoryContributionService, StatutoryContributionService>();
builder.Services.AddTransient<IPensionProjectionService, PensionProjectionService>();
builder.Services.AddScoped<IMedicalOptionRepository, MedicalOptionRepository>();
builder.Services.AddScoped<IMedicalOptionService, MedicalOptionService>();

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

// Ensure database migrations are applied
using (var scope = app.Services.CreateScope())
{
  var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
  dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI(c =>
  {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "HRConnect.Api v1");
  });
}

app.UseCors("AllowReact");
app.UseGlobalExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionMiddleware>();
app.MapControllers();
app.Run();
