using Microsoft.EntityFrameworkCore;
using RetailOrderingWebsite.Models;

namespace RetailOrderingWebsite.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Seller> Sellers => Set<Seller>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Seller)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.SellerId);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId);

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Pizza" },
            new Category { Id = 2, Name = "Drinks" },
            new Category { Id = 3, Name = "Bread" }
        );

        modelBuilder.Entity<Seller>().HasData(
            new Seller { Id = 1, Name = "City Pizza", Email = "pizza@seller.com", Phone = "111-111-1111", Address = "Main Street" },
            new Seller { Id = 2, Name = "Cool Drinks", Email = "drinks@seller.com", Phone = "222-222-2222", Address = "River Road" },
            new Seller { Id = 3, Name = "Bread House", Email = "bread@seller.com", Phone = "333-333-3333", Address = "Baker Lane" }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Margherita Pizza", Price = 8.99m, CategoryId = 1, SellerId = 1, Stock = 25 },
            new Product { Id = 2, Name = "Pepperoni Pizza", Price = 10.99m, CategoryId = 1, SellerId = 1, Stock = 20 },
            new Product { Id = 3, Name = "Cola", Price = 1.99m, CategoryId = 2, SellerId = 2, Stock = 100 },
            new Product { Id = 4, Name = "Orange Juice", Price = 2.49m, CategoryId = 2, SellerId = 2, Stock = 80 },
            new Product { Id = 5, Name = "Garlic Bread", Price = 3.99m, CategoryId = 3, SellerId = 3, Stock = 40 },
            new Product { Id = 6, Name = "Whole Wheat Bread", Price = 2.99m, CategoryId = 3, SellerId = 3, Stock = 50 }
        );
    }
}
