using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Interfaces;
using VlogForge.Infrastructure.Data;
using VlogForge.Infrastructure.Data.Repositories;
using VlogForge.Infrastructure.Services;

namespace VlogForge.Infrastructure;

/// <summary>
/// Extension methods for configuring Infrastructure layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure layer services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            // Use PostgreSQL (change to UseSqlServer for SQL Server)
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            });
        });

        // Register Unit of Work
        services.AddScoped<IUnitOfWork>(provider => 
            provider.GetRequiredService<ApplicationDbContext>());

        // Register repositories
        // services.AddScoped<IYourRepository, YourRepository>();

        // Register services
        services.AddSingleton<IDateTimeService, DateTimeService>();

        // Register caching (Redis)
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "VlogForge:";
            });
        }
        else
        {
            // Fallback to in-memory cache for development
            services.AddDistributedMemoryCache();
        }

        return services;
    }
}
