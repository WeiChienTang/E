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
                    Code = "PCS",
                    Name = "個",
                    EnglishName = "Pieces",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "KG",
                    Name = "公斤",
                    EnglishName = "Kilogram",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "PACK",
                    Name = "包",
                    EnglishName = "Pack",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "M",
                    Name = "公尺",
                    EnglishName = "Meter",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "CM",
                    Name = "公分",
                    EnglishName = "Centimeter",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "SQM",
                    Name = "平方公尺",
                    EnglishName = "Square Meter",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },

                // 商品相關的額外單位
                new Unit
                {
                    Code = "PC",
                    Name = "台",
                    EnglishName = "Piece",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "PEN",
                    Name = "支",
                    EnglishName = "Pen",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "BAG",
                    Name = "袋",
                    EnglishName = "Bag",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "ROL",
                    Name = "捲",
                    EnglishName = "Roll",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "BOX",
                    Name = "箱",
                    EnglishName = "Box",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "DOZ",
                    Name = "打",
                    EnglishName = "Dozen",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "SET",
                    Name = "組",
                    EnglishName = "Set",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "BTL",
                    Name = "瓶",
                    EnglishName = "Bottle",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "CAN",
                    Name = "罐",
                    EnglishName = "Can",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "SHT",
                    Name = "張",
                    EnglishName = "Sheet",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "BK",
                    Name = "本",
                    EnglishName = "Book",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "DRUM",
                    Name = "桶",
                    EnglishName = "Drum",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "L",
                    Name = "公升",
                    EnglishName = "Liter",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "ML",
                    Name = "毫升",
                    EnglishName = "Milliliter",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "G",
                    Name = "克",
                    EnglishName = "Gram",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "TON",
                    Name = "噸",
                    EnglishName = "Ton",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "LB",
                    Name = "磅",
                    EnglishName = "Pound",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "PAIR",
                    Name = "對",
                    EnglishName = "Pair",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "CTN",
                    Name = "紙箱",
                    EnglishName = "Carton",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "REEL",
                    Name = "盤",
                    EnglishName = "Reel",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "STRIP",
                    Name = "條",
                    EnglishName = "Strip",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "PALLET",
                    Name = "棧板",
                    EnglishName = "Pallet",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "CASE",
                    Name = "盒",
                    EnglishName = "Case",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "TUBE",
                    Name = "管",
                    EnglishName = "Tube",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "BUNDLE",
                    Name = "束",
                    EnglishName = "Bundle",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "TRAY",
                    Name = "盤(食品)",
                    EnglishName = "Tray",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "PKG",
                    Name = "包裝",
                    EnglishName = "Package",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "CARTON",
                    Name = "卡通箱",
                    EnglishName = "Carton Box",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "SACK",
                    Name = "麻袋",
                    EnglishName = "Sack",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "BLOCK",
                    Name = "塊",
                    EnglishName = "Block",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "FOOT",
                    Name = "英呎",
                    EnglishName = "Foot",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "INCH",
                    Name = "英吋",
                    EnglishName = "Inch",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "YARD",
                    Name = "碼",
                    EnglishName = "Yard",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "GAL",
                    Name = "加侖",
                    EnglishName = "Gallon",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "OZ",
                    Name = "盎司",
                    EnglishName = "Ounce",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "BAL",
                    Name = "包(大)",
                    EnglishName = "Bale",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "PLATE",
                    Name = "片",
                    EnglishName = "Plate",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "UNIT",
                    Name = "單位",
                    EnglishName = "Unit",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "PAIL",
                    Name = "桶(小)",
                    EnglishName = "Pail",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "SKID",
                    Name = "滑板",
                    EnglishName = "Skid",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Unit
                {
                    Code = "LOT",
                    Name = "批",
                    EnglishName = "Lot",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
            };

            await context.Units.AddRangeAsync(units);
            await context.SaveChangesAsync();
        }
    }
}
