using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 紙張設定服務介面
    /// </summary>
    public interface IPaperSettingService : IGenericManagementService<PaperSetting>
    {
        /// <summary>
        /// 取得系統預設紙張設定
        /// </summary>
        /// <returns>預設紙張設定，若無則返回第一個啟用的紙張設定</returns>
        Task<PaperSetting?> GetDefaultPaperSettingAsync();

        /// <summary>
        /// 根據編號取得紙張設定
        /// </summary>
        /// <param name="code">紙張設定編號</param>
        /// <returns>紙張設定資料</returns>
        Task<PaperSetting?> GetByCodeAsync(string code);

        /// <summary>
        /// 檢查編號是否已存在
        /// </summary>
        /// <param name="code">紙張設定編號</param>
        /// <param name="excludeId">排除的ID（編輯時使用）</param>
        /// <returns>是否已存在</returns>
        Task<bool> IsCodeExistsAsync(string code, int? excludeId = null);

        /// <summary>
        /// 檢查紙張設定編號是否已存在
        /// </summary>
        /// <param name="code">紙張設定編號</param>
        /// <param name="excludeId">排除的ID（編輯時使用）</param>
        /// <returns>是否已存在</returns>
        Task<bool> IsPaperSettingCodeExistsAsync(string code, int? excludeId = null);

        /// <summary>
        /// 取得所有啟用的紙張設定
        /// </summary>
        /// <returns>啟用的紙張設定清單</returns>
        Task<List<PaperSetting>> GetActivePaperSettingsAsync();

        /// <summary>
        /// 根據紙張類型取得紙張設定
        /// </summary>
        /// <param name="paperType">紙張類型</param>
        /// <returns>對應的紙張設定清單</returns>
        Task<List<PaperSetting>> GetByPaperTypeAsync(string paperType);

        /// <summary>
        /// 設定為系統預設（會取消其他設定的預設狀態）
        /// </summary>
        /// <param name="id">紙張設定ID</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> SetAsDefaultAsync(int id);

        /// <summary>
        /// 驗證紙張尺寸是否合理
        /// </summary>
        /// <param name="width">寬度</param>
        /// <param name="height">高度</param>
        /// <returns>是否有效</returns>
        bool ValidatePaperSize(decimal width, decimal height);

        /// <summary>
        /// 驗證邊距設定是否合理
        /// </summary>
        /// <param name="paperSetting">紙張設定</param>
        /// <returns>是否有效</returns>
        bool ValidateMargins(PaperSetting paperSetting);

        /// <summary>
        /// 取得常用紙張規格
        /// </summary>
        /// <returns>常用紙張規格字典（名稱 -> 尺寸）</returns>
        Dictionary<string, (decimal Width, decimal Height)> GetStandardPaperSizes();
    }
}
