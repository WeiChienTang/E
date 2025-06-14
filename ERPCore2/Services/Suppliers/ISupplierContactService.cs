using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 廠商聯絡方式服務介面 - 繼承通用管理服務
    /// </summary>
    public interface ISupplierContactService : IGenericManagementService<SupplierContact>
    {
        #region 業務特定查詢方法
        
        /// <summary>
        /// 根據廠商ID取得聯絡方式列表
        /// </summary>
        Task<List<SupplierContact>> GetBySupplierIdAsync(int supplierId);
        
        /// <summary>
        /// 根據聯絡類型取得聯絡方式列表
        /// </summary>
        Task<List<SupplierContact>> GetByContactTypeAsync(int contactTypeId);
        
        /// <summary>
        /// 取得廠商的主要聯絡方式
        /// </summary>
        Task<SupplierContact?> GetPrimaryContactAsync(int supplierId);
        
        /// <summary>
        /// 取得廠商的特定類型聯絡方式
        /// </summary>
        Task<SupplierContact?> GetContactByTypeAsync(int supplierId, int contactTypeId);
        
        #endregion

        #region 業務邏輯操作
        
        /// <summary>
        /// 設定主要聯絡方式
        /// </summary>
        Task<ServiceResult> SetPrimaryContactAsync(int contactId);
        
        /// <summary>
        /// 複製聯絡方式到其他廠商
        /// </summary>
        Task<ServiceResult<SupplierContact>> CopyContactToSupplierAsync(SupplierContact sourceContact, int targetSupplierId, int? targetContactTypeId = null);
        
        /// <summary>
        /// 確保廠商有主要聯絡方式
        /// </summary>
        Task<ServiceResult> EnsureSupplierHasPrimaryContactAsync(int supplierId);
        
        /// <summary>
        /// 取得廠商聯絡方式及預設類型
        /// </summary>
        Task<List<SupplierContact>> GetContactsWithDefaultAsync(int supplierId, List<ContactType> contactTypes);
        
        /// <summary>
        /// 更新廠商聯絡方式
        /// </summary>
        Task<ServiceResult> UpdateSupplierContactsAsync(int supplierId, List<SupplierContact> contacts);
        
        #endregion

        #region 記憶體操作方法（用於UI編輯）
        
        /// <summary>
        /// 建立新的聯絡方式
        /// </summary>
        SupplierContact CreateNewContact(int supplierId, int contactCount);
        
        /// <summary>
        /// 初始化預設聯絡方式
        /// </summary>
        void InitializeDefaultContacts(List<SupplierContact> contactList, List<ContactType> contactTypes);
        
        /// <summary>
        /// 取得預設聯絡類型ID
        /// </summary>
        int? GetDefaultContactTypeId(int contactCount, List<ContactType> contactTypes);
        
        /// <summary>
        /// 確保主要聯絡方式存在
        /// </summary>
        ServiceResult EnsurePrimaryContactExists(List<SupplierContact> contacts);
        
        /// <summary>
        /// 取得完整聯絡方式數量
        /// </summary>
        int GetCompletedContactCount(List<SupplierContact> contacts);
        
        /// <summary>
        /// 取得聯絡方式完成欄位數量
        /// </summary>
        int GetContactCompletedFieldsCount(List<SupplierContact> contacts);
        
        #endregion
    }
}
