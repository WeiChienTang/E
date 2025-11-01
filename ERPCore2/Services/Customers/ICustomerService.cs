using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 客戶服務介面 - 繼承通用管理服務
    /// </summary>
    public interface ICustomerService : IGenericManagementService<Customer>
    {
        #region 業務特定查詢方法
        
        /// <summary>
        /// 根據客戶代碼取得客戶
        /// </summary>
        Task<Customer?> GetByCustomerCodeAsync(string customerCode);
        
        /// <summary>
        /// 根據公司名稱搜尋客戶
        /// </summary>
        Task<List<Customer>> GetByCompanyNameAsync(string companyName);
        
        /// <summary>
        /// 檢查客戶代碼是否已存在
        /// </summary>
        Task<bool> IsCustomerCodeExistsAsync(string customerCode, int? excludeId = null);
        
        /// <summary>
        /// 檢查公司名稱是否已存在
        /// </summary>
        Task<bool> IsCompanyNameExistsAsync(string companyName, int? excludeId = null);
        
        #endregion

        #region 輔助方法
        
        /// <summary>
        /// 初始化新客戶
        /// </summary>
        void InitializeNewCustomer(Customer customer);
        
        /// <summary>
        /// 取得基本必填欄位數量
        /// </summary>
        int GetBasicRequiredFieldsCount();
        
        /// <summary>
        /// 取得基本完成欄位數量
        /// </summary>
        int GetBasicCompletedFieldsCount(Customer customer);
        
        #endregion
    }
}

