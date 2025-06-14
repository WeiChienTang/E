using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    public class InventorySeeder : IDataSeeder
    {
        public int Order => 5;
        public string Name => "庫存管理";

        public async Task SeedAsync(AppDbContext context)
        {
            // 確保此方法是否已經有資料
            bool hasData = await context.Warehouses.AnyAsync() ||
                          await context.Units.AnyAsync() ||
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

            // 建立倉庫
            var warehouses = new List<Warehouse>
            {
                new Warehouse
                {
                    WarehouseCode = "WH001",
                    WarehouseName = "主倉庫",
                    Address = "台中市北屯區文心路四段123號",
                    ContactPerson = "王倉管",
                    Phone = "04-2234-5678",
                    WarehouseType = WarehouseTypeEnum.Main,
                    IsDefault = true,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Warehouse
                {
                    WarehouseCode = "WH002",
                    WarehouseName = "次倉庫",
                    Address = "台中市西屯區台灣大道三段456號",
                    ContactPerson = "李倉管",
                    Phone = "04-2345-6789",
                    WarehouseType = WarehouseTypeEnum.Branch,
                    IsDefault = false,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Warehouse
                {
                    WarehouseCode = "WH003",
                    WarehouseName = "退貨倉庫",
                    Address = "台中市南屯區向上路二段789號",
                    ContactPerson = "張倉管",
                    Phone = "04-2456-7890",
                    WarehouseType = WarehouseTypeEnum.Return,
                    IsDefault = false,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                }
            };

            await context.Warehouses.AddRangeAsync(warehouses);
            await context.SaveChangesAsync();

            // 建立庫位
            var warehouseLocations = new List<WarehouseLocation>();
            var mainWarehouse = warehouses.First(w => w.WarehouseCode == "WH001");
            var branchWarehouse = warehouses.First(w => w.WarehouseCode == "WH002");

            // 主倉庫庫位
            for (int zone = 1; zone <= 3; zone++)
            {
                for (int aisle = 1; aisle <= 5; aisle++)
                {
                    for (int level = 1; level <= 4; level++)
                    {
                        warehouseLocations.Add(new WarehouseLocation
                        {
                            WarehouseId = mainWarehouse.Id,
                            LocationCode = $"A{zone:D1}-{aisle:D2}-{level:D2}",
                            LocationName = $"A區{zone}道{aisle}層{level}",
                            Zone = $"A{zone}",
                            Aisle = aisle.ToString("D2"),
                            Level = level.ToString("D2"),
                            Position = "01",
                            MaxCapacity = 100,
                            IsActive = true,
                            Status = EntityStatus.Active,
                            CreatedAt = DateTime.Now,
                            CreatedBy = "System"
                        });
                    }
                }
            }

            // 次倉庫庫位
            for (int zone = 1; zone <= 2; zone++)
            {
                for (int aisle = 1; aisle <= 3; aisle++)
                {
                    for (int level = 1; level <= 3; level++)
                    {
                        warehouseLocations.Add(new WarehouseLocation
                        {
                            WarehouseId = branchWarehouse.Id,
                            LocationCode = $"B{zone:D1}-{aisle:D2}-{level:D2}",
                            LocationName = $"B區{zone}道{aisle}層{level}",
                            Zone = $"B{zone}",
                            Aisle = aisle.ToString("D2"),
                            Level = level.ToString("D2"),
                            Position = "01",
                            MaxCapacity = 50,
                            IsActive = true,
                            Status = EntityStatus.Active,
                            CreatedAt = DateTime.Now,
                            CreatedBy = "System"
                        });
                    }
                }
            }

            await context.WarehouseLocations.AddRangeAsync(warehouseLocations);

            // 建立異動類型
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
            Console.WriteLine($"   - 建立了 {warehouses.Count} 個倉庫");
            Console.WriteLine($"   - 建立了 {warehouseLocations.Count} 個庫位");
            Console.WriteLine($"   - 建立了 {transactionTypes.Count} 個異動類型");
        }
    }
}
