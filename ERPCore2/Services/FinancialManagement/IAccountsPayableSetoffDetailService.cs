using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 應付帳款沖款明細服務介面
    /// </summary>
    public interface IAccountsPayableSetoffDetailService : IGenericManagementService<AccountsPayableSetoffDetail>
    {
        /// <summary>
        /// 依據沖款單ID取得明細列表
        /// </summary>
        /// <param name="setoffId">沖款單ID</param>
        /// <returns>明細列表</returns>
        Task<List<AccountsPayableSetoffDetail>> GetBySetoffIdAsync(int setoffId);

        /// <summary>
        /// 依據採購進貨明細ID取得沖款明細列表
        /// </summary>
        /// <param name="purchaseReceivingDetailId">採購進貨明細ID</param>
        /// <returns>沖款明細列表</returns>
        Task<List<AccountsPayableSetoffDetail>> GetByPurchaseReceivingDetailIdAsync(int purchaseReceivingDetailId);

        /// <summary>
        /// 依據採購退回明細ID取得沖款明細列表
        /// </summary>
        /// <param name="purchaseReturnDetailId">採購退回明細ID</param>
        /// <returns>沖款明細列表</returns>
        Task<List<AccountsPayableSetoffDetail>> GetByPurchaseReturnDetailIdAsync(int purchaseReturnDetailId);

        /// <summary>
        /// 計算指定採購進貨明細的累計付款金額
        /// </summary>
        /// <param name="purchaseReceivingDetailId">採購進貨明細ID</param>
        /// <returns>累計付款金額</returns>
        Task<decimal> CalculateTotalPaidAmountByPurchaseReceivingDetailAsync(int purchaseReceivingDetailId);

        /// <summary>
        /// 計算指定採購退回明細的累計收款金額
        /// </summary>
        /// <param name="purchaseReturnDetailId">採購退回明細ID</param>
        /// <returns>累計收款金額</returns>
        Task<decimal> CalculateTotalReceivedAmountByPurchaseReturnDetailAsync(int purchaseReturnDetailId);

        /// <summary>
        /// 檢查指定的採購進貨明細是否已完全付款
        /// </summary>
        /// <param name="purchaseReceivingDetailId">採購進貨明細ID</param>
        /// <returns>是否已完全付款</returns>
        Task<bool> IsPurchaseReceivingDetailFullyPaidAsync(int purchaseReceivingDetailId);

        /// <summary>
        /// 檢查指定的採購退回明細是否已完全收款
        /// </summary>
        /// <param name="purchaseReturnDetailId">採購退回明細ID</param>
        /// <returns>是否已完全收款</returns>
        Task<bool> IsPurchaseReturnDetailFullyReceivedAsync(int purchaseReturnDetailId);

        /// <summary>
        /// 批次新增沖款明細
        /// </summary>
        /// <param name="setoffId">沖款單ID</param>
        /// <param name="details">明細列表</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> CreateBatchForSetoffAsync(int setoffId, List<AccountsPayableSetoffDetail> details);

        /// <summary>
        /// 依據沖款單ID刪除所有明細
        /// </summary>
        /// <param name="setoffId">沖款單ID</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> DeleteBySetoffIdAsync(int setoffId);

        /// <summary>
        /// 驗證沖款明細的沖款金額是否有效
        /// </summary>
        /// <param name="detail">沖款明細</param>
        /// <returns>驗證結果</returns>
        Task<ServiceResult> ValidateSetoffAmountAsync(AccountsPayableSetoffDetail detail);

        /// <summary>
        /// 取得供應商的未完全付款項目（可用於沖款的項目）
        /// </summary>
        /// <param name="supplierId">供應商ID</param>
        /// <returns>未完全付款項目列表</returns>
        Task<List<dynamic>> GetAvailableItemsForSetoffAsync(int supplierId);

        /// <summary>
        /// 取得供應商的未結清明細項目（轉換為統一的 DTO 格式）
        /// </summary>
        /// <param name="supplierId">供應商ID</param>
        /// <returns>未結清明細 DTO 列表</returns>
        Task<List<SetoffDetailDto>> GetSupplierPendingDetailsAsync(int supplierId);

        /// <summary>
        /// 取得供應商的所有明細項目（編輯模式用，包含已完成的）
        /// </summary>
        /// <param name="supplierId">供應商ID</param>
        /// <param name="setoffId">當前沖款單ID（用於載入現有沖款記錄）</param>
        /// <returns>所有明細 DTO 列表</returns>
        Task<List<SetoffDetailDto>> GetSupplierAllDetailsForEditAsync(int supplierId, int setoffId);

        /// <summary>
        /// 更新明細的累計付款金額和剩餘金額
        /// </summary>
        /// <param name="detailId">明細ID</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> UpdatePayableAmountsAsync(int detailId);
    }
}
