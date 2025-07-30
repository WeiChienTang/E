using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 尺寸服務介面 - 繼承通用管理服務
    /// </summary>
    public interface ISizeService : IGenericManagementService<Size>
    {
        #region 業務特定查詢方法
        
        /// <summary>
        /// 根據尺寸代碼取得尺寸
        /// </summary>
        Task<Size?> GetBySizeCodeAsync(string sizeCode);
        
        /// <summary>
        /// 檢查尺寸代碼是否存在
        /// </summary>
        Task<bool> IsSizeCodeExistsAsync(string sizeCode, int? excludeId = null);
        
        /// <summary>
        /// 取得啟用的尺寸列表
        /// </summary>
        Task<List<Size>> GetActiveSizesAsync();
        
        /// <summary>
        /// 根據尺寸名稱搜尋
        /// </summary>
        Task<List<Size>> GetBySizeNameAsync(string sizeName);
        
        #endregion
    }
}

