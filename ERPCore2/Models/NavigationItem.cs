namespace ERPCore2.Models;

/// <summary>
/// 導航項目模型
/// </summary>
public class NavigationItem
{
    /// <summary>
    /// 功能名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 功能描述
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// 路由路徑
    /// </summary>
    public string Route { get; set; } = string.Empty;
    
    /// <summary>
    /// 圖示 CSS 類別
    /// </summary>
    public string IconClass { get; set; } = string.Empty;
    
    /// <summary>
    /// 分類
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// 權限要求
    /// </summary>
    public string? RequiredPermission { get; set; }
    
    /// <summary>
    /// 選單識別鍵（用於權限收集）
    /// </summary>
    public string? MenuKey { get; set; }
    
    /// <summary>
    /// 搜尋關鍵字（包含同義詞等）
    /// </summary>
    public List<string> SearchKeywords { get; set; } = new();
    
    /// <summary>
    /// 是否為父級選單項目
    /// </summary>
    public bool IsParent { get; set; } = false;
    
    /// <summary>
    /// 子選單項目
    /// </summary>
    public List<NavigationItem> Children { get; set; } = new();
}