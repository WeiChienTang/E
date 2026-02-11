using ERPCore2.Data.Entities;
using ERPCore2.Models.Navigation;

namespace ERPCore2.Services
{
    /// <summary>
    /// 儀表板服務介面 - 管理首頁小工具配置
    /// 使用 NavigationConfig 作為小工具資料來源
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// 取得該員工有權限使用的所有導航項目（用於「新增捷徑」選擇畫面）
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <returns>有權限的導航項目清單</returns>
        Task<List<NavigationItem>> GetAvailableWidgetsAsync(int employeeId);

        /// <summary>
        /// 取得該員工目前的首頁配置（已選用的捷徑 + 排序）
        /// 如果沒有任何配置則自動建立預設配置
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <returns>員工的儀表板配置清單（包含對應的 NavigationItem）</returns>
        Task<List<DashboardConfigWithNavItem>> GetEmployeeDashboardAsync(int employeeId);

        /// <summary>
        /// 新增一個捷徑到員工首頁
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <param name="navigationItemKey">導航項目識別鍵</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> AddWidgetAsync(int employeeId, string navigationItemKey);

        /// <summary>
        /// 批次新增捷徑到員工首頁
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <param name="navigationItemKeys">導航項目識別鍵清單</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> AddWidgetBatchAsync(int employeeId, List<string> navigationItemKeys);

        /// <summary>
        /// 從員工首頁移除一個捷徑
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <param name="configId">配置ID</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> RemoveWidgetAsync(int employeeId, int configId);

        /// <summary>
        /// 批次更新排序（拖曳排序後呼叫，configIds 的順序即為新排序）
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <param name="configIds">按新排序排列的配置ID清單</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> UpdateSortOrderAsync(int employeeId, List<int> configIds);

        /// <summary>
        /// 根據員工權限，從預設捷徑清單建立預設配置
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <returns>新建立的配置清單</returns>
        Task<List<DashboardConfigWithNavItem>> InitializeDefaultDashboardAsync(int employeeId);

        /// <summary>
        /// 重置為預設配置（清除現有配置後重新初始化）
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> ResetToDefaultAsync(int employeeId);
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
