using ERPCore2.Data;

namespace ERPCore2.Helpers.EditModal;

/// <summary>
/// 實體顯示輔助工具
/// 提供統一的「依 ID 查找清單取得顯示文字」邏輯，避免在各 EditModal 中重複相同程式碼
/// </summary>
public static class EntityDisplayHelper
{
    /// <summary>
    /// 從清單中依 ID 取得指定欄位的顯示值
    /// </summary>
    /// <typeparam name="T">實體類型（繼承 BaseEntity）</typeparam>
    /// <param name="id">要查找的 ID（int?，null 或 0 均返回 null）</param>
    /// <param name="list">要搜尋的清單</param>
    /// <param name="getValue">取得顯示值的委派（例如 c => c.CompanyName 或 c => $"{c.Code} {c.Name}"）</param>
    /// <returns>顯示值，若 ID 無效或未找到則返回 null</returns>
    public static string? GetDisplayValue<T>(int? id, IEnumerable<T> list, Func<T, string?> getValue)
        where T : BaseEntity
    {
        if (id > 0)
        {
            var entity = list.FirstOrDefault(e => e.Id == id);
            return entity != null ? getValue(entity) : null;
        }
        return null;
    }
}
