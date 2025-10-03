using ERPCore2.Data.Entities;
using ERPCore2.Models;

namespace ERPCore2.Services
{
    /// <summary>
    /// 應收帳款沖款付款明細服務介面
    /// </summary>
    public interface ISettoffPaymentDetailService : IGenericManagementService<AccountsReceivableSetoffPaymentDetail>
    {
        /// <summary>
        /// 依據沖款單ID取得付款明細列表
        /// </summary>
        /// <param name="setoffId">沖款單ID</param>
        /// <returns>付款明細DTO列表</returns>
        Task<List<SetoffPaymentDetailDto>> GetBySetoffIdAsync(int setoffId);

        /// <summary>
        /// 批次儲存付款明細
        /// </summary>
        /// <param name="setoffId">沖款單ID</param>
        /// <param name="details">付款明細DTO列表</param>
        /// <param name="deletedIds">已刪除的明細ID列表</param>
        /// <returns>操作結果</returns>
        Task<(bool Success, string Message)> SavePaymentDetailsAsync(
            int setoffId, 
            List<SetoffPaymentDetailDto> details, 
            List<int> deletedIds);

        /// <summary>
        /// 驗證付款明細總額是否符合沖款總額
        /// </summary>
        /// <param name="setoffId">沖款單ID</param>
        /// <param name="details">付款明細DTO列表</param>
        /// <param name="totalSetoffAmount">沖款總額</param>
        /// <returns>驗證結果</returns>
        Task<(bool IsValid, string? ErrorMessage)> ValidatePaymentDetailsAsync(
            int setoffId,
            List<SetoffPaymentDetailDto> details, 
            decimal totalSetoffAmount);

        /// <summary>
        /// 計算付款明細總額
        /// </summary>
        /// <param name="setoffId">沖款單ID</param>
        /// <returns>付款總額</returns>
        Task<decimal> CalculateTotalPaymentAmountAsync(int setoffId);

        /// <summary>
        /// 刪除指定沖款單的所有付款明細
        /// </summary>
        /// <param name="setoffId">沖款單ID</param>
        /// <returns>操作結果</returns>
        Task<bool> DeleteBySetoffIdAsync(int setoffId);
    }
}
