using ERPCore2.Data.Entities;
using ERPCore2.Services;

// 使用別名來避免命名衝突
using EntityPurchaseReturnReason = ERPCore2.Data.Entities.PurchaseReturnReason;

namespace ERPCore2.Services
{
    /// <summary>
    /// 進貨退出原因服務介面
    /// </summary>
    public interface IPurchaseReturnReasonService : IGenericManagementService<EntityPurchaseReturnReason>
    {
        /// <summary>
        /// 取得所有啟用的退出原因（按排序順序）
        /// </summary>
        Task<List<EntityPurchaseReturnReason>> GetActiveReasonsAsync();

        /// <summary>
        /// 檢查退出原因編號是否已存在
        /// </summary>
        /// <param name="code">退出原因編號</param>
        /// <param name="excludeId">排除的ID（編輯時使用）</param>
        /// <returns>如果編號已存在回傳true，否則回傳false</returns>
        Task<bool> IsPurchaseReturnReasonCodeExistsAsync(string code, int? excludeId = null);
    }
}
