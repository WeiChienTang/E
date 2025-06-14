using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 顏色資料種子器
    /// </summary>
    public class ColorSeeder : IDataSeeder
    {
        public int Order => 8; // 在天氣資料之後執行
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
            var (createdAt2, _) = SeedDataHelper.GetSystemCreateInfo(28);
            var (createdAt3, _) = SeedDataHelper.GetSystemCreateInfo(26);
            var (createdAt4, _) = SeedDataHelper.GetSystemCreateInfo(24);
            var (createdAt5, _) = SeedDataHelper.GetSystemCreateInfo(22);
            var (createdAt6, _) = SeedDataHelper.GetSystemCreateInfo(20);
            var (createdAt7, _) = SeedDataHelper.GetSystemCreateInfo(18);
            var (createdAt8, _) = SeedDataHelper.GetSystemCreateInfo(16);
            var (createdAt9, _) = SeedDataHelper.GetSystemCreateInfo(14);
            var (createdAt10, _) = SeedDataHelper.GetSystemCreateInfo(12);

            var colors = new[]
            {
                new Color
                {
                    Name = "紅色",
                    Code = "RED",
                    Description = "鮮豔的紅色",
                    HexCode = "#FF0000",
                    RedValue = 255,
                    GreenValue = 0,
                    BlueValue = 0,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt1,
                    CreatedBy = createdBy
                },
                new Color
                {
                    Name = "綠色",
                    Code = "GREEN",
                    Description = "自然的綠色",
                    HexCode = "#00FF00",
                    RedValue = 0,
                    GreenValue = 255,
                    BlueValue = 0,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt2,
                    CreatedBy = createdBy
                },
                new Color
                {
                    Name = "藍色",
                    Code = "BLUE",
                    Description = "純淨的藍色",
                    HexCode = "#0000FF",
                    RedValue = 0,
                    GreenValue = 0,
                    BlueValue = 255,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt3,
                    CreatedBy = createdBy
                },
                new Color
                {
                    Name = "黃色",
                    Code = "YELLOW",
                    Description = "明亮的黃色",
                    HexCode = "#FFFF00",
                    RedValue = 255,
                    GreenValue = 255,
                    BlueValue = 0,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt4,
                    CreatedBy = createdBy
                },
                new Color
                {
                    Name = "紫色",
                    Code = "PURPLE",
                    Description = "優雅的紫色",
                    HexCode = "#800080",
                    RedValue = 128,
                    GreenValue = 0,
                    BlueValue = 128,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt5,
                    CreatedBy = createdBy
                },
                new Color
                {
                    Name = "橙色",
                    Code = "ORANGE",
                    Description = "溫暖的橙色",
                    HexCode = "#FFA500",
                    RedValue = 255,
                    GreenValue = 165,
                    BlueValue = 0,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt6,
                    CreatedBy = createdBy
                },
                new Color
                {
                    Name = "粉紅色",
                    Code = "PINK",
                    Description = "柔和的粉紅色",
                    HexCode = "#FFC0CB",
                    RedValue = 255,
                    GreenValue = 192,
                    BlueValue = 203,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt7,
                    CreatedBy = createdBy
                },
                new Color
                {
                    Name = "黑色",
                    Code = "BLACK",
                    Description = "深沉的黑色",
                    HexCode = "#000000",
                    RedValue = 0,
                    GreenValue = 0,
                    BlueValue = 0,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt8,
                    CreatedBy = createdBy
                },
                new Color
                {
                    Name = "白色",
                    Code = "WHITE",
                    Description = "純潔的白色",
                    HexCode = "#FFFFFF",
                    RedValue = 255,
                    GreenValue = 255,
                    BlueValue = 255,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt9,
                    CreatedBy = createdBy
                },
                new Color
                {
                    Name = "灰色",
                    Code = "GRAY",
                    Description = "中性的灰色",
                    HexCode = "#808080",
                    RedValue = 128,
                    GreenValue = 128,
                    BlueValue = 128,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt10,
                    CreatedBy = createdBy
                }
            };

            await context.Colors.AddRangeAsync(colors);
            await context.SaveChangesAsync();
        }
    }
}
