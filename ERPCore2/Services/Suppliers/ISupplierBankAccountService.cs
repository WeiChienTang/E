using ERPCore2.Data.Entities.Suppliers;

namespace ERPCore2.Services.Suppliers
{
    public interface ISupplierBankAccountService : IGenericManagementService<SupplierBankAccount>
    {
        /// <summary>取得指定廠商的所有銀行帳戶</summary>
        Task<List<SupplierBankAccount>> GetBySupplierIdAsync(int supplierId);

        /// <summary>取得指定廠商的主要帳戶</summary>
        Task<SupplierBankAccount?> GetPrimaryAccountAsync(int supplierId);

        /// <summary>設定指定帳戶為主要帳戶（同時取消同廠商其他帳戶的主要標記）</summary>
        Task<ServiceResult> SetPrimaryAsync(int bankAccountId, int supplierId);
    }
}
