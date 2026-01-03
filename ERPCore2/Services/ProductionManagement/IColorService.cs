using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    public interface IColorService : IGenericManagementService<Color>
    {
        /// <summary>
        /// 檢查顏色編號是否已存在
        /// </summary>
        /// <param name="code">顏色編號</param>
        /// <param name="excludeId">排除的ID（用於編輯時檢查）</param>
        /// <returns>是否存在</returns>
        Task<bool> IsCodeExistsAsync(string code, int? excludeId = null);

        /// <summary>
        /// 根據編號取得顏色資料
        /// </summary>
        /// <param name="code">顏色編號</param>
        /// <returns>顏色資料</returns>
        Task<Color?> GetByCodeAsync(string code);


    }
}

