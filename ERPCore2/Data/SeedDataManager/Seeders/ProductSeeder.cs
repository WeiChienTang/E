using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 產品資料種子類別
    /// </summary>
    public class ProductSeeder : IDataSeeder
    {
        public int Order => 22;
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
            var electronicsCategory = await context.ProductCategories.FirstOrDefaultAsync(pc => pc.Code == "PC001");
            var officeCategory = await context.ProductCategories.FirstOrDefaultAsync(pc => pc.Code == "PC002");
            var industrialCategory = await context.ProductCategories.FirstOrDefaultAsync(pc => pc.Code == "PC003");
            var softwareCategory = await context.ProductCategories.FirstOrDefaultAsync(pc => pc.Code == "PC004");
            var materialCategory = await context.ProductCategories.FirstOrDefaultAsync(pc => pc.Code == "PC005");

            // 取得供應商
            var supplier1 = await context.Suppliers.FirstOrDefaultAsync(s => s.Code == "S001");
            var supplier2 = await context.Suppliers.FirstOrDefaultAsync(s => s.Code == "S002");
            var supplier3 = await context.Suppliers.FirstOrDefaultAsync(s => s.Code == "S003");
            var supplier4 = await context.Suppliers.FirstOrDefaultAsync(s => s.Code == "S004");

            // 取得單位
            var unitPc = await context.Units.FirstOrDefaultAsync(u => u.Code == "PC");     // 台
            var unitPcs = await context.Units.FirstOrDefaultAsync(u => u.Code == "PCS");   // 個
            var unitPack = await context.Units.FirstOrDefaultAsync(u => u.Code == "PACK"); // 包
            var unitPen = await context.Units.FirstOrDefaultAsync(u => u.Code == "PEN");   // 支
            var unitTop = await context.Units.FirstOrDefaultAsync(u => u.Code == "TOP");   // 頂
            var unitLic = await context.Units.FirstOrDefaultAsync(u => u.Code == "LIC");   // 授權
            var unitSht = await context.Units.FirstOrDefaultAsync(u => u.Code == "SHT");   // 張
            var unitBag = await context.Units.FirstOrDefaultAsync(u => u.Code == "BAG");   // 袋
            var unitRol = await context.Units.FirstOrDefaultAsync(u => u.Code == "ROL");   // 捲

            var products = new[]
            {
                new Product
                {
                    Code = "P003",
                    Name = "安全帽",
                    UnitId = unitTop?.Id,
                    ProductCategoryId = industrialCategory?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-23),
                    CreatedBy = "System"
                },
                // 原物料
                new Product
                {
                    Code = "P005",
                    Name = "不鏽鋼板材",
                    UnitId = unitSht?.Id,
                    ProductCategoryId = materialCategory?.Id,
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
