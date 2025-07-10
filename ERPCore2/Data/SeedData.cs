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
                Console.WriteLine("正在檢查並初始化資料庫...");
                
                // 確保資料庫存在並執行遷移
                await context.Database.MigrateAsync();
                Console.WriteLine("資料庫遷移完成");

                // 開始交易
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // 取得所有種子器並按順序執行
                    var seeders = GetAllSeeders().OrderBy(s => s.Order).ToList();

                    foreach (var seeder in seeders)
                    {
                        Console.WriteLine($"正在初始化 {seeder.Name} 資料...");
                        await seeder.SeedAsync(context);
                        Console.WriteLine($"{seeder.Name} 資料初始化完成");
                    }

                    // 提交交易
                    await transaction.CommitAsync();
                    Console.WriteLine("所有種子資料初始化完成");
                }
                catch (Exception ex)
                {
                    // 回滾交易
                    await transaction.RollbackAsync();
                    Console.WriteLine($"種子資料初始化失敗，已回滾：{ex.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"種子資料初始化過程中發生錯誤：{ex.Message}");
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
                new EmployeeSeeder(),
                new BasicDataSeeder(),
                new EmployeePositionSeeder(),
                new ColorSeeder(),
                new MaterialSeeder(),
                new UnitSeeder(),
                new SizeSeeder(),
                new WarehouseSeeder(), 
                new CustomerSeeder(),
                new SupplierSeeder(),
                new ProductSeeder(),
                new InventorySeeder(),
                new InventoryStockSeeder(), 
                new WeatherSeeder(),
                new ErrorLogSeeder()
            };
        }
    }
}
