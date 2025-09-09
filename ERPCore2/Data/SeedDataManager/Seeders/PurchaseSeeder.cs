using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    public class PurchaseSeeder : IDataSeeder
    {
        public int Order => 25; // 在庫存資料之後
        public string Name => "採購測試資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedPurchaseOrdersAsync(context);
        }

        private static async Task SeedPurchaseOrdersAsync(AppDbContext context)
        {
            if (await context.PurchaseOrders.AnyAsync()) return;

            var suppliers = await context.Suppliers.Where(s => !s.IsDeleted).ToListAsync();
            var products = await context.Products.Where(p => !p.IsDeleted).Take(10).ToListAsync();
            var warehouses = await context.Warehouses.Where(w => !w.IsDeleted).ToListAsync();

            if (!suppliers.Any() || !products.Any() || !warehouses.Any()) return;

            var (createdAt1, createdBy) = SeedDataHelper.GetSystemCreateInfo(30);
            var (createdAt2, _) = SeedDataHelper.GetSystemCreateInfo(25);
            var (createdAt3, _) = SeedDataHelper.GetSystemCreateInfo(20);

            var random = new Random();
            var orders = new List<PurchaseOrder>();

            // 建立幾筆採購訂單
            for (int i = 1; i <= 5; i++)
            {
                var supplier = suppliers[random.Next(suppliers.Count)];
                var warehouse = warehouses[random.Next(warehouses.Count)];
                var orderDate = DateTime.Today.AddDays(-random.Next(1, 30));
                
                var order = new PurchaseOrder
                {
                    PurchaseOrderNumber = $"PO{orderDate:yyyyMMdd}{i:D3}",
                    OrderDate = orderDate,
                    ExpectedDeliveryDate = orderDate.AddDays(random.Next(7, 21)),
                    // 移除 OrderStatus 和 PurchaseType 欄位
                    PurchasePersonnel = "採購人員" + i,
                    Remarks = $"測試採購訂單 {i}",
                    SupplierId = supplier.Id,
                    WarehouseId = warehouse.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = i <= 2 ? createdAt1 : i <= 4 ? createdAt2 : createdAt3,
                    CreatedBy = createdBy
                };

                // 簡化邏輯，只設定前幾個訂單為已核准
                if (i <= 4)
                {
                    order.ApprovedBy = 24; // 使用實際存在的 admin 員工 ID
                    order.ApprovedAt = order.OrderDate.AddDays(1);
                }

                orders.Add(order);
            }

            await context.PurchaseOrders.AddRangeAsync(orders);
            await context.SaveChangesAsync();

            // 為每個採購訂單建立明細
            var orderDetails = new List<PurchaseOrderDetail>();
            var orderIndex = 1;
            foreach (var order in orders)
            {
                var orderProductCount = random.Next(2, 5); // 每張訂單2-4個商品
                var selectedProducts = products.OrderBy(x => random.Next()).Take(orderProductCount);

                foreach (var product in selectedProducts)
                {
                    var quantity = random.Next(10, 100);
                    var unitPrice = (decimal)(random.NextDouble() * 500 + 50); // 50-550之間

                    var detail = new PurchaseOrderDetail
                    {
                        PurchaseOrderId = order.Id,
                        ProductId = product.Id,
                        OrderQuantity = quantity,
                        ReceivedQuantity = orderIndex <= 2 ? quantity : orderIndex <= 4 ? quantity / 2 : 0, // 簡化邏輯
                        UnitPrice = unitPrice,
                        Remarks = $"採購 {product.Name}",
                        ExpectedDeliveryDate = order.ExpectedDeliveryDate,
                        Status = EntityStatus.Active,
                        CreatedAt = order.CreatedAt,
                        CreatedBy = order.CreatedBy
                    };

                    detail.ReceivedAmount = detail.ReceivedQuantity * detail.UnitPrice;
                    orderDetails.Add(detail);
                }
                orderIndex++; // 增加訂單索引
            }

            await context.PurchaseOrderDetails.AddRangeAsync(orderDetails);
            await context.SaveChangesAsync();

            // 更新採購訂單的總金額和已進貨金額
            foreach (var order in orders)
            {
                var details = orderDetails.Where(od => od.PurchaseOrderId == order.Id).ToList();
                order.TotalAmount = details.Sum(d => d.SubtotalAmount);
                order.ReceivedAmount = details.Sum(d => d.ReceivedAmount);
            }

            await context.SaveChangesAsync();

            // 為已進貨的訂單建立進貨單
            var receipts = new List<PurchaseReceiving>();
            var receiptDetails = new List<PurchaseReceivingDetail>();

            var ordersWithReceiving = orders.Where(o => o.ReceivedAmount > 0).ToList(); // 改為用已進貨金額判斷

            foreach (var order in ordersWithReceiving)
            {
                var receiptDate = order.OrderDate.AddDays(random.Next(3, 10));
                var receipt = new PurchaseReceiving
                {
                    ReceiptNumber = $"PR{receiptDate:yyyyMMdd}{order.Id:D3}",
                    ReceiptDate = receiptDate,
                    Remarks = $"採購訂單 {order.PurchaseOrderNumber} 進貨",
                    PurchaseOrderId = order.Id,
                    SupplierId = order.SupplierId,
                    ConfirmedBy = 24, // 使用實際存在的 admin 員工 ID
                    ConfirmedAt = receiptDate.AddHours(2),
                    Status = EntityStatus.Active,
                    CreatedAt = receiptDate,
                    CreatedBy = createdBy
                };

                receipts.Add(receipt);
            }

            await context.PurchaseReceivings.AddRangeAsync(receipts);
            await context.SaveChangesAsync();

            // 建立進貨明細
            foreach (var receipt in receipts)
            {
                var order = orders.First(o => o.Id == receipt.PurchaseOrderId);
                var orderDetailsList = orderDetails.Where(od => od.PurchaseOrderId == order.Id).ToList();

                foreach (var orderDetail in orderDetailsList)
                {
                    if (orderDetail.ReceivedQuantity > 0)
                    {
                        var receiptDetail = new PurchaseReceivingDetail
                        {
                            PurchaseReceivingId = receipt.Id,
                            PurchaseOrderDetailId = orderDetail.Id,
                            ProductId = orderDetail.ProductId,
                            ReceivedQuantity = orderDetail.ReceivedQuantity,
                            UnitPrice = orderDetail.UnitPrice,
                            WarehouseId = order.WarehouseId!.Value,
                            InspectionRemarks = "品質良好",
                            BatchNumber = $"BATCH{DateTime.Now:yyyyMMdd}{random.Next(100, 999)}",
                            Status = EntityStatus.Active,
                            CreatedAt = receipt.CreatedAt,
                            CreatedBy = receipt.CreatedBy
                        };

                        receiptDetails.Add(receiptDetail);
                    }
                }
            }

            await context.PurchaseReceivingDetails.AddRangeAsync(receiptDetails);
            await context.SaveChangesAsync();

            // 更新進貨單總金額
            foreach (var receipt in receipts)
            {
                var details = receiptDetails.Where(rd => rd.PurchaseReceivingId == receipt.Id).ToList();
                receipt.TotalAmount = details.Sum(d => d.SubtotalAmount);
            }

            await context.SaveChangesAsync();
        }
    }
}
