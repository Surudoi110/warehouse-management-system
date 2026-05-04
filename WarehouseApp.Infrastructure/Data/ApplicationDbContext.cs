using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Core.Entities;

namespace WarehouseApp.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Location> Locations => Set<Location>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Enforce uniqueness at the database level
        builder.Entity<Product>().HasIndex(p => p.Sku).IsUnique();
        builder.Entity<Location>().HasIndex(l => l.Code).IsUnique();

        // MySQL needs explicit decimal precision
        builder.Entity<Product>().Property(p => p.UnitPrice).HasPrecision(10, 2);
    }
}