using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 顏色資料種子器
    /// </summary>
    public class ColorSeeder : IDataSeeder
    {
        public int Order => 10; // 在天氣資料之後執行
        public string Name => "顏色資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedColorsAsync(context);
        }

        /// <summary>
        /// 初始化顏色資料
        /// </summary>
        private static async Task SeedColorsAsync(AppDbContext context)
        {
            // 檢查資料是否已存在
            if (await context.Colors.AnyAsync())
                return;

            // 取得建立時間和建立者資訊
            var (createdAt1, createdBy) = SeedDataHelper.GetSystemCreateInfo(30);

            var colors = new[]
            {
                new Color
                {
                    Name = "紅色",
                    Code = "RED",
                    Description = "鮮豔的紅色",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt1,
                    CreatedBy = createdBy
                }                
            };

            await context.Colors.AddRangeAsync(colors);
            await context.SaveChangesAsync();
        }
    }
}
