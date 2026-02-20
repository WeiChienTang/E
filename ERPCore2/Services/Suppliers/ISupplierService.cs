using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;

namespace ERPCore2.Services
{
    /// <summary>
    /// 廠商服務介面 - 繼承通用管理服務
    /// </summary>
    public interface ISupplierService : IGenericManagementService<Supplier>
    {
        #region 業務特定查詢方法
        
        /// <summary>
        /// 根據廠商編號取得廠商
        /// </summary>
        Task<Supplier?> GetBySupplierCodeAsync(string supplierCode);
        
        /// <summary>
        /// 檢查廠商編號是否存在
        /// </summary>
        Task<bool> IsSupplierCodeExistsAsync(string supplierCode, int? excludeId = null);
        
        /// <summary>
        /// 檢查公司名稱是否存在
        /// </summary>
        Task<bool> IsCompanyNameExistsAsync(string companyName, int? excludeId = null);
        
        #endregion

        #region 聯絡資料管理
        
        // 聯絡資料管理已移至 IContactService
        
        #endregion

        #region 地址資料管理
        
        // 地址相關方法已移至 IAddressService
        // /// <summary>
        // /// 取得廠商地址資料
        // /// </summary>
        // Task<List<SupplierAddress>> GetSupplierAddressesAsync(int supplierId);
        
        // /// <summary>
        // /// 更新廠商地址資料
        // /// </summary>
        // Task<ServiceResult> UpdateSupplierAddressesAsync(int supplierId, List<SupplierAddress> addresses);
        
        #endregion

        #region 狀態管理
        
        /// <summary>
        /// 更新廠商狀態
        /// </summary>
        Task<ServiceResult> UpdateSupplierStatusAsync(int supplierId, EntityStatus status);
        
        #endregion

        #region 輔助方法
        
        /// <summary>
        /// 初始化新廠商
        /// </summary>
        void InitializeNewSupplier(Supplier supplier);
        
        /// <summary>
        /// 取得基本必填欄位數量
        /// </summary>
        int GetBasicRequiredFieldsCount();
        
        /// <summary>
        /// 取得基本完成欄位數量
        /// </summary>
        int GetBasicCompletedFieldsCount(Supplier supplier);
        
        #endregion

        #region 跨實體業務操作

        /// <summary>
        /// 將廠商資料複製為新客戶
        /// 自動產生客戶編號，對應可共用欄位，並驗證客戶公司名稱不重複
        /// </summary>
        /// <param name="supplierId">來源廠商 ID</param>
        /// <returns>成功時回傳建立的客戶；失敗時回傳 ServiceResult 錯誤訊息</returns>
        Task<ServiceResult<Customer>> CopyToCustomerAsync(int supplierId);

        #endregion
    }
}

