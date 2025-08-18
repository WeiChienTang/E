using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 倉庫種子資料建立器
    /// </summary>
    public class WarehouseSeeder : IDataSeeder
    {
        public int Order => 6; // 在基礎資料之後，庫存資料之前
        public string Name => "倉庫管理";

        public async Task SeedAsync(AppDbContext context)
        {
            // 檢查是否已有倉庫資料
            bool hasWarehouse = await context.Warehouses.AnyAsync();

            if (hasWarehouse)
            {
                return;
            }

            // 建立倉庫資料
            var warehouses = new List<Warehouse>
            {
                new Warehouse
                {
                    Code = "WH001",
                    Name = "主倉庫",
                    Address = "台中市北屯區文心路四段123號",
                    ContactPerson = "王倉管",
                    Phone = "04-2234-5678",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Warehouse
                {
                    Code = "WH002",
                    Name = "分倉庫A",
                    Address = "台中市西屯區台灣大道三段456號",
                    ContactPerson = "李倉管",
                    Phone = "04-2345-6789",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
            };

            await context.Warehouses.AddRangeAsync(warehouses);
            await context.SaveChangesAsync();

            // 建立庫位資料
            await CreateWarehouseLocations(context, warehouses);
        }

        /// <summary>
        /// 建立倉庫庫位資料
        /// </summary>
        private async Task CreateWarehouseLocations(AppDbContext context, List<Warehouse> warehouses)
        {
            var warehouseLocations = new List<WarehouseLocation>();

            // 主倉庫庫位 (WH001)
            var mainWarehouse = warehouses.First(w => w.Code == "WH001");
            for (int zone = 1; zone <= 3; zone++)
            {
                for (int aisle = 1; aisle <= 5; aisle++)
                {
                    for (int level = 1; level <= 4; level++)
                    {
                        warehouseLocations.Add(new WarehouseLocation
                        {
                            WarehouseId = mainWarehouse.Id,
                            Code = $"A{zone:D1}-{aisle:D2}-{level:D2}",
                            Name = $"A區{zone}道{aisle}層{level}",
                            Zone = $"A{zone}",
                            Aisle = aisle.ToString("D2"),
                            Level = level.ToString("D2"),
                            Position = "01",
                            MaxCapacity = 100,
                            CreatedAt = DateTime.Now,
                            CreatedBy = "System"
                        });
                    }
                }
            }

            // 分倉庫A庫位 (WH002)
            var branchWarehouseA = warehouses.First(w => w.Code == "WH002");
            for (int zone = 1; zone <= 2; zone++)
            {
                for (int aisle = 1; aisle <= 3; aisle++)
                {
                    for (int level = 1; level <= 3; level++)
                    {
                        warehouseLocations.Add(new WarehouseLocation
                        {
                            WarehouseId = branchWarehouseA.Id,
                            Code = $"B{zone:D1}-{aisle:D2}-{level:D2}",
                            Name = $"B區{zone}道{aisle}層{level}",
                            Zone = $"B{zone}",
                            Aisle = aisle.ToString("D2"),
                            Level = level.ToString("D2"),
                            Position = "01",
                            MaxCapacity = 50,
                            CreatedAt = DateTime.Now,
                            CreatedBy = "System"
                        });
                    }
                }
            }

            await context.WarehouseLocations.AddRangeAsync(warehouseLocations);
            await context.SaveChangesAsync();
        }
    }
}
