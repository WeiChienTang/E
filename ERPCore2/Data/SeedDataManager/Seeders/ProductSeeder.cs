using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 產品資料種子器
    /// </summary>
    public class ProductSeeder : IDataSeeder
    {
        public int Order => 7;
        public string Name => "產品資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedProductCategoriesAsync(context);
            await SeedUnitsAsync(context);
            await SeedProductsAsync(context);
        }

        /// <summary>
        /// 初始化商品分類資料
        /// </summary>
        private static async Task SeedProductCategoriesAsync(AppDbContext context)
        {
            if (await context.ProductCategories.AnyAsync())
                return; // 商品分類資料已存在

            var categories = new[]
            {
                new ProductCategory
                {
                    CategoryCode = "PC001",
                    CategoryName = "電子產品",
                    Description = "各類電子產品及配件",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-30),
                    CreatedBy = "System"
                },
                new ProductCategory
                {
                    CategoryCode = "PC002",
                    CategoryName = "辦公用品",
                    Description = "辦公室文具用品",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-30),
                    CreatedBy = "System"
                },
                new ProductCategory
                {
                    CategoryCode = "PC003",
                    CategoryName = "工業設備",
                    Description = "工業生產設備及工具",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-30),
                    CreatedBy = "System"
                },
                new ProductCategory
                {
                    CategoryCode = "PC004",
                    CategoryName = "軟體授權",
                    Description = "軟體產品及授權",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-30),
                    CreatedBy = "System"
                },
                new ProductCategory
                {
                    CategoryCode = "PC005",
                    CategoryName = "原物料",
                    Description = "生產用原物料",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-30),
                    CreatedBy = "System"
                }
            };

            await context.ProductCategories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// 初始化單位資料
        /// </summary>
        private static async Task SeedUnitsAsync(AppDbContext context)
        {
            if (await context.Units.AnyAsync())
                return; // 單位資料已存在

            var units = new[]
            {
                new Unit
                {
                    UnitCode = "PC",
                    UnitName = "台",
                    Symbol = "台",
                    IsBaseUnit = true,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-31),
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "PCS",
                    UnitName = "個",
                    Symbol = "個",
                    IsBaseUnit = true,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-31),
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "PKG",
                    UnitName = "包",
                    Symbol = "包",
                    IsBaseUnit = true,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-31),
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "PEN",
                    UnitName = "支",
                    Symbol = "支",
                    IsBaseUnit = true,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-31),
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "TOP",
                    UnitName = "頂",
                    Symbol = "頂",
                    IsBaseUnit = true,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-31),
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "LIC",
                    UnitName = "授權",
                    Symbol = "授權",
                    IsBaseUnit = true,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-31),
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "SHT",
                    UnitName = "片",
                    Symbol = "片",
                    IsBaseUnit = true,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-31),
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "BAG",
                    UnitName = "袋",
                    Symbol = "袋",
                    IsBaseUnit = true,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-31),
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "ROL",
                    UnitName = "捲",
                    Symbol = "捲",
                    IsBaseUnit = true,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-31),
                    CreatedBy = "System"
                }
            };

            await context.Units.AddRangeAsync(units);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// 初始化商品資料
        /// </summary>
        private static async Task SeedProductsAsync(AppDbContext context)
        {
            if (await context.Products.AnyAsync())
                return; // 商品資料已存在

            // 取得商品分類
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
            var unitPkg = await context.Units.FirstOrDefaultAsync(u => u.UnitCode == "PKG");   // 包
            var unitPen = await context.Units.FirstOrDefaultAsync(u => u.UnitCode == "PEN");   // 支
            var unitTop = await context.Units.FirstOrDefaultAsync(u => u.UnitCode == "TOP");   // 頂
            var unitLic = await context.Units.FirstOrDefaultAsync(u => u.UnitCode == "LIC");   // 授權
            var unitSht = await context.Units.FirstOrDefaultAsync(u => u.UnitCode == "SHT");   // 片
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
                    Specification = "14吋 FHD IPS螢幕，重量1.13kg，電池續航力19.5小時",
                    UnitId = unitPc?.Id,
                    UnitPrice = 45000,
                    CostPrice = 38000,
                    MinStockLevel = 5,
                    MaxStockLevel = 50,
                    CurrentStock = 25,
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
                    Specification = "1600 DPI，6個按鍵，節能設計",
                    UnitId = unitPcs?.Id,
                    UnitPrice = 850,
                    CostPrice = 600,
                    MinStockLevel = 20,
                    MaxStockLevel = 200,
                    CurrentStock = 150,
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
                    Description = "24吋 Full HD LED背光液晶顯示器",
                    Specification = "1920x1080解析度，IPS面板，HDMI/VGA介面",
                    UnitId = unitPc?.Id,
                    UnitPrice = 8500,
                    CostPrice = 7200,
                    MinStockLevel = 10,
                    MaxStockLevel = 80,
                    CurrentStock = 35,
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
                    Description = "高品質A4白色影印紙，適合各種印表機",
                    Specification = "80gsm，500張/包，FSC認證環保紙張",
                    UnitId = unitPkg?.Id,
                    UnitPrice = 120,
                    CostPrice = 85,
                    MinStockLevel = 100,
                    MaxStockLevel = 1000,
                    CurrentStock = 500,
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
                    Description = "藍色原子筆，書寫流暢不斷水",
                    Specification = "0.7mm筆芯，人體工學握把設計",
                    UnitId = unitPen?.Id,
                    UnitPrice = 25,
                    CostPrice = 15,
                    MinStockLevel = 200,
                    MaxStockLevel = 2000,
                    CurrentStock = 1200,
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
                    Description = "重型工業電鑽，適合各種材質鑽孔作業",
                    Specification = "功率1200W，無級變速，附鑽頭組",
                    UnitId = unitPc?.Id,
                    UnitPrice = 15000,
                    CostPrice = 12000,
                    MinStockLevel = 3,
                    MaxStockLevel = 30,
                    CurrentStock = 12,
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
                    Specification = "ABS材質，可調節帽圍，重量350g",
                    UnitId = unitTop?.Id,
                    UnitPrice = 450,
                    CostPrice = 320,
                    MinStockLevel = 50,
                    MaxStockLevel = 500,
                    CurrentStock = 280,
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
                    ProductName = "Microsoft Office 365商務版",
                    Description = "Microsoft Office 365商務版年度授權",
                    Specification = "包含Word、Excel、PowerPoint、Outlook等應用程式",
                    UnitId = unitLic?.Id,
                    UnitPrice = 6800,
                    CostPrice = 5500,
                    MinStockLevel = 10,
                    MaxStockLevel = 100,
                    CurrentStock = 45,
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
                    Specification = "支援Windows/Mac，即時防護，雲端管理",
                    UnitId = unitLic?.Id,
                    UnitPrice = 1200,
                    CostPrice = 950,
                    MinStockLevel = 20,
                    MaxStockLevel = 200,
                    CurrentStock = 85,
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
                    Description = "304不鏽鋼板材，工業用途",
                    Specification = "厚度2mm，尺寸1000x2000mm，表面2B處理",
                    UnitId = unitSht?.Id,
                    UnitPrice = 3500,
                    CostPrice = 2800,
                    MinStockLevel = 20,
                    MaxStockLevel = 200,
                    CurrentStock = 95,
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
                    Specification = "高光澤，耐衝擊，包裝25kg/袋",
                    UnitId = unitBag?.Id,
                    UnitPrice = 2200,
                    CostPrice = 1800,
                    MinStockLevel = 50,
                    MaxStockLevel = 500,
                    CurrentStock = 320,
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
                    Description = "強力工業用膠帶，多用途黏貼",
                    Specification = "寬度50mm，長度25m，耐溫-40°C到+80°C",
                    UnitId = unitRol?.Id,
                    UnitPrice = 180,
                    CostPrice = 130,
                    MinStockLevel = 100,
                    MaxStockLevel = 1000,
                    CurrentStock = 650,
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