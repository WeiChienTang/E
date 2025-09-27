using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 應收帳款沖款明細服務介面
    /// </summary>
    public interface IAccountsReceivableSetoffDetailService : IGenericManagementService<AccountsReceivableSetoffDetail>
    {
        /// <summary>
        /// 依據沖款單ID取得明細列表
        /// </summary>
        /// <param name="setoffId">沖款單ID</param>
        /// <returns>明細列表</returns>
        Task<List<AccountsReceivableSetoffDetail>> GetBySetoffIdAsync(int setoffId);

        /// <summary>
        /// 依據銷貨訂單明細ID取得沖款明細列表
        /// </summary>
        /// <param name="salesOrderDetailId">銷貨訂單明細ID</param>
        /// <returns>沖款明細列表</returns>
        Task<List<AccountsReceivableSetoffDetail>> GetBySalesOrderDetailIdAsync(int salesOrderDetailId);

        /// <summary>
        /// 依據銷貨退回明細ID取得沖款明細列表
        /// </summary>
        /// <param name="salesReturnDetailId">銷貨退回明細ID</param>
        /// <returns>沖款明細列表</returns>
        Task<List<AccountsReceivableSetoffDetail>> GetBySalesReturnDetailIdAsync(int salesReturnDetailId);

        /// <summary>
        /// 計算指定銷貨訂單明細的累計收款金額
        /// </summary>
        /// <param name="salesOrderDetailId">銷貨訂單明細ID</param>
        /// <returns>累計收款金額</returns>
        Task<decimal> CalculateTotalReceivedAmountBySalesOrderDetailAsync(int salesOrderDetailId);

        /// <summary>
        /// 計算指定銷貨退回明細的累計退款金額
        /// </summary>
        /// <param name="salesReturnDetailId">銷貨退回明細ID</param>
        /// <returns>累計退款金額</returns>
        Task<decimal> CalculateTotalReceivedAmountBySalesReturnDetailAsync(int salesReturnDetailId);

        /// <summary>
        /// 檢查指定的銷貨訂單明細是否已完全收款
        /// </summary>
        /// <param name="salesOrderDetailId">銷貨訂單明細ID</param>
        /// <returns>是否已完全收款</returns>
        Task<bool> IsSalesOrderDetailFullyReceivedAsync(int salesOrderDetailId);

        /// <summary>
        /// 檢查指定的銷貨退回明細是否已完全退款
        /// </summary>
        /// <param name="salesReturnDetailId">銷貨退回明細ID</param>
        /// <returns>是否已完全退款</returns>
        Task<bool> IsSalesReturnDetailFullyReceivedAsync(int salesReturnDetailId);

        /// <summary>
        /// 批次新增沖款明細
        /// </summary>
        /// <param name="setoffId">沖款單ID</param>
        /// <param name="details">明細列表</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> CreateBatchForSetoffAsync(int setoffId, List<AccountsReceivableSetoffDetail> details);

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
        Task<ServiceResult> ValidateSetoffAmountAsync(AccountsReceivableSetoffDetail detail);

        /// <summary>
        /// 取得客戶的未完全收款項目（可用於沖款的項目）
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <returns>未完全收款項目列表</returns>
        Task<List<dynamic>> GetAvailableItemsForSetoffAsync(int customerId);

        /// <summary>
        /// 取得客戶的未結清明細項目（轉換為統一的 DTO 格式）
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <returns>未結清明細 DTO 列表</returns>
        Task<List<SetoffDetailDto>> GetCustomerPendingDetailsAsync(int customerId);

        /// <summary>
        /// 取得客戶的所有明細項目（編輯模式用，包含已完成的）
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <param name="setoffId">當前沖款單ID（用於載入現有沖款記錄）</param>
        /// <returns>所有明細 DTO 列表</returns>
        Task<List<SetoffDetailDto>> GetCustomerAllDetailsForEditAsync(int customerId, int setoffId);

        /// <summary>
        /// 更新明細的累計收款金額和剩餘金額
        /// </summary>
        /// <param name="detailId">明細ID</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> UpdateReceivableAmountsAsync(int detailId);
    }
}