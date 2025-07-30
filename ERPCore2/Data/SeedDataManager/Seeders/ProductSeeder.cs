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
                    IsActive = true,
                    ProductCategoryId = electronicsCategory?.Id,
                    PrimarySupplierId = supplier1?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-25),
                    CreatedBy = "System"
                },
                new Product
                {
                    ProductCode = "P002",
                    ProductName = "無線滑鼠",
                    Description = "人體工學設計無線滑鼠，2.4GHz無線連接",
                    Specification = "1600 DPI，可調式設計",
                    UnitId = unitPcs?.Id,
                    UnitPrice = 850,
                    CostPrice = 600,
                    IsActive = true,
                    ProductCategoryId = electronicsCategory?.Id,
                    PrimarySupplierId = supplier2?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-24),
                    CreatedBy = "System"
                },
                new Product
                {
                    ProductCode = "P003",
                    ProductName = "24吋顯示器",
                    Description = "24吋Full HD LED背光液晶顯示器",
                    Specification = "1920x1080解析度，IPS面板，HDMI/VGA介面",
                    UnitId = unitPc?.Id,
                    UnitPrice = 8500,
                    CostPrice = 7200,
                    IsActive = true,
                    ProductCategoryId = electronicsCategory?.Id,
                    PrimarySupplierId = supplier1?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-23),
                    CreatedBy = "System"
                },
                // 辦公用品
                new Product
                {
                    ProductCode = "P004",
                    ProductName = "A4影印紙",
                    Description = "高品質A4白色影印紙，適用各種印表機",
                    Specification = "80gsm，500張包裝，FSC認證環保紙張",
                    UnitId = unitPack?.Id,
                    UnitPrice = 120,
                    CostPrice = 85,
                    IsActive = true,
                    ProductCategoryId = officeCategory?.Id,
                    PrimarySupplierId = supplier4?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-22),
                    CreatedBy = "System"
                },
                new Product
                {
                    ProductCode = "P005",
                    ProductName = "原子筆",
                    Description = "藍色原子筆，書寫流暢不斷墨",
                    Specification = "0.7mm筆芯，人體工學握桿設計",
                    UnitId = unitPen?.Id,
                    UnitPrice = 25,
                    CostPrice = 15,
                    IsActive = true,
                    ProductCategoryId = officeCategory?.Id,
                    PrimarySupplierId = supplier4?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-21),
                    CreatedBy = "System"
                },
                // 工業設備
                new Product
                {
                    ProductCode = "P006",
                    ProductName = "工業級電鑽",
                    Description = "重型工業電鑽，適用各種材質鑽孔作業",
                    Specification = "功率1200W，無級變速鑽夾頭",
                    UnitId = unitPc?.Id,
                    UnitPrice = 15000,
                    CostPrice = 12000,
                    IsActive = true,
                    ProductCategoryId = industrialCategory?.Id,
                    PrimarySupplierId = supplier1?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-20),
                    CreatedBy = "System"
                },
                new Product
                {
                    ProductCode = "P007",
                    ProductName = "安全帽",
                    Description = "工業用安全帽，符合CNS標準",
                    Specification = "ABS材質，可調整帽圍，重量350g",
                    UnitId = unitTop?.Id,
                    UnitPrice = 450,
                    CostPrice = 320,
                    IsActive = true,
                    ProductCategoryId = industrialCategory?.Id,
                    PrimarySupplierId = supplier4?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-19),
                    CreatedBy = "System"
                },
                // 軟體授權
                new Product
                {
                    ProductCode = "P008",
                    ProductName = "Microsoft Office 365授權",
                    Description = "Microsoft Office 365商業年度授權",
                    Specification = "包含Word、Excel、PowerPoint、Outlook等應用程式",
                    UnitId = unitLic?.Id,
                    UnitPrice = 6800,
                    CostPrice = 5500,
                    IsActive = true,
                    ProductCategoryId = softwareCategory?.Id,
                    PrimarySupplierId = supplier3?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-18),
                    CreatedBy = "System"
                },
                new Product
                {
                    ProductCode = "P009",
                    ProductName = "防毒軟體企業版",
                    Description = "企業級防毒軟體年度授權",
                    Specification = "支援Windows/Mac，即時防護及端點管理",
                    UnitId = unitLic?.Id,
                    UnitPrice = 1200,
                    CostPrice = 950,
                    IsActive = true,
                    ProductCategoryId = softwareCategory?.Id,
                    PrimarySupplierId = supplier3?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-17),
                    CreatedBy = "System"
                },
                // 原物料
                new Product
                {
                    ProductCode = "P010",
                    ProductName = "不鏽鋼板材",
                    Description = "304不鏽鋼板材，適用工業用途",
                    Specification = "厚度2mm，尺寸1000x2000mm，表面2B處理",
                    UnitId = unitSht?.Id,
                    UnitPrice = 3500,
                    CostPrice = 2800,
                    IsActive = true,
                    ProductCategoryId = materialCategory?.Id,
                    PrimarySupplierId = supplier4?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-16),
                    CreatedBy = "System"
                },
                new Product
                {
                    ProductCode = "P011",
                    ProductName = "塑膠粒料",
                    Description = "ABS塑膠粒料，射出成型用",
                    Specification = "高光澤，優良耐衝擊性，25kg/袋",
                    UnitId = unitBag?.Id,
                    UnitPrice = 2200,
                    CostPrice = 1800,
                    IsActive = true,
                    ProductCategoryId = materialCategory?.Id,
                    PrimarySupplierId = supplier4?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-15),
                    CreatedBy = "System"
                },
                new Product
                {
                    ProductCode = "P012",
                    ProductName = "工業膠帶",
                    Description = "強力工業級膠帶，多用途黏著",
                    Specification = "寬度50mm，長度25m，耐溫-40°C至80°C",
                    UnitId = unitRol?.Id,
                    UnitPrice = 180,
                    CostPrice = 130,
                    IsActive = true,
                    ProductCategoryId = materialCategory?.Id,
                    PrimarySupplierId = supplier4?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-14),
                    CreatedBy = "System"
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}
