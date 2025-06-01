using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using HomeBudget.Directories.EF.DAL.Models;
using Microsoft.Extensions.Configuration;
using System.Xml.Linq;

namespace HomeBudget.Directories.EF.DAL;

public class DirectoriesContext : DbContext
{
    public DbSet<Categories> Categories { get; set; }
    public DbSet<Currency> Сurrencies { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder options)
    //{
    //    var config = new ConfigurationBuilder()
    //                    .AddJsonFile("appsettings.json")
    //                    .SetBasePath(Directory.GetCurrentDirectory())
    //                    .Build();

    //    options.UseNpgsql(config.GetConnectionString("postgreSQL"));
    //}

    public DirectoriesContext(DbContextOptions<DirectoriesContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categories>().Property(x => x.Id).HasDefaultValueSql("NEWID()");
    }
}