using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 單位轉換關係種子類別
    /// </summary>
    public class UnitConversionSeeder : IDataSeeder
    {
        public int Order => 7; // 在 UnitSeeder 之後
        public string Name => "單位轉換關係";

        public async Task SeedAsync(AppDbContext context)
        {
            // 檢查是否已經有單位轉換資料
            bool hasData = await context.UnitConversions.AnyAsync();

            if (hasData)
            {
                return;
            }

            // 取得已建立的單位
            var packUnit = await context.Units.FirstOrDefaultAsync(u => u.UnitCode == "PACK");
            var pcsUnit = await context.Units.FirstOrDefaultAsync(u => u.UnitCode == "PCS");
            var meterUnit = await context.Units.FirstOrDefaultAsync(u => u.UnitCode == "M");
            var cmUnit = await context.Units.FirstOrDefaultAsync(u => u.UnitCode == "CM");

            // 建立單位轉換關係
            var unitConversions = new List<UnitConversion>();

            // 只有當對應的單位存在時才建立轉換關係
            if (packUnit != null && pcsUnit != null)
            {
                unitConversions.Add(new UnitConversion
                {
                    FromUnitId = packUnit.Id,
                    ToUnitId = pcsUnit.Id,
                    ConversionRate = 10, // 1包 = 10個
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                });
            }

            if (meterUnit != null && cmUnit != null)
            {
                unitConversions.Add(new UnitConversion
                {
                    FromUnitId = meterUnit.Id,
                    ToUnitId = cmUnit.Id,
                    ConversionRate = 100, // 1公尺 = 100公分
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                });
            }

            if (unitConversions.Any())
            {
                await context.UnitConversions.AddRangeAsync(unitConversions);
                await context.SaveChangesAsync();
            }
        }
    }
}
