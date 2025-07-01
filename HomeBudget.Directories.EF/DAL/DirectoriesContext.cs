using HomeBudget.Directories.EF.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HomeBudget.Directories.EF.DAL;

public class DirectoriesContext : DbContext
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Currency> Сurrencies { get; set; }

    public DirectoriesContext(DbContextOptions<DirectoriesContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
    }
}