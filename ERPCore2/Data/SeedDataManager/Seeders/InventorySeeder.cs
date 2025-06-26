using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    public class InventorySeeder : IDataSeeder
    {
        public int Order => 6; // 在倉庫資料之後
        public string Name => "庫存管理";

        public async Task SeedAsync(AppDbContext context)
        {
            // 確保此方法是否已經有資料 (移除倉庫檢查，因為倉庫由 WarehouseSeeder 處理)
            bool hasData = await context.Units.AnyAsync() ||
                          await context.InventoryTransactionTypes.AnyAsync();

            if (hasData)
            {
                Console.WriteLine("🔄 庫存管理資料已存在，跳過種子資料建立");
                return;
            }

            Console.WriteLine("🌱 開始建立庫存管理種子資料...");

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
                }
            };

            await context.UnitConversions.AddRangeAsync(unitConversions);

            // 建立異動類型 (倉庫資料移至 WarehouseSeeder)
            var transactionTypes = new List<InventoryTransactionType>
            {
                new InventoryTransactionType
                {
                    TypeCode = "IN001",
                    TypeName = "採購入庫",
                    TransactionType = InventoryTransactionTypeEnum.In,
                    AffectsCost = true,
                    RequiresApproval = true,
                    AutoGenerateNumber = true,
                    NumberPrefix = "PI",
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new InventoryTransactionType
                {
                    TypeCode = "IN002",
                    TypeName = "生產入庫",
                    TransactionType = InventoryTransactionTypeEnum.In,
                    AffectsCost = true,
                    RequiresApproval = false,
                    AutoGenerateNumber = true,
                    NumberPrefix = "MI",
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new InventoryTransactionType
                {
                    TypeCode = "IN003",
                    TypeName = "調撥入庫",
                    TransactionType = InventoryTransactionTypeEnum.In,
                    AffectsCost = false,
                    RequiresApproval = true,
                    AutoGenerateNumber = true,
                    NumberPrefix = "TI",
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new InventoryTransactionType
                {
                    TypeCode = "OUT001",
                    TypeName = "銷售出庫",
                    TransactionType = InventoryTransactionTypeEnum.Out,
                    AffectsCost = false,
                    RequiresApproval = false,
                    AutoGenerateNumber = true,
                    NumberPrefix = "SO",
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new InventoryTransactionType
                {
                    TypeCode = "OUT002",
                    TypeName = "調撥出庫",
                    TransactionType = InventoryTransactionTypeEnum.Out,
                    AffectsCost = false,
                    RequiresApproval = true,
                    AutoGenerateNumber = true,
                    NumberPrefix = "TO",
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new InventoryTransactionType
                {
                    TypeCode = "OUT003",
                    TypeName = "盤虧出庫",
                    TransactionType = InventoryTransactionTypeEnum.Out,
                    AffectsCost = true,
                    RequiresApproval = true,
                    AutoGenerateNumber = true,
                    NumberPrefix = "LO",
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new InventoryTransactionType
                {
                    TypeCode = "ADJ001",
                    TypeName = "盤點調整",
                    TransactionType = InventoryTransactionTypeEnum.Adjustment,
                    AffectsCost = true,
                    RequiresApproval = true,
                    AutoGenerateNumber = true,
                    NumberPrefix = "ADJ",
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                }
            };

            await context.InventoryTransactionTypes.AddRangeAsync(transactionTypes);
            await context.SaveChangesAsync();

            Console.WriteLine("✅ 庫存管理種子資料建立完成");
            Console.WriteLine($"   - 建立了 {units.Count} 個計量單位");
            Console.WriteLine($"   - 建立了 {unitConversions.Count} 個單位轉換關係");
            Console.WriteLine($"   - 建立了 {transactionTypes.Count} 個異動類型");
        }
    }
}
