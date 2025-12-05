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
                    new CompanySeeder(),            // 公司資料 - 基礎資料，優先執行
                    new SystemParameterSeeder(),    // 系統參數 - 系統基礎設定
                    new PermissionSeeder(),
                    new RoleSeeder(),
                    new RolePermissionSeeder(),
                    new PaymentMethodSeeder(),      // 付款方式
                    new PrepaymentTypeSeeder(),     // 預收付款項類型
                    new BankSeeder(),               // 銀行別
                    new PaperSettingSeeder(),       // 紙張設定
                    new PrinterConfigurationSeeder(), // 印表機配置
                    new EmployeeSeeder(),
                    new EmployeePositionSeeder(),
                    new DepartmentSeeder(),
                    new ColorSeeder(),
                    new MaterialSeeder(),
                    new CompositionCategorySeeder(), // 合成表類型
                    new ProductCategorySeeder(),    // 商品類別 - 必須在 ProductSeeder 之前
                    new UnitSeeder(),
                    new UnitConversionSeeder(),     // 單位轉換關係 - 必須在 UnitSeeder 之後
                    new SizeSeeder(),
                    new WarehouseSeeder(),
                    new CustomerSeeder(),           // 依賴 CustomerTypes
                    new SupplierSeeder(),           // 依賴 SupplierTypes
                    new ProductSeeder(),            // 依賴 ProductCategory, Unit, Supplier
                    new InventorySeeder(),
                    new WeatherSeeder(),
                    new SalesReturnReasonSeeder(),  // 銷貨退貨原因
                    new CurrencySeeder(),           // 貨幣資料
                };
            }

            return new List<IDataSeeder>
            {
                new SystemParameterSeeder(),    
                new RoleSeeder(),
                new RolePermissionSeeder(),
                new PaymentMethodSeeder(),
                new EmployeePositionSeeder(),
            };
        }
    }
}
