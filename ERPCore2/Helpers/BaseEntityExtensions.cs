using ERPCore2.Data;

namespace ERPCore2.Helpers;

/// <summary>
/// BaseEntity 通用擴充方法
/// </summary>
public static class BaseEntityExtensions
{
    /// <summary>
    /// 從清單中排除草稿（IsDraft = true）的記錄。
    /// 報表列印時應一律排除草稿，避免列印出尚未完整填寫的資料。
    /// </summary>
    public static List<T> ExcludeDrafts<T>(this List<T> entities) where T : BaseEntity
        => entities.Where(e => !e.IsDraft).ToList();
}
