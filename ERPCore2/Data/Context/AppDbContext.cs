using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Context
{
    public class AppDbContext : DbContext
    {
      public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
      {
      }
      public DbSet<Customer> Customers { get; set; }
      public DbSet<CustomerType> CustomerTypes { get; set; }
      public DbSet<IndustryType> IndustryTypes { get; set; }
      public DbSet<ContactType> ContactTypes { get; set; }
      public DbSet<AddressType> AddressTypes { get; set; }
      public DbSet<CustomerContact> CustomerContacts { get; set; }
      public DbSet<CustomerAddress> CustomerAddresses { get; set; }      
      public DbSet<Employee> Employees { get; set; }
      public DbSet<Role> Roles { get; set; }
      public DbSet<Permission> Permissions { get; set; }
      public DbSet<RolePermission> RolePermissions { get; set; }
      public DbSet<Product> Products { get; set; }
      public DbSet<ProductCategory> ProductCategories { get; set; }
      public DbSet<ProductSupplier> ProductSuppliers { get; set; }
      public DbSet<Supplier> Suppliers { get; set; }
      public DbSet<SupplierType> SupplierTypes { get; set; }
      public DbSet<SupplierAddress> SupplierAddresses { get; set; }
      public DbSet<SupplierContact> SupplierContacts { get; set; }
      
      // Inventory Management
      public DbSet<Warehouse> Warehouses { get; set; }
      public DbSet<WarehouseLocation> WarehouseLocations { get; set; }
      public DbSet<Unit> Units { get; set; }
      public DbSet<UnitConversion> UnitConversions { get; set; }
      public DbSet<InventoryTransactionType> InventoryTransactionTypes { get; set; }
      
      // Basic Units
      public DbSet<Weather> Weathers { get; set; }
      public DbSet<Color> Colors { get; set; }

      // BOM Foundations
      public DbSet<Material> Materials { get; set; }

      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>(entity =>
            {
                  entity.HasOne(e => e.CustomerType)
                  .WithMany(ct => ct.Customers)
                  .OnDelete(DeleteBehavior.SetNull);

                  entity.HasOne(e => e.IndustryType)
                  .WithMany(i => i.Customers)
                  .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<CustomerContact>(entity =>
            {
                  entity.HasOne(e => e.Customer)
                  .WithMany(c => c.CustomerContacts)
                  .OnDelete(DeleteBehavior.Cascade);

                  entity.HasOne(e => e.ContactType)
                  .WithMany(ct => ct.CustomerContacts)
                  .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<CustomerAddress>(entity =>
            {
                  entity.HasOne(e => e.Customer)
                  .WithMany(c => c.CustomerAddresses)
                  .OnDelete(DeleteBehavior.Cascade);

                  entity.HasOne(e => e.AddressType)
                  .WithMany(at => at.CustomerAddresses)
                  .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                  entity.HasOne(e => e.Role)
                  .WithMany(r => r.Employees)
                  .OnDelete(DeleteBehavior.Restrict);
            });            modelBuilder.Entity<RolePermission>(entity =>
            {
                  entity.HasOne(rp => rp.Role)
                  .WithMany(r => r.RolePermissions)
                  .OnDelete(DeleteBehavior.Cascade);

                  entity.HasOne(rp => rp.Permission)
                  .WithMany(p => p.RolePermissions)
                  .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                  entity.HasOne(e => e.ProductCategory)
                  .WithMany(pc => pc.Products)
                  .OnDelete(DeleteBehavior.SetNull);

                  entity.HasOne(e => e.PrimarySupplier)
                  .WithMany(s => s.PrimaryProducts)
                  .OnDelete(DeleteBehavior.SetNull);

                  entity.HasOne(e => e.Unit)
                  .WithMany(u => u.Products)
                  .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ProductSupplier>(entity =>
            {
                  entity.HasOne(ps => ps.Product)
                  .WithMany(p => p.ProductSuppliers)
                  .OnDelete(DeleteBehavior.Cascade);

                  entity.HasOne(ps => ps.Supplier)
                  .WithMany(s => s.ProductSuppliers)
                  .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Supplier>(entity =>
            {
                  entity.HasOne(e => e.SupplierType)
                  .WithMany(st => st.Suppliers)
                  .OnDelete(DeleteBehavior.SetNull);

                  entity.HasOne(e => e.IndustryType)
                  .WithMany(i => i.Suppliers)
                  .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<SupplierContact>(entity =>
            {
                  entity.HasOne(e => e.Supplier)
                  .WithMany(s => s.SupplierContacts)
                  .OnDelete(DeleteBehavior.Cascade);

                  entity.HasOne(e => e.ContactType)
                  .WithMany(ct => ct.SupplierContacts)
                  .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<SupplierAddress>(entity =>
            {
                  entity.HasOne(e => e.Supplier)
                  .WithMany(s => s.SupplierAddresses)
                  .OnDelete(DeleteBehavior.Cascade);

                  entity.HasOne(e => e.AddressType)
                  .WithMany(at => at.SupplierAddresses)
                  .OnDelete(DeleteBehavior.SetNull);
            });
            
            // Inventory Management Relationships
            modelBuilder.Entity<WarehouseLocation>(entity =>
            {
                  entity.HasOne(wl => wl.Warehouse)
                  .WithMany(w => w.WarehouseLocations)
                  .OnDelete(DeleteBehavior.Cascade);
            });
            
            modelBuilder.Entity<UnitConversion>(entity =>
            {
                  entity.HasOne(uc => uc.FromUnit)
                  .WithMany(u => u.FromUnitConversions)
                  .HasForeignKey(uc => uc.FromUnitId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
                  entity.HasOne(uc => uc.ToUnit)
                  .WithMany(u => u.ToUnitConversions)
                  .HasForeignKey(uc => uc.ToUnitId)
                  .OnDelete(DeleteBehavior.Restrict);
            });
            
            // BOM Foundations Relationships
            modelBuilder.Entity<Material>(entity =>
            {
                  entity.HasOne(m => m.Supplier)
                  .WithMany()
                  .HasForeignKey(m => m.SupplierId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
                  entity.Property(m => m.Density)
                  .HasPrecision(10, 4);
                  
                  entity.Property(m => m.MeltingPoint)
                  .HasPrecision(10, 2);
            });
      }
    }
}
