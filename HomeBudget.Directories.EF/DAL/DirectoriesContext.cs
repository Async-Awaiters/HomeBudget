using HomeBudget.Directories.EF.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeBudget.Directories.EF.DAL;

public class DirectoriesContext : DbContext
{
    public DbSet<Categories> Categories { get; set; }
    public DbSet<Currency> Сurrencies { get; set; }
    public DirectoriesContext(DbContextOptions<DirectoriesContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categories>().Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
    }
}