using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    public class UnitSeeder : IDataSeeder
    {
        public int Order => 6; // 在基礎資料之後，庫存管理之前
        public string Name => "計量單位";

        public async Task SeedAsync(AppDbContext context)
        {
            // 檢查是否已經有單位資料
            bool hasData = await context.Units.AnyAsync();

            if (hasData)
            {
                return;
            }

            // 建立計量單位
            var units = new List<Unit>
            {
                new Unit
                {
                    UnitCode = "PCS",
                    UnitName = "個",
                    Symbol = "個",
                    IsBaseUnit = true,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "BOX",
                    UnitName = "箱",
                    Symbol = "箱",
                    IsBaseUnit = false,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "KG",
                    UnitName = "公斤",
                    Symbol = "kg",
                    IsBaseUnit = true,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "G",
                    UnitName = "公克",
                    Symbol = "g",
                    IsBaseUnit = false,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "PACK",
                    UnitName = "包",
                    Symbol = "包",
                    IsBaseUnit = false,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "M",
                    UnitName = "公尺",
                    Symbol = "m",
                    IsBaseUnit = true,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "CM",
                    UnitName = "公分",
                    Symbol = "cm",
                    IsBaseUnit = false,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "SQM",
                    UnitName = "平方公尺",
                    Symbol = "m²",
                    IsBaseUnit = true,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "L",
                    UnitName = "公升",
                    Symbol = "L",
                    IsBaseUnit = true,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "ML",
                    UnitName = "毫升",
                    Symbol = "ml",
                    IsBaseUnit = false,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                }
            };

            await context.Units.AddRangeAsync(units);
            await context.SaveChangesAsync();

            // 建立單位轉換關係
            var unitConversions = new List<UnitConversion>
            {
                new UnitConversion
                {
                    FromUnitId = units.First(u => u.UnitCode == "BOX").Id,
                    ToUnitId = units.First(u => u.UnitCode == "PCS").Id,
                    ConversionRate = 12, // 1箱 = 12個
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new UnitConversion
                {
                    FromUnitId = units.First(u => u.UnitCode == "KG").Id,
                    ToUnitId = units.First(u => u.UnitCode == "G").Id,
                    ConversionRate = 1000, // 1公斤 = 1000公克
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new UnitConversion
                {
                    FromUnitId = units.First(u => u.UnitCode == "PACK").Id,
                    ToUnitId = units.First(u => u.UnitCode == "PCS").Id,
                    ConversionRate = 10, // 1包 = 10個
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new UnitConversion
                {
                    FromUnitId = units.First(u => u.UnitCode == "M").Id,
                    ToUnitId = units.First(u => u.UnitCode == "CM").Id,
                    ConversionRate = 100, // 1公尺 = 100公分
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new UnitConversion
                {
                    FromUnitId = units.First(u => u.UnitCode == "L").Id,
                    ToUnitId = units.First(u => u.UnitCode == "ML").Id,
                    ConversionRate = 1000, // 1公升 = 1000毫升
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                }
            };

            await context.UnitConversions.AddRangeAsync(unitConversions);
            await context.SaveChangesAsync();

        }
    }
}
