using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    public interface IWeatherService : IGenericManagementService<Weather>
    {
        /// <summary>
        /// 檢查天氣編號是否已存在
        /// </summary>
        /// <param name="code">天氣編號</param>
        /// <param name="excludeId">排除的ID（用於編輯時檢查）</param>
        /// <returns>是否存在</returns>
        Task<bool> IsCodeExistsAsync(string code, int? excludeId = null);

        /// <summary>
        /// 根據編號取得天氣資料
        /// </summary>
        /// <param name="code">天氣編號</param>
        /// <returns>天氣資料</returns>
        Task<Weather?> GetByCodeAsync(string code);


    }
}
