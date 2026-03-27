using ERPCore2.Data;
using ERPCore2.Services;
using Microsoft.AspNetCore.Components;

namespace ERPCore2.Components.Shared.Page;

/// <summary>
/// 導航與按鈕事件委派（partial class）
/// 負責：頁面跳轉、HandleAddClick/HandleRowClick、所有 Handle*Click 的 EventCallback 委派
/// </summary>
public partial class GenericIndexPageComponent<TEntity, TService>
    where TEntity : BaseEntity
    where TService : IGenericManagementService<TEntity>
{
    #region 導航方法

    public void NavigateToCreate()
    {
        var url = !string.IsNullOrEmpty(CreateUrl) ? CreateUrl : $"{EntityBasePath}/edit";
        Navigation.NavigateTo(url);
    }

    public void NavigateToEdit(TEntity entity)
    {
        var url = !string.IsNullOrEmpty(EditUrl)
            ? EditUrl.Replace("{id}", entity.Id.ToString())
            : $"{EntityBasePath}/edit/{entity.Id}";
        Navigation.NavigateTo(url);
    }

    public void NavigateToDetail(TEntity entity)
    {
        var url = !string.IsNullOrEmpty(DetailUrl)
            ? DetailUrl.Replace("{id}", entity.Id.ToString())
            : $"{EntityBasePath}/detail/{entity.Id}";
        Navigation.NavigateTo(url);
    }

    #endregion

    #region 主要點擊事件

    private async Task HandleAddClick()
    {
        if (OnAddClick.HasDelegate)
            await OnAddClick.InvokeAsync();
        else
            NavigateToCreate();
    }

    private async Task HandleRowClick(TEntity entity)
    {
        if (OnRowClick.HasDelegate)
            await OnRowClick.InvokeAsync(entity);
        else
            NavigateToDetail(entity);
    }

    #endregion

    #region EventCallback 委派（共用 helper 消除重複樣板）

    // 若 EventCallback 有綁定則呼叫，否則直接完成（取代 7 個幾乎相同的 Handle* 方法）
    private static Task InvokeIfBound(EventCallback cb)
        => cb.HasDelegate ? cb.InvokeAsync() : Task.CompletedTask;

    private Task HandleExportExcelClick()
    {
        // 若外部有綁定自訂處理則委派，否則使用內建匯出
        if (OnExportExcelClick.HasDelegate)
            return OnExportExcelClick.InvokeAsync();
        return ExecuteBuiltInExportExcelAsync();
    }
    private Task HandleExportPdfClick()      => InvokeIfBound(OnExportPdfClick);
    private Task HandleBatchPrintClick()     => InvokeIfBound(OnBatchPrintClick);
    private Task HandleBarcodePrintClick()   => InvokeIfBound(OnBarcodePrintClick);
    private Task HandleBatchApprovalClick()  => InvokeIfBound(OnBatchApprovalClick);
    private Task HandleImportScheduleClick() => InvokeIfBound(OnImportScheduleClick);
    private Task HandleBatchDeleteClick()
    {
        // 若設定了內建 BatchDeleteColumnDefinitions 或為 SuperAdmin，開啟內建 modal
        if (BatchDeleteColumnDefinitions != null || _isSuperAdmin)
        {
            _showInternalBatchDeleteModal = true;
            StateHasChanged();
            return Task.CompletedTask;
        }
        return InvokeIfBound(OnBatchDeleteClick);
    }

    #endregion
}
