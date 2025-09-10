using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    public class PurchaseReturnSeeder : IDataSeeder
    {
        public int Order => 26; // 在採購訂單和進貨後執行
        public string Name => "採購退回測試資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedPurchaseReturnsAsync(context);
        }

        private static async Task SeedPurchaseReturnsAsync(AppDbContext context)
        {
            if (await context.PurchaseReturns.AnyAsync()) return;

            var (createdAt1, createdBy) = SeedDataHelper.GetSystemCreateInfo(10);
            var (createdAt2, _) = SeedDataHelper.GetSystemCreateInfo(5);
            var (createdAt3, _) = SeedDataHelper.GetSystemCreateInfo(2);

            // 取得現有的採購訂單和供應商
            var purchaseOrder = await context.PurchaseOrders.FirstOrDefaultAsync();
            var supplier = await context.Suppliers.FirstOrDefaultAsync();
            var product = await context.Products.FirstOrDefaultAsync();
            var warehouse = await context.Warehouses.FirstOrDefaultAsync();
            var employee = await context.Employees.FirstOrDefaultAsync();

            if (purchaseOrder == null || supplier == null || product == null) return;

            var purchaseReturns = new[]
            {
                new PurchaseReturn
                {
                    PurchaseReturnNumber = "PR20250101",
                    ReturnDate = DateTime.Today.AddDays(-10),
                    SupplierId = supplier.Id,
                    PurchaseOrderId = purchaseOrder.Id,
                    ProcessPersonnel = "品管部",
                    TotalReturnAmount = 5000.00m,
                    ReturnTaxAmount = 250.00m,
                    TotalReturnAmountWithTax = 5250.00m,
                    IsRefunded = true,
                    RefundDate = DateTime.Today.AddDays(-3),
                    RefundAmount = 5250.00m,
                    ActualProcessDate = DateTime.Today.AddDays(-5),
                    WarehouseId = warehouse?.Id,
                    EmployeeId = employee?.Id,
                    ConfirmedBy = employee?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt1,
                    CreatedBy = createdBy
                },
                new PurchaseReturn
                {
                    PurchaseReturnNumber = "PR20250102",
                    ReturnDate = DateTime.Today.AddDays(-5),
                    SupplierId = supplier.Id,
                    PurchaseOrderId = purchaseOrder.Id,
                    ProcessPersonnel = "採購部",
                    TotalReturnAmount = 3000.00m,
                    ReturnTaxAmount = 150.00m,
                    TotalReturnAmountWithTax = 3150.00m,
                    IsRefunded = false,
                    ExpectedProcessDate = DateTime.Today.AddDays(2),
                    WarehouseId = warehouse?.Id,
                    EmployeeId = employee?.Id,
                    ConfirmedBy = employee?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt2,
                    CreatedBy = createdBy
                },
                new PurchaseReturn
                {
                    PurchaseReturnNumber = "PR20250103",
                    ReturnDate = DateTime.Today.AddDays(-2),
                    SupplierId = supplier.Id,
                    ProcessPersonnel = "倉庫管理員",
                    TotalReturnAmount = 1500.00m,
                    ReturnTaxAmount = 75.00m,
                    TotalReturnAmountWithTax = 1575.00m,
                    IsRefunded = false,
                    ExpectedProcessDate = DateTime.Today.AddDays(5),
                    WarehouseId = warehouse?.Id,
                    EmployeeId = employee?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt3,
                    CreatedBy = createdBy
                }
            };

            await context.PurchaseReturns.AddRangeAsync(purchaseReturns);
            await context.SaveChangesAsync();

            // 添加退回明細
            await SeedPurchaseReturnDetailsAsync(context, purchaseReturns, product);
        }

        private static async Task SeedPurchaseReturnDetailsAsync(AppDbContext context, PurchaseReturn[] purchaseReturns, Product product)
        {
            var (createdAt1, createdBy) = SeedDataHelper.GetSystemCreateInfo(10);
            var (createdAt2, _) = SeedDataHelper.GetSystemCreateInfo(5);

            var unit = await context.Units.FirstOrDefaultAsync();
            var warehouseLocation = await context.WarehouseLocations.FirstOrDefaultAsync();

            var details = new List<PurchaseReturnDetail>();

            // 第一筆退回單的明細（已完成）
            details.Add(new PurchaseReturnDetail
            {
                PurchaseReturnId = purchaseReturns[0].Id,
                ProductId = product.Id,
                ReturnQuantity = 50,
                OriginalUnitPrice = 100.00m,
                ReturnUnitPrice = 100.00m,
                ProcessedQuantity = 50,
                ShippedQuantity = 50,
                IsShipped = true,
                ShippedDate = DateTime.Today.AddDays(-5),
                BatchNumber = "BATCH001",
                QualityCondition = "品質不良，無法使用",
                UnitId = unit?.Id,
                WarehouseLocationId = warehouseLocation?.Id,
                Status = EntityStatus.Active,
                CreatedAt = createdAt1,
                CreatedBy = createdBy
            });

            // 第二筆退回單的明細（處理中）
            details.Add(new PurchaseReturnDetail
            {
                PurchaseReturnId = purchaseReturns[1].Id,
                ProductId = product.Id,
                ReturnQuantity = 30,
                OriginalUnitPrice = 100.00m,
                ReturnUnitPrice = 100.00m,
                ProcessedQuantity = 0,
                ShippedQuantity = 0,
                IsShipped = false,
                BatchNumber = "BATCH002",
                QualityCondition = "規格不符",
                UnitId = unit?.Id,
                WarehouseLocationId = warehouseLocation?.Id,
                Status = EntityStatus.Active,
                CreatedAt = createdAt2,
                CreatedBy = createdBy
            });

            // 第三筆退回單的明細（待處理）
            details.Add(new PurchaseReturnDetail
            {
                PurchaseReturnId = purchaseReturns[2].Id,
                ProductId = product.Id,
                ReturnQuantity = 15,
                OriginalUnitPrice = 100.00m,
                ReturnUnitPrice = 100.00m,
                ProcessedQuantity = 0,
                ShippedQuantity = 0,
                IsShipped = false,
                BatchNumber = "BATCH003",
                QualityCondition = "數量錯誤",
                UnitId = unit?.Id,
                WarehouseLocationId = warehouseLocation?.Id,
                Status = EntityStatus.Active,
                CreatedAt = createdAt2,
                CreatedBy = createdBy
            });

            await context.PurchaseReturnDetails.AddRangeAsync(details);
            await context.SaveChangesAsync();
        }
    }
}
