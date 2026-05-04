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
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Product>().HasIndex(p => p.Sku).IsUnique();
        builder.Entity<Location>().HasIndex(l => l.Code).IsUnique();
        builder.Entity<Product>().Property(p => p.UnitPrice).HasPrecision(10, 2);

        // Each (Product, Location) pair has exactly one InventoryItem row
        builder.Entity<InventoryItem>()
            .HasIndex(i => new { i.ProductId, i.LocationId })
            .IsUnique();
    }
}