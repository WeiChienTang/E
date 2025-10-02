using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;

namespace ERPCore2.Services
{
    /// <summary>
    /// 預先款項服務介面
    /// </summary>
    public interface IPrepaymentService : IGenericManagementService<Prepayment>
    {
        /// <summary>
        /// 檢查預先款項代碼是否存在
        /// </summary>
        Task<bool> IsCodeExistsAsync(string code, int? excludeId = null);

        /// <summary>
        /// 依據客戶取得預收款列表
        /// </summary>
        Task<List<Prepayment>> GetByCustomerIdAsync(int customerId);

        /// <summary>
        /// 依據供應商取得預付款列表
        /// </summary>
        Task<List<Prepayment>> GetBySupplierIdAsync(int supplierId);

        /// <summary>
        /// 依據款項類型取得列表
        /// </summary>
        Task<List<Prepayment>> GetByPrepaymentTypeAsync(PrepaymentType prepaymentType);

        /// <summary>
        /// 取得指定日期範圍的款項
        /// </summary>
        Task<List<Prepayment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// 取得客戶的可用預收款餘額
        /// </summary>
        Task<decimal> GetAvailableBalanceByCustomerAsync(int customerId);

        /// <summary>
        /// 取得供應商的可用預付款餘額
        /// </summary>
        Task<decimal> GetAvailableBalanceBySupplierAsync(int supplierId);
    }
}
