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
      public DbSet<PaymentMethod> PaymentMethods { get; set; }
      public DbSet<Bank> Banks { get; set; }            
      public DbSet<Employee> Employees { get; set; }
      public DbSet<EmployeePosition> EmployeePositions { get; set; }
      public DbSet<Department> Departments { get; set; }
      public DbSet<Role> Roles { get; set; }
      public DbSet<Permission> Permissions { get; set; }
      public DbSet<RolePermission> RolePermissions { get; set; }
      public DbSet<Product> Products { get; set; }
      public DbSet<ProductCategory> ProductCategories { get; set; }
      public DbSet<SupplierPricing> SupplierPricings { get; set; }
      public DbSet<PriceHistory> PriceHistories { get; set; }      
      public DbSet<Supplier> Suppliers { get; set; }      
      public DbSet<Warehouse> Warehouses { get; set; }
      public DbSet<WarehouseLocation> WarehouseLocations { get; set; }
      public DbSet<InventoryStock> InventoryStocks { get; set; }
      public DbSet<InventoryStockDetail> InventoryStockDetails { get; set; }
      public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
      public DbSet<InventoryReservation> InventoryReservations { get; set; }
      public DbSet<StockTaking> StockTakings { get; set; }
      public DbSet<StockTakingDetail> StockTakingDetails { get; set; }
      public DbSet<MaterialIssue> MaterialIssues { get; set; }
      public DbSet<MaterialIssueDetail> MaterialIssueDetails { get; set; }
      public DbSet<Unit> Units { get; set; }
      public DbSet<UnitConversion> UnitConversions { get; set; }
      public DbSet<InventoryTransactionType> InventoryTransactionTypes { get; set; }
      public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
      public DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
      public DbSet<PurchaseReceiving> PurchaseReceivings { get; set; }
      public DbSet<PurchaseReceivingDetail> PurchaseReceivingDetails { get; set; }
      public DbSet<PurchaseReturn> PurchaseReturns { get; set; }
      public DbSet<PurchaseReturnDetail> PurchaseReturnDetails { get; set; }
      public DbSet<SalesOrder> SalesOrders { get; set; }
      public DbSet<SalesOrderDetail> SalesOrderDetails { get; set; }
      public DbSet<SalesDelivery> SalesDeliveries { get; set; }
      public DbSet<SalesDeliveryDetail> SalesDeliveryDetails { get; set; }
      public DbSet<SalesReturn> SalesReturns { get; set; }
      public DbSet<SalesReturnDetail> SalesReturnDetails { get; set; }
      public DbSet<SalesReturnReason> SalesReturnReasons { get; set; }
      public DbSet<Quotation> Quotations { get; set; }
      public DbSet<QuotationDetail> QuotationDetails { get; set; }
      public DbSet<QuotationCompositionDetail> QuotationCompositionDetails { get; set; }
      public DbSet<SalesOrderCompositionDetail> SalesOrderCompositionDetails { get; set; }
      public DbSet<SetoffDocument> SetoffDocuments { get; set; }
      public DbSet<SetoffPayment> SetoffPayments { get; set; }
      public DbSet<SetoffProductDetail> SetoffProductDetails { get; set; }
      public DbSet<SetoffPrepayment> SetoffPrepayments { get; set; }
      public DbSet<SetoffPrepaymentUsage> SetoffPrepaymentUsages { get; set; }
      public DbSet<FinancialTransaction> FinancialTransactions { get; set; }
      public DbSet<Entities.PrepaymentType> PrepaymentTypes { get; set; }
      public DbSet<Currency> Currencies { get; set; }
      public DbSet<Material> Materials { get; set; }
      public DbSet<Weather> Weathers { get; set; }
      public DbSet<Color> Colors { get; set; }
      public DbSet<Size> Sizes { get; set; }      
      public DbSet<CompositionCategory> CompositionCategories { get; set; }
      public DbSet<ProductComposition> ProductCompositions { get; set; }
      public DbSet<ProductCompositionDetail> ProductCompositionDetails { get; set; }
      public DbSet<ProductionSchedule> ProductionSchedules { get; set; }
      public DbSet<ProductionScheduleDetail> ProductionScheduleDetails { get; set; }
      public DbSet<ErrorLog> ErrorLogs { get; set; }
      public DbSet<DeletedRecord> DeletedRecords { get; set; }
      public DbSet<SystemParameter> SystemParameters { get; set; }
      public DbSet<Company> Companies { get; set; }
      public DbSet<PaperSetting> PaperSettings { get; set; }
      public DbSet<PrinterConfiguration> PrinterConfigurations { get; set; }
      public DbSet<ReportPrintConfiguration> ReportPrintConfigurations { get; set; }

      protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                  base.OnModelCreating(modelBuilder);

                  // 客戶相關
                  modelBuilder.Entity<Customer>(entity =>
                  {
                        // 欄位對應 - 主鍵在資料庫中就叫 Id，不需要欄位對應
                        entity.Property(e => e.Id).ValueGeneratedOnAdd(); // 確保 Identity
                        
                        // 業務負責人關聯
                        entity.HasOne<Employee>()
                              .WithMany()
                              .HasForeignKey(c => c.EmployeeId)
                              .OnDelete(DeleteBehavior.SetNull);
                        
                        // 付款方式關聯
                        entity.HasOne(c => c.PaymentMethod)
                              .WithMany()
                              .HasForeignKey(c => c.PaymentMethodId)
                              .OnDelete(DeleteBehavior.SetNull);
                  });

                  // 供應商相關
                  modelBuilder.Entity<Supplier>(entity =>
                  {
                        // 欄位對應 - 主鍵在資料庫中就叫 Id，不需要欄位對應
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
                        
                        entity.HasOne(e => e.Supplier)
                        .WithMany(s => s.Products)
                        .HasForeignKey(e => e.SupplierId)
                        .OnDelete(DeleteBehavior.SetNull);
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

                  modelBuilder.Entity<InventoryStock>(entity =>
                  {
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                        
                        // 商品關聯
                        entity.HasOne(invStock => invStock.Product)
                        .WithMany(p => p.InventoryStocks)
                        .HasForeignKey(invStock => invStock.ProductId)
                        .OnDelete(DeleteBehavior.Restrict);

                        // 庫存明細關聯
                        entity.HasMany(invStock => invStock.InventoryStockDetails)
                        .WithOne(isd => isd.InventoryStock)
                        .HasForeignKey(isd => isd.InventoryStockId)
                        .OnDelete(DeleteBehavior.Cascade);
                  });

                  modelBuilder.Entity<InventoryStockDetail>(entity =>
                  {
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                        
                        // 庫存主檔關聯
                        entity.HasOne(isd => isd.InventoryStock)
                        .WithMany(invStock => invStock.InventoryStockDetails)
                        .HasForeignKey(isd => isd.InventoryStockId)
                        .OnDelete(DeleteBehavior.Cascade);

                        // 倉庫關聯
                        entity.HasOne(isd => isd.Warehouse)
                        .WithMany(w => w.InventoryStockDetails)
                        .HasForeignKey(isd => isd.WarehouseId)
                        .OnDelete(DeleteBehavior.Restrict);

                        // 倉庫位置關聯
                        entity.HasOne(isd => isd.WarehouseLocation)
                        .WithMany()
                        .HasForeignKey(isd => isd.WarehouseLocationId)
                        .OnDelete(DeleteBehavior.SetNull);

                        // decimal 屬性設定
                        entity.Property(e => e.AverageCost)
                        .HasPrecision(18, 4);
                  });

                  // 庫存交易記錄關聯
                  modelBuilder.Entity<InventoryTransaction>(entity =>
                  {
                        // 庫存明細關聯
                        entity.HasOne(it => it.InventoryStockDetail)
                        .WithMany(isd => isd.InventoryTransactions)
                        .HasForeignKey(it => it.InventoryStockDetailId)
                        .OnDelete(DeleteBehavior.SetNull);
                  });

                  // 庫存預留關聯
                  modelBuilder.Entity<InventoryReservation>(entity =>
                  {
                        // 庫存明細關聯
                        entity.HasOne(ir => ir.InventoryStockDetail)
                        .WithMany(isd => isd.InventoryReservations)
                        .HasForeignKey(ir => ir.InventoryStockDetailId)
                        .OnDelete(DeleteBehavior.SetNull);
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
                        .OnDelete(DeleteBehavior.SetNull);

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

                        entity.HasOne(so => so.Quotation)
                        .WithMany(q => q.SalesOrders)
                        .HasForeignKey(so => so.QuotationId)
                        .OnDelete(DeleteBehavior.Restrict);

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

                        entity.HasOne(sr => sr.SalesDelivery)
                        .WithMany(sd => sd.SalesReturns)
                        .HasForeignKey(sr => sr.SalesDeliveryId)
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

                        entity.HasOne(srd => srd.SalesDeliveryDetail)
                        .WithMany(sdd => sdd.SalesReturnDetails)
                        .HasForeignKey(srd => srd.SalesDeliveryDetailId)
                        .OnDelete(DeleteBehavior.SetNull);
                  });

                  // Quotation Management
                  modelBuilder.Entity<Quotation>(entity =>
                  {
                        entity.HasKey(q => q.Id);

                        entity.HasOne(q => q.Customer)
                        .WithMany()
                        .HasForeignKey(q => q.CustomerId)
                        .OnDelete(DeleteBehavior.Restrict);

                        entity.HasOne(q => q.Employee)
                        .WithMany()
                        .HasForeignKey(q => q.EmployeeId)
                        .OnDelete(DeleteBehavior.SetNull);

                        entity.HasOne(q => q.ApprovedByUser)
                        .WithMany()
                        .HasForeignKey(q => q.ApprovedBy)
                        .OnDelete(DeleteBehavior.Restrict);
                  });

                  modelBuilder.Entity<QuotationDetail>(entity =>
                  {
                        entity.HasKey(qd => qd.Id);

                        entity.HasOne(qd => qd.Quotation)
                        .WithMany(q => q.QuotationDetails)
                        .HasForeignKey(qd => qd.QuotationId)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(qd => qd.Product)
                        .WithMany()
                        .HasForeignKey(qd => qd.ProductId)
                        .OnDelete(DeleteBehavior.Restrict);

                        entity.HasOne(qd => qd.Unit)
                        .WithMany()
                        .HasForeignKey(qd => qd.UnitId)
                        .OnDelete(DeleteBehavior.SetNull);
                  });

                  // 報價單組成明細配置
                  modelBuilder.Entity<QuotationCompositionDetail>(entity =>
                  {
                        entity.HasKey(qcd => qcd.Id);
                        
                        // 複合唯一索引：同一報價明細中，同一組成商品只能出現一次
                        entity.HasIndex(e => new { e.QuotationDetailId, e.ComponentProductId })
                              .IsUnique();

                        // 與 QuotationDetail 的關聯
                        entity.HasOne(qcd => qcd.QuotationDetail)
                              .WithMany(qd => qd.CompositionDetails)
                              .HasForeignKey(qcd => qcd.QuotationDetailId)
                              .OnDelete(DeleteBehavior.Cascade);

                        // 與 Product 的關聯
                        entity.HasOne(qcd => qcd.ComponentProduct)
                              .WithMany()
                              .HasForeignKey(qcd => qcd.ComponentProductId)
                              .OnDelete(DeleteBehavior.Restrict);

                        // 與 Unit 的關聯
                        entity.HasOne(qcd => qcd.Unit)
                              .WithMany()
                              .HasForeignKey(qcd => qcd.UnitId)
                              .OnDelete(DeleteBehavior.ClientSetNull);
                  });

                  // 銷貨訂單組成明細配置
                  modelBuilder.Entity<SalesOrderCompositionDetail>(entity =>
                  {
                        entity.HasKey(socd => socd.Id);
                        
                        // 複合唯一索引：同一銷貨訂單明細中，同一組成商品只能出現一次
                        entity.HasIndex(e => new { e.SalesOrderDetailId, e.ComponentProductId })
                              .IsUnique();

                        // 與 SalesOrderDetail 的關聯
                        entity.HasOne(socd => socd.SalesOrderDetail)
                              .WithMany(sod => sod.CompositionDetails)
                              .HasForeignKey(socd => socd.SalesOrderDetailId)
                              .OnDelete(DeleteBehavior.Cascade);

                        // 與 Product 的關聯
                        entity.HasOne(socd => socd.ComponentProduct)
                              .WithMany()
                              .HasForeignKey(socd => socd.ComponentProductId)
                              .OnDelete(DeleteBehavior.Restrict);

                        // 與 Unit 的關聯
                        entity.HasOne(socd => socd.Unit)
                              .WithMany()
                              .HasForeignKey(socd => socd.UnitId)
                              .OnDelete(DeleteBehavior.ClientSetNull);
                  });

                  // Financial Management Relationships
                  
                  // 統一沖款單配置
                  modelBuilder.Entity<SetoffDocument>(entity =>
                  {
                        entity.HasKey(sd => sd.Id);

                        // 公司關聯
                        entity.HasOne(sd => sd.Company)
                              .WithMany()
                              .HasForeignKey(sd => sd.CompanyId)
                              .OnDelete(DeleteBehavior.Restrict);

                        // decimal 屬性設定
                        entity.Property(e => e.TotalSetoffAmount)
                              .HasPrecision(18, 2);

                        // 設定唯一索引
                        entity.HasIndex(e => e.Code)
                              .IsUnique();

                        // 設定其他索引
                        entity.HasIndex(e => e.SetoffType);
                        entity.HasIndex(e => e.SetoffDate);
                        entity.HasIndex(e => e.RelatedPartyId);
                        entity.HasIndex(e => e.CompanyId);
                  });

                  // 統一沖款收款記錄配置
                  modelBuilder.Entity<SetoffPayment>(entity =>
                  {
                        entity.HasKey(sp => sp.Id);

                        // 沖款單關聯
                        entity.HasOne(sp => sp.SetoffDocument)
                              .WithMany(sd => sd.SetoffPayments)
                              .HasForeignKey(sp => sp.SetoffDocumentId)
                              .OnDelete(DeleteBehavior.Cascade);

                        // 銀行關聯（可選）
                        entity.HasOne(sp => sp.Bank)
                              .WithMany()
                              .HasForeignKey(sp => sp.BankId)
                              .OnDelete(DeleteBehavior.SetNull);

                        // 付款方式關聯（可選）
                        entity.HasOne(sp => sp.PaymentMethod)
                              .WithMany()
                              .HasForeignKey(sp => sp.PaymentMethodId)
                              .OnDelete(DeleteBehavior.SetNull);

                        // decimal 屬性設定
                        entity.Property(e => e.ReceivedAmount)
                              .HasPrecision(18, 2);
                        entity.Property(e => e.AllowanceAmount)
                              .HasPrecision(18, 2);

                        // 設定索引
                        entity.HasIndex(e => e.SetoffDocumentId);
                        entity.HasIndex(e => e.BankId);
                        entity.HasIndex(e => e.PaymentMethodId);
                  });

                  // 統一沖款商品明細配置
                  modelBuilder.Entity<SetoffProductDetail>(entity =>
                  {
                        entity.HasKey(spd => spd.Id);

                        // 沖款單關聯
                        entity.HasOne(spd => spd.SetoffDocument)
                              .WithMany(sd => sd.SetoffProductDetails)
                              .HasForeignKey(spd => spd.SetoffDocumentId)
                              .OnDelete(DeleteBehavior.Cascade);

                        // 商品關聯
                        entity.HasOne(spd => spd.Product)
                              .WithMany()
                              .HasForeignKey(spd => spd.ProductId)
                              .OnDelete(DeleteBehavior.Restrict);

                        // decimal 屬性設定
                        entity.Property(e => e.CurrentSetoffAmount)
                              .HasPrecision(18, 2);
                        entity.Property(e => e.TotalSetoffAmount)
                              .HasPrecision(18, 2);
                        entity.Property(e => e.CurrentAllowanceAmount)
                              .HasPrecision(18, 2);
                        entity.Property(e => e.TotalAllowanceAmount)
                              .HasPrecision(18, 2);

                        // 設定索引
                        entity.HasIndex(e => e.SetoffDocumentId);
                        entity.HasIndex(e => e.ProductId);
                        entity.HasIndex(e => new { e.SourceDetailType, e.SourceDetailId });
                  });
                  
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

                  modelBuilder.Entity<SetoffPrepayment>(entity =>
                  {
                        entity.HasKey(p => p.Id);

                        // 沖款單關聯（級聯刪除）
                        entity.HasOne(p => p.SetoffDocument)
                              .WithMany(sd => sd.Prepayments)
                              .HasForeignKey(p => p.SetoffDocumentId)
                              .OnDelete(DeleteBehavior.Cascade);

                        // 客戶關聯（預收款時使用）
                        entity.HasOne(p => p.Customer)
                              .WithMany()
                              .HasForeignKey(p => p.CustomerId)
                              .OnDelete(DeleteBehavior.Restrict);

                        // 供應商關聯（預付款時使用）
                        entity.HasOne(p => p.Supplier)
                              .WithMany()
                              .HasForeignKey(p => p.SupplierId)
                              .OnDelete(DeleteBehavior.Restrict);

                        // 為 decimal 屬性設定精確度和小數位數
                        entity.Property(e => e.Amount)
                              .HasPrecision(18, 2);
                        entity.Property(e => e.UsedAmount)
                              .HasPrecision(18, 2);

                        // 設定唯一索引
                        entity.HasIndex(e => e.SourceDocumentCode);

                        // 設定其他索引
                        entity.HasIndex(e => e.PrepaymentTypeId);
                        entity.HasIndex(e => e.CustomerId);
                        entity.HasIndex(e => e.SupplierId);
                        entity.HasIndex(e => e.SetoffDocumentId);
                  });

                  // 預收付款項使用記錄配置
                  modelBuilder.Entity<SetoffPrepaymentUsage>(entity =>
                  {
                        entity.HasKey(pu => pu.Id);

                        // 沖款單關聯 - 使用 Restrict 避免循環刪除
                        entity.HasOne(pu => pu.SetoffDocument)
                              .WithMany(sd => sd.PrepaymentUsages)
                              .HasForeignKey(pu => pu.SetoffDocumentId)
                              .OnDelete(DeleteBehavior.Restrict);

                        // 預收付款項關聯 - 使用 Cascade
                        entity.HasOne(pu => pu.SetoffPrepayment)
                              .WithMany(p => p.UsageRecords)
                              .HasForeignKey(pu => pu.SetoffPrepaymentId)
                              .OnDelete(DeleteBehavior.Cascade);

                        // 為 decimal 屬性設定精確度和小數位數
                        entity.Property(e => e.UsedAmount)
                              .HasPrecision(18, 2);

                        // 設定索引
                        entity.HasIndex(e => e.SetoffPrepaymentId);
                        entity.HasIndex(e => e.SetoffDocumentId);
                        entity.HasIndex(e => e.UsageDate);
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

                  // 公司資料相關
                  modelBuilder.Entity<Company>(entity =>
                  {
                        // 欄位對應
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();

                        // 為 decimal 屬性設定精確度和小數位數
                        entity.Property(e => e.CapitalAmount)
                              .HasPrecision(18, 2); // 總共18位數，小數點後2位
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

                  // Composition Category Management
                  modelBuilder.Entity<CompositionCategory>(entity =>
                  {
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();

                        // 設定索引
                        entity.HasIndex(e => e.Name);
                  });

                  // Product Composition (BOM) Management
                  modelBuilder.Entity<ProductComposition>(entity =>
                  {
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();

                        // 關聯設定
                        entity.HasOne(pc => pc.ParentProduct)
                              .WithMany(p => p.ProductCompositions)
                              .HasForeignKey(pc => pc.ParentProductId)
                              .OnDelete(DeleteBehavior.Restrict);

                        entity.HasOne(pc => pc.CompositionCategory)
                              .WithMany(cc => cc.ProductCompositions)
                              .HasForeignKey(pc => pc.CompositionCategoryId)
                              .OnDelete(DeleteBehavior.SetNull);

                        entity.HasMany(pc => pc.CompositionDetails)
                              .WithOne(pcd => pcd.ProductComposition)
                              .HasForeignKey(pcd => pcd.ProductCompositionId)
                              .OnDelete(DeleteBehavior.Cascade);
                  });

                  modelBuilder.Entity<ProductCompositionDetail>(entity =>
                  {
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();

                        // 關聯設定
                        entity.HasOne(pcd => pcd.ProductComposition)
                              .WithMany(pc => pc.CompositionDetails)
                              .HasForeignKey(pcd => pcd.ProductCompositionId)
                              .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(pcd => pcd.ComponentProduct)
                              .WithMany(p => p.ComponentInCompositions)
                              .HasForeignKey(pcd => pcd.ComponentProductId)
                              .OnDelete(DeleteBehavior.Restrict);

                        entity.HasOne(pcd => pcd.Unit)
                              .WithMany()
                              .HasForeignKey(pcd => pcd.UnitId)
                              .OnDelete(DeleteBehavior.SetNull);

                        // decimal 屬性設定
                        entity.Property(e => e.Quantity)
                              .HasPrecision(18, 4);
                        entity.Property(e => e.ComponentCost)
                              .HasPrecision(18, 2);
                  });

                  // Production Schedule Management
                  modelBuilder.Entity<ProductionSchedule>(entity =>
                  {
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();

                        // 關聯設定
                        entity.HasOne(ps => ps.CreatedByEmployee)
                              .WithMany()
                              .HasForeignKey(ps => ps.CreatedByEmployeeId)
                              .OnDelete(DeleteBehavior.SetNull);

                        entity.HasOne(ps => ps.Customer)
                              .WithMany()
                              .HasForeignKey(ps => ps.CustomerId)
                              .OnDelete(DeleteBehavior.SetNull);

                        entity.HasMany(ps => ps.ScheduleDetails)
                              .WithOne(psd => psd.ProductionSchedule)
                              .HasForeignKey(psd => psd.ProductionScheduleId)
                              .OnDelete(DeleteBehavior.Cascade);

                        // 設定索引
                        entity.HasIndex(e => e.Code)
                              .IsUnique();
                        entity.HasIndex(e => e.ScheduleDate);
                  });

                  modelBuilder.Entity<ProductionScheduleDetail>(entity =>
                  {
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();

                        // 關聯設定
                        entity.HasOne(psd => psd.ProductionSchedule)
                              .WithMany(ps => ps.ScheduleDetails)
                              .HasForeignKey(psd => psd.ProductionScheduleId)
                              .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(psd => psd.ComponentProduct)
                              .WithMany()
                              .HasForeignKey(psd => psd.ComponentProductId)
                              .OnDelete(DeleteBehavior.Restrict);

                        entity.HasOne(psd => psd.ProductCompositionDetail)
                              .WithMany()
                              .HasForeignKey(psd => psd.ProductCompositionDetailId)
                              .OnDelete(DeleteBehavior.SetNull);

                        entity.HasOne(psd => psd.Warehouse)
                              .WithMany()
                              .HasForeignKey(psd => psd.WarehouseId)
                              .OnDelete(DeleteBehavior.SetNull);

                        // decimal 屬性設定
                        entity.Property(e => e.RequiredQuantity)
                              .HasPrecision(18, 4);
                        entity.Property(e => e.EstimatedUnitCost)
                              .HasPrecision(18, 2);
                        entity.Property(e => e.ActualUnitCost)
                              .HasPrecision(18, 2);
                        entity.Property(e => e.TotalCost)
                              .HasPrecision(18, 2);

                        // 設定索引
                        entity.HasIndex(e => new { e.ProductionScheduleId, e.ComponentProductId });
                        entity.HasIndex(e => e.ComponentProductId);
                  });
            }
    }
}
