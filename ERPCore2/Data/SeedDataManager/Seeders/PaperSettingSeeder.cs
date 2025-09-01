using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 紙張設定資料種子器
    /// </summary>
    public class PaperSettingSeeder : IDataSeeder
    {
        public int Order => 7; // 在基礎資料之後執行
        public string Name => "紙張設定資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedPaperSettingsAsync(context);
        }

        private static async Task SeedPaperSettingsAsync(AppDbContext context)
        {
            if (await context.PaperSettings.AnyAsync()) return;

            var (createdAt1, createdBy) = SeedDataHelper.GetSystemCreateInfo(30);

            var paperSettings = new[]
            {
                new PaperSetting
                {
                    Code = "A4",
                    Name = "中一刀",
                    PaperType = "中一刀",
                    Width = 21.49m,
                    Height = 14m,
                    TopMargin = 1,
                    BottomMargin = 1,
                    LeftMargin = 1,
                    RightMargin = 1,
                    Orientation = "Portrait",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt1,
                    CreatedBy = createdBy,
                    Remarks = "標準中一刀紙張設定，適用於一般報表"
                },
            };

            await context.PaperSettings.AddRangeAsync(paperSettings);
            await context.SaveChangesAsync();
        }
    }
}
