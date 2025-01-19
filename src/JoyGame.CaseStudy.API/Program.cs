using System.Text;
using JoyGame.CaseStudy.API.Middleware;
using JoyGame.CaseStudy.API.Security;
using JoyGame.CaseStudy.Infrastructure;
using JoyGame.CaseStudy.Persistence;
using JoyGame.CaseStudy.Persistence.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// First, let's configure essential services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON serialization settings
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Configure CORS for our Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Your Angular app's URL
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CategoryManagement", policy =>
        policy.Requirements.Add(new PermissionRequirement("category_manage")));
    options.AddPolicy("ProductManagement", policy =>
        policy.Requirements.Add(new PermissionRequirement("product_manage")));
});

// Register our custom services from other layers
builder.Services.AddPersistence(builder.Configuration); // Database and repositories
builder.Services.AddInfrastructure(builder.Configuration); // Services and infrastructure components

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
if (!builder.Environment.IsDevelopment())
{
    builder.Logging.AddFile("Logs/app-{Date}.log"); // Using Serilog file logging
}

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // Use custom exception handler middleware in production
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
   .WithName("HealthCheck")
   .WithOpenApi();

// Initialize the database (apply migrations, seed data if needed)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await app.Services.InitializeDatabaseAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database");
    }
}

app.Run();