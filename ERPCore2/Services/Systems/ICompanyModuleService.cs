using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 公司模組管理服務介面
    /// 控制此公司可使用的功能模組，僅 SuperAdmin 可管理
    /// </summary>
    public interface ICompanyModuleService
    {
        /// <summary>取得所有模組清單（含啟用狀態）</summary>
        Task<List<CompanyModule>> GetAllAsync();

        /// <summary>
        /// 判斷指定模組是否已啟用
        /// SuperAdmin 永遠回傳 true
        /// </summary>
        Task<bool> IsModuleEnabledAsync(string moduleKey);

        /// <summary>更新模組啟用狀態（批次儲存）</summary>
        Task<ServiceResult> UpdateModulesAsync(List<CompanyModule> modules, string updatedBy);

        /// <summary>清除模組啟用狀態快取</summary>
        void ClearCache();
    }
}
