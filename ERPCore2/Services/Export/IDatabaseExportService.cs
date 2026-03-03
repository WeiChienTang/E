using ERPCore2.Models.Export;
using ERPCore2.Models.Import;

namespace ERPCore2.Services.Export
{
    /// <summary>
    /// 資料庫匯出服務介面 — SuperAdmin 工具
    /// 提供 Entity → Excel 的匯出功能
    /// </summary>
    public interface IDatabaseExportService
    {
        /// <summary>
        /// 取得 AppDbContext 中所有 DbSet 的資訊清單（複用 Import 的 EntityTableInfo）
        /// </summary>
        List<EntityTableInfo> GetEntityTableList();

        /// <summary>
        /// 取得指定 Entity 的所有可匯出屬性資訊（包含 BaseEntity 欄位）
        /// </summary>
        /// <param name="dbSetName">DbSet 名稱（如 "Customers"）</param>
        List<EntityPropertyInfo> GetExportableProperties(string dbSetName);

        /// <summary>
        /// 取得指定資料表的資料筆數
        /// </summary>
        /// <param name="dbSetName">DbSet 名稱</param>
        Task<int> GetTableRowCountAsync(string dbSetName);

        /// <summary>
        /// 匯出單一資料表為 Excel
        /// </summary>
        /// <param name="dbSetName">DbSet 名稱</param>
        /// <param name="progressCallback">進度回調（百分比 0~100）</param>
        Task<ExportResult> ExportSingleTableAsync(string dbSetName, Action<int>? progressCallback = null);

        /// <summary>
        /// 匯出多個資料表為 Excel（每個表一個 Worksheet）
        /// </summary>
        /// <param name="dbSetNames">要匯出的 DbSet 名稱清單</param>
        /// <param name="progressCallback">進度回調（百分比 0~100）</param>
        Task<ExportResult> ExportMultipleTablesAsync(List<string> dbSetNames, Action<int>? progressCallback = null);

        /// <summary>
        /// 匯出全部資料表為 Excel（每個表一個 Worksheet）
        /// </summary>
        /// <param name="progressCallback">進度回調（百分比 0~100）</param>
        Task<ExportResult> ExportAllTablesAsync(Action<int>? progressCallback = null);
    }
}
