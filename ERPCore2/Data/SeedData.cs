using ERPCore2.Data.Context;
using ERPCore2.Data.SeedDataManager.Interfaces;
using ERPCore2.Data.SeedDataManager.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

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

            // 確保資料庫存在並執行遷移（智慧初始化，支援全新資料庫）
            await EnsureDatabaseSchemaAsync(context);

            // 從設定檔讀取是否啟用測試資料
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            // EnableRetryOnFailure 需要透過 CreateExecutionStrategy 包裝 user-initiated transaction
            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // 取得所有種子器並按順序執行
                    var seeders = GetAllSeeders(configuration).OrderBy(s => s.Order).ToList();

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
            });
        }

        /// <summary>
        /// 智慧資料庫初始化：
        /// - 全新 DB（不存在 / 空白）→ CreateTablesAsync + 同步 Migration 歷史
        /// - 現有 DB（有資料表）     → 一般 MigrateAsync
        /// </summary>
        private static async Task EnsureDatabaseSchemaAsync(AppDbContext context)
        {
            var databaseCreator = context.Database.GetService<IRelationalDatabaseCreator>();

            // 資料庫不存在 → 全部從頭建立
            if (!await databaseCreator.ExistsAsync())
            {
                await databaseCreator.CreateAsync();
                await databaseCreator.CreateTablesAsync();
                await SyncMigrationHistoryAsync(context);
                return;
            }

            // 資料庫存在但完全空白 → 建立所有資料表
            if (!await databaseCreator.HasTablesAsync())
            {
                await databaseCreator.CreateTablesAsync();
                await SyncMigrationHistoryAsync(context);
                return;
            }

            // 資料庫有現有資料表 → 正常套用待處理的 Migration
            await context.Database.MigrateAsync();
        }

        /// <summary>
        /// 建立 __EFMigrationsHistory 表格並將所有已知 Migration 標記為已套用。
        /// 用於 EnsureCreated / CreateTables 路徑，避免後續 MigrateAsync 重複套用。
        /// </summary>
        private static async Task SyncMigrationHistoryAsync(AppDbContext context)
        {
            // 修正：ProductVersion 曾被全域 Product→Item 替換誤改為 ItemVersion，此處修復現有資料庫
            await context.Database.ExecuteSqlRawAsync(@"
                IF EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = '__EFMigrationsHistory' AND COLUMN_NAME = 'ItemVersion'
                )
                EXEC sp_rename '[__EFMigrationsHistory].[ItemVersion]', 'ProductVersion', 'COLUMN'");

            // 建立 Migration 歷史表（若不存在）
            await context.Database.ExecuteSqlRawAsync(@"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_NAME = '__EFMigrationsHistory'
                )
                CREATE TABLE [__EFMigrationsHistory] (
                    [MigrationId]    nvarchar(150) NOT NULL,
                    [ProductVersion] nvarchar(32)  NOT NULL,
                    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                )");

            // 將所有已定義的 Migration 標記為已套用
            var allMigrations = context.Database.GetMigrations().ToList();
            foreach (var migrationId in allMigrations)
            {
                await context.Database.ExecuteSqlRawAsync(
                    "IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = {0}) " +
                    "INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ({0}, {1})",
                    migrationId, "9.0.0");
            }
        }

        /// <summary>
        /// 取得所有種子器
        /// </summary>
        /// <returns>種子器集合</returns>
        private static IEnumerable<IDataSeeder> GetAllSeeders(IConfiguration configuration)
        {
            bool isTest = true;

            // 從 appsettings.json 讀取（Development = true，Production = false，預設 false）
            bool enableTestData = configuration.GetValue<bool>("SeedData:EnableTestData", false);

            if (isTest)
            {
                var seeders = new List<IDataSeeder>
                {
                    new SystemParameterSeeder(),
                    new CompanyModuleSeeder(),              // 公司模組（從 NavigationConfig 自動衍生）
                    new PermissionSeeder(),
                    new RoleSeeder(),
                    new RolePermissionSeeder(),     // 在 Role 和 Permission 之後執行
                    new PaymentMethodSeeder(),
                    new PrepaymentTypeSeeder(),
                    new EmployeeSeeder(),
                    new UnitSeeder(),
                    new InventorySeeder(),
                    new SalesReturnReasonSeeder(),
                    new PurchaseReturnReasonSeeder(),
                    new CurrencySeeder(),
                    new PaperSettingSeeder(),
                    new ReportPrintConfigurationSeeder(),  // 報表列印配置（在紙張設定之後）
                    new VehicleTypeSeeder(),               // 車輛類型（車型基礎資料）
                    new AccountItemSeeder(),               // 會計科目表（商業會計項目表 112 年度）
                    new AccountItemCashFlowCategorySeeder(), // 設定會計科目現金流量分類（FN014 間接法）
                    new BankSeeder(),                      // 台灣金融機構代碼（30 家主要銀行）
                    new DocumentCategorySeeder(),          // 檔案分類（政府公文、廠商合約等）
                    new PayrollItemSeeder(),               // 薪資項目（16個預設項目）
                    new InsuranceRateSeeder(),             // 保費費率（114年費率）
                    new CodeSettingSeeder(),               // 代碼自動產生設定（12 個模組預設值）
                };

                if (enableTestData)
                {
                    seeders.Add(new TestEmployeeSeeder());  // 測試員工 20 筆（EMP001-EMP020）
                    seeders.Add(new TestCustomerSeeder());  // 測試客戶 20 筆（TC001-TC020）
                    seeders.Add(new TestSupplierSeeder());  // 測試廠商 20 筆（TS001-TS020）
                    seeders.Add(new TestItemSeeder());             // 測試品項 20 筆 + 5 個分類（P001-P020）
                    seeders.Add(new TestItemCompositionSeeder()); // 測試 BOM 2 筆（依賴 TestItemSeeder）
                }

                return seeders;
            }

            return new List<IDataSeeder>
            {
                new SystemParameterSeeder(),
                new CompanyModuleSeeder(),              // 公司模組（從 NavigationConfig 自動衍生）
                new PermissionSeeder(),
                new RoleSeeder(),
                new RolePermissionSeeder(),
                new PaymentMethodSeeder(),
                new BankSeeder(),                       // 台灣金融機構代碼
                new DocumentCategorySeeder(),           // 檔案分類
            };
        }
    }
}
