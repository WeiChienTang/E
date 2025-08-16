using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 尺寸種子器
    /// </summary>
    public class SizeSeeder : IDataSeeder
    {
        public int Order => 5; // 在基礎資料之後，產品之前
        public string Name => "尺寸資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedSizesAsync(context);
        }

        /// <summary>
        /// 初始化尺寸資料
        /// </summary>
        private static async Task SeedSizesAsync(AppDbContext context)
        {
            // 檢查資料是否已存在
            if (await context.Sizes.AnyAsync())
                return;

            // 取得建立時間和建立者資訊
            var (createdAt1, createdBy) = SeedDataHelper.GetSystemCreateInfo(25);

            var sizes = new[]
            {
                new Size
                {
                    Code = "CUSTOM",
                    Name = "客製",
                    Remarks = "客製化尺寸",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt1,
                    CreatedBy = createdBy
                }
            };

            await context.Sizes.AddRangeAsync(sizes);
            await context.SaveChangesAsync();
        }
    }
}
