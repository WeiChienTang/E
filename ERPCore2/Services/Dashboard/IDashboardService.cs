using ERPCore2.Data.Entities;
using ERPCore2.Models.Navigation;

namespace ERPCore2.Services
{
    /// <summary>
    /// 儀表板服務介面 - 管理首頁動態面板與項目配置
    /// </summary>
    public interface IDashboardService
    {
        // ===== 面板管理 =====

        /// <summary>
        /// 取得員工的所有面板（含面板內的項目）
        /// 如果沒有任何面板且未初始化，則自動套用預設配置
        /// </summary>
        Task<List<DashboardPanelWithItems>> GetEmployeePanelsAsync(int employeeId);

        /// <summary>
        /// 建立新面板
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <param name="title">面板標題</param>
        /// <returns>新建立的面板（含空項目清單）</returns>
        Task<ServiceResult<DashboardPanelWithItems>> CreatePanelAsync(int employeeId, string title);

        /// <summary>
        /// 更新面板標題
        /// </summary>
        Task<ServiceResult> UpdatePanelTitleAsync(int panelId, string title);

        /// <summary>
        /// 刪除面板（連同其下所有項目）
        /// </summary>
        Task<ServiceResult> DeletePanelAsync(int panelId);

        /// <summary>
        /// 更新面板排序
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <param name="panelIds">按新排序排列的面板ID清單</param>
        Task<ServiceResult> UpdatePanelSortOrderAsync(int employeeId, List<int> panelIds);

        // ===== 項目管理 =====

        /// <summary>
        /// 取得員工有權限使用的所有導航項目（用於「新增捷徑」選擇畫面）
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <param name="isQuickAction">true 取得快速功能項目，false 取得頁面連結項目</param>
        Task<List<NavigationItem>> GetAvailableWidgetsAsync(int employeeId, bool isQuickAction);

        /// <summary>
        /// 取得面板內已存在的項目識別鍵（用於排除已加入的項目）
        /// </summary>
        Task<HashSet<string>> GetPanelExistingKeysAsync(int panelId);

        /// <summary>
        /// 批次新增項目到面板
        /// </summary>
        Task<ServiceResult> AddWidgetBatchAsync(int panelId, List<string> navigationItemKeys);

        /// <summary>
        /// 從面板移除一個項目
        /// </summary>
        Task<ServiceResult> RemoveWidgetAsync(int configId);

        /// <summary>
        /// 更新面板內項目的排序
        /// </summary>
        /// <param name="panelId">面板ID</param>
        /// <param name="configIds">按新排序排列的配置ID清單</param>
        Task<ServiceResult> UpdateItemSortOrderAsync(int panelId, List<int> configIds);

        // ===== 初始化與重置 =====

        /// <summary>
        /// 根據員工權限，建立預設面板與項目配置
        /// </summary>
        Task<List<DashboardPanelWithItems>> InitializeDefaultDashboardAsync(int employeeId);

        /// <summary>
        /// 重置指定面板為預設配置（根據面板標題匹配預設定義）
        /// </summary>
        Task<ServiceResult> ResetPanelToDefaultAsync(int panelId);

        /// <summary>
        /// 重置所有面板為預設配置
        /// </summary>
        Task<ServiceResult> ResetAllPanelsToDefaultAsync(int employeeId);
    }

    /// <summary>
    /// 面板與其項目的組合類別
    /// </summary>
    public class DashboardPanelWithItems
    {
        /// <summary>
        /// 面板實體
        /// </summary>
        public EmployeeDashboardPanel Panel { get; set; } = null!;

        /// <summary>
        /// 面板內的項目清單（已對應導航項目）
        /// </summary>
        public List<DashboardConfigWithNavItem> Items { get; set; } = new();
    }

    /// <summary>
    /// 儀表板配置與導航項目的組合類別
    /// </summary>
    public class DashboardConfigWithNavItem
    {
        /// <summary>
        /// 員工儀表板配置
        /// </summary>
        public EmployeeDashboardConfig Config { get; set; } = null!;

        /// <summary>
        /// 對應的導航項目（從 NavigationConfig 取得）
        /// </summary>
        public NavigationItem NavigationItem { get; set; } = null!;
    }
}
