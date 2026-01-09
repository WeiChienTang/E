using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 文字訊息範本服務介面
    /// </summary>
    public interface ITextMessageTemplateService : IGenericManagementService<TextMessageTemplate>
    {
        /// <summary>
        /// 根據範本代碼取得範本
        /// </summary>
        /// <param name="templateCode">範本代碼（如：PurchaseOrder, SalesOrder）</param>
        /// <returns>啟用中的範本，若無則回傳 null</returns>
        Task<TextMessageTemplate?> GetByTemplateCodeAsync(string templateCode);

        /// <summary>
        /// 取得所有啟用的範本
        /// </summary>
        /// <returns>啟用中的範本列表</returns>
        Task<List<TextMessageTemplate>> GetActiveTemplatesAsync();

        /// <summary>
        /// 檢查範本代碼是否已存在
        /// </summary>
        /// <param name="templateCode">範本代碼</param>
        /// <param name="excludeId">排除的 ID（用於編輯時檢查）</param>
        /// <returns>是否存在</returns>
        Task<bool> IsTemplateCodeExistsAsync(string templateCode, int? excludeId = null);
        
        /// <summary>
        /// 檢查範本 Code 是否已存在（符合 EntityCodeGenerationHelper 約定）
        /// </summary>
        /// <param name="code">範本 Code</param>
        /// <param name="excludeId">排除的 ID（用於編輯時檢查）</param>
        /// <returns>是否存在</returns>
        Task<bool> IsTextMessageTemplateCodeExistsAsync(string code, int? excludeId = null);
    }
}
