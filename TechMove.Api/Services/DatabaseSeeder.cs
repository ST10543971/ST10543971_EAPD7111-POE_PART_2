using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechMove.Api.Data;
using TechMove.Api.Models;

namespace TechMove.Api.Services;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
        if (await db.AppUsers.AnyAsync(cancellationToken)) return;

        var hasher = new PasswordHasher<AppUser>();
        var admin = new AppUser { Email = "admin@techmove.test", Role = "Admin" };
        admin.PasswordHash = hasher.HashPassword(admin, services.GetRequiredService<IConfiguration>()["Seed:AdminPassword"] ?? "Admin123!");
        var manager = new AppUser { Email = "manager@techmove.test", Role = "Manager" };
        manager.PasswordHash = hasher.HashPassword(manager, services.GetRequiredService<IConfiguration>()["Seed:ManagerPassword"] ?? "Manager123!");
        db.AppUsers.AddRange(admin, manager);
        await db.SaveChangesAsync(cancellationToken);
    }
}
