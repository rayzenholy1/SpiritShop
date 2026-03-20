using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SpiritShop.Application.Interfaces;
using SpiritShop.Domain.Entities;

namespace SpiritShop.Infrastructure.Data;
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}

public class AppDbContext : IdentityDbContext<ApplicationUser>, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Price).HasPrecision(10, 2);
            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(50);
        });

        builder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.TotalAmount).HasPrecision(10, 2);
        });

        builder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Whiskey" },
            new Category { Id = 2, Name = "Vodka" },
            new Category { Id = 3, Name = "Wine" },
            new Category { Id = 4, Name = "Beer" },
            new Category { Id = 5, Name = "Cognac" }
        );

        builder.Entity<Product>().HasData(
            new Product
            {
                Id = 1, Name = "Jack Daniel's Old No.7", Price = 599.99m,
                AlcoholPercentage = 40, StockQuantity = 50, CategoryId = 1,
                Description = "Tennessee whiskey with smooth character",
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 2, Name = "Johnnie Walker Black Label", Price = 849.00m,
                AlcoholPercentage = 40, StockQuantity = 30, CategoryId = 1,
                Description = "12 Year Old Blended Scotch Whisky",
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 3, Name = "Nemiroff Original", Price = 249.00m,
                AlcoholPercentage = 40, StockQuantity = 100, CategoryId = 2,
                Description = "Premium Ukrainian vodka",
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            }
        );
    }
}
