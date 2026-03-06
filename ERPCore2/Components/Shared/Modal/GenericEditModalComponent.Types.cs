using ERPCore2.Data;
using Microsoft.AspNetCore.Components;

namespace ERPCore2.Components.Shared.Modal;

/// <summary>
/// 型別定義（partial class）— CustomModule、ModalSize、BadgeVariant
/// </summary>
public partial class GenericEditModalComponent<TEntity, TService>
    where TEntity : BaseEntity, new()
{
    // ===== 自訂模組類別 =====

    /// <summary>
    /// 自訂模組類別，用於定義可重複使用的頁面模組
    /// </summary>
    public class CustomModule
    {
        /// <summary>
        /// 模組標題（可選）
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// 標題的 CSS 類別（可選，例如 "text-primary fw-bold"）
        /// </summary>
        public string? TitleCssClass { get; set; }

        /// <summary>
        /// 模組內容
        /// </summary>
        public RenderFragment? Content { get; set; }

        /// <summary>
        /// 排序順序，數字越小越靠前
        /// </summary>
        public int Order { get; set; } = 0;

        /// <summary>
        /// 自訂 CSS 類別
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// 模組唯一識別符（可選，用於除錯或特殊處理）
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// 是否顯示此模組
        /// </summary>
        public bool IsVisible { get; set; } = true;
    }

    // ===== Modal 尺寸列舉 =====

    public enum ModalSize
    {
        Small,
        Default,
        Large,
        ExtraLarge,
        Desktop
    }

    // ===== 狀態徽章顏色列舉 =====

    /// <summary>
    /// Bootstrap 徽章顏色變體
    /// </summary>
    public enum BadgeVariant
    {
        Primary,    // 藍色 - 主要
        Secondary,  // 灰色 - 次要
        Success,    // 綠色 - 成功
        Danger,     // 紅色 - 危險/錯誤
        Warning,    // 黃色 - 警告
        Info,       // 淺藍色 - 資訊（預設）
        Light,      // 淺色
        Dark        // 深色
    }
}
