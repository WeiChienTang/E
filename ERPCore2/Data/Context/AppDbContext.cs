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
      public DbSet<ContactType> ContactTypes { get; set; }
      public DbSet<AddressType> AddressTypes { get; set; }
      public DbSet<PaymentMethod> PaymentMethods { get; set; }
      public DbSet<Bank> Banks { get; set; }
      
      // 統一聯絡方式管理
      public DbSet<Contact> Contacts { get; set; }
      
      // 統一地址管理
      public DbSet<Address> Addresses { get; set; }
            
      public DbSet<Employee> Employees { get; set; }
      public DbSet<EmployeePosition> EmployeePositions { get; set; }
      public DbSet<Department> Departments { get; set; }
      public DbSet<Role> Roles { get; set; }
      public DbSet<Permission> Permissions { get; set; }
      public DbSet<RolePermission> RolePermissions { get; set; }
      public DbSet<Product> Products { get; set; }
      public DbSet<ProductCategory> ProductCategories { get; set; }
      public DbSet<ProductSupplier> ProductSuppliers { get; set; }
      
      // Product Pricing Management
      public DbSet<ProductPricing> ProductPricings { get; set; }
      public DbSet<SupplierPricing> SupplierPricings { get; set; }
      public DbSet<PriceHistory> PriceHistories { get; set; }
      
      public DbSet<Supplier> Suppliers { get; set; }
      public DbSet<SupplierType> SupplierTypes { get; set; }
      
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
      public DbSet<PurchaseReceiving> PurchaseReceivings { get; set; }
      public DbSet<PurchaseReceivingDetail> PurchaseReceivingDetails { get; set; }
      public DbSet<PurchaseReturn> PurchaseReturns { get; set; }
      public DbSet<PurchaseReturnDetail> PurchaseReturnDetails { get; set; }
      
      // Sales Management
      public DbSet<SalesOrder> SalesOrders { get; set; }
      public DbSet<SalesOrderDetail> SalesOrderDetails { get; set; }
      public DbSet<SalesReturn> SalesReturns { get; set; }
      public DbSet<SalesReturnDetail> SalesReturnDetails { get; set; }
      public DbSet<ERPCore2.Data.Entities.SalesReturnReason> SalesReturnReasons { get; set; }
      
      // Financial Management
      public DbSet<AccountsReceivableSetoff> AccountsReceivableSetoffs { get; set; }
      public DbSet<AccountsReceivableSetoffDetail> AccountsReceivableSetoffDetails { get; set; }
      public DbSet<AccountsReceivableSetoffPaymentDetail> AccountsReceivableSetoffPaymentDetails { get; set; }
      public DbSet<Prepayment> Prepayments { get; set; }
      public DbSet<Prepaid> Prepaids { get; set; }
      public DbSet<FinancialTransaction> FinancialTransactions { get; set; }
      // Currency
      public DbSet<Currency> Currencies { get; set; }
      
      // BOM Foundations
      public DbSet<Material> Materials { get; set; }
      public DbSet<Weather> Weathers { get; set; }
      public DbSet<Color> Colors { get; set; }
      public DbSet<Size> Sizes { get; set; }
      
      // System Logs
      public DbSet<ErrorLog> ErrorLogs { get; set; }
      public DbSet<DeletedRecord> DeletedRecords { get; set; }
      
      // System Settings
      public DbSet<SystemParameter> SystemParameters { get; set; }
      public DbSet<Company> Companies { get; set; }
      public DbSet<PaperSetting> PaperSettings { get; set; }
      public DbSet<PrinterConfiguration> PrinterConfigurations { get; set; }
      public DbSet<ReportPrintConfiguration> ReportPrintConfigurations { get; set; }

      protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                  base.OnModelCreating(modelBuilder);

                  // === 實體設定（包含欄位對應和關聯） ===
                  
                  // 統一地址配置
                  modelBuilder.Entity<Address>(entity =>
                  {
                        // 欄位對應
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                        entity.Property(e => e.OwnerType).IsRequired().HasMaxLength(20);
                        entity.Property(e => e.OwnerId).IsRequired();
                        entity.Property(e => e.AddressLine).HasMaxLength(200);
                        
                        // 索引設定
                        entity.HasIndex(e => new { e.OwnerType, e.OwnerId })
                              .HasDatabaseName("IX_Address_OwnerType_OwnerId");
                        
                        // 關聯設定
                        entity.HasOne(e => e.AddressType)
                              .WithMany(at => at.Addresses)
                              .HasForeignKey(e => e.AddressTypeId)
                              .OnDelete(DeleteBehavior.SetNull);
                  });
                  
                  // 客戶相關
                  modelBuilder.Entity<Customer>(entity =>
                  {
                        // 欄位對應 - 主鍵在資料庫中就叫 Id，不需要欄位對應
                        entity.Property(e => e.Id).ValueGeneratedOnAdd(); // 確保 Identity
                        
                        // 關聯設定
                        entity.HasOne(e => e.CustomerType)
                        .WithMany(ct => ct.Customers)
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
                  });
                  
                  modelBuilder.Entity<EmployeePosition>(entity =>
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

                        entity.HasOne(ps => ps.Unit)
                        .WithMany()
                        .OnDelete(DeleteBehavior.SetNull);
                  });
                  
                  // 共用參考資料
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
                  
                  modelBuilder.Entity<PaymentMethod>(entity =>
                  {
                        // 欄位對應                        
                        entity.Property(e => e.Id).ValueGeneratedOnAdd(); // 確保 Identity
                        entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
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

                  modelBuilder.Entity<PurchaseReceiving>(entity =>
                  {
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                        
                        // 關聯設定
                        entity.HasOne(pr => pr.PurchaseOrder)
                        .WithMany(po => po.PurchaseReceivings)
                        .HasForeignKey(pr => pr.PurchaseOrderId)
                        .OnDelete(DeleteBehavior.Restrict);
                  });

                  modelBuilder.Entity<PurchaseReceivingDetail>(entity =>
                  {
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                        
                        // 關聯設定
                        entity.HasOne(prd => prd.PurchaseReceiving)
                        .WithMany(pr => pr.PurchaseReceivingDetails)
                        .HasForeignKey(prd => prd.PurchaseReceivingId)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(prd => prd.PurchaseOrderDetail)
                        .WithMany(pod => pod.PurchaseReceivingDetails)
                        .HasForeignKey(prd => prd.PurchaseOrderDetailId)
                        .OnDelete(DeleteBehavior.Restrict);

                        entity.HasOne(prd => prd.Product)
                        .WithMany()
                        .HasForeignKey(prd => prd.ProductId)
                        .OnDelete(DeleteBehavior.Restrict);

                        entity.HasOne(prd => prd.Warehouse)
                        .WithMany()
                        .HasForeignKey(prd => prd.WarehouseId)
                        .OnDelete(DeleteBehavior.Restrict);

                        entity.HasOne(prd => prd.WarehouseLocation)
                        .WithMany()
                        .HasForeignKey(prd => prd.WarehouseLocationId)
                        .OnDelete(DeleteBehavior.SetNull);
                  });

                  modelBuilder.Entity<PurchaseReturn>(entity =>
                  {
                        entity.HasKey(pr => pr.Id);

                        entity.HasOne(pr => pr.Supplier)
                        .WithMany()
                        .HasForeignKey(pr => pr.SupplierId)
                        .OnDelete(DeleteBehavior.Restrict);

                        entity.HasOne(pr => pr.PurchaseReceiving)
                        .WithMany()
                        .HasForeignKey(pr => pr.PurchaseReceivingId)
                        .OnDelete(DeleteBehavior.Restrict);
                  });

                  modelBuilder.Entity<PurchaseReturnDetail>(entity =>
                  {
                        entity.HasKey(prd => prd.Id);

                        entity.HasOne(prd => prd.PurchaseReturn)
                        .WithMany(pr => pr.PurchaseReturnDetails)
                        .HasForeignKey(prd => prd.PurchaseReturnId)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(prd => prd.Product)
                        .WithMany()
                        .HasForeignKey(prd => prd.ProductId)
                        .OnDelete(DeleteBehavior.Restrict);

                        entity.HasOne(prd => prd.PurchaseOrderDetail)
                        .WithMany()
                        .HasForeignKey(prd => prd.PurchaseOrderDetailId)
                        .OnDelete(DeleteBehavior.SetNull);

                        entity.HasOne(prd => prd.PurchaseReceivingDetail)
                        .WithMany()
                        .HasForeignKey(prd => prd.PurchaseReceivingDetailId)
                        .OnDelete(DeleteBehavior.SetNull);

                        entity.HasOne(prd => prd.Unit)
                        .WithMany()
                        .HasForeignKey(prd => prd.UnitId)
                        .OnDelete(DeleteBehavior.SetNull);

                        entity.HasOne(prd => prd.WarehouseLocation)
                        .WithMany()
                        .HasForeignKey(prd => prd.WarehouseLocationId)
                        .OnDelete(DeleteBehavior.SetNull);
                  });

                  // Sales Management Relationships
                  modelBuilder.Entity<SalesOrder>(entity =>
                  {
                        entity.HasKey(so => so.Id);

                        entity.HasOne(so => so.Customer)
                        .WithMany()
                        .HasForeignKey(so => so.CustomerId)
                        .OnDelete(DeleteBehavior.Restrict);

                        entity.HasOne(so => so.Employee)
                        .WithMany()
                        .HasForeignKey(so => so.EmployeeId)
                        .OnDelete(DeleteBehavior.SetNull);
                  });

                  modelBuilder.Entity<SalesOrderDetail>(entity =>
                  {
                        entity.HasKey(sod => sod.Id);

                        entity.HasOne(sod => sod.SalesOrder)
                        .WithMany(so => so.SalesOrderDetails)
                        .HasForeignKey(sod => sod.SalesOrderId)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(sod => sod.Product)
                        .WithMany()
                        .HasForeignKey(sod => sod.ProductId)
                        .OnDelete(DeleteBehavior.Restrict);

                        entity.HasOne(sod => sod.Unit)
                        .WithMany()
                        .HasForeignKey(sod => sod.UnitId)
                        .OnDelete(DeleteBehavior.SetNull);
                  });



                  // Sales Return Relationships
                  modelBuilder.Entity<SalesReturn>(entity =>
                  {
                        entity.HasKey(sr => sr.Id);

                        entity.HasOne(sr => sr.Customer)
                        .WithMany()
                        .HasForeignKey(sr => sr.CustomerId)
                        .OnDelete(DeleteBehavior.Restrict);

                        entity.HasOne(sr => sr.SalesOrder)
                        .WithMany()
                        .HasForeignKey(sr => sr.SalesOrderId)
                        .OnDelete(DeleteBehavior.SetNull);

                        entity.HasOne(sr => sr.Employee)
                        .WithMany()
                        .HasForeignKey(sr => sr.EmployeeId)
                        .OnDelete(DeleteBehavior.SetNull);

                        entity.HasOne(sr => sr.ReturnReason)
                        .WithMany(srr => srr.SalesReturns)
                        .HasForeignKey(sr => sr.ReturnReasonId)
                        .OnDelete(DeleteBehavior.Restrict);
                  });

                  modelBuilder.Entity<ERPCore2.Data.Entities.SalesReturnReason>(entity =>
                  {
                        entity.HasKey(srr => srr.Id);
                        
                        entity.HasIndex(srr => srr.Name).IsUnique();
                  });

                  modelBuilder.Entity<SalesReturnDetail>(entity =>
                  {
                        entity.HasKey(srd => srd.Id);

                        entity.HasOne(srd => srd.SalesReturn)
                        .WithMany(sr => sr.SalesReturnDetails)
                        .HasForeignKey(srd => srd.SalesReturnId)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(srd => srd.Product)
                        .WithMany()
                        .HasForeignKey(srd => srd.ProductId)
                        .OnDelete(DeleteBehavior.Restrict);

                        entity.HasOne(srd => srd.SalesOrderDetail)
                        .WithMany()
                        .HasForeignKey(srd => srd.SalesOrderDetailId)
                        .OnDelete(DeleteBehavior.SetNull);
                  });

                  // Financial Management Relationships
                  modelBuilder.Entity<FinancialTransaction>(entity =>
                  {
                        entity.HasKey(ft => ft.Id);

                        // 關聯設定
                        entity.HasOne(ft => ft.Customer)
                              .WithMany()
                              .HasForeignKey(ft => ft.CustomerId)
                              .OnDelete(DeleteBehavior.SetNull);

                        entity.HasOne(ft => ft.Company)
                              .WithMany()
                              .HasForeignKey(ft => ft.CompanyId)
                              .OnDelete(DeleteBehavior.Restrict);

                        entity.HasOne(ft => ft.PaymentMethod)
                              .WithMany()
                              .HasForeignKey(ft => ft.PaymentMethodId)
                              .OnDelete(DeleteBehavior.SetNull);

                        entity.HasOne(ft => ft.ReversalTransaction)
                              .WithMany(rft => rft.ReversedTransactions)
                              .HasForeignKey(ft => ft.ReversalTransactionId)
                              .OnDelete(DeleteBehavior.Restrict);

                        // 為 decimal 屬性設定精確度和小數位數
                        entity.Property(e => e.Amount)
                              .HasPrecision(18, 2);
                        entity.Property(e => e.BalanceBefore)
                              .HasPrecision(18, 2);
                        entity.Property(e => e.BalanceAfter)
                              .HasPrecision(18, 2);
                        entity.Property(e => e.OriginalAmount)
                              .HasPrecision(18, 4);
                        entity.Property(e => e.ExchangeRate)
                              .HasPrecision(18, 6);

                        // 設定索引
                        entity.HasIndex(e => e.TransactionNumber)
                              .IsUnique();
                        entity.HasIndex(e => new { e.TransactionType, e.TransactionDate });
                        entity.HasIndex(e => new { e.CustomerId, e.TransactionDate });
                        entity.HasIndex(e => new { e.CompanyId, e.TransactionDate });
                        entity.HasIndex(e => new { e.SourceDocumentType, e.SourceDocumentId });
                        entity.HasIndex(e => e.VendorId); // 為未來的供應商功能預留
                  });

                  modelBuilder.Entity<AccountsReceivableSetoff>(entity =>
                  {
                        entity.HasKey(ars => ars.Id);

                        entity.HasOne(ars => ars.Customer)
                        .WithMany()
                        .HasForeignKey(ars => ars.CustomerId)
                        .OnDelete(DeleteBehavior.Restrict);

                        entity.HasOne(ars => ars.PaymentMethod)
                        .WithMany()
                        .HasForeignKey(ars => ars.PaymentMethodId)
                        .OnDelete(DeleteBehavior.SetNull);

                        // 為 decimal 屬性設定精確度和小數位數
                        entity.Property(e => e.TotalSetoffAmount)
                              .HasPrecision(18, 2);

                        // 設定唯一索引
                        entity.HasIndex(e => e.SetoffNumber)
                              .IsUnique();

                        // 設定日期索引
                        entity.HasIndex(e => e.SetoffDate);
                        entity.HasIndex(e => e.CustomerId);
                  });

                  modelBuilder.Entity<AccountsReceivableSetoffDetail>(entity =>
                  {
                        entity.HasKey(arsd => arsd.Id);

                        entity.HasOne(arsd => arsd.Setoff)
                        .WithMany(ars => ars.SetoffDetails)
                        .HasForeignKey(arsd => arsd.SetoffId)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(arsd => arsd.SalesOrderDetail)
                        .WithMany()
                        .HasForeignKey(arsd => arsd.SalesOrderDetailId)
                        .OnDelete(DeleteBehavior.SetNull);

                        entity.HasOne(arsd => arsd.SalesReturnDetail)
                        .WithMany()
                        .HasForeignKey(arsd => arsd.SalesReturnDetailId)
                        .OnDelete(DeleteBehavior.SetNull);

                        // 為 decimal 屬性設定精確度和小數位數
                        entity.Property(e => e.SetoffAmount)
                              .HasPrecision(18, 2);

                        // 設定索引
                        entity.HasIndex(e => e.SetoffId);
                        entity.HasIndex(e => e.SalesOrderDetailId);
                        entity.HasIndex(e => e.SalesReturnDetailId);

                        // 設定 Table 檢查約束
                        entity.ToTable(t => t.HasCheckConstraint("CK_AccountsReceivableSetoffDetail_RelatedDetail", 
                              "SalesOrderDetailId IS NOT NULL OR SalesReturnDetailId IS NOT NULL"));
                  });

                  modelBuilder.Entity<Prepayment>(entity =>
                  {
                        entity.HasKey(p => p.Id);

                        entity.HasOne(p => p.AccountsReceivableSetoff)
                        .WithMany()
                        .HasForeignKey(p => p.SetoffId)
                        .OnDelete(DeleteBehavior.Restrict);

                        // 為 decimal 屬性設定精確度和小數位數
                        entity.Property(e => e.PrepaymentAmount)
                              .HasPrecision(18, 2);

                        // 設定唯一索引
                        entity.HasIndex(e => e.Code)
                              .IsUnique();

                        // 設定其他索引
                        entity.HasIndex(e => e.SetoffId);
                        entity.HasIndex(e => e.PrepaymentDate);
                  });

                  // 紙張設定相關
                  modelBuilder.Entity<PaperSetting>(entity =>
                  {
                        // 欄位對應
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();

                        // 為 decimal 屬性設定精確度和小數位數
                        entity.Property(e => e.Width)
                              .HasPrecision(8, 2); // 總共8位數，小數點後2位

                        entity.Property(e => e.Height)
                              .HasPrecision(8, 2); // 總共8位數，小數點後2位

                        entity.Property(e => e.TopMargin)
                              .HasPrecision(6, 2); // 總共6位數，小數點後2位

                        entity.Property(e => e.BottomMargin)
                              .HasPrecision(6, 2); // 總共6位數，小數點後2位

                        entity.Property(e => e.LeftMargin)
                              .HasPrecision(6, 2); // 總共6位數，小數點後2位

                        entity.Property(e => e.RightMargin)
                              .HasPrecision(6, 2); // 總共6位數，小數點後2位
                  });

                  // 報表列印配置相關
                  modelBuilder.Entity<ReportPrintConfiguration>(entity =>
                  {
                        // 欄位對應
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();

                        // 外鍵關係設定
                        entity.HasOne(e => e.PrinterConfiguration)
                              .WithMany()
                              .HasForeignKey(e => e.PrinterConfigurationId)
                              .OnDelete(DeleteBehavior.SetNull);

                        entity.HasOne(e => e.PaperSetting)
                              .WithMany()
                              .HasForeignKey(e => e.PaperSettingId)
                              .OnDelete(DeleteBehavior.SetNull);

                        // 建立複合索引確保報表類型的唯一性
                        entity.HasIndex(e => e.ReportType)
                              .IsUnique();
                  });

                  // 系統參數相關
                  modelBuilder.Entity<SystemParameter>(entity =>
                  {
                        // 欄位對應
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();

                        // 為 decimal 屬性設定精確度和小數位數
                        entity.Property(e => e.TaxRate)
                              .HasPrecision(5, 2); // 總共5位數，小數點後2位（可表示 0.00 到 999.99）
                  });

                  // Currency
                  modelBuilder.Entity<Currency>(entity =>
                  {
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                        entity.Property(e => e.Code).HasMaxLength(10).IsRequired();
                        entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                        entity.Property(e => e.Symbol).HasMaxLength(10);
                        entity.Property(e => e.ExchangeRate).HasPrecision(18, 6);

                        entity.HasIndex(e => e.Code).IsUnique();
                  });
            }
    }
}
