using Microsoft.AspNetCore.Components;
using ERPCore2.Components.Shared.Modal;
using ERPCore2.Data;

namespace ERPCore2.Helpers.EditModal;

/// <summary>
/// 自訂模組建立輔助工具
/// 封裝 GetCustomModules 中重複的 null 檢查和物件建立樣板
/// </summary>
public static class CustomModuleHelper
{
    /// <summary>
    /// 建立包含單一模組的清單（含 null 保護）
    /// 當 editModalComponent 為 null 時回傳空清單
    /// </summary>
    /// <typeparam name="TEntity">實體型別</typeparam>
    /// <typeparam name="TService">服務型別</typeparam>
    /// <param name="editModalComponent">編輯元件參考（用於 null 檢查）</param>
    /// <param name="content">模組內容 RenderFragment</param>
    /// <param name="order">排序順序（預設 1）</param>
    /// <param name="title">模組標題（可選）</param>
    /// <param name="cssClass">CSS 類別（可選）</param>
    /// <returns>CustomModule 清單</returns>
    public static List<GenericEditModalComponent<TEntity, TService>.CustomModule> CreateSingle<TEntity, TService>(
        GenericEditModalComponent<TEntity, TService>? editModalComponent,
        RenderFragment content,
        int order = 1,
        string? title = null,
        string? cssClass = null)
        where TEntity : BaseEntity, new()
    {
        if (editModalComponent == null)
            return new List<GenericEditModalComponent<TEntity, TService>.CustomModule>();

        return new List<GenericEditModalComponent<TEntity, TService>.CustomModule>
        {
            new GenericEditModalComponent<TEntity, TService>.CustomModule
            {
                Title = title,
                Order = order,
                CssClass = cssClass,
                IsVisible = true,
                Content = content
            }
        };
    }
}
