using Microsoft.EntityFrameworkCore;

namespace Northwind.Services.EntityFramework.Entities;

public class NorthwindContext : DbContext
{
    public NorthwindContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }

    public DbSet<Employee> Employees { get; set; }

    public DbSet<OrderDetail> OrderDetails { get; set; }

    public DbSet<Product> Products { get; set; }

    public DbSet<Shipper> Shippers { get; set; }

    public DbSet<Supplier> Suppliers { get; set; }

    public DbSet<Customer> Customers { get; set; }

    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Category entity
        _ = modelBuilder.Entity<Category>(entity =>
        {
            _ = entity.HasKey(e => e.CategoryId);

            _ = entity.Property(e => e.CategoryName).IsRequired().HasMaxLength(15);

            _ = entity.Property(e => e.Description).HasColumnType("ntext");
        });

        // Configure Customer entity
        _ = modelBuilder.Entity<Customer>(entity =>
        {
            _ = entity.HasKey(e => e.CustomerId);
            _ = entity.Property(e => e.CustomerId).HasMaxLength(5);
            _ = entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(40);
            _ = entity.Property(e => e.ContactName).HasMaxLength(30);
            _ = entity.Property(e => e.ContactTitle).HasMaxLength(30);
            _ = entity.Property(e => e.Address).HasMaxLength(60);
            _ = entity.Property(e => e.City).HasMaxLength(15);
            _ = entity.Property(e => e.Region).HasMaxLength(15);
            _ = entity.Property(e => e.PostalCode).HasMaxLength(10);
            _ = entity.Property(e => e.Country).HasMaxLength(15);
            _ = entity.Property(e => e.Phone).HasMaxLength(24);
            _ = entity.Property(e => e.Fax).HasMaxLength(24);
        });

        // Configure Employee entity
        _ = modelBuilder.Entity<Employee>(entity =>
        {
            _ = entity.HasKey(e => e.EmployeeId);
            _ = entity.Property(e => e.LastName).IsRequired().HasMaxLength(20);
            _ = entity.Property(e => e.FirstName).IsRequired().HasMaxLength(10);
            _ = entity.Property(e => e.Title).HasMaxLength(30);
            _ = entity.Property(e => e.TitleOfCourtesy).HasMaxLength(25);
            _ = entity.Property(e => e.Address).HasMaxLength(60);
            _ = entity.Property(e => e.City).HasMaxLength(15);
            _ = entity.Property(e => e.Region).HasMaxLength(15);
            _ = entity.Property(e => e.PostalCode).HasMaxLength(10);
            _ = entity.Property(e => e.Country).HasMaxLength(15);
            _ = entity.Property(e => e.HomePhone).HasMaxLength(24);
            _ = entity.Property(e => e.Extension).HasMaxLength(4);
            _ = entity.Property(e => e.Notes).HasColumnType("ntext");
            _ = entity.Property(e => e.PhotoPath).HasMaxLength(255);

            // Self-referencing relationship for ReportsTo
            _ = entity.HasOne<Employee>()
                  .WithMany()
                  .HasForeignKey(e => e.ReportsTo)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .IsRequired(false);
        });

        // Configure Order entity
        _ = modelBuilder.Entity<Order>(entity =>
        {
            _ = entity.HasKey(e => e.OrderId);
            _ = entity.Property(e => e.CustomerId).HasMaxLength(5);
            _ = entity.Property(e => e.ShipName).HasMaxLength(40);
            _ = entity.Property(e => e.ShipAddress).HasMaxLength(60);
            _ = entity.Property(e => e.ShipCity).HasMaxLength(15);
            _ = entity.Property(e => e.ShipRegion).HasMaxLength(15);
            _ = entity.Property(e => e.ShipPostalCode).HasMaxLength(10);
            _ = entity.Property(e => e.ShipCountry).HasMaxLength(15);

            // Configure relationship with Customer
            _ = entity.HasOne(o => o.Customer)
                  .WithMany(c => c.Orders)
                  .HasForeignKey(o => o.CustomerId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .IsRequired(false);

            // Configure relationship with Employee
            _ = entity.HasOne(o => o.Employee)
                  .WithMany(e => e.Orders)
                  .HasForeignKey(o => o.EmployeeId)
                  .OnDelete(DeleteBehavior.ClientSetNull);

            // Configure relationship with Shipper
            _ = entity.HasOne(o => o.Shipper)
                  .WithMany(s => s.Orders)
                  .HasForeignKey(o => o.ShipVia)
                  .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // Configure OrderDetail entity
        _ = modelBuilder.Entity<OrderDetail>(entity =>
        {
            _ = entity.HasKey(e => new { e.OrderId, e.ProductId });

            // Configure relationship with Order
            _ = entity.HasOne(od => od.Order)
                  .WithMany(o => o.OrderDetails)
                  .HasForeignKey(od => od.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Configure relationship with Product
            _ = entity.HasOne(od => od.Product)
                  .WithMany(p => p.OrderDetails)
                  .HasForeignKey(od => od.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Product entity
        _ = modelBuilder.Entity<Product>(entity =>
        {
            _ = entity.HasKey(e => e.ProductId);
            _ = entity.Property(e => e.ProductName).IsRequired().HasMaxLength(40);
            _ = entity.Property(e => e.QuantityPerUnit).HasMaxLength(20);

            // Configure relationship with Category
            _ = entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.ClientSetNull);

            // Configure relationship with Supplier
            _ = entity.HasOne(p => p.Supplier)
                  .WithMany(s => s.Products)
                  .HasForeignKey(p => p.SupplierId)
                  .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // Configure Shipper entity
        _ = modelBuilder.Entity<Shipper>(entity =>
        {
            _ = entity.HasKey(e => e.ShipperId);
            _ = entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(40);
            _ = entity.Property(e => e.Phone).HasMaxLength(24);
        });

        // Configure Supplier entity
        _ = modelBuilder.Entity<Supplier>(entity =>
        {
            _ = entity.HasKey(e => e.SupplierId);
            _ = entity.Property(e => e.SupplierId).HasMaxLength(5);
            _ = entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(40);
            _ = entity.Property(e => e.ContactName).HasMaxLength(30);
            _ = entity.Property(e => e.ContactTitle).HasMaxLength(30);
            _ = entity.Property(e => e.Address).HasMaxLength(60);
            _ = entity.Property(e => e.City).HasMaxLength(15);
            _ = entity.Property(e => e.Region).HasMaxLength(15);
            _ = entity.Property(e => e.PostalCode).HasMaxLength(10);
            _ = entity.Property(e => e.Country).HasMaxLength(15);
            _ = entity.Property(e => e.Phone).HasMaxLength(24);
            _ = entity.Property(e => e.Fax).HasMaxLength(24);
            _ = entity.Property(e => e.HomePage).HasColumnType("ntext");
        });
    }
}
