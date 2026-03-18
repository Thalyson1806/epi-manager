using EpiManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EpiManagement.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Sector> Sectors => Set<Sector>();
    public DbSet<Epi> Epis => Set<Epi>();
    public DbSet<SectorEpi> SectorEpis => Set<SectorEpi>();
    public DbSet<EpiDelivery> EpiDeliveries => Set<EpiDelivery>();
    public DbSet<EpiDeliveryItem> EpiDeliveryItems => Set<EpiDeliveryItem>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemConfig> SystemConfigs => Set<SystemConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
