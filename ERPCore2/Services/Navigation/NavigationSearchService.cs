using ERPCore2.Models;
using ERPCore2.Data.Navigation;

namespace ERPCore2.Services;

/// <summary>
/// 導航搜尋服務實作
/// 提供系統功能導航項目的搜尋和管理功能
/// 注意: 此服務現在使用 NavigationConfig 作為唯一資料來源
/// </summary>
public class NavigationSearchService : INavigationSearchService
{
    private readonly List<NavigationItem> _navigationItems;

    public NavigationSearchService()
    {
        // 從 NavigationConfig 取得扁平化的導航項目（用於搜尋）
        _navigationItems = NavigationConfig.GetFlattenedNavigationItems();
    }

    /// <summary>
    /// 獲取所有導航項目
    /// </summary>
    public List<NavigationItem> GetAllNavigationItems()
    {
        return _navigationItems.ToList();
    }

    /// <summary>
    /// 搜尋導航項目
    /// </summary>
    /// <param name="searchTerm">搜尋關鍵字</param>
    /// <returns>符合搜尋條件的導航項目清單</returns>
    public List<NavigationItem> SearchNavigationItems(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return GetAllNavigationItems();
        }

        var results = new List<NavigationItem>();
        searchTerm = searchTerm.Trim().ToLower();

        foreach (var item in _navigationItems)
        {
            // 搜尋父級項目
            if (MatchesSearchTerm(item, searchTerm))
            {
                results.Add(item);
            }
            
            // 搜尋子級項目
            foreach (var child in item.Children)
            {
                if (MatchesSearchTerm(child, searchTerm))
                {
                    results.Add(child);
                }
            }
        }

        return results.Distinct().ToList();
    }

    /// <summary>
    /// 根據分類獲取導航項目
    /// </summary>
    /// <param name="category">分類名稱</param>
    /// <returns>指定分類的導航項目清單</returns>
    public List<NavigationItem> GetNavigationItemsByCategory(string category)
    {
        var results = new List<NavigationItem>();
        
        foreach (var item in _navigationItems)
        {
            if (item.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(item);
            }
            
            // 檢查子項目
            foreach (var child in item.Children)
            {
                if (child.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(child);
                }
            }
        }
        
        return results;
    }

    /// <summary>
    /// 獲取所有分類
    /// </summary>
    /// <returns>所有分類清單</returns>
    public List<string> GetAllCategories()
    {
        var categories = new HashSet<string>();
        
        foreach (var item in _navigationItems)
        {
            if (!string.IsNullOrEmpty(item.Category))
            {
                categories.Add(item.Category);
            }
            
            foreach (var child in item.Children)
            {
                if (!string.IsNullOrEmpty(child.Category))
                {
                    categories.Add(child.Category);
                }
            }
        }
        
        return categories.OrderBy(c => c).ToList();
    }

    /// <summary>
    /// 根據權限篩選導航項目
    /// </summary>
    /// <param name="userPermissions">使用者權限清單</param>
    /// <returns>使用者有權限的導航項目</returns>
    public List<NavigationItem> GetNavigationItemsByPermissions(List<string> userPermissions)
    {
        var results = new List<NavigationItem>();
        
        foreach (var item in _navigationItems)
        {
            // 如果項目不需要權限或使用者有相應權限
            if (string.IsNullOrEmpty(item.RequiredPermission) || 
                userPermissions.Contains(item.RequiredPermission))
            {
                // 創建項目副本並篩選子項目
                var filteredItem = new NavigationItem
                {
                    Name = item.Name,
                    Description = item.Description,
                    Route = item.Route,
                    IconClass = item.IconClass,
                    Category = item.Category,
                    RequiredPermission = item.RequiredPermission,
                    SearchKeywords = item.SearchKeywords.ToList(),
                    IsParent = item.IsParent,
                    Children = new List<NavigationItem>()
                };
                
                // 篩選子項目
                foreach (var child in item.Children)
                {
                    if (string.IsNullOrEmpty(child.RequiredPermission) || 
                        userPermissions.Contains(child.RequiredPermission))
                    {
                        filteredItem.Children.Add(child);
                    }
                }
                
                results.Add(filteredItem);
            }
        }
        
        return results;
    }

    #region 私有方法

    /// <summary>
    /// 檢查導航項目是否符合搜尋條件
    /// </summary>
    private bool MatchesSearchTerm(NavigationItem item, string searchTerm)
    {
        // 檢查名稱
        if (item.Name.ToLower().Contains(searchTerm))
            return true;
            
        // 檢查描述
        if (item.Description.ToLower().Contains(searchTerm))
            return true;
            
        // 檢查分類
        if (item.Category.ToLower().Contains(searchTerm))
            return true;
            
        // 檢查搜尋關鍵字
        if (item.SearchKeywords.Any(keyword => keyword.ToLower().Contains(searchTerm)))
            return true;

        return false;
    }

    #endregion
}
