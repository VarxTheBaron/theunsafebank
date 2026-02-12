using Microsoft.EntityFrameworkCore;
using theunsafebank.Models;

namespace theunsafebank.Data;

public class BankContext : DbContext
{
    public BankContext(DbContextOptions<BankContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transfer> Transfers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationships
        modelBuilder.Entity<Customer>()
            .HasOne(c => c.Account)
            .WithOne(a => a.Customer)
            .HasForeignKey<Account>(a => a.CustomerId);

        modelBuilder.Entity<Transfer>()
            .HasOne(t => t.FromAccount)
            .WithMany(a => a.TransfersFrom)
            .HasForeignKey(t => t.FromAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transfer>()
            .HasOne(t => t.ToAccount)
            .WithMany(a => a.TransfersTo)
            .HasForeignKey(t => t.ToAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed an admin customer with name admin and password admin.
        // Also add an account for that customer with 100,000SEK.
        var adminCustomer = new Customer
        {
            Id = 123,
            FullName = "admin",
            Username = "admin",
            Password = "admin"
        };

        var adminAccount = new Account
        {
            Id = 1,
            AccountNumber = "1000",
            Balance = 100000m,
            CustomerId = adminCustomer.Id
        };

        modelBuilder.Entity<Customer>().HasData(adminCustomer);
        modelBuilder.Entity<Account>().HasData(adminAccount);
    }
}
