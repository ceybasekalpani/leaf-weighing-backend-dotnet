using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using LeafWeighing.Api.Middleware;
using LeafWeighing.Application.Interfaces.Repositories;
using LeafWeighing.Application.Interfaces.Services;
using LeafWeighing.Application.Services;
using LeafWeighing.Infrastructure.Data;
using LeafWeighing.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Load configuration
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add services to container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Leaf Weighing API",
        Version = "v1",
        Description = "Backend API for Leaf Weighing Application"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "leaf-weighing-secret-key-2024-for-development-only";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "LeafWeighingApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "LeafWeighingClient";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

// Configure Database Connections
var boughtLeafConnectionString = builder.Configuration.GetConnectionString("BoughtLeafDatabase");
var setupConnectionString = builder.Configuration.GetConnectionString("SetupDatabase");

if (string.IsNullOrEmpty(boughtLeafConnectionString))
{
    throw new InvalidOperationException("BoughtLeafDatabase connection string is not configured");
}

if (string.IsNullOrEmpty(setupConnectionString))
{
    throw new InvalidOperationException("SetupDatabase connection string is not configured");
}

builder.Services.AddDbContext<BoughtLeafDbContext>(options =>
    options.UseSqlServer(boughtLeafConnectionString));

builder.Services.AddDbContext<SetupDbContext>(options =>
    options.UseSqlServer(setupConnectionString));

// Register Repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ITrLeafCollectionRepository, TrLeafCollectionRepository>();
builder.Services.AddScoped<IRegLeafCountRepository, RegLeafCountRepository>();
builder.Services.AddScoped<IUserSetupRepository, UserSetupRepository>();

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICollectionService, CollectionService>();
builder.Services.AddScoped<IDeductionService, DeductionService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<ILeafCountService, LeafCountService>();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Health check endpoint
app.MapGet("/api/health", () => new { success = true, message = "Server is running", timestamp = DateTime.UtcNow });

app.MapControllers();

// 404 handler
app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
    {
        context.Response.ContentType = "application/json";
        var response = new { success = false, message = $"Route {context.Request.Method} {context.Request.Path} not found" };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
});

app.Run();