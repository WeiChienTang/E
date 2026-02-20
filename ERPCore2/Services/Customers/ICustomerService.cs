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
        /// 根據客戶編號取得客戶
        /// </summary>
        Task<Customer?> GetByCustomerCodeAsync(string customerCode);
        
        /// <summary>
        /// 根據公司名稱搜尋客戶
        /// </summary>
        Task<List<Customer>> GetByCompanyNameAsync(string companyName);
        
        /// <summary>
        /// 檢查客戶編號是否已存在
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

        #region 跨實體業務操作

        /// <summary>
        /// 將客戶資料複製為新廠商
        /// 自動產生廠商編號，對應可共用欄位，並驗證廠商公司名稱不重複
        /// </summary>
        /// <param name="customerId">來源客戶 ID</param>
        /// <returns>成功時回傳建立的廠商；失敗時回傳 ServiceResult 錯誤訊息</returns>
        Task<ServiceResult<Supplier>> CopyToSupplierAsync(int customerId);

        #endregion
    }
}

