using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;

namespace ERPCore2.Services
{
    /// <summary>
    /// 預收付款項服務介面
    /// </summary>
    public interface ISetoffPrepaymentService : IGenericManagementService<SetoffPrepayment>
    {
        /// <summary>
        /// 根據來源單號取得預收付款項
        /// </summary>
        /// <param name="sourceDocumentCode">來源單號</param>
        /// <returns>預收付款項</returns>
        Task<SetoffPrepayment?> GetBySourceDocumentCodeAsync(string sourceDocumentCode);

        /// <summary>
        /// 根據客戶ID取得可用的預收款項
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <returns>可用的預收款項列表</returns>
        Task<List<SetoffPrepayment>> GetAvailableByCustomerIdAsync(int customerId);

        /// <summary>
        /// 根據供應商ID取得可用的預付款項
        /// </summary>
        /// <param name="supplierId">供應商ID</param>
        /// <returns>可用的預付款項列表</returns>
        Task<List<SetoffPrepayment>> GetAvailableBySupplierIdAsync(int supplierId);

        /// <summary>
        /// 根據沖款單ID取得預收付款項
        /// </summary>
        /// <param name="setoffDocumentId">沖款單ID</param>
        /// <returns>預收付款項列表</returns>
        Task<List<SetoffPrepayment>> GetBySetoffDocumentIdAsync(int setoffDocumentId);

        /// <summary>
        /// 根據預收付類型取得款項
        /// </summary>
        /// <param name="prepaymentType">預收付類型</param>
        /// <returns>預收付款項列表</returns>
        Task<List<SetoffPrepayment>> GetByPrepaymentTypeAsync(PrepaymentType prepaymentType);

        /// <summary>
        /// 檢查來源單號是否存在
        /// </summary>
        /// <param name="sourceDocumentCode">來源單號</param>
        /// <param name="excludeId">排除的ID（用於更新時檢查）</param>
        /// <returns>是否存在</returns>
        Task<bool> IsSourceDocumentCodeExistsAsync(string sourceDocumentCode, int? excludeId = null);

        /// <summary>
        /// 更新已用金額
        /// </summary>
        /// <param name="id">預收付款項ID</param>
        /// <param name="amountToAdd">要增加的金額</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> UpdateUsedAmountAsync(int id, decimal amountToAdd);

        /// <summary>
        /// 檢查可用餘額是否足夠
        /// </summary>
        /// <param name="id">預收付款項ID</param>
        /// <param name="requiredAmount">需要的金額</param>
        /// <returns>是否足夠</returns>
        Task<bool> HasSufficientBalanceAsync(int id, decimal requiredAmount);
    }
}
