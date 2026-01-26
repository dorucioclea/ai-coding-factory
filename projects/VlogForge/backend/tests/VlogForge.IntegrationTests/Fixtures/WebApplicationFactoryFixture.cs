using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Testcontainers.PostgreSql;
using VlogForge.Infrastructure.Data;
using VlogForge.Infrastructure.Identity;
using Xunit;

namespace VlogForge.IntegrationTests.Fixtures;

/// <summary>
/// Web application factory for integration tests using TestContainers.
/// Story: ACF-001
/// </summary>
public class WebApplicationFactoryFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string TestJwtSecret = "TestSecretKeyForIntegrationTests_MustBeAtLeast32Chars!";

    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("vlogforge_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public string ConnectionString => _dbContainer.GetConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Add test configuration
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = TestJwtSecret,
                ["Jwt:Issuer"] = "VlogForge.Tests",
                ["Jwt:Audience"] = "VlogForge.Tests",
                ["Jwt:AccessTokenExpirationMinutes"] = "60",
                ["Jwt:RefreshTokenExpirationDays"] = "7"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Remove existing DbContext registration
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();

            // Add DbContext with TestContainer connection string and SQL logging
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString(), npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                });
                // Enable sensitive data logging and SQL logging for debugging
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
                options.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
            });

            // Configure JWT authentication for tests
            services.Configure<JwtSettings>(options =>
            {
                options.Secret = TestJwtSecret;
                options.Issuer = "VlogForge.Tests";
                options.Audience = "VlogForge.Tests";
                options.AccessTokenExpirationMinutes = 60;
                options.RefreshTokenExpirationDays = 7;
            });

            // Ensure JWT Bearer authentication is configured (may not be set up if secret was empty)
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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtSecret)),
                    ValidateIssuer = true,
                    ValidIssuer = "VlogForge.Tests",
                    ValidateAudience = true,
                    ValidAudience = "VlogForge.Tests",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Create and apply migrations
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}
