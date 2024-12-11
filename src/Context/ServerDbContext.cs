using CloudInteractive.HomNetBridge.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudInteractive.HomNetBridge.Context;

public class ServerDbContext : DbContext
{
    public DbSet<Car> Cars { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options) => 
        options.UseMySQL(Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING") ?? "EMPTY");
}