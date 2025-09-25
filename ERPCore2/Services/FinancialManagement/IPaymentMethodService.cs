using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 付款方式服務介面
    /// </summary>
    public interface IPaymentMethodService : IGenericManagementService<PaymentMethod>
    {
        /// <summary>
        /// 檢查付款方式代碼是否已存在
        /// </summary>
        /// <param name="code">付款方式代碼</param>
        /// <param name="excludeId">排除的ID（用於更新時檢查）</param>
        /// <returns>是否存在</returns>
        Task<bool> IsPaymentMethodCodeExistsAsync(string code, int? excludeId = null);

        /// <summary>
        /// 檢查付款方式名稱是否已存在
        /// </summary>
        /// <param name="name">付款方式名稱</param>
        /// <param name="excludeId">排除的ID（用於更新時檢查）</param>
        /// <returns>是否存在</returns>
        new Task<bool> IsNameExistsAsync(string name, int? excludeId = null);

        /// <summary>
        /// 獲取預設付款方式
        /// </summary>
        /// <returns>預設付款方式</returns>
        Task<PaymentMethod?> GetDefaultAsync();

        /// <summary>
        /// 設定預設付款方式
        /// </summary>
        /// <param name="paymentMethodId">付款方式ID</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> SetDefaultAsync(int paymentMethodId);
    }
}