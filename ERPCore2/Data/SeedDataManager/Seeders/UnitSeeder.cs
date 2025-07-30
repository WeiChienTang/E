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
                // 基本計量單位  
                new Unit
                {
                    UnitCode = "PCS",
                    UnitName = "個",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "KG",
                    UnitName = "公斤",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "PACK",
                    UnitName = "包",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "M",
                    UnitName = "公尺",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "CM",
                    UnitName = "公分",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "SQM",
                    UnitName = "平方公尺",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },

                // 產品相關的額外單位
                new Unit
                {
                    UnitCode = "PC",
                    UnitName = "台",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-31),
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "PEN",
                    UnitName = "支",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-31),
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "BAG",
                    UnitName = "袋",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-31),
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "ROL",
                    UnitName = "捲",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-31),
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "TOP",
                    UnitName = "頂",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-31),
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "LIC",
                    UnitName = "授權",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-31),
                    CreatedBy = "System"
                },
                new Unit
                {
                    UnitCode = "SHT",
                    UnitName = "張",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-31),
                    CreatedBy = "System"
                }
            };

            await context.Units.AddRangeAsync(units);
            await context.SaveChangesAsync();
        }
    }
}
