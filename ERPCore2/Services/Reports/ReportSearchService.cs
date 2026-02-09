using ERPCore2.Data;
using ERPCore2.Models.Reports;
using ERPCore2.Services.Reports.Interfaces;

namespace ERPCore2.Services.Reports;

/// <summary>
/// 報表搜尋服務實作
/// 提供報表定義的搜尋和查詢功能
/// </summary>
public class ReportSearchService : IReportSearchService
{
    private readonly List<ReportDefinition> _reportDefinitions;
    
    /// <summary>
    /// 分類對應中文名稱的字典
    /// </summary>
    private static readonly Dictionary<string, string> CategoryDisplayNames = new()
    {
        { ReportCategory.Customer, "客戶報表" },
        { ReportCategory.Supplier, "廠商報表" },
        { ReportCategory.Product, "商品報表" },
        { ReportCategory.Sales, "銷售報表" },
        { ReportCategory.Purchase, "採購報表" },
        { ReportCategory.Inventory, "庫存報表" },
        { ReportCategory.Financial, "財務報表" }
    };

    public ReportSearchService()
    {
        // 從 ReportRegistry 取得所有報表定義
        _reportDefinitions = ReportRegistry.GetAllReports();
    }

    /// <inheritdoc />
    public List<ReportDefinition> SearchReports(string searchTerm, bool onlyEnabled = true)
    {
        var query = _reportDefinitions.AsEnumerable();
        
        if (onlyEnabled)
        {
            query = query.Where(r => r.IsEnabled);
        }

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return query.OrderBy(r => r.Category).ThenBy(r => r.SortOrder).ToList();
        }

        var term = searchTerm.Trim().ToLower();
        
        return query
            .Where(r => MatchesSearchTerm(r, term))
            .OrderBy(r => r.Category)
            .ThenBy(r => r.SortOrder)
            .ToList();
    }

    /// <inheritdoc />
    public List<ReportDefinition> GetAllReports(bool onlyEnabled = true)
    {
        var query = _reportDefinitions.AsEnumerable();
        
        if (onlyEnabled)
        {
            query = query.Where(r => r.IsEnabled);
        }

        return query.OrderBy(r => r.Category).ThenBy(r => r.SortOrder).ToList();
    }

    /// <inheritdoc />
    public List<ReportDefinition> GetReportsByCategory(string category, bool onlyEnabled = true)
    {
        var query = _reportDefinitions
            .Where(r => r.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        
        if (onlyEnabled)
        {
            query = query.Where(r => r.IsEnabled);
        }

        return query.OrderBy(r => r.SortOrder).ToList();
    }

    /// <inheritdoc />
    public List<string> GetAllCategories()
    {
        return _reportDefinitions
            .Select(r => r.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }

    /// <inheritdoc />
    public string GetCategoryDisplayName(string category)
    {
        return CategoryDisplayNames.TryGetValue(category, out var displayName) 
            ? displayName 
            : category;
    }

    /// <summary>
    /// 判斷報表定義是否符合搜尋條件
    /// </summary>
    private bool MatchesSearchTerm(ReportDefinition report, string searchTerm)
    {
        // 搜尋報表名稱
        if (report.Name.ToLower().Contains(searchTerm))
            return true;

        // 搜尋報表描述
        if (!string.IsNullOrEmpty(report.Description) && 
            report.Description.ToLower().Contains(searchTerm))
            return true;

        // 搜尋報表 ID
        if (report.Id.ToLower().Contains(searchTerm))
            return true;

        // 搜尋分類（支援中英文）
        if (report.Category.ToLower().Contains(searchTerm))
            return true;

        // 搜尋分類中文名稱
        var categoryDisplayName = GetCategoryDisplayName(report.Category);
        if (categoryDisplayName.ToLower().Contains(searchTerm))
            return true;

        return false;
    }
}
