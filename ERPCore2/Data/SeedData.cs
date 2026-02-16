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
            bool isTest = true;

            if (isTest)
            {
                return new List<IDataSeeder>
                {        
                    new SystemParameterSeeder(),    
                    new PermissionSeeder(),                    
                    new RoleSeeder(),
                    new RolePermissionSeeder(),     // 在 Role 和 Permission 之後執行
                    new PaymentMethodSeeder(),      
                    new PrepaymentTypeSeeder(),     
                    new EmployeeSeeder(),
                    new UnitSeeder(),      
                    new InventorySeeder(),
                    new SalesReturnReasonSeeder(),  
                    new CurrencySeeder(),          
                    new PaperSettingSeeder(),
                    new ReportPrintConfigurationSeeder(),  // 報表列印配置（在紙張設定之後）
                    new VehicleTypeSeeder(),               // 車輛類型（車型基礎資料）
                };
            }

            return new List<IDataSeeder>
            {
                new SystemParameterSeeder(),    
                new RoleSeeder(),
                new RolePermissionSeeder(),
                new PaymentMethodSeeder(),
            };
        }
    }
}
