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
    public async Task DeleteEntityAsync(TEntity entity)
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

            // 步驟 1：確認刪除
            var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", new object[] { GetDeleteConfirmMessage(displayName) });
            if (!confirmed) return;

            // 步驟 2：執行刪除
            bool deleteSuccess;

            if (CustomDeleteHandler != null)
            {
                deleteSuccess = await CustomDeleteHandler(entity);
            }
            else
            {
                // 透過介面直接呼叫 PermanentDeleteAsync（內部已包含 CanDeleteAsync 檢查）
                var result = await Service.PermanentDeleteAsync(entity.Id);
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
