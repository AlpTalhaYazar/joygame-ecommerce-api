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
            if (context.Database.GetPendingMigrations().Any())
            {
                logger.LogInformation("Starting to apply pending database migrations...");

                await context.Database.MigrateAsync();

                logger.LogInformation("Successfully applied all pending migrations");
            }
            else
            {
                logger.LogInformation("No pending migrations found. Database is up to date");
            }

            await context.Database.EnsureCreatedAsync();

            logger.LogInformation("Database initialization completed successfully");

            await CreateStoredProceduresAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database");

            if (environment.IsDevelopment())
            {
                throw;
            }

            throw new ApplicationException("Failed to initialize the database. Please check the logs for more details.");
        }
    }

    private async Task CreateStoredProceduresAsync()
    {
        try
        {
            await context.Database.ExecuteSqlRawAsync(StoredProcedures.StoredProcedures.GetProductsWithCategories);
            await context.Database.ExecuteSqlRawAsync(StoredProcedures.StoredProcedures.GetRecursiveCategories);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to create stored procedures", ex);
        }
    }
}