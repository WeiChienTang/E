using ERPCore2.Data.Context;
using ERPCore2.Data.SeedDataManager.Interfaces;
using ERPCore2.Data.SeedDataManager.Seeders;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data
{
    /// <summary>
    /// 資料種子初始化協調器
    /// </summary>
    public static class SeedData
    {
        /// <summary>
        /// 初始化所有種子資料
        /// </summary>
        /// <param name="serviceProvider">服務提供者</param>
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                // 確保資料庫存在並執行遷移
                await context.Database.MigrateAsync();

                // 開始交易
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // 取得所有種子器並按順序執行
                    var seeders = GetAllSeeders().OrderBy(s => s.Order).ToList();

                    foreach (var seeder in seeders)
                    {
                        await seeder.SeedAsync(context);
                    }

                    // 提交交易
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    // 回滾交易
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 取得所有種子器
        /// </summary>
        /// <returns>種子器集合</returns>
        private static IEnumerable<IDataSeeder> GetAllSeeders()
        {
            return new List<IDataSeeder>
            {
                new PermissionSeeder(),
                new RoleSeeder(),
                new RolePermissionSeeder(),
                new BasicDataSeeder(),          // 必須在 CustomerSeeder 和 SupplierSeeder 之前
                new EmployeeSeeder(),
                new EmployeePositionSeeder(),
                new DepartmentSeeder(),
                new ColorSeeder(),
                new MaterialSeeder(),
                new UnitSeeder(),
                new SizeSeeder(),
                new WarehouseSeeder(),
                new CustomerSeeder(),           // 依賴 CustomerTypes (在 BasicDataSeeder 中)
                new SupplierSeeder(),           // 依賴 SupplierTypes (在 BasicDataSeeder 中)
                new ProductSeeder(),
                new InventorySeeder(),
                new InventoryStockSeeder(), 
                new SalesSeeder(),              // 依賴 Customer, Employee, Product (必須在這些之後)
                // PurchaseSeeder 已移除 - 改為手動新增測試資料
                new WeatherSeeder(),
                new ErrorLogSeeder()
            };
        }
    }
}
