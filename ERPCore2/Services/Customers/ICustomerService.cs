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
        
        #endregion

        #region 關聯資料查詢
        
        /// <summary>
        /// 取得客戶類型清單
        /// </summary>
        Task<List<CustomerType>> GetCustomerTypesAsync();
        
        /// <summary>
        /// 取得行業類型清單
        /// </summary>
        Task<List<IndustryType>> GetIndustryTypesAsync();
        
        /// <summary>
        /// 取得聯絡類型清單
        /// </summary>
        Task<List<ContactType>> GetContactTypesAsync();
          /// <summary>
        /// 取得地址類型清單
        /// </summary>
        Task<List<AddressType>> GetAddressTypesAsync();
        
        /// <summary>
        /// 根據關鍵字搜尋客戶類型
        /// </summary>
        Task<List<CustomerType>> SearchCustomerTypesAsync(string keyword);
        
        /// <summary>
        /// 根據關鍵字搜尋行業類型
        /// </summary>
        Task<List<IndustryType>> SearchIndustryTypesAsync(string keyword);
        
        #endregion

        #region 聯絡資料管理
        
        /// <summary>
        /// 取得客戶聯絡資料
        /// </summary>
        Task<List<CustomerContact>> GetCustomerContactsAsync(int customerId);
        
        /// <summary>
        /// 更新客戶聯絡資料
        /// </summary>
        Task<ServiceResult> UpdateCustomerContactsAsync(int customerId, List<CustomerContact> contacts);
        
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

