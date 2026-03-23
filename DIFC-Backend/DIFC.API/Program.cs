using DIFC.API.Middleware;
using DIFC.Application.Interfaces;
using DIFC.Application.Interfaces.Auth;
using DIFC.Application.Interfaces.Role;
using DIFC.Application.Interfaces.User;
using DIFC.Application.Services;
using DIFC.Application.Services.Auth;
using DIFC.Application.Services.Role;
using DIFC.Application.Services.User;
using DIFC.Application.Validators;
using DIFC.Application.Validators.Auth;
using DIFC.Application.Validators.User;
using DIFC.Domain.Entities;
using DIFC.Infrastructure.Data;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

#region SerilogLogging
// SERILOG — Structured Logging
// WHY Serilog? The default .NET ILogger only writes text.
// Serilog writes "structured" logs — each field is a separate
// searchable property. This matters when you have thousands of
// logs and need to filter by UserId, IP, endpoint etc.
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()  // Show logs in terminal
    .WriteTo.File(      // Also write to daily rolling log files
        "logs/app-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30) // Keep 30 days of logs
    .CreateLogger();

builder.Host.UseSerilog(); // Replace default logger with Serilog
#endregion SerilogLogging

#region Services
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // ── Password rules ──
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;

        // ── Lockout settings ──
        // After 5 wrong passwords → lock account for 15 minutes.
        // This stops brute force attacks dead. Without this, an 
        // attacker can try millions of passwords per second.
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // ── User settings ──
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

#region #jwtRegister
// ═══════════════════════════════════════════════════════════
// JWT AUTHENTICATION
// ═══════════════════════════════════════════════════════════
// This tells ASP.NET HOW to validate incoming JWT tokens.
// When a request arrives with "Authorization: Bearer <token>",
// this middleware intercepts it and validates the token BEFORE
// your controller code runs.
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(options =>
{
    // Set JWT as the DEFAULT scheme for both auth and challenge.
    // Without this, ASP.NET defaults to cookies (not what we want for APIs).
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,               // Check token was made by us
        ValidateAudience = true,             // Check token is meant for our app
        ValidateLifetime = true,             // Reject expired tokens
        ValidateIssuerSigningKey = true,     // Verify the signature is valid
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),

        //RoleClaimType need to be configured in below format as per JWT payload
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",


        // IMPORTANT: By default .NET adds 5 MINUTES of tolerance
        // to token expiry. ClockSkew = Zero means expired = expired,
        // period. This is important for security with short-lived tokens.
        ClockSkew = TimeSpan.Zero // Remove default 5 min clock skew
    };
});

#endregion #jwtRegister

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

#region DependencyInjection

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRoleService, RoleService>();

#endregion DependencyInjection

#endregion Services

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>(); // ← MUST be first, wraps everything   

app.UseCors("AllowFrontend");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();

