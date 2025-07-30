using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 產品類別資料種子類別
    /// </summary>
    public class ProductCategorySeeder : IDataSeeder
    {
        public int Order => 5;
        public string Name => "產品類別";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedProductCategoriesAsync(context);
        }

        /// <summary>
        /// 種子商品類別資料
        /// </summary>
        private static async Task SeedProductCategoriesAsync(AppDbContext context)
        {
            if (await context.ProductCategories.AnyAsync())
                return; // 資料已存在

            var categories = new[]
            {
                new ProductCategory
                {
                    CategoryCode = "PC001",
                    CategoryName = "電子產品",
                    Description = "電腦週邊、電子設備",
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
                    Description = "軟體授權及服務",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-30),
                    CreatedBy = "System"
                },
                new ProductCategory
                {
                    CategoryCode = "PC005",
                    CategoryName = "原物料",
                    Description = "生產原料材料",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-30),
                    CreatedBy = "System"
                }
            };

            await context.ProductCategories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
    }
}
