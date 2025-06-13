using ERPCore2.Data.Context;

namespace ERPCore2.Data.SeedDataManager.Interfaces
{
    /// <summary>
    /// 資料種子器介面
    /// </summary>
    public interface IDataSeeder
    {
        /// <summary>
        /// 執行順序（數字越小越早執行）
        /// </summary>
        int Order { get; }

        /// <summary>
        /// 種子器名稱
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 執行資料種子初始化
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        /// <returns></returns>
        Task SeedAsync(AppDbContext context);
    }
}
