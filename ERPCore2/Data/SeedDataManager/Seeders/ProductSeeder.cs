using ERPCore2.Data.Context;
using ERPCore2.Data.SeedDataManager.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 產品資料種子器
    /// </summary>
    public class ProductSeeder : IDataSeeder
    {
        public int Order => 4;
        public string Name => "產品資料";

        public async Task SeedAsync(AppDbContext context)
        {
            // 預留給未來產品資料的種子初始化
            await Task.CompletedTask;
        }
    }
}
