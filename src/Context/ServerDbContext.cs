using CloudInteractive.HomNetBridge.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudInteractive.HomNetBridge.Context;

public class ServerDbContext : DbContext
{
    public DbSet<Car> Cars { get; }

    protected override void OnConfiguring(DbContextOptionsBuilder options) => 
        options.UseSqlite(Environment.GetEnvironmentVariable("SQLITE_CONNECTION_STRING"));

    protected override void OnModelCreating(ModelBuilder builder) => builder.Entity<Car>()
        .HasIndex(x => x.LicensePlate).IsUnique();
}