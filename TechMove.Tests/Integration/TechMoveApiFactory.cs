using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TechMove.Api.Data;
using TechMove.Api.Models;
using TechMove.Api.Services;

namespace TechMove.Tests.Integration;

public sealed class TechMoveApiFactory : WebApplicationFactory<ApiProgram>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly string _fileStorageRoot = Path.Combine(Path.GetTempPath(), "TechMoveTests", Guid.NewGuid().ToString("N"));

    public TechMoveApiFactory()
    {
        _connection.Open();
        Directory.CreateDirectory(_fileStorageRoot);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Jwt:Key", "testing-key-that-is-at-least-32-characters-long");
        builder.UseSetting("Jwt:Issuer", "TechMove.Api.Test");
        builder.UseSetting("Jwt:Audience", "TechMove.Tests");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:",
                ["Jwt:Key"] = "testing-key-that-is-at-least-32-characters-long",
                ["Jwt:Issuer"] = "TechMove.Api.Test",
                ["Jwt:Audience"] = "TechMove.Tests",
                ["Seed:AdminPassword"] = "Admin123!",
                ["Seed:ManagerPassword"] = "Manager123!",
                ["FileStorage:RootPath"] = _fileStorageRoot,
                ["Swagger:Enabled"] = "false"
            });
        });
        builder.ConfigureServices(services =>
        {
            RemoveService<ApplicationDbContext>(services);
            RemoveService<DbContextOptions<ApplicationDbContext>>(services);
            foreach (var descriptor in services.Where(x => x.ServiceType.IsGenericType
                && x.ServiceType.GetGenericTypeDefinition() == typeof(IDbContextOptionsConfiguration<>)).ToList())
            {
                services.Remove(descriptor);
            }
            RemoveService<ICurrencyService>(services);
            RemoveService<CurrencyService>(services);
            RemoveService<IFileStorageService>(services);

            services.AddSingleton(_connection);
            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
                options.UseSqlite(serviceProvider.GetRequiredService<SqliteConnection>()));
            services.AddSingleton<ICurrencyService, FakeCurrencyService>();
            services.AddScoped<IFileStorageService, FileStorageService>();
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        database.Database.EnsureCreated();
        SeedUsers(scope.ServiceProvider, database);
        return host;
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _connection.DisposeAsync();
        if (Directory.Exists(_fileStorageRoot)) Directory.Delete(_fileStorageRoot, recursive: true);
    }

    private static void RemoveService<T>(IServiceCollection services)
    {
        foreach (var descriptor in services.Where(x => x.ServiceType == typeof(T)).ToList())
        {
            services.Remove(descriptor);
        }
    }

    private static void SeedUsers(IServiceProvider services, ApplicationDbContext database)
    {
        if (database.AppUsers.Any()) return;
        var hasher = new PasswordHasher<AppUser>();
        var admin = new AppUser { Email = "admin@techmove.test", Role = "Admin" };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");
        var manager = new AppUser { Email = "manager@techmove.test", Role = "Manager" };
        manager.PasswordHash = hasher.HashPassword(manager, "Manager123!");
        database.AppUsers.AddRange(admin, manager);
        database.SaveChanges();
    }
}

public sealed class FakeCurrencyService : ICurrencyService
{
    public Task<decimal> GetUsdToZarRateAsync(CancellationToken cancellationToken = default) => Task.FromResult(18.50m);
}
