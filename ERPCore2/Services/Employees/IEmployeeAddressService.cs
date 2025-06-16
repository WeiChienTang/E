using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 員工地址管理服務介面 - 統一處理所有員工地址相關操作
    /// </summary>
    public interface IEmployeeAddressService : IGenericManagementService<EmployeeAddress>
    {
        #region 業務特定查詢方法
        
        /// <summary>
        /// 根據員工ID取得地址清單
        /// </summary>
        Task<List<EmployeeAddress>> GetByEmployeeIdAsync(int employeeId);
        
        /// <summary>
        /// 取得員工的主要地址
        /// </summary>
        Task<EmployeeAddress?> GetPrimaryAddressAsync(int employeeId);
        
        /// <summary>
        /// 根據地址類型取得地址清單
        /// </summary>
        Task<List<EmployeeAddress>> GetByAddressTypeAsync(int addressTypeId);
        
        #endregion

        #region 業務邏輯操作
        
        /// <summary>
        /// 設定主要地址
        /// </summary>
        Task<ServiceResult> SetPrimaryAddressAsync(int addressId);
        
        /// <summary>
        /// 複製地址到其他員工
        /// </summary>
        Task<ServiceResult<EmployeeAddress>> CopyAddressToEmployeeAsync(EmployeeAddress sourceAddress, int targetEmployeeId, int? targetAddressTypeId = null);
        
        /// <summary>
        /// 確保員工至少有一個主要地址
        /// </summary>
        Task<ServiceResult> EnsureEmployeeHasPrimaryAddressAsync(int employeeId);
        
        /// <summary>
        /// 取得員工地址清單並初始化預設地址（如果需要）
        /// </summary>
        Task<List<EmployeeAddress>> GetAddressesWithDefaultAsync(int employeeId, List<AddressType> addressTypes);
        
        /// <summary>
        /// 更新或新增員工地址
        /// </summary>
        Task<ServiceResult<EmployeeAddress>> UpdateOrCreateAddressAsync(EmployeeAddress address);
        
        /// <summary>
        /// 驗證地址資料完整性
        /// </summary>
        ServiceResult ValidateAddress(EmployeeAddress address);
        
        /// <summary>
        /// 格式化完整地址字串
        /// </summary>
        string FormatFullAddress(EmployeeAddress address);
        
        /// <summary>
        /// 檢查地址是否重複
        /// </summary>
        Task<bool> IsDuplicateAddressAsync(EmployeeAddress address);
        
        #endregion

        #region 統計與報告
        
        /// <summary>
        /// 取得員工地址完成度統計
        /// </summary>
        Task<ServiceResult<Dictionary<string, int>>> GetAddressCompletionStatsAsync(int employeeId);
        
        /// <summary>
        /// 取得指定城市的員工地址清單
        /// </summary>
        Task<List<EmployeeAddress>> GetAddressesByCityAsync(string city);
        
        /// <summary>
        /// 取得指定行政區的員工地址清單
        /// </summary>
        Task<List<EmployeeAddress>> GetAddressesByDistrictAsync(string district);
        
        /// <summary>
        /// 取得指定郵遞區號的員工地址清單
        /// </summary>
        Task<List<EmployeeAddress>> GetAddressesByPostalCodeAsync(string postalCode);
        
        #endregion

        #region 地址類型相關操作
        
        /// <summary>
        /// 根據地址類型名稱取得員工地址值
        /// </summary>
        string GetAddressValue(int employeeId, string addressTypeName, 
            List<AddressType> addressTypes, List<EmployeeAddress> employeeAddresses);
        
        /// <summary>
        /// 更新員工特定類型的地址
        /// </summary>
        ServiceResult UpdateAddressValue(int employeeId, string addressTypeName, EmployeeAddress addressData,
            List<AddressType> addressTypes, List<EmployeeAddress> employeeAddresses);
        
        /// <summary>
        /// 計算已完成的地址欄位數量
        /// </summary>
        int GetAddressCompletedFieldsCount(List<EmployeeAddress> employeeAddresses);
        
        /// <summary>
        /// 驗證員工地址資料
        /// </summary>
        ServiceResult ValidateEmployeeAddresses(List<EmployeeAddress> employeeAddresses);
        
        /// <summary>
        /// 確保每種地址類型只有一個主要地址
        /// </summary>
        ServiceResult EnsureUniquePrimaryAddresses(List<EmployeeAddress> employeeAddresses);
        
        #endregion
    }
}
