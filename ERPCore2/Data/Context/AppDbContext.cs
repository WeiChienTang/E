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
      public DbSet<EmployeePosition> EmployeePositions { get; set; }
      public DbSet<EmployeeContact> EmployeeContacts { get; set; }
      public DbSet<EmployeeAddress> EmployeeAddresses { get; set; }
      public DbSet<Department> Departments { get; set; }
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
      public DbSet<InventoryStock> InventoryStocks { get; set; }
      public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
      public DbSet<InventoryReservation> InventoryReservations { get; set; }
      public DbSet<StockTaking> StockTakings { get; set; }
      public DbSet<StockTakingDetail> StockTakingDetails { get; set; }
      public DbSet<Unit> Units { get; set; }
      public DbSet<UnitConversion> UnitConversions { get; set; }
      public DbSet<InventoryTransactionType> InventoryTransactionTypes { get; set; }
      
      // Purchase Management
      public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
      public DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
      public DbSet<PurchaseReceipt> PurchaseReceipts { get; set; }
      public DbSet<PurchaseReceiptDetail> PurchaseReceiptDetails { get; set; }
      
      // BOM Foundations
      public DbSet<Material> Materials { get; set; }
      public DbSet<Weather> Weathers { get; set; }
      public DbSet<Color> Colors { get; set; }
      public DbSet<Size> Sizes { get; set; }
      
      // System Logs
      public DbSet<ErrorLog> ErrorLogs { get; set; }

      protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                  base.OnModelCreating(modelBuilder);

                  // === 實體設定（包含欄位對應和關聯） ===
                  
                  // 客戶相關
                  modelBuilder.Entity<Customer>(entity =>
                  {
                        // 欄位對應 - 主鍵在資料庫中就叫 Id，不需要欄位對應
                        entity.Property(e => e.Id).ValueGeneratedOnAdd(); // 確保 Identity
                        
                        // 關聯設定
                        entity.HasOne(e => e.CustomerType)
                        .WithMany(ct => ct.Customers)
                        .OnDelete(DeleteBehavior.SetNull);

                        entity.HasOne(e => e.IndustryType)
                        .WithMany(i => i.Customers)
                        .OnDelete(DeleteBehavior.SetNull);
                  });
                  
                  modelBuilder.Entity<CustomerContact>(entity =>
                  {
                        // 欄位對應 - 主鍵在資料庫中就叫 Id，不需要欄位對應
                        entity.Property(e => e.Id).ValueGeneratedOnAdd(); // 確保 Identity
                        
                        // 關聯設定
                        entity.HasOne(e => e.Customer)
                        .WithMany(c => c.CustomerContacts)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(e => e.ContactType)
                        .WithMany(ct => ct.CustomerContacts)
                        .OnDelete(DeleteBehavior.SetNull);
                  });
                  
                  modelBuilder.Entity<CustomerAddress>(entity =>
                  {
                        // 欄位對應 - 主鍵在資料庫中就叫 Id，不需要欄位對應
                        entity.Property(e => e.Id).ValueGeneratedOnAdd(); // 確保 Identity
                        
                        // 關聯設定
                        entity.HasOne(e => e.Customer)
                        .WithMany(c => c.CustomerAddresses)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(e => e.AddressType)
                        .WithMany(at => at.CustomerAddresses)
                        .OnDelete(DeleteBehavior.SetNull);
                  });
                  
                  modelBuilder.Entity<CustomerType>(entity =>
                  {
                        // 欄位對應 - 主鍵在資料庫中就叫 Id，不需要欄位對應
                        entity.Property(e => e.Id).ValueGeneratedOnAdd(); // 確保 Identity
                  });

                  // 供應商相關
                  modelBuilder.Entity<Supplier>(entity =>
                  {
                        // 欄位對應 - 主鍵在資料庫中就叫 Id，不需要欄位對應
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                        
                        // 關聯設定
                        entity.HasOne(e => e.SupplierType)
                        .WithMany(st => st.Suppliers)
                        .OnDelete(DeleteBehavior.SetNull);

                        entity.HasOne(e => e.IndustryType)
                        .WithMany(i => i.Suppliers)
                        .OnDelete(DeleteBehavior.SetNull);
                  });
                  
                  modelBuilder.Entity<SupplierContact>(entity =>
                  {
                        // 欄位對應 - 主鍵在資料庫中就叫 Id，不需要欄位對應
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                        
                        // 關聯設定
                        entity.HasOne(e => e.Supplier)
                        .WithMany(s => s.SupplierContacts)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(e => e.ContactType)
                        .WithMany(ct => ct.SupplierContacts)
                        .OnDelete(DeleteBehavior.SetNull);
                  });
                  
                  modelBuilder.Entity<SupplierAddress>(entity =>
                  {
                        // 欄位對應 - 主鍵在資料庫中就叫 Id，不需要欄位對應
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                        
                        // 關聯設定
                        entity.HasOne(e => e.Supplier)
                        .WithMany(s => s.SupplierAddresses)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(e => e.AddressType)
                        .WithMany(at => at.SupplierAddresses)
                        .OnDelete(DeleteBehavior.SetNull);
                  });
                  
                  modelBuilder.Entity<SupplierType>(entity =>
                  {
                        // 欄位對應                        
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                  });
                  
                  // 員工相關
                  modelBuilder.Entity<Employee>(entity =>
                  {
                        // 欄位對應                        
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                        
                        // 關聯設定
                        entity.HasOne(e => e.Role)
                        .WithMany(r => r.Employees)
                        .OnDelete(DeleteBehavior.Restrict);
                        
                        entity.HasOne(e => e.EmployeePosition)
                        .WithMany(ep => ep.Employees)
                        .OnDelete(DeleteBehavior.SetNull);
                        
                        entity.HasOne(e => e.Department)
                        .WithMany(d => d.Employees)
                        .OnDelete(DeleteBehavior.SetNull);
                  });
                  
                  modelBuilder.Entity<Department>(entity =>
                  {
                        // 欄位對應                        
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                        
                        // 關聯設定 - 自我參考（上下級部門）
                        entity.HasOne(d => d.ParentDepartment)
                        .WithMany(d => d.ChildDepartments)
                        .HasForeignKey(d => d.ParentDepartmentId)
                        .OnDelete(DeleteBehavior.Restrict);
                  });
                  
                  modelBuilder.Entity<EmployeePosition>(entity =>
                  {
                        // 欄位對應                        
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                  });
                  
                  modelBuilder.Entity<EmployeeContact>(entity =>
                  {
                        // 欄位對應                        
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                  });
                  
                  modelBuilder.Entity<EmployeeAddress>(entity =>
                  {
                        // 欄位對應                        
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                  });
                  
                  // 產品相關
                  modelBuilder.Entity<Product>(entity =>
                  {
                        // 欄位對應                        
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                        
                        // 關聯設定
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
                        // 欄位對應                        
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                        
                        // 關聯設定
                        entity.HasOne(ps => ps.Product)
                        .WithMany(p => p.ProductSuppliers)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(ps => ps.Supplier)
                        .WithMany(s => s.ProductSuppliers)
                        .OnDelete(DeleteBehavior.Cascade);
                  });
                  
                  // 共用參考資料
                  modelBuilder.Entity<IndustryType>(entity =>
                  {
                        // 欄位對應                        
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                  });
                  
                  modelBuilder.Entity<AddressType>(entity =>
                  {
                        // 欄位對應                        
                        entity.Property(e => e.Id).ValueGeneratedOnAdd(); // 確保 Identity
                  });
                  
                  modelBuilder.Entity<ContactType>(entity =>
                  {
                        // 欄位對應                        
                        entity.Property(e => e.Id).ValueGeneratedOnAdd(); // 確保 Identity
                  });

                  // 權限管理
                  modelBuilder.Entity<RolePermission>(entity =>
                  {
                        entity.HasOne(rp => rp.Role)
                        .WithMany(r => r.RolePermissions)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(rp => rp.Permission)
                        .WithMany(p => p.RolePermissions)
                        .OnDelete(DeleteBehavior.Cascade);
                  });

                  // 庫存管理
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

                  // BOM 基礎
                  modelBuilder.Entity<Material>(entity =>
                  {
                        entity.HasOne(m => m.Supplier)
                        .WithMany()
                        .HasForeignKey(m => m.SupplierId)
                        .OnDelete(DeleteBehavior.SetNull);
                  });

                  // 庫存盤點相關
                  modelBuilder.Entity<StockTaking>(entity =>
                  {
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                        
                        // 關聯設定
                        entity.HasOne(st => st.Warehouse)
                        .WithMany()
                        .HasForeignKey(st => st.WarehouseId)
                        .OnDelete(DeleteBehavior.Restrict);

                        entity.HasOne(st => st.WarehouseLocation)
                        .WithMany()
                        .HasForeignKey(st => st.WarehouseLocationId)
                        .OnDelete(DeleteBehavior.SetNull);

                        entity.HasOne(st => st.ApprovedByUser)
                        .WithMany()
                        .HasForeignKey(st => st.ApprovedBy)
                        .OnDelete(DeleteBehavior.SetNull);

                        entity.HasMany(st => st.StockTakingDetails)
                        .WithOne(std => std.StockTaking)
                        .HasForeignKey(std => std.StockTakingId)
                        .OnDelete(DeleteBehavior.Cascade);
                  });

                  modelBuilder.Entity<StockTakingDetail>(entity =>
                  {
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                        
                        // 關聯設定
                        entity.HasOne(std => std.StockTaking)
                        .WithMany(st => st.StockTakingDetails)
                        .HasForeignKey(std => std.StockTakingId)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(std => std.Product)
                        .WithMany()
                        .HasForeignKey(std => std.ProductId)
                        .OnDelete(DeleteBehavior.Restrict);

                        entity.HasOne(std => std.WarehouseLocation)
                        .WithMany()
                        .HasForeignKey(std => std.WarehouseLocationId)
                        .OnDelete(DeleteBehavior.SetNull);
                  });

                  // 採購相關
                  modelBuilder.Entity<PurchaseOrder>(entity =>
                  {
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                        
                        // 關聯設定
                        entity.HasOne(po => po.Supplier)
                        .WithMany()
                        .HasForeignKey(po => po.SupplierId)
                        .OnDelete(DeleteBehavior.Restrict);

                        entity.HasOne(po => po.Warehouse)
                        .WithMany()
                        .HasForeignKey(po => po.WarehouseId)
                        .OnDelete(DeleteBehavior.SetNull);

                        entity.HasOne(po => po.ApprovedByUser)
                        .WithMany()
                        .HasForeignKey(po => po.ApprovedBy)
                        .OnDelete(DeleteBehavior.SetNull);
                  });

                  modelBuilder.Entity<PurchaseOrderDetail>(entity =>
                  {
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                        
                        // 關聯設定
                        entity.HasOne(pod => pod.PurchaseOrder)
                        .WithMany(po => po.PurchaseOrderDetails)
                        .HasForeignKey(pod => pod.PurchaseOrderId)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(pod => pod.Product)
                        .WithMany()
                        .HasForeignKey(pod => pod.ProductId)
                        .OnDelete(DeleteBehavior.Restrict);
                  });

                  modelBuilder.Entity<PurchaseReceipt>(entity =>
                  {
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                        
                        // 關聯設定
                        entity.HasOne(pr => pr.PurchaseOrder)
                        .WithMany(po => po.PurchaseReceipts)
                        .HasForeignKey(pr => pr.PurchaseOrderId)
                        .OnDelete(DeleteBehavior.Restrict);

                        entity.HasOne(pr => pr.Warehouse)
                        .WithMany()
                        .HasForeignKey(pr => pr.WarehouseId)
                        .OnDelete(DeleteBehavior.Restrict);

                        entity.HasOne(pr => pr.ConfirmedByUser)
                        .WithMany()
                        .HasForeignKey(pr => pr.ConfirmedBy)
                        .OnDelete(DeleteBehavior.SetNull);
                  });

                  modelBuilder.Entity<PurchaseReceiptDetail>(entity =>
                  {
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                        
                        // 關聯設定
                        entity.HasOne(prd => prd.PurchaseReceipt)
                        .WithMany(pr => pr.PurchaseReceiptDetails)
                        .HasForeignKey(prd => prd.PurchaseReceiptId)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(prd => prd.PurchaseOrderDetail)
                        .WithMany(pod => pod.PurchaseReceiptDetails)
                        .HasForeignKey(prd => prd.PurchaseOrderDetailId)
                        .OnDelete(DeleteBehavior.Restrict);

                        entity.HasOne(prd => prd.Product)
                        .WithMany()
                        .HasForeignKey(prd => prd.ProductId)
                        .OnDelete(DeleteBehavior.Restrict);

                        entity.HasOne(prd => prd.WarehouseLocation)
                        .WithMany()
                        .HasForeignKey(prd => prd.WarehouseLocationId)
                        .OnDelete(DeleteBehavior.SetNull);
                  });
            }
    }
}
