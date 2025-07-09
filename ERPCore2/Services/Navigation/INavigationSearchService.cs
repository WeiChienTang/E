using ERPCore2.Models;

namespace ERPCore2.Services;

/// <summary>
/// 導航搜尋服務介面
/// </summary>
public interface INavigationSearchService
{
    /// <summary>
    /// 獲取所有導航項目
    /// </summary>
    List<NavigationItem> GetAllNavigationItems();
    
    /// <summary>
    /// 搜尋導航項目
    /// </summary>
    /// <param name="searchTerm">搜尋關鍵字</param>
    /// <returns>符合搜尋條件的導航項目清單</returns>
    List<NavigationItem> SearchNavigationItems(string searchTerm);
    
    /// <summary>
    /// 根據分類獲取導航項目
    /// </summary>
    /// <param name="category">分類名稱</param>
    /// <returns>指定分類的導航項目清單</returns>
    List<NavigationItem> GetNavigationItemsByCategory(string category);
    
    /// <summary>
    /// 獲取所有分類
    /// </summary>
    /// <returns>所有分類清單</returns>
    List<string> GetAllCategories();
    
    /// <summary>
    /// 根據權限篩選導航項目
    /// </summary>
    /// <param name="userPermissions">使用者權限清單</param>
    /// <returns>使用者有權限的導航項目</returns>
    List<NavigationItem> GetNavigationItemsByPermissions(List<string> userPermissions);
}
