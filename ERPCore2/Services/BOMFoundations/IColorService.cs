using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    public interface IColorService : IGenericManagementService<Color>
    {
        /// <summary>
        /// 檢查顏色代碼是否已存在
        /// </summary>
        /// <param name="code">顏色代碼</param>
        /// <param name="excludeId">排除的ID（用於編輯時檢查）</param>
        /// <returns>是否存在</returns>
        Task<bool> IsCodeExistsAsync(string code, int? excludeId = null);

        /// <summary>
        /// 根據代碼取得顏色資料
        /// </summary>
        /// <param name="code">顏色代碼</param>
        /// <returns>顏色資料</returns>
        Task<Color?> GetByCodeAsync(string code);

        /// <summary>
        /// 根據十六進位色碼取得顏色資料
        /// </summary>
        /// <param name="hexCode">十六進位色碼</param>
        /// <returns>顏色資料</returns>
        Task<Color?> GetByHexCodeAsync(string hexCode);

        /// <summary>
        /// 檢查十六進位色碼是否已存在
        /// </summary>
        /// <param name="hexCode">十六進位色碼</param>
        /// <param name="excludeId">排除的ID（用於編輯時檢查）</param>
        /// <returns>是否存在</returns>
        Task<bool> IsHexCodeExistsAsync(string hexCode, int? excludeId = null);
    }
}
