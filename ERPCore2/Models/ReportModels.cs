// ============================================================================
// 向後相容性檔案 - 已遷移至 Models/Reports/ReportModels.cs
// 此檔案保留以維持現有程式碼的相容性，建議逐步更新 using 語句至新位置
// ============================================================================

// 重新導出新位置的類別 - 讓 ERPCore2.Models.Reports 命名空間可用
global using ERPCore2.Models.Reports;

namespace ERPCore2.Models
{
    // ======================================================================
    // 向後相容性 - 在 ERPCore2.Models 命名空間重新定義常用枚舉
    // 這些定義與 ERPCore2.Models.Reports 中的定義相同
    // 用於支援使用 Models.SortDirection 語法的現有程式碼
    // ======================================================================

    /// <summary>
    /// 排序方向列舉（向後相容性）
    /// </summary>
    public enum SortDirection
    {
        /// <summary>
        /// 升序
        /// </summary>
        Ascending,

        /// <summary>
        /// 降序
        /// </summary>
        Descending
    }
}
