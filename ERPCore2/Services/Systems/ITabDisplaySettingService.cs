using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// Tab 頁籤顯示設定服務介面
    /// EBC 可配置化 Level 4 延伸：管理各模組 Tab 的顯示/隱藏與排序設定（全公司適用）
    /// </summary>
    public interface ITabDisplaySettingService
    {
        /// <summary>
        /// 取得指定模組的所有 Tab 顯示設定（快取）
        /// </summary>
        Task<List<TabDisplaySetting>> GetByModuleAsync(string targetModule);

        /// <summary>
        /// 批次儲存指定模組的 Tab 顯示設定
        /// 會自動處理新增、更新、刪除（恢復預設值的項目會被刪除）
        /// </summary>
        Task<ServiceResult> SaveModuleSettingsAsync(string targetModule, List<TabDisplaySetting> settings, string updatedBy);

        /// <summary>
        /// 恢復指定模組的所有 Tab 為預設值（刪除所有覆蓋設定）
        /// </summary>
        Task<ServiceResult> ResetModuleSettingsAsync(string targetModule);

        /// <summary>
        /// 清除指定模組的快取
        /// </summary>
        void ClearCache(string targetModule);
    }
}
