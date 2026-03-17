using ERPCore2.Data;
using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Services;

namespace ERPCore2.Components.Shared.Modal;

/// <summary>
/// 通用刪除操作（partial class）
/// </summary>
public partial class GenericEditModalComponent<TEntity, TService>
    where TEntity : BaseEntity, new()
{
    // ===== 通用刪除操作 =====

    /// <summary>
    /// 通用刪除方法：進行前置檢查後開啟確認 Modal
    /// </summary>
    public async Task DeleteEntityAsync()
    {
        if (Entity == null || Entity.Id <= 0)
            return;

        var displayName = GetEntityDisplayName(Entity);

        try
        {
            // 步驟0：檢查是否允許刪除此實體（雙重保護）
            if (!IsEntityDeletable(Entity))
            {
                await NotificationService.ShowWarningAsync($"此{EntityName}「{displayName}」無法刪除");
                return;
            }

            // 步驟1：先檢查是否可以刪除（檢查關聯）
            bool canDelete = false;
            string? canDeleteErrorMessage = null;

            if (CustomDeleteHandler != null)
            {
                // 如果有自訂刪除處理器，直接允許
                canDelete = true;
            }
            else
            {
                // 使用快取反射調用服務的 CanDeleteAsync 方法（指定參數類型避免多載衝突）
                var canDeleteMethod = Service != null ? GetCachedMethod(Service.GetType(), "CanDeleteAsync", new[] { typeof(TEntity) }) : null;
                if (canDeleteMethod != null)
                {
                    var canDeleteResult = await (Task<ServiceResult>)canDeleteMethod.Invoke(Service, new object[] { Entity })!;
                    canDelete = canDeleteResult.IsSuccess;
                    if (!canDelete)
                        canDeleteErrorMessage = canDeleteResult.ErrorMessage ?? "無法刪除此資料";
                }
                else
                {
                    // 如果沒有 CanDeleteAsync 方法，預設允許刪除
                    canDelete = true;
                }
            }

            // 步驟2：如果不能刪除，顯示錯誤訊息
            if (!canDelete)
            {
                await NotificationService.ShowWarningAsync(canDeleteErrorMessage ?? $"無法刪除{EntityName}「{displayName}」，因為有其他資料正在使用此{EntityName}");
                return;
            }

            // 步驟3：開啟確認 Modal
            _showDeleteConfirmModal = true;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await NotificationService.ShowErrorAsync($"刪除時發生錯誤：{ex.Message}");
            LogError("DeleteEntityAsync", ex);
        }
    }

    /// <summary>
    /// 刪除確認後執行實際刪除（Modal 確認後呼叫）
    /// </summary>
    private async Task ExecuteDeleteConfirmedAsync()
    {
        _showDeleteConfirmModal = false;

        if (Entity == null || Entity.Id <= 0) return;

        try
        {
            bool deleteSuccess = false;

            if (CustomDeleteHandler != null)
            {
                deleteSuccess = await CustomDeleteHandler(Entity);
            }
            else
            {
                // 優先使用介面直接呼叫，避免反射
                var genericService = Service as IGenericManagementService<TEntity>;
                if (genericService != null)
                {
                    var deleteResult = await genericService.DeleteAsync(Entity.Id);
                    if (deleteResult.IsSuccess)
                    {
                        deleteSuccess = true;
                    }
                    else
                    {
                        await NotificationService.ShowErrorAsync($"刪除失敗：{deleteResult.ErrorMessage ?? "刪除失敗"}");
                        return;
                    }
                }
                else
                {
                    await NotificationService.ShowErrorAsync("找不到永久刪除方法");
                    return;
                }
            }

            // 步驟5：顯示成功訊息並關閉 Modal
            if (deleteSuccess)
            {
                var successMessage = GetDeleteSuccessMessage();
                await NotificationService.ShowSuccessAsync(successMessage);

                // 觸發成功事件（通知父組件刷新資料）
                if (OnDeleteSuccess.HasDelegate)
                    await OnDeleteSuccess.InvokeAsync();

                // 關閉 Modal
                await CloseModal();
            }
        }
        catch (Exception ex)
        {
            await NotificationService.ShowErrorAsync($"刪除時發生錯誤：{ex.Message}");
            LogError("ExecuteDeleteConfirmedAsync", ex);
        }
    }

    /// <summary>
    /// 判斷實體是否可以刪除（結合預設系統資料保護和自訂判斷邏輯）
    /// </summary>
    private bool IsEntityDeletable(TEntity entity)
    {
        if (CanDelete != null)
            return CanDelete(entity);

        if (EnableSystemDataProtection)
            return entity.CreatedBy != "System";

        return true;
    }

    /// <summary>
    /// 取得刪除成功訊息，優先使用自訂值，否則根據 EntityName 自動產生
    /// </summary>
    private string GetDeleteSuccessMessage() =>
        !string.IsNullOrWhiteSpace(DeleteSuccessMessage) ? DeleteSuccessMessage : $"{EntityName}刪除成功";

    /// <summary>
    /// 取得刪除確認訊息，優先使用自訂值，否則根據 EntityName 自動產生
    /// </summary>
    private string GetDeleteConfirmMessage(string displayName)
    {
        if (!string.IsNullOrWhiteSpace(DeleteConfirmMessage))
            return string.Format(DeleteConfirmMessage, displayName);

        return "確定刪除嗎？";
    }
}
