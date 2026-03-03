using ERPCore2.Models.Import;

namespace ERPCore2.Services.Import
{
    /// <summary>
    /// 資料庫匯入服務介面 — SuperAdmin 工具
    /// 提供 Excel → Entity 的匯入流程所需的所有方法
    /// </summary>
    public interface IDatabaseImportService
    {
        /// <summary>
        /// 取得 AppDbContext 中所有 DbSet 的資訊清單
        /// </summary>
        List<EntityTableInfo> GetEntityTableList();

        /// <summary>
        /// 取得指定 Entity 的可對應屬性資訊（排除 BaseEntity 共用欄位與導航屬性）
        /// </summary>
        /// <param name="dbSetName">DbSet 名稱（如 "Customers"）</param>
        List<EntityPropertyInfo> GetEntityProperties(string dbSetName);

        /// <summary>
        /// 解析 Excel 檔案（讀取標頭與資料行）
        /// </summary>
        /// <param name="stream">Excel 檔案的 Stream</param>
        /// <param name="fileName">檔案名稱（用於驗證副檔名）</param>
        ExcelParseResult ParseExcel(Stream stream, string fileName);

        /// <summary>
        /// 自動配對欄位（名稱相似度演算法）
        /// </summary>
        /// <param name="targetProperties">目標 Entity 屬性清單</param>
        /// <param name="sourceHeaders">來源 Excel 欄位標頭清單</param>
        /// <returns>自動配對後的 ColumnMapping 清單</returns>
        List<ColumnMapping> AutoMapColumns(List<EntityPropertyInfo> targetProperties, List<string> sourceHeaders);

        /// <summary>
        /// 取得指定屬性的智慧預設值字串（依型別自動產生合理預設值）
        /// </summary>
        /// <param name="property">目標屬性資訊</param>
        /// <returns>預設值字串，或 null 表示無法產生</returns>
        string? GetSmartDefaultValue(EntityPropertyInfo property);

        /// <summary>
        /// 產生預覽資料（前 N 筆轉換結果 + 驗證）
        /// </summary>
        /// <param name="dbSetName">目標 DbSet 名稱</param>
        /// <param name="mappings">欄位對應清單</param>
        /// <param name="sourceData">來源 Excel 資料行</param>
        /// <param name="previewCount">預覽行數（預設 10）</param>
        List<ImportPreviewRow> GeneratePreview(
            string dbSetName,
            List<ColumnMapping> mappings,
            List<Dictionary<string, string?>> sourceData,
            int previewCount = 10);

        /// <summary>
        /// 驗證所有必填欄位是否都已有對應或預設值
        /// </summary>
        /// <param name="mappings">欄位對應清單</param>
        /// <returns>未解決的必填欄位清單（空表示全部通過）</returns>
        List<string> ValidateMappings(List<ColumnMapping> mappings);

        /// <summary>
        /// 執行匯入（EF Core + Transaction）
        /// </summary>
        /// <param name="dbSetName">目標 DbSet 名稱</param>
        /// <param name="mappings">欄位對應清單</param>
        /// <param name="sourceData">來源 Excel 資料行</param>
        /// <param name="currentUserId">當前使用者 ID（填入 CreatedBy）</param>
        /// <param name="progressCallback">進度回調（百分比 0~100）</param>
        Task<ImportResult> ExecuteImportAsync(
            string dbSetName,
            List<ColumnMapping> mappings,
            List<Dictionary<string, string?>> sourceData,
            string? currentUserId,
            Action<int>? progressCallback = null);
    }
}
