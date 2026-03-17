using ERPCore2.Data.Entities.Customers;

namespace ERPCore2.Services.Customers
{
    public interface ICustomerBankAccountService : IGenericManagementService<CustomerBankAccount>
    {
        /// <summary>取得指定客戶的所有銀行帳戶</summary>
        Task<List<CustomerBankAccount>> GetByCustomerIdAsync(int customerId);

        /// <summary>取得指定客戶的主要帳戶</summary>
        Task<CustomerBankAccount?> GetPrimaryAccountAsync(int customerId);

        /// <summary>設定指定帳戶為主要帳戶（同時取消同客戶其他帳戶的主要標記）</summary>
        Task<ServiceResult> SetPrimaryAsync(int bankAccountId, int customerId);
    }
}
