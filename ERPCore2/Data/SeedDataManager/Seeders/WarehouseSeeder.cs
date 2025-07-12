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
                    WarehouseCode = "WH001",
                    WarehouseName = "主倉庫",
                    Address = "台中市北屯區文心路四段123號",
                    ContactPerson = "王倉管",
                    Phone = "04-2234-5678",
                    WarehouseType = WarehouseTypeEnum.Main,
                    IsDefault = true,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Warehouse
                {
                    WarehouseCode = "WH002",
                    WarehouseName = "分倉庫A",
                    Address = "台中市西屯區台灣大道三段456號",
                    ContactPerson = "李倉管",
                    Phone = "04-2345-6789",
                    WarehouseType = WarehouseTypeEnum.Branch,
                    IsDefault = false,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Warehouse
                {
                    WarehouseCode = "WH003",
                    WarehouseName = "分倉庫B",
                    Address = "台中市南屯區向上路二段789號",
                    ContactPerson = "張倉管",
                    Phone = "04-2456-7890",
                    WarehouseType = WarehouseTypeEnum.Branch,
                    IsDefault = false,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Warehouse
                {
                    WarehouseCode = "WH004",
                    WarehouseName = "退貨倉庫",
                    Address = "台中市東區東英路100號",
                    ContactPerson = "陳倉管",
                    Phone = "04-2567-8901",
                    WarehouseType = WarehouseTypeEnum.Return,
                    IsDefault = false,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Warehouse
                {
                    WarehouseCode = "WH005",
                    WarehouseName = "虛擬倉庫",
                    Address = null,
                    ContactPerson = "系統管理員",
                    Phone = null,
                    WarehouseType = WarehouseTypeEnum.Virtual,
                    IsDefault = false,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new Warehouse
                {
                    WarehouseCode = "WH999",
                    WarehouseName = "測試倉庫(停用)",
                    Address = "台中市測試區測試路999號",
                    ContactPerson = "測試人員",
                    Phone = "04-9999-9999",
                    WarehouseType = WarehouseTypeEnum.Branch,
                    IsDefault = false,
                    IsActive = false,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                }
            };

            await context.Warehouses.AddRangeAsync(warehouses);            await context.SaveChangesAsync();

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
            var mainWarehouse = warehouses.First(w => w.WarehouseCode == "WH001");
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
                            CreatedAt = DateTime.Now,
                            CreatedBy = "System"
                        });
                    }
                }
            }

            // 分倉庫A庫位 (WH002)
            var branchWarehouseA = warehouses.First(w => w.WarehouseCode == "WH002");
            for (int zone = 1; zone <= 2; zone++)
            {
                for (int aisle = 1; aisle <= 3; aisle++)
                {
                    for (int level = 1; level <= 3; level++)
                    {
                        warehouseLocations.Add(new WarehouseLocation
                        {
                            WarehouseId = branchWarehouseA.Id,
                            LocationCode = $"B{zone:D1}-{aisle:D2}-{level:D2}",
                            LocationName = $"B區{zone}道{aisle}層{level}",
                            Zone = $"B{zone}",
                            Aisle = aisle.ToString("D2"),
                            Level = level.ToString("D2"),
                            Position = "01",
                            MaxCapacity = 50,
                            IsActive = true,
                            CreatedAt = DateTime.Now,
                            CreatedBy = "System"
                        });
                    }
                }
            }

            // 分倉庫B庫位 (WH003)
            var branchWarehouseB = warehouses.First(w => w.WarehouseCode == "WH003");
            for (int zone = 1; zone <= 2; zone++)
            {
                for (int aisle = 1; aisle <= 4; aisle++)
                {
                    for (int level = 1; level <= 2; level++)
                    {
                        warehouseLocations.Add(new WarehouseLocation
                        {
                            WarehouseId = branchWarehouseB.Id,
                            LocationCode = $"C{zone:D1}-{aisle:D2}-{level:D2}",
                            LocationName = $"C區{zone}道{aisle}層{level}",
                            Zone = $"C{zone}",
                            Aisle = aisle.ToString("D2"),
                            Level = level.ToString("D2"),
                            Position = "01",
                            MaxCapacity = 30,
                            IsActive = true,
                            CreatedAt = DateTime.Now,
                            CreatedBy = "System"
                        });
                    }
                }
            }

            // 退貨倉庫庫位 (WH004)
            var returnWarehouse = warehouses.First(w => w.WarehouseCode == "WH004");
            for (int aisle = 1; aisle <= 2; aisle++)
            {
                for (int level = 1; level <= 2; level++)
                {
                    warehouseLocations.Add(new WarehouseLocation
                    {
                        WarehouseId = returnWarehouse.Id,
                        LocationCode = $"R-{aisle:D2}-{level:D2}",
                        LocationName = $"退貨區道{aisle}層{level}",
                        Zone = "R",
                        Aisle = aisle.ToString("D2"),
                        Level = level.ToString("D2"),
                        Position = "01",
                        MaxCapacity = 20,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = "System"
                    });
                }
            }

            await context.WarehouseLocations.AddRangeAsync(warehouseLocations);
            await context.SaveChangesAsync();
        }
    }
}
