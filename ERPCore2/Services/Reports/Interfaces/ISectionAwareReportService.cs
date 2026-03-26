using ERPCore2.Data;
using ERPCore2.Models.Reports;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 支援區段選擇的報表服務介面
    /// 讓使用者可以在列印前選擇要包含的內容區段（如銀行帳戶、車輛資訊等）
    /// 與 IEntityReportService 分離，不影響現有 40+ 個報表服務
    /// </summary>
    /// <typeparam name="TEntity">實體類型，須繼承自 BaseEntity</typeparam>
    public interface ISectionAwareReportService<TEntity> where TEntity : BaseEntity
    {
        /// <summary>
        /// 取得可用的報表區段清單（已做權限/模組檢查）
        /// </summary>
        /// <param name="entityId">實體 ID</param>
        /// <returns>區段定義清單，含啟用狀態與預設勾選</returns>
        Task<List<ReportSectionDefinition>> GetAvailableSectionsAsync(int entityId);

        /// <summary>
        /// 根據選取的區段產生報表文件
        /// </summary>
        /// <param name="entityId">實體 ID</param>
        /// <param name="selectedSectionKeys">使用者勾選的區段 Key 清單</param>
        /// <returns>格式化報表文件</returns>
        Task<FormattedDocument> GenerateReportAsync(int entityId, List<string> selectedSectionKeys);
    }
}
