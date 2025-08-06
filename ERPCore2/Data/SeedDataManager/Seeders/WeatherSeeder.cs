using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 天氣資料種子器
    /// </summary>
    public class WeatherSeeder : IDataSeeder
    {
        public int Order => 9; // 在基礎資料之後執行
        public string Name => "天氣資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedWeathersAsync(context);
        }

        /// <summary>
        /// 初始化天氣資料
        /// </summary>
        private static async Task SeedWeathersAsync(AppDbContext context)
        {
            // 檢查資料是否已存在
            if (await context.Weathers.AnyAsync())
                return;

            // 取得建立時間和建立者資訊
            var (createdAt1, createdBy) = SeedDataHelper.GetSystemCreateInfo(30);
            var (createdAt2, _) = SeedDataHelper.GetSystemCreateInfo(25);
            var (createdAt3, _) = SeedDataHelper.GetSystemCreateInfo(20);

            var weathers = new[]
            {
                new Weather
                {
                    Name = "晴天",
                    Code = "SUNNY",
                    Description = "陽光充足，萬里無雲",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt1,
                    CreatedBy = createdBy
                },
                new Weather
                {
                    Name = "陰天",
                    Code = "OVERCAST",
                    Description = "天空完全被雲層覆蓋",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt2,
                    CreatedBy = createdBy
                },
                new Weather
                {
                    Name = "雨天",
                    Code = "RAINY",
                    Description = "降雨天氣，濕度較高",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt3,
                    CreatedBy = createdBy
                }
            };

            await context.Weathers.AddRangeAsync(weathers);
            await context.SaveChangesAsync();
        }
    }
}
