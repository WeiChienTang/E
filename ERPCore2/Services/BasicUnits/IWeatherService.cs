using ERPCore2.Data.Entities;

namespace ERPCore2.Services;

public interface IWeatherService : IGenericManagementService<Weather>
{
    /// <summary>
    /// 檢查天氣代碼是否已存在
    /// </summary>
    /// <param name="code">天氣代碼</param>
    /// <param name="excludeId">排除的ID（用於編輯時檢查）</param>
    /// <returns>是否存在</returns>
    Task<bool> IsCodeExistsAsync(string code, int? excludeId = null);

    /// <summary>
    /// 根據代碼取得天氣資料
    /// </summary>
    /// <param name="code">天氣代碼</param>
    /// <returns>天氣資料</returns>
    Task<Weather?> GetByCodeAsync(string code);

    /// <summary>
    /// 根據溫度範圍取得天氣資料
    /// </summary>
    /// <param name="minTemperature">最低溫度</param>
    /// <param name="maxTemperature">最高溫度</param>
    /// <returns>天氣資料列表</returns>
    Task<List<Weather>> GetByTemperatureRangeAsync(decimal minTemperature, decimal maxTemperature);
}
