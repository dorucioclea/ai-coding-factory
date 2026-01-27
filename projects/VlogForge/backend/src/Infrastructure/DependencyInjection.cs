using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Domain.Interfaces;
using VlogForge.Infrastructure.Data;
using VlogForge.Infrastructure.Data.Repositories;
using VlogForge.Infrastructure.Identity;
using VlogForge.Infrastructure.Services;
using VlogForge.Infrastructure.Services.OAuth;
using VlogForge.Infrastructure.Services.PlatformData;

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
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICreatorProfileRepository, CreatorProfileRepository>();
        services.AddScoped<IPlatformConnectionRepository, PlatformConnectionRepository>();

        // Register analytics repositories (ACF-004)
        services.AddScoped<IPlatformMetricsRepository, PlatformMetricsRepository>();
        services.AddScoped<IMetricsSnapshotRepository, MetricsSnapshotRepository>();
        services.AddScoped<IContentPerformanceRepository, ContentPerformanceRepository>();

        // Register content ideas repository (ACF-005)
        services.AddScoped<IContentItemRepository, ContentItemRepository>();

        // Register encryption service (ACF-003)
        services.AddSingleton<IEncryptionService, EncryptionService>();

        // Register OAuth state and redirect validation services (ACF-003)
        services.AddSingleton<IOAuthStateService, OAuthStateService>();
        services.AddSingleton<IOAuthRedirectValidator, OAuthRedirectValidator>();

        // Register OAuth services (ACF-003) - using mock implementations for development
        services.AddScoped<IPlatformOAuthService, MockYouTubeOAuthService>();
        services.AddScoped<IPlatformOAuthService, MockInstagramOAuthService>();
        services.AddScoped<IPlatformOAuthService, MockTikTokOAuthService>();

        // Register identity services
        services.AddSingleton<IIdentityService, IdentityService>();

        // Register JWT settings and token service
        var jwtSection = configuration.GetSection(JwtSettings.SectionName);
        services.Configure<JwtSettings>(jwtSection);
        services.AddSingleton<ITokenService, TokenService>();

        // Configure JWT Authentication
        var jwtSettings = jwtSection.Get<JwtSettings>();
        if (jwtSettings != null && !string.IsNullOrEmpty(jwtSettings.Secret))
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        }

        // Register email service
        services.AddScoped<IEmailService, EmailService>();

        // Register other services
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

        // Register cache service
        services.AddScoped<ICacheService, CacheService>();

        // Register file storage service
        services.Configure<FileStorageSettings>(configuration.GetSection(FileStorageSettings.SectionName));
        services.AddScoped<IFileStorageService, FileStorageService>();

        // Register platform data services (ACF-004) - using mock implementations for development
        services.AddScoped<IPlatformDataService, MockYouTubeDataService>();
        services.AddScoped<IPlatformDataService, MockInstagramDataService>();
        services.AddScoped<IPlatformDataService, MockTikTokDataService>();

        // Register background services for analytics (ACF-004)
        services.AddHostedService<AnalyticsSyncService>();
        services.AddHostedService<DailySnapshotService>();

        return services;
    }
}
