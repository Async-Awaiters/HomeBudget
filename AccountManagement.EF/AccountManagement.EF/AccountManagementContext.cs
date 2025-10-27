using AccountManagement.EF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AccountManagement.EF;

public class AccountManagementContext : DbContext
{
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;

    // TODO: Добавить миграции
    public AccountManagementContext(DbContextOptions<AccountManagementContext> options) : base(options)
    {
        Database.EnsureDeleted();
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>().Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");

        modelBuilder.Entity<Account>()
            .Property(a => a.IsDeleted)
            .HasDefaultValue(false);
        modelBuilder.Entity<Account>()
            .Property(a => a.IsActive)
            .HasDefaultValue(true);
        modelBuilder.Entity<Account>()
            .Property(a => a.Balance)
            .HasDefaultValue(0);

        modelBuilder.Entity<Transaction>().Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        modelBuilder.Entity<Transaction>()
            .Property(t => t.IsDeleted)
            .HasDefaultValue(false);
        modelBuilder.Entity<Transaction>()
            .Property(t => t.IsApproved)
            .HasDefaultValue(true);
        modelBuilder.Entity<Transaction>()
            .Property(t => t.IsRepeated)
            .HasDefaultValue(false);
        modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasDefaultValue(0);
        modelBuilder.Entity<Transaction>()
            .HasOne(a => a.Account)
            .WithMany(t => t.Transactions)
            .HasForeignKey(a => a.AccountId)
            .IsRequired();
        modelBuilder.Entity<Transaction>()
            .Property(t => t.Date)
            .HasDefaultValueSql("now()");
    }
}
