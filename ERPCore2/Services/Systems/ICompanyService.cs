using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 公司服務介面
    /// </summary>
    public interface ICompanyService : IGenericManagementService<Company>
    {
        /// <summary>
        /// 取得主要公司（系統預設顯示用）
        /// </summary>
        /// <returns>主要公司資料，若無則返回第一個啟用的公司</returns>
        Task<Company?> GetPrimaryCompanyAsync();

        /// <summary>
        /// 根據編號取得公司
        /// </summary>
        /// <param name="code">公司編號</param>
        /// <returns>公司資料</returns>
        Task<Company?> GetByCodeAsync(string code);

        /// <summary>
        /// 檢查編號是否已存在
        /// </summary>
        /// <param name="code">公司編號</param>
        /// <param name="excludeId">排除的ID（編輯時使用）</param>
        /// <returns>是否已存在</returns>
        Task<bool> IsCompanyCodeExistsAsync(string code, int? excludeId = null);

        /// <summary>
        /// 取得所有啟用的公司
        /// </summary>
        /// <returns>啟用的公司清單</returns>
        Task<List<Company>> GetActiveCompaniesAsync();

        /// <summary>
        /// 驗證統一編號格式
        /// </summary>
        /// <param name="taxId">統一編號</param>
        /// <returns>是否有效</returns>
        bool ValidateTaxId(string? taxId);

        /// <summary>
        /// 更新公司 LOGO 路徑
        /// </summary>
        /// <param name="companyId">公司 ID</param>
        /// <param name="logoPath">LOGO 檔案路徑</param>
        /// <returns>更新結果</returns>
        Task<ServiceResult> UpdateLogoPathAsync(int companyId, string logoPath);

        /// <summary>
        /// 設定指定公司為預設公司
        /// </summary>
        /// <param name="companyId">要設為預設的公司 ID</param>
        /// <returns>設定結果</returns>
        Task<ServiceResult> SetDefaultCompanyAsync(int companyId);
    }
}
