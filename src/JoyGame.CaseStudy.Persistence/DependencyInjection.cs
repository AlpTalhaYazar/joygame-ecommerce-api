using JoyGame.CaseStudy.Application.Interfaces;
using JoyGame.CaseStudy.Persistence.Context;
using JoyGame.CaseStudy.Persistence.Database;
using JoyGame.CaseStudy.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JoyGame.CaseStudy.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddScoped<DatabaseInitializer>();
        services.AddScoped<DatabaseSeeder>();

        return services;
    }

    // This method will handle both migration and seeding
    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<IServiceProvider>>();

        try
        {
            logger.LogInformation("Starting database initialization...");

            // First, initialize the database (migrations)
            var initializer = services.GetRequiredService<DatabaseInitializer>();
            await initializer.InitializeAsync();

            // Then, seed the data
            var seeder = services.GetRequiredService<DatabaseSeeder>();
            await seeder.SeedAsync();

            logger.LogInformation("Database initialization and seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }
    }
}