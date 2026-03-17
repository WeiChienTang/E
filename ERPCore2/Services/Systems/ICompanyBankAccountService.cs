using ERPCore2.Data.Entities.Systems;

namespace ERPCore2.Services.Systems
{
    public interface ICompanyBankAccountService : IGenericManagementService<CompanyBankAccount>
    {
        /// <summary>取得指定公司的所有銀行帳戶</summary>
        Task<List<CompanyBankAccount>> GetByCompanyIdAsync(int companyId);

        /// <summary>取得指定公司的主要帳戶</summary>
        Task<CompanyBankAccount?> GetPrimaryAccountAsync(int companyId);

        /// <summary>設定指定帳戶為主要帳戶（同時取消同公司其他帳戶的主要標記）</summary>
        Task<ServiceResult> SetPrimaryAsync(int bankAccountId, int companyId);
    }
}
