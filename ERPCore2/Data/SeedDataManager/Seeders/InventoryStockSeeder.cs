using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    public class InventoryStockSeeder : IDataSeeder
    {
        public int Order => 24; // 在Product、Warehouse和InventorySeeder之後執行
        public string Name => "庫存主檔資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedInventoryStockAsync(context);
            await SeedSampleTransactionsAsync(context);
            await SeedSampleReservationsAsync(context);
        }

        private static async Task SeedInventoryStockAsync(AppDbContext context)
        {
            if (await context.InventoryStocks.AnyAsync()) return;

            var products = await context.Products.Take(10).ToListAsync();
            var warehouses = await context.Warehouses.Take(2).ToListAsync();
            var locations = await context.WarehouseLocations.ToListAsync();

            if (!products.Any() || !warehouses.Any()) return;

            var (createdAt1, createdBy) = SeedDataHelper.GetSystemCreateInfo(15);
            var (createdAt2, _) = SeedDataHelper.GetSystemCreateInfo(10);

            var stocks = new List<InventoryStock>();
            var random = new Random();

            foreach (var product in products)
            {
                foreach (var warehouse in warehouses)
                {
                    var location = locations.FirstOrDefault(l => l.WarehouseId == warehouse.Id);
                    var currentStock = random.Next(50, 500);
                    var reservedStock = random.Next(0, currentStock / 4);

                    stocks.Add(new InventoryStock
                    {
                        ProductId = product.Id,
                        WarehouseId = warehouse.Id,
                        WarehouseLocationId = location?.Id,
                        CurrentStock = currentStock,
                        ReservedStock = reservedStock,
                        InTransitStock = 0,
                        MinStockLevel = 20,
                        MaxStockLevel = 1000,
                        AverageCost = (decimal)(random.NextDouble() * 100 + 10),
                        LastTransactionDate = DateTime.Now.AddDays(-random.Next(1, 30)),
                        Status = EntityStatus.Active,
                        CreatedAt = random.Next(2) == 0 ? createdAt1 : createdAt2,
                        CreatedBy = createdBy
                    });
                }
            }

            await context.InventoryStocks.AddRangeAsync(stocks);
            await context.SaveChangesAsync();
        }

        private static async Task SeedSampleTransactionsAsync(AppDbContext context)
        {
            if (await context.InventoryTransactions.AnyAsync()) return;

            var stocks = await context.InventoryStocks
                .Include(s => s.Product)
                .Include(s => s.Warehouse)
                .Take(5)
                .ToListAsync();

            if (!stocks.Any()) return;

            var (createdAt1, createdBy) = SeedDataHelper.GetSystemCreateInfo(10);
            var (createdAt2, _) = SeedDataHelper.GetSystemCreateInfo(5);

            var transactions = new List<InventoryTransaction>();
            var random = new Random();

            foreach (var stock in stocks)
            {
                // 為每個庫存建立期初記錄
                var transactionDate = DateTime.Now.AddDays(-random.Next(30, 90));
                
                // 期初庫存
                transactions.Add(new InventoryTransaction
                {
                    TransactionNumber = $"INIT{stock.ProductId:D6}{stock.WarehouseId ?? 0:D3}",
                    ProductId = stock.ProductId,
                    WarehouseId = stock.WarehouseId ?? 0,
                    WarehouseLocationId = stock.WarehouseLocationId,
                    TransactionType = InventoryTransactionTypeEnum.OpeningBalance,
                    TransactionDate = transactionDate,
                    Quantity = stock.CurrentStock / 2, // 部分期初
                    UnitCost = stock.AverageCost,
                    StockBefore = 0,
                    StockAfter = stock.CurrentStock / 2,
                    TransactionRemarks = "期初庫存",
                    InventoryStockId = stock.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt1,
                    CreatedBy = createdBy
                });

                // 進貨記錄
                var purchaseQuantity = stock.CurrentStock - (stock.CurrentStock / 2);
                if (purchaseQuantity > 0)
                {
                    transactions.Add(new InventoryTransaction
                    {
                        TransactionNumber = $"PUR{DateTime.Now:yyyyMMdd}{random.Next(1000, 9999)}",
                        ProductId = stock.ProductId,
                        WarehouseId = stock.WarehouseId ?? 0,
                        WarehouseLocationId = stock.WarehouseLocationId,
                        TransactionType = InventoryTransactionTypeEnum.Purchase,
                        TransactionDate = transactionDate.AddDays(random.Next(1, 15)),
                        Quantity = purchaseQuantity,
                        UnitCost = stock.AverageCost,
                        StockBefore = stock.CurrentStock / 2,
                        StockAfter = stock.CurrentStock,
                        TransactionRemarks = "進貨入庫",
                        ReferenceNumber = $"PO{random.Next(10000, 99999)}",
                        InventoryStockId = stock.Id,
                        Status = EntityStatus.Active,
                        CreatedAt = createdAt2,
                        CreatedBy = createdBy
                    });
                }
            }

            await context.InventoryTransactions.AddRangeAsync(transactions);
            await context.SaveChangesAsync();
        }

        private static async Task SeedSampleReservationsAsync(AppDbContext context)
        {
            if (await context.InventoryReservations.AnyAsync()) return;

            var stocks = await context.InventoryStocks
                .Where(s => s.CurrentStock > s.ReservedStock)
                .Take(3)
                .ToListAsync();

            if (!stocks.Any()) return;

            var (createdAt, createdBy) = SeedDataHelper.GetSystemCreateInfo(5);
            var reservations = new List<InventoryReservation>();
            var random = new Random();

            foreach (var stock in stocks)
            {
                var reserveQty = Math.Min(stock.ReservedStock, stock.CurrentStock - stock.ReservedStock);
                if (reserveQty > 0)
                {
                    reservations.Add(new InventoryReservation
                    {
                        ReservationNumber = $"RSV{DateTime.Now:yyyyMMdd}{random.Next(1000, 9999)}",
                        ProductId = stock.ProductId,
                        WarehouseId = stock.WarehouseId ?? 0,
                        WarehouseLocationId = stock.WarehouseLocationId,
                        ReservationType = InventoryReservationType.SalesOrder,
                        ReservationStatus = InventoryReservationStatus.Reserved,
                        ReservationDate = DateTime.Now.AddDays(-random.Next(1, 10)),
                        ExpiryDate = DateTime.Now.AddDays(random.Next(30, 90)),
                        ReservedQuantity = reserveQty,
                        ReleasedQuantity = 0,
                        ReferenceNumber = $"SO{random.Next(10000, 99999)}",
                        ReservationRemarks = "銷售訂單預留",
                        InventoryStockId = stock.Id,
                        Status = EntityStatus.Active,
                        CreatedAt = createdAt,
                        CreatedBy = createdBy
                    });
                }
            }

            if (reservations.Any())
            {
                await context.InventoryReservations.AddRangeAsync(reservations);
                await context.SaveChangesAsync();
            }
        }
    }
}
