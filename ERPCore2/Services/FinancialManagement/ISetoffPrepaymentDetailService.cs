using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Models;

namespace ERPCore2.Services
{
    /// <summary>
    /// 沖款預收/預付款明細服務介面
    /// </summary>
    public interface ISetoffPrepaymentDetailService : IGenericManagementService<PrepaymentDetail>
    {
        /// <summary>
        /// 取得應收沖款單的預收款明細
        /// </summary>
        /// <param name="setoffId">應收沖款單ID</param>
        /// <returns>預收款明細列表</returns>
        Task<List<SetoffPrepaymentDto>> GetByReceivableSetoffIdAsync(int setoffId);

        /// <summary>
        /// 取得應付沖款單的預付款明細
        /// </summary>
        /// <param name="setoffId">應付沖款單ID</param>
        /// <returns>預付款明細列表</returns>
        Task<List<SetoffPrepaymentDto>> GetByPayableSetoffIdAsync(int setoffId);

        /// <summary>
        /// 取得客戶可用的預收款列表（用於新增沖款單）
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <param name="excludeSetoffId">要排除的沖款單ID（編輯模式用）</param>
        /// <returns>可用預收款列表</returns>
        Task<List<SetoffPrepaymentDto>> GetAvailablePrepaymentsByCustomerAsync(int customerId, int? excludeSetoffId = null);

        /// <summary>
        /// 取得供應商可用的預付款列表（用於新增沖款單）
        /// </summary>
        /// <param name="supplierId">供應商ID</param>
        /// <param name="excludeSetoffId">要排除的沖款單ID（編輯模式用）</param>
        /// <returns>可用預付款列表</returns>
        Task<List<SetoffPrepaymentDto>> GetAvailablePrepaidsBySupplierAsync(int supplierId, int? excludeSetoffId = null);

        /// <summary>
        /// 取得客戶所有預收款（含已使用和可用的，用於編輯模式）
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <param name="setoffId">沖款單ID</param>
        /// <returns>所有預收款列表</returns>
        Task<List<SetoffPrepaymentDto>> GetAllPrepaymentsForEditAsync(int customerId, int setoffId);

        /// <summary>
        /// 取得供應商所有預付款（含已使用和可用的，用於編輯模式）
        /// </summary>
        /// <param name="supplierId">供應商ID</param>
        /// <param name="setoffId">沖款單ID</param>
        /// <returns>所有預付款列表</returns>
        Task<List<SetoffPrepaymentDto>> GetAllPrepaidsForEditAsync(int supplierId, int setoffId);

        /// <summary>
        /// 儲存應收沖款單的預收款明細
        /// </summary>
        /// <param name="setoffId">沖款單ID</param>
        /// <param name="prepayments">預收款明細列表</param>
        /// <param name="deletedDetailIds">要刪除的明細ID列表</param>
        /// <returns>儲存結果</returns>
        Task<ServiceResult> SaveReceivableSetoffPrepaymentsAsync(int setoffId, List<SetoffPrepaymentDto> prepayments, List<int> deletedDetailIds);

        /// <summary>
        /// 儲存應付沖款單的預付款明細
        /// </summary>
        /// <param name="setoffId">沖款單ID</param>
        /// <param name="prepayments">預付款明細列表</param>
        /// <param name="deletedDetailIds">要刪除的明細ID列表</param>
        /// <returns>儲存結果</returns>
        Task<ServiceResult> SavePayableSetoffPrepaymentsAsync(int setoffId, List<SetoffPrepaymentDto> prepayments, List<int> deletedDetailIds);

        /// <summary>
        /// 刪除應收沖款單的所有預收款明細
        /// </summary>
        /// <param name="setoffId">沖款單ID</param>
        /// <returns>刪除結果</returns>
        Task<ServiceResult> DeleteByReceivableSetoffIdAsync(int setoffId);

        /// <summary>
        /// 刪除應付沖款單的所有預付款明細
        /// </summary>
        /// <param name="setoffId">沖款單ID</param>
        /// <returns>刪除結果</returns>
        Task<ServiceResult> DeleteByPayableSetoffIdAsync(int setoffId);

        /// <summary>
        /// 計算預收/預付款的已用金額
        /// </summary>
        /// <param name="prepaymentId">預收/預付款ID</param>
        /// <param name="excludeSetoffId">要排除的沖款單ID（編輯模式用）</param>
        /// <returns>已用金額</returns>
        Task<decimal> GetUsedAmountAsync(int prepaymentId, int? excludeSetoffId = null);
        
        /// <summary>
        /// 取得客戶有剩餘預收款的應收沖款單列表
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <param name="excludeSetoffId">要排除的沖款單ID（當前編輯的沖款單）</param>
        /// <returns>有剩餘預收款的沖款單列表</returns>
        Task<List<SetoffPrepaymentDto>> GetReceivableSetoffsWithAvailablePrepaymentAsync(int customerId, int? excludeSetoffId = null);
        
        /// <summary>
        /// 取得供應商有剩餘預付款的應付沖款單列表
        /// </summary>
        /// <param name="supplierId">供應商ID</param>
        /// <param name="excludeSetoffId">要排除的沖款單ID（當前編輯的沖款單）</param>
        /// <returns>有剩餘預付款的沖款單列表</returns>
        Task<List<SetoffPrepaymentDto>> GetPayableSetoffsWithAvailablePrepaidAsync(int supplierId, int? excludeSetoffId = null);
    }
}
