using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;

namespace ERPCore2.Services
{
    /// <summary>
    /// 廠商服務介面 - 繼承通用管理服務
    /// </summary>
    public interface ISupplierService : IGenericManagementService<Supplier>
    {
        #region 業務特定查詢方法
        
        /// <summary>
        /// 根據廠商代碼取得廠商
        /// </summary>
        Task<Supplier?> GetBySupplierCodeAsync(string supplierCode);
        
        /// <summary>
        /// 檢查廠商代碼是否存在
        /// </summary>
        Task<bool> IsSupplierCodeExistsAsync(string supplierCode, int? excludeId = null);
        
        /// <summary>
        /// 根據廠商類型取得廠商列表
        /// </summary>
        Task<List<Supplier>> GetBySupplierTypeAsync(int supplierTypeId);
        
        #endregion

        #region 輔助資料查詢
        
        /// <summary>
        /// 取得所有廠商類型
        /// </summary>
        Task<List<SupplierType>> GetSupplierTypesAsync();
        
        #endregion

        #region 聯絡資料管理
        
        /// <summary>
        /// 取得廠商聯絡資料
        /// </summary>
        Task<List<SupplierContact>> GetSupplierContactsAsync(int supplierId);
        
        /// <summary>
        /// 更新廠商聯絡資料
        /// </summary>
        Task<ServiceResult> UpdateSupplierContactsAsync(int supplierId, List<SupplierContact> contacts);
        
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
    }
}

