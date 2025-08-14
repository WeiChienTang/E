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
                new ContactTypeSeeder(),        // 聯絡類型
                new AddressTypeSeeder(),        // 地址類型  
                new CustomerTypeSeeder(),       // 客戶類型
                new SupplierTypeSeeder(),       // 廠商類型
                new EmployeeSeeder(),
                new EmployeePositionSeeder(),
                new DepartmentSeeder(),
                new ColorSeeder(),
                new MaterialSeeder(),
                new ProductCategorySeeder(),    // 產品類別 - 必須在 ProductSeeder 之前
                new UnitSeeder(),
                new UnitConversionSeeder(),     // 單位轉換關係 - 必須在 UnitSeeder 之後
                new SizeSeeder(),
                new WarehouseSeeder(),
                new CustomerSeeder(),           // 依賴 CustomerTypes
                new SupplierSeeder(),           // 依賴 SupplierTypes
                new ProductSeeder(),            // 依賴 ProductCategory, Unit, Supplier
                new InventorySeeder(),
                new WeatherSeeder(),
                // new BasicDataSeeder(),       // 已廢棄，功能已分拆到專屬 Seeder
            };
        }
    }
}
