using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    public class SalesSeeder : IDataSeeder
    {
        public int Order => 23; // 設定執行順序，在客戶、員工、產品之後
        public string Name => "銷貨資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedSalesOrdersAsync(context);
            await SeedSalesReturnsAsync(context);
        }

        private static async Task SeedSalesOrdersAsync(AppDbContext context)
        {
            if (await context.SalesOrders.AnyAsync()) return;

            var (createdAt1, createdBy) = SeedDataHelper.GetSystemCreateInfo(30);
            var (createdAt2, _) = SeedDataHelper.GetSystemCreateInfo(25);
            var (createdAt3, _) = SeedDataHelper.GetSystemCreateInfo(20);

            // 取得客戶和員工資料用於關聯
            var customers = await context.Customers.Take(3).ToListAsync();
            var employees = await context.Employees.Take(2).ToListAsync();
            var products = await context.Products.Take(5).ToListAsync();
            var units = await context.Units.Take(3).ToListAsync();

            if (!customers.Any() || !employees.Any() || !products.Any())
                return; // 沒有必要的關聯資料時跳過

            var salesOrders = new[]
            {
                new SalesOrder
                {
                    SalesOrderNumber = "SO-2024-001",
                    OrderDate = DateTime.Today.AddDays(-10),
                    ExpectedDeliveryDate = DateTime.Today.AddDays(5),
                    OrderStatus = SalesOrderStatus.Confirmed,
                    SalesType = SalesType.Normal,
                    SalesPersonnel = "王銷售",
                    OrderRemarks = "第一筆測試銷貨訂單",
                    TotalAmount = 50000,
                    TaxAmount = 2500,
                    TotalAmountWithTax = 52500,
                    PaymentTerms = "月結30天",
                    DeliveryTerms = "工廠交貨",
                    CustomerId = customers[0].Id,
                    EmployeeId = employees[0].Id,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt1,
                    CreatedBy = createdBy
                },
                new SalesOrder
                {
                    SalesOrderNumber = "SO-2024-002",
                    OrderDate = DateTime.Today.AddDays(-5),
                    ExpectedDeliveryDate = DateTime.Today.AddDays(10),
                    OrderStatus = SalesOrderStatus.PartialDelivered,
                    SalesType = SalesType.Urgent,
                    SalesPersonnel = "李銷售",
                    OrderRemarks = "緊急訂單，需要優先處理",
                    TotalAmount = 75000,
                    TaxAmount = 3750,
                    TotalAmountWithTax = 78750,
                    PaymentTerms = "預付款",
                    DeliveryTerms = "客戶自取",
                    CustomerId = customers.Count > 1 ? customers[1].Id : customers[0].Id,
                    EmployeeId = employees.Count > 1 ? employees[1].Id : employees[0].Id,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt2,
                    CreatedBy = createdBy
                },
                new SalesOrder
                {
                    SalesOrderNumber = "SO-2024-003",
                    OrderDate = DateTime.Today.AddDays(-2),
                    ExpectedDeliveryDate = DateTime.Today.AddDays(15),
                    OrderStatus = SalesOrderStatus.Draft,
                    SalesType = SalesType.Project,
                    SalesPersonnel = "陳銷售",
                    OrderRemarks = "專案訂單，分批交貨",
                    TotalAmount = 120000,
                    TaxAmount = 6000,
                    TotalAmountWithTax = 126000,
                    PaymentTerms = "分期付款",
                    DeliveryTerms = "貨到付款",
                    CustomerId = customers.Count > 2 ? customers[2].Id : customers[0].Id,
                    EmployeeId = employees[0].Id,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt3,
                    CreatedBy = createdBy
                }
            };

            await context.SalesOrders.AddRangeAsync(salesOrders);
            await context.SaveChangesAsync();

            // 建立銷貨訂單明細
            await SeedSalesOrderDetailsAsync(context, salesOrders, products, units);

            // 建立部分出貨資料
            await SeedSalesDeliveriesAsync(context, salesOrders, employees);
        }

        private static async Task SeedSalesOrderDetailsAsync(AppDbContext context, SalesOrder[] salesOrders, List<Product> products, List<Unit> units)
        {
            if (await context.SalesOrderDetails.AnyAsync()) return;

            var salesOrderDetails = new List<SalesOrderDetail>();

            // 第一筆訂單的明細
            salesOrderDetails.AddRange(new[]
            {
                new SalesOrderDetail
                {
                    SalesOrderId = salesOrders[0].Id,
                    ProductId = products[0].Id,
                    UnitId = units.FirstOrDefault()?.Id,
                    OrderQuantity = 100,
                    UnitPrice = 250,
                    DiscountPercentage = 5,
                    DiscountAmount = 1250,
                    Subtotal = 23750,
                    DeliveredQuantity = 100,
                    PendingQuantity = 0,
                    DetailRemarks = "第一項商品",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-10),
                    CreatedBy = "System"
                },
                new SalesOrderDetail
                {
                    SalesOrderId = salesOrders[0].Id,
                    ProductId = products.Count > 1 ? products[1].Id : products[0].Id,
                    UnitId = units.FirstOrDefault()?.Id,
                    OrderQuantity = 50,
                    UnitPrice = 500,
                    DiscountPercentage = 0,
                    DiscountAmount = 0,
                    Subtotal = 25000,
                    DeliveredQuantity = 50,
                    PendingQuantity = 0,
                    DetailRemarks = "第二項商品",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-10),
                    CreatedBy = "System"
                }
            });

            // 第二筆訂單的明細
            salesOrderDetails.AddRange(new[]
            {
                new SalesOrderDetail
                {
                    SalesOrderId = salesOrders[1].Id,
                    ProductId = products.Count > 2 ? products[2].Id : products[0].Id,
                    UnitId = units.FirstOrDefault()?.Id,
                    OrderQuantity = 200,
                    UnitPrice = 300,
                    DiscountPercentage = 10,
                    DiscountAmount = 6000,
                    Subtotal = 54000,
                    DeliveredQuantity = 150,
                    PendingQuantity = 50,
                    DetailRemarks = "部分出貨商品",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-5),
                    CreatedBy = "System"
                }
            });

            await context.SalesOrderDetails.AddRangeAsync(salesOrderDetails);
            await context.SaveChangesAsync();
        }

        private static async Task SeedSalesDeliveriesAsync(AppDbContext context, SalesOrder[] salesOrders, List<Employee> employees)
        {
            if (await context.SalesDeliveries.AnyAsync()) return;

            var salesDeliveries = new[]
            {
                new SalesDelivery
                {
                    DeliveryNumber = "SD-2024-001",
                    DeliveryDate = DateTime.Today.AddDays(-5),
                    ExpectedArrivalDate = DateTime.Today.AddDays(-3),
                    ActualArrivalDate = DateTime.Today.AddDays(-2),
                    DeliveryStatus = SalesDeliveryStatus.Received,
                    DeliveryPersonnel = "張出貨",
                    ShippingMethod = "宅急便",
                    TrackingNumber = "TRK001234567",
                    DeliveryAddress = "台北市信義區信義路五段7號",
                    DeliveryContact = "林收貨",
                    DeliveryPhone = "02-2345-6789",
                    DeliveryRemarks = "已順利送達",
                    SalesOrderId = salesOrders[0].Id,
                    EmployeeId = employees[0].Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-5),
                    CreatedBy = "System"
                },
                new SalesDelivery
                {
                    DeliveryNumber = "SD-2024-002",
                    DeliveryDate = DateTime.Today.AddDays(-3),
                    ExpectedArrivalDate = DateTime.Today.AddDays(-1),
                    DeliveryStatus = SalesDeliveryStatus.Delivered,
                    DeliveryPersonnel = "陳出貨",
                    ShippingMethod = "自有車輛",
                    TrackingNumber = "TRK002345678",
                    DeliveryAddress = "新北市板橋區文化路一段188號",
                    DeliveryContact = "劉收貨",
                    DeliveryPhone = "02-2987-6543",
                    DeliveryRemarks = "第一批部分出貨",
                    SalesOrderId = salesOrders[1].Id,
                    EmployeeId = employees.Count > 1 ? employees[1].Id : employees[0].Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-3),
                    CreatedBy = "System"
                }
            };

            await context.SalesDeliveries.AddRangeAsync(salesDeliveries);
            await context.SaveChangesAsync();
        }

        private static async Task SeedSalesReturnsAsync(AppDbContext context)
        {
            if (await context.SalesReturns.AnyAsync()) return;

            var (createdAt1, createdBy) = SeedDataHelper.GetSystemCreateInfo(15);
            var (createdAt2, _) = SeedDataHelper.GetSystemCreateInfo(10);

            // 取得相關資料用於關聯
            var customers = await context.Customers.Take(2).ToListAsync();
            var employees = await context.Employees.Take(2).ToListAsync();
            var salesOrders = await context.SalesOrders.Take(2).ToListAsync();
            var salesDeliveries = await context.SalesDeliveries.Take(2).ToListAsync();

            if (!customers.Any() || !salesOrders.Any())
                return; // 沒有必要的關聯資料時跳過

            var salesReturns = new[]
            {
                new SalesReturn
                {
                    SalesReturnNumber = "SR202501001",
                    ReturnDate = DateTime.Today.AddDays(-7),
                    ExpectedProcessDate = DateTime.Today.AddDays(-5),
                    ActualProcessDate = DateTime.Today.AddDays(-3),
                    ReturnStatus = SalesReturnStatus.Completed,
                    ReturnReason = SalesReturnReason.QualityIssue,
                    ProcessPersonnel = "王處理",
                    ReturnDescription = "商品品質不良，客戶要求退回",
                    ProcessRemarks = "已完成退款處理",
                    TotalReturnAmount = 12500,
                    ReturnTaxAmount = 625,
                    TotalReturnAmountWithTax = 13125,
                    IsRefunded = true,
                    RefundDate = DateTime.Today.AddDays(-3),
                    RefundAmount = 13125,
                    RefundRemarks = "已退回原付款帳戶",
                    CustomerId = customers[0].Id,
                    SalesOrderId = salesOrders[0].Id,
                    SalesDeliveryId = salesDeliveries.FirstOrDefault()?.Id,
                    EmployeeId = employees[0].Id,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt1,
                    CreatedBy = createdBy
                },
                new SalesReturn
                {
                    SalesReturnNumber = "SR202501002",
                    ReturnDate = DateTime.Today.AddDays(-3),
                    ExpectedProcessDate = DateTime.Today.AddDays(2),
                    ReturnStatus = SalesReturnStatus.Submitted,
                    ReturnReason = SalesReturnReason.SpecificationMismatch,
                    ProcessPersonnel = "李處理",
                    ReturnDescription = "規格不符合要求，需要更換",
                    TotalReturnAmount = 15000,
                    ReturnTaxAmount = 750,
                    TotalReturnAmountWithTax = 15750,
                    IsRefunded = false,
                    CustomerId = customers.Count > 1 ? customers[1].Id : customers[0].Id,
                    SalesOrderId = salesOrders.Count > 1 ? salesOrders[1].Id : salesOrders[0].Id,
                    SalesDeliveryId = salesDeliveries.Count > 1 ? salesDeliveries[1].Id : salesDeliveries.FirstOrDefault()?.Id,
                    EmployeeId = employees.Count > 1 ? employees[1].Id : employees[0].Id,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt2,
                    CreatedBy = createdBy
                }
            };

            await context.SalesReturns.AddRangeAsync(salesReturns);
            await context.SaveChangesAsync();

            // 建立銷貨退回明細
            await SeedSalesReturnDetailsAsync(context, salesReturns);
        }

        private static async Task SeedSalesReturnDetailsAsync(AppDbContext context, SalesReturn[] salesReturns)
        {
            if (await context.SalesReturnDetails.AnyAsync()) return;

            var products = await context.Products.Take(3).ToListAsync();
            var salesOrderDetails = await context.SalesOrderDetails.Take(3).ToListAsync();
            var salesDeliveryDetails = await context.SalesDeliveryDetails.Take(3).ToListAsync();

            if (!products.Any())
                return;

            var salesReturnDetails = new List<SalesReturnDetail>();

            // 第一筆退回的明細
            salesReturnDetails.AddRange(new[]
            {
                new SalesReturnDetail
                {
                    SalesReturnId = salesReturns[0].Id,
                    ProductId = products[0].Id,
                    ReturnQuantity = 50,
                    OriginalUnitPrice = 250,
                    ReturnUnitPrice = 250,
                    DiscountPercentage = 0,
                    DiscountAmount = 0,
                    ReturnSubtotal = 12500,
                    ProcessedQuantity = 50,
                    PendingQuantity = 0,
                    IsRestocked = true,
                    RestockedQuantity = 45,
                    ScrapQuantity = 5,
                    DetailRemarks = "部分商品報廢，其餘入庫",
                    QualityCondition = "輕微瑕疵",
                    SalesOrderDetailId = salesOrderDetails.FirstOrDefault()?.Id,
                    SalesDeliveryDetailId = salesDeliveryDetails.FirstOrDefault()?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-7),
                    CreatedBy = "System"
                }
            });

            // 第二筆退回的明細
            salesReturnDetails.AddRange(new[]
            {
                new SalesReturnDetail
                {
                    SalesReturnId = salesReturns[1].Id,
                    ProductId = products.Count > 1 ? products[1].Id : products[0].Id,
                    ReturnQuantity = 30,
                    OriginalUnitPrice = 500,
                    ReturnUnitPrice = 500,
                    DiscountPercentage = 0,
                    DiscountAmount = 0,
                    ReturnSubtotal = 15000,
                    ProcessedQuantity = 0,
                    PendingQuantity = 30,
                    IsRestocked = false,
                    RestockedQuantity = 0,
                    ScrapQuantity = 0,
                    DetailRemarks = "等待處理",
                    QualityCondition = "待檢查",
                    SalesOrderDetailId = salesOrderDetails.Count > 1 ? salesOrderDetails[1].Id : salesOrderDetails.FirstOrDefault()?.Id,
                    SalesDeliveryDetailId = salesDeliveryDetails.Count > 1 ? salesDeliveryDetails[1].Id : salesDeliveryDetails.FirstOrDefault()?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-3),
                    CreatedBy = "System"
                }
            });

            await context.SalesReturnDetails.AddRangeAsync(salesReturnDetails);
            await context.SaveChangesAsync();
        }
    }
}
