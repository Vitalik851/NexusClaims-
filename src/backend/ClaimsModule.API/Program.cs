using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Hangfire;
using ClaimsModule.Application;
using ClaimsModule.Persistence;
using ClaimsModule.Infrastructure;
using ClaimsModule.Persistence.Context;
using ClaimsModule.API.Middleware;
using ClaimsModule.Infrastructure.Jobs;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Application, Persistence and Infrastructure Layers
builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

// 2. Configure CORS for Angular Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Angular default port
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 3. Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "DiceusClaimsManagementSystemSecretKey2026";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "DiceusClaimsAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "DiceusClaimsApp";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// 4. Configure Swagger with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Claims Management System API", 
        Version = "v1",
        Description = "Clean Architecture API for Insurance Claims and Reserves Management Module."
    });

    // Define the Security Scheme
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter JWT Bearer token **_only_**",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

var app = builder.Build();

// 5. Database Initialization (Auto-run migrations or EnsureCreated)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ClaimsDbContext>();
        // EnsureCreated builds the database structure & seeds lookup and policies
        context.Database.EnsureCreated();
        
        // Build sequence in database (failsafe if database provider didn't build it)
        try
        {
            context.Database.ExecuteSqlRaw(
                "IF NOT EXISTS (SELECT * FROM sys.sequences WHERE name = 'ClaimNumberSequence') " +
                "CREATE SEQUENCE ClaimNumberSequence START WITH 1 INCREMENT BY 1;");
        }
        catch
        {
            // Ignore if sequence already exists or unsupported
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

// 6. Request Pipeline Middleware
app.UseMiddleware<ExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Claims API v1"));

app.UseCors("CorsPolicy");

// Serve wwwroot directory (uploads, static assets)
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// 7. Hangfire Dashboard (with basic security, or open for development)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    // Authorization rules can be added here
});

// Configure Hangfire Recurring SLA Job
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobManager.AddOrUpdate<ISlaMonitoringJob>(
        "sla-monitoring-job",
        job => job.ExecuteAsync(CancellationToken.None),
        "*/15 * * * *");
}

app.MapControllers();

app.Run();
