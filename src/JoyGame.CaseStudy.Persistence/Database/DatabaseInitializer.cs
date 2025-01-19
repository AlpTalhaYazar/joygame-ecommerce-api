using JoyGame.CaseStudy.Persistence.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JoyGame.CaseStudy.Persistence.Database;

public class DatabaseInitializer(
    ApplicationDbContext context,
    ILogger<DatabaseInitializer> logger,
    IWebHostEnvironment environment)
{
    public async Task InitializeAsync()
    {
        try
        {
            // Check if we need to perform any migrations
            if (context.Database.GetPendingMigrations().Any())
            {
                logger.LogInformation("Starting to apply pending database migrations...");

                // Apply any pending migrations
                await context.Database.MigrateAsync();

                logger.LogInformation("Successfully applied all pending migrations");
            }
            else
            {
                logger.LogInformation("No pending migrations found. Database is up to date");
            }

            // Ensure database exists (this is safe to call even if DB exists)
            await context.Database.EnsureCreatedAsync();

            logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database");

            // In development, we might want to see the full error
            if (environment.IsDevelopment())
            {
                throw; // Re-throw in development for detailed error information
            }

            // In production, throw a more user-friendly exception
            throw new ApplicationException("Failed to initialize the database. Please check the logs for more details.");
        }
    }
}