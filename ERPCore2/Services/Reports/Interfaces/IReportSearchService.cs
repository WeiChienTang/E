using ERPCore2.Models.Reports;

namespace ERPCore2.Services.Reports.Interfaces;

/// <summary>
/// 報表搜尋服務介面
/// 提供報表定義的搜尋和查詢功能
/// </summary>
public interface IReportSearchService
{
    /// <summary>
    /// 搜尋報表定義
    /// </summary>
    /// <param name="searchTerm">搜尋關鍵字（支援名稱、描述、分類）</param>
    /// <param name="onlyEnabled">是否僅顯示已啟用的報表</param>
    /// <returns>符合搜尋條件的報表定義清單</returns>
    List<ReportDefinition> SearchReports(string searchTerm, bool onlyEnabled = true);

    /// <summary>
    /// 取得所有報表定義
    /// </summary>
    /// <param name="onlyEnabled">是否僅顯示已啟用的報表</param>
    /// <returns>報表定義清單</returns>
    List<ReportDefinition> GetAllReports(bool onlyEnabled = true);

    /// <summary>
    /// 根據分類取得報表定義
    /// </summary>
    /// <param name="category">報表分類（Customer、Supplier、Sales 等）</param>
    /// <param name="onlyEnabled">是否僅顯示已啟用的報表</param>
    /// <returns>指定分類的報表定義清單</returns>
    List<ReportDefinition> GetReportsByCategory(string category, bool onlyEnabled = true);

    /// <summary>
    /// 取得所有報表分類
    /// </summary>
    /// <returns>報表分類清單</returns>
    List<string> GetAllCategories();

    /// <summary>
    /// 取得報表分類的中文顯示名稱
    /// </summary>
    /// <param name="category">報表分類代碼</param>
    /// <returns>中文顯示名稱</returns>
    string GetCategoryDisplayName(string category);
}
