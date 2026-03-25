using ERPCore2.Data;
using ERPCore2.Helpers;

namespace ERPCore2.Components.Shared.Modal;

/// <summary>
/// 審核操作處理（partial class）
/// </summary>
public partial class GenericEditModalComponent<TEntity, TService>
    where TEntity : BaseEntity, new()
{
    private async Task OpenApproveConfirmModal()
    {
        if (IsApproving || IsRejecting) return;
        _currentUserName = await CurrentUserHelper.GetCurrentUserFullNameAsync(AuthenticationStateProvider);
        _isApproveConfirmModalVisible = true;
        StateHasChanged();
    }

    private async Task HandleApproveConfirm()
    {
        if (OnApprove == null || IsApproving || IsRejecting) return;

        try
        {
            IsApproving = true;
            StateHasChanged();

            var success = await OnApprove();

            if (success)
            {
                await ShowSuccessMessage("審核通過");
                await RefreshEntityAsync();

                // 通知父頁面刷新列表（審核狀態已變更）
                if (OnEntitySaved.HasDelegate && Entity != null)
                    await OnEntitySaved.InvokeAsync(Entity);
            }
            else
            {
                await ShowErrorMessage("審核通過失敗");
            }
        }
        catch (Exception ex)
        {
            await ShowErrorMessage($"審核通過時發生錯誤：{ex.Message}");
            LogError("HandleApprove", ex);
        }
        finally
        {
            IsApproving = false;
            StateHasChanged();
        }
    }

    private async Task OpenRejectConfirmModal()
    {
        if (IsApproving || IsRejecting) return;
        _currentUserName = await CurrentUserHelper.GetCurrentUserFullNameAsync(AuthenticationStateProvider);
        _isRejectConfirmModalVisible = true;
        StateHasChanged();
    }

    private async Task HandleRejectConfirm(string rejectReason)
    {
        try
        {
            IsRejecting = true;
            StateHasChanged();

            if (OnRejectWithReason == null)
            {
                await ShowErrorMessage("未設定駁回處理程序");
                return;
            }

            bool success = await OnRejectWithReason(rejectReason);

            if (success)
            {
                await ShowSuccessMessage("審核駁回");
                await RefreshEntityAsync();

                // 通知父頁面刷新列表（審核狀態已變更）
                if (OnEntitySaved.HasDelegate && Entity != null)
                    await OnEntitySaved.InvokeAsync(Entity);
            }
        }
        catch (Exception ex)
        {
            await ShowErrorMessage($"審核駁回時發生錯誤：{ex.Message}");
            LogError("HandleRejectConfirm", ex);
        }
        finally
        {
            IsRejecting = false;
            StateHasChanged();
        }
    }

    private Task HandleRejectCancel()
    {
        _isRejectConfirmModalVisible = false;
        return Task.CompletedTask;
    }
}
