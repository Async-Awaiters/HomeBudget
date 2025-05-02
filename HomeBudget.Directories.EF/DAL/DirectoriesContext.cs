using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using HomeBudget.Directories.EF.DAL.Models;
using Microsoft.Extensions.Configuration;


namespace HomeBudget.Directories.EF.DAL;

public class DirectoriesContext : DbContext
{

    public DbSet<Categories> Categories { get; set; }
    public DbSet<Currency> Currency { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var config = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json")
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .Build();

        options.UseNpgsql(config.GetConnectionString("postgreSQL"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categories>(entity =>
        {
            entity.Property(e => e.Id)
                  .HasDefaultValueSql("gen_random_uuid()");
        });
    }
}