using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 產品資料種子類別
    /// </summary>
    public class ProductSeeder : IDataSeeder
    {
        public int Order => 8;
        public string Name => "產品資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedProductsAsync(context);
        }

        /// <summary>
        /// 種子商品資料
        /// </summary>
        private static async Task SeedProductsAsync(AppDbContext context)
        {
            if (await context.Products.AnyAsync())
                return; // 資料已存在

            // 取得商品類別
            var electronicsCategory = await context.ProductCategories.FirstOrDefaultAsync(pc => pc.CategoryCode == "PC001");
            var officeCategory = await context.ProductCategories.FirstOrDefaultAsync(pc => pc.CategoryCode == "PC002");
            var industrialCategory = await context.ProductCategories.FirstOrDefaultAsync(pc => pc.CategoryCode == "PC003");
            var softwareCategory = await context.ProductCategories.FirstOrDefaultAsync(pc => pc.CategoryCode == "PC004");
            var materialCategory = await context.ProductCategories.FirstOrDefaultAsync(pc => pc.CategoryCode == "PC005");

            // 取得供應商
            var supplier1 = await context.Suppliers.FirstOrDefaultAsync(s => s.SupplierCode == "S001");
            var supplier2 = await context.Suppliers.FirstOrDefaultAsync(s => s.SupplierCode == "S002");
            var supplier3 = await context.Suppliers.FirstOrDefaultAsync(s => s.SupplierCode == "S003");
            var supplier4 = await context.Suppliers.FirstOrDefaultAsync(s => s.SupplierCode == "S004");

            // 取得單位
            var unitPc = await context.Units.FirstOrDefaultAsync(u => u.UnitCode == "PC");     // 台
            var unitPcs = await context.Units.FirstOrDefaultAsync(u => u.UnitCode == "PCS");   // 個
            var unitPack = await context.Units.FirstOrDefaultAsync(u => u.UnitCode == "PACK"); // 包
            var unitPen = await context.Units.FirstOrDefaultAsync(u => u.UnitCode == "PEN");   // 支
            var unitTop = await context.Units.FirstOrDefaultAsync(u => u.UnitCode == "TOP");   // 頂
            var unitLic = await context.Units.FirstOrDefaultAsync(u => u.UnitCode == "LIC");   // 授權
            var unitSht = await context.Units.FirstOrDefaultAsync(u => u.UnitCode == "SHT");   // 張
            var unitBag = await context.Units.FirstOrDefaultAsync(u => u.UnitCode == "BAG");   // 袋
            var unitRol = await context.Units.FirstOrDefaultAsync(u => u.UnitCode == "ROL");   // 捲

            var products = new[]
            {
                // 電子產品
                new Product
                {
                    ProductCode = "P001",
                    ProductName = "筆記型電腦 - ThinkPad X1",
                    Description = "高效能商務筆記型電腦，Intel i7處理器，16GB記憶體，512GB SSD",
                    Specification = "14吋FHD IPS螢幕，重量1.13kg，電池續航19.5小時",
                    UnitId = unitPc?.Id,
                    UnitPrice = 45000,
                    CostPrice = 38000,
                    ProductCategoryId = electronicsCategory?.Id,
                    PrimarySupplierId = supplier1?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-25),
                    CreatedBy = "System"
                },
                // 辦公用品
                new Product
                {
                    ProductCode = "P002",
                    ProductName = "A4影印紙",
                    Description = "高品質A4白色影印紙，適用各種印表機",
                    Specification = "80gsm，500張包裝，FSC認證環保紙張",
                    UnitId = unitPack?.Id,
                    UnitPrice = 120,
                    CostPrice = 85,
                    ProductCategoryId = officeCategory?.Id,
                    PrimarySupplierId = supplier4?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-24),
                    CreatedBy = "System"
                },
                // 工業設備
                new Product
                {
                    ProductCode = "P003",
                    ProductName = "安全帽",
                    Description = "工業用安全帽，符合CNS標準",
                    Specification = "ABS材質，可調整帽圍，重量350g",
                    UnitId = unitTop?.Id,
                    UnitPrice = 450,
                    CostPrice = 320,
                    ProductCategoryId = industrialCategory?.Id,
                    PrimarySupplierId = supplier4?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-23),
                    CreatedBy = "System"
                },
                // 軟體授權
                new Product
                {
                    ProductCode = "P004",
                    ProductName = "Microsoft Office 365授權",
                    Description = "Microsoft Office 365商業年度授權",
                    Specification = "包含Word、Excel、PowerPoint、Outlook等應用程式",
                    UnitId = unitLic?.Id,
                    UnitPrice = 6800,
                    CostPrice = 5500,
                    ProductCategoryId = softwareCategory?.Id,
                    PrimarySupplierId = supplier3?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-22),
                    CreatedBy = "System"
                },
                // 原物料
                new Product
                {
                    ProductCode = "P005",
                    ProductName = "不鏽鋼板材",
                    Description = "304不鏽鋼板材，適用工業用途",
                    Specification = "厚度2mm，尺寸1000x2000mm，表面2B處理",
                    UnitId = unitSht?.Id,
                    UnitPrice = 3500,
                    CostPrice = 2800,
                    ProductCategoryId = materialCategory?.Id,
                    PrimarySupplierId = supplier4?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-21),
                    CreatedBy = "System"
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}
