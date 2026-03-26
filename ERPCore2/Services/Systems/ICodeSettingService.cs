using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 代碼自動產生設定服務介面
    /// </summary>
    public interface ICodeSettingService : IGenericManagementService<CodeSetting>
    {
        /// <summary>
        /// 取得所有模組設定（用於 UI 列表）
        /// </summary>
        Task<List<CodeSetting>> GetAllSettingsAsync();

        /// <summary>
        /// 取得特定模組設定
        /// </summary>
        Task<CodeSetting?> GetByModuleKeyAsync(string moduleKey);

        /// <summary>
        /// 產生並更新代碼（含樂觀鎖重試）
        /// IsAutoCode = false 時回傳 null，呼叫端保留原本的手動輸入邏輯
        /// </summary>
        Task<ServiceResult<string?>> GenerateCodeAsync(string moduleKey);

        /// <summary>
        /// 批次儲存所有設定
        /// </summary>
        Task<ServiceResult> SaveAllAsync(List<CodeSetting> settings);

        /// <summary>
        /// 重置特定模組的序號（SuperAdmin 操作）
        /// </summary>
        Task<ServiceResult> ResetSeqAsync(string moduleKey);

        /// <summary>
        /// 預覽代碼（不實際遞增序號，用於 UI 即時預覽）
        /// </summary>
        string PreviewCode(string prefix, string formatTemplate, int seq = 1);

        /// <summary>
        /// 從格式樣板推斷重置規則
        /// </summary>
        string InferResetMode(string formatTemplate);
    }
}
