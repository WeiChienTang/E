using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 合成表類型種子器
    /// </summary>
    public class CompositionCategorySeeder : IDataSeeder
    {
        public int Order => 14; // 在基礎資料之後、商品相關資料之前執行
        public string Name => "合成表類型資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedCompositionCategoriesAsync(context);
        }

        /// <summary>
        /// 初始化合成表類型資料
        /// </summary>
        private static async Task SeedCompositionCategoriesAsync(AppDbContext context)
        {
            // 檢查資料是否已存在
            if (await context.CompositionCategories.AnyAsync())
                return;

            // 取得建立時間和建立者資訊
            var (createdAt1, createdBy) = SeedDataHelper.GetSystemCreateInfo(30);
            var (createdAt2, _) = SeedDataHelper.GetSystemCreateInfo(25);
            var (createdAt3, _) = SeedDataHelper.GetSystemCreateInfo(20);
            var (createdAt4, _) = SeedDataHelper.GetSystemCreateInfo(15);

            var categories = new[]
            {
                new CompositionCategory
                {
                    Code = "STD",
                    Name = "標準配方",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt1,
                    CreatedBy = createdBy
                },
                new CompositionCategory
                {
                    Code = "ALT",
                    Name = "替代配方",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt2,
                    CreatedBy = createdBy
                },
                new CompositionCategory
                {
                    Code = "SIM",
                    Name = "簡化配方",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt3,
                    CreatedBy = createdBy
                },
                new CompositionCategory
                {
                    Code = "CUST",
                    Name = "客製配方",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt4,
                    CreatedBy = createdBy
                }
            };

            await context.CompositionCategories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
    }
}
