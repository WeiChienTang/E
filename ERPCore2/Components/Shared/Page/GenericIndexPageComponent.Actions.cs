using ERPCore2.Data;
using ERPCore2.Services;
using Microsoft.AspNetCore.Components;

namespace ERPCore2.Components.Shared.Page;

/// <summary>
/// 刪除操作（partial class）
/// 負責：DeleteEntityAsync、IsEntityDeletable、刪除訊息 helper
/// 注意：GetStandardActionsTemplate / GetFinalActionsTemplate 含 Razor 語法，保留在 .razor 的 @code 區塊
/// </summary>
public partial class GenericIndexPageComponent<TEntity, TService>
    where TEntity : BaseEntity
    where TService : IGenericManagementService<TEntity>
{
    #region 刪除操作

    /// <summary>通用刪除方法（永久刪除）</summary>
    public async Task DeleteEntityAsync(TEntity entity, bool skipConfirm = false)
    {
        var displayName = GetEntityDisplayName(entity);

        try
        {
            // 步驟 0：前端保護（系統資料保護 / 自訂 CanDelete）
            if (!IsEntityDeletable(entity))
            {
                await NotificationService.ShowWarningAsync(string.Format(L["Message.EntityCannotDelete"], EntityName, displayName));
                return;
            }

            // 步驟 1：確認刪除（Modal 已確認時跳過 JS confirm）
            if (!skipConfirm)
            {
                var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", new object[] { GetDeleteConfirmMessage(displayName) });
                if (!confirmed) return;
            }

            // 步驟 2：執行刪除
            bool deleteSuccess;

            if (CustomDeleteHandler != null)
            {
                deleteSuccess = await CustomDeleteHandler(entity);
            }
            else
            {
                var result = await Service.DeleteAsync(entity.Id);
                if (!result.IsSuccess)
                {
                    await NotificationService.ShowErrorAsync(string.Format(L["Message.EntityDeleteFailed"], result.ErrorMessage));
                    return;
                }
                deleteSuccess = true;
            }

            // 步驟 3：成功後顯示訊息並刷新
            if (deleteSuccess)
            {
                await NotificationService.ShowSuccessAsync(GetDeleteSuccessMessage());
                await Refresh();
            }
        }
        catch (Exception ex)
        {
            await NotificationService.ShowErrorAsync(string.Format(L["Message.EntityDeleteError"], ex.Message));
        }
    }

    /// <summary>多選批次刪除確認後執行</summary>
    private async Task ExecuteMultiDeleteAsync()
    {
        _showMultiDeleteModal = false;
        var targets = _selectedItems.ToList();
        _selectedItems.Clear();

        int successCount = 0;
        var failMessages = new List<string>();

        foreach (var entity in targets)
        {
            try
            {
                bool deleteSuccess;
                if (CustomDeleteHandler != null)
                {
                    deleteSuccess = await CustomDeleteHandler(entity);
                }
                else
                {
                    var result = await Service.DeleteAsync(entity.Id);
                    deleteSuccess = result.IsSuccess;
                    if (!result.IsSuccess)
                        failMessages.Add(result.ErrorMessage ?? "");
                }
                if (deleteSuccess) successCount++;
            }
            catch (Exception ex)
            {
                failMessages.Add(ex.Message);
            }
        }

        if (successCount > 0)
            await NotificationService.ShowSuccessAsync(string.Format(L["Message.EntityDeleteSuccess"], $"{successCount} 筆{EntityName}"));
        if (failMessages.Any())
            await NotificationService.ShowErrorAsync(string.Format(L["Message.EntityDeleteFailed"], string.Join("; ", failMessages.Take(3))));

        StateHasChanged();
        await Refresh();
    }

    /// <summary>操作欄刪除確認後執行（Modal 確認後呼叫）</summary>
    private async Task ExecuteActionDeleteAsync()
    {
        if (_actionDeleteTarget == null) return;
        var target = _actionDeleteTarget;
        _actionDeleteTarget = null;
        _showActionDeleteModal = false;
        await DeleteEntityAsync(target, skipConfirm: true);
    }

    /// <summary>右鍵選單刪除確認後執行（Modal 確認後呼叫）</summary>
    private async Task ExecuteContextMenuDeleteAsync()
    {
        if (_contextMenuDeleteTarget == null) return;
        var target = _contextMenuDeleteTarget;
        _contextMenuDeleteTarget = null;
        _showContextMenuDeleteModal = false;
        await DeleteEntityAsync(target, skipConfirm: true);
    }

    #endregion

    #region 內建批次刪除

    private string GetBatchDeleteTitle() =>
        !string.IsNullOrEmpty(BatchDeleteTitle) ? BatchDeleteTitle : $"批次刪除{EntityName}";

    private async Task HandleInternalBatchDeleteAsync(List<TEntity> selectedItems)
    {
        if (selectedItems.Count == 0) return;

        try
        {
            _isInternalBatchDeleting = true;
            StateHasChanged();

            if (OnBatchDelete.HasDelegate)
            {
                await OnBatchDelete.InvokeAsync(selectedItems);
            }
            else
            {
                // SuperAdmin 預設行為：逐筆呼叫 Service.DeleteAsync
                int successCount = 0;
                var failMessages = new List<string>();
                foreach (var entity in selectedItems)
                {
                    var result = await Service.DeleteAsync(entity.Id);
                    if (result.IsSuccess) successCount++;
                    else failMessages.Add(result.ErrorMessage ?? "");
                }
                if (successCount > 0)
                    await NotificationService.ShowSuccessAsync($"成功刪除 {successCount} 筆{EntityName}");
                if (failMessages.Any())
                    await NotificationService.ShowErrorAsync($"批次刪除部分失敗：{string.Join("; ", failMessages.Take(3))}");
            }

            _showInternalBatchDeleteModal = false;
            await Refresh();
        }
        catch (Exception ex)
        {
            await NotificationService.ShowErrorAsync(string.Format(L["Message.EntityDeleteError"], ex.Message));
        }
        finally
        {
            _isInternalBatchDeleting = false;
            StateHasChanged();
        }
    }

    #endregion

    #region 判斷與訊息 Helper

    /// <summary>判斷實體是否可刪除（結合系統資料保護與自訂 CanDelete）</summary>
    private bool IsEntityDeletable(TEntity entity)
    {
        if (CanDelete != null)
            return CanDelete(entity);

        if (EnableSystemDataProtection)
            return entity.CreatedBy != "System";

        return true;
    }

    private string GetDeleteSuccessMessage() =>
        !string.IsNullOrWhiteSpace(DeleteSuccessMessage) ? DeleteSuccessMessage : string.Format(L["Message.EntityDeleteSuccess"], EntityName);

    private string GetDeleteConfirmMessage(string displayName) =>
        !string.IsNullOrWhiteSpace(DeleteConfirmMessage)
            ? string.Format(DeleteConfirmMessage, displayName)
            : string.Format(L["Message.EntityDeleteConfirm"], EntityName, displayName);

    #endregion
}
