using Microsoft.EntityFrameworkCore;
using TechMove.Api.Models;

namespace TechMove.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>().Property(x => x.Name).IsRequired();
        modelBuilder.Entity<Client>().Property(x => x.ContactDetails).IsRequired();
        modelBuilder.Entity<Client>().Property(x => x.Region).IsRequired();
        modelBuilder.Entity<Contract>().Property(x => x.ServiceLevel).IsRequired();
        modelBuilder.Entity<Contract>().HasOne(x => x.Client).WithMany(x => x.Contracts).HasForeignKey(x => x.ClientId);
        modelBuilder.Entity<ServiceRequest>().Property(x => x.UsdAmount).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<ServiceRequest>().Property(x => x.Cost).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<ServiceRequest>().Property(x => x.ExchangeRate).HasColumnType("decimal(18,6)");
        modelBuilder.Entity<ServiceRequest>().HasOne(x => x.Contract).WithMany(x => x.ServiceRequests).HasForeignKey(x => x.ContractId);
        modelBuilder.Entity<AppUser>().HasIndex(x => x.Email).IsUnique();
    }
}
