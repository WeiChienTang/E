using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;

namespace ERPCore2.Services
{
    /// <summary>
    /// 財務交易服務介面
    /// </summary>
    public interface IFinancialTransactionService : IGenericManagementService<FinancialTransaction>
    {
        /// <summary>
        /// 檢查交易單號是否已存在
        /// </summary>
        Task<bool> IsTransactionNumberExistsAsync(string transactionNumber, int? excludeId = null);

        /// <summary>
        /// 根據客戶ID獲取財務交易記錄
        /// </summary>
        Task<List<FinancialTransaction>> GetTransactionsByCustomerIdAsync(int customerId);

        /// <summary>
        /// 根據公司ID獲取財務交易記錄
        /// </summary>
        Task<List<FinancialTransaction>> GetTransactionsByCompanyIdAsync(int companyId);

        /// <summary>
        /// 根據交易類型獲取財務交易記錄
        /// </summary>
        Task<List<FinancialTransaction>> GetTransactionsByTypeAsync(FinancialTransactionTypeEnum transactionType);

        /// <summary>
        /// 根據日期範圍獲取財務交易記錄
        /// </summary>
        Task<List<FinancialTransaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// 獲取客戶列表（用於下拉選單）
        /// </summary>
        Task<List<Customer>> GetCustomersAsync();

        /// <summary>
        /// 獲取公司列表（用於下拉選單）
        /// </summary>
        Task<List<Company>> GetCompaniesAsync();

        /// <summary>
        /// 獲取收付款方式列表（用於下拉選單）
        /// </summary>
        Task<List<PaymentMethod>> GetPaymentMethodsAsync();

        /// <summary>
        /// 產生下一個交易單號
        /// </summary>
        Task<string> GenerateNextTransactionNumberAsync();

        /// <summary>
        /// 沖銷交易
        /// </summary>
        Task<ServiceResult> ReverseTransactionAsync(int transactionId, string reversalReason);
    }
}