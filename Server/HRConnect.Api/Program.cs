using System;
using System.Text;
using HRConnect.Api.Data;
using HRConnect.Api.Interfaces;
using HRConnect.Api.Models;
using HRConnect.Api.Repository;
using HRConnect.Api.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

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
// Office Use Only - using EF Core with MQSL

builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SecondaryConnection")));

// Home Usage Only
// Configure Entity Framework Core with MySQL
/*
builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("TertiaryConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("TertiaryConnection"))
    )
);
*/
// Configure Entity Framework Core with Postgres
/*
builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
           //.UseSnakeCaseNamingConvention()); // Optional: improves PostgreSQL compatibility
*/

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
    .AddPolicy("NormalUserOnly", policy => policy.RequireRole("NormalUser"));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<HRConnect.Api.Interfaces.IUserService, HRConnect.Api.Services.UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();
builder.Services.AddScoped<HRConnect.Api.Interfaces.IAuthService, HRConnect.Api.Services.AuthService>();
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();


