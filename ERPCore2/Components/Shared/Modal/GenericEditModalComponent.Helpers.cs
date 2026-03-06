using ERPCore2.Data;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports;
using Microsoft.AspNetCore.Components.Forms;

namespace ERPCore2.Components.Shared.Modal;

/// <summary>
/// 輔助方法、列印、Modal 開關（partial class）
/// </summary>
public partial class GenericEditModalComponent<TEntity, TService>
    where TEntity : BaseEntity, new()
{
    // ===== 列印處理 =====

    private async Task HandlePrint()
    {
        if (CanPrintCheck != null && !CanPrintCheck())
        {
            await NotificationService.ShowWarningAsync($"請先審核通過後才能列印{EntityName}");
            return;
        }

        if (ReportService != null)
        {
            await HandlePrintInternal();
            return;
        }

        if (OnPrint.HasDelegate)
            await OnPrint.InvokeAsync();
    }

    private async Task HandlePrintInternal()
    {
        try
        {
            if (Entity == null || Entity.Id <= 0)
            {
                await NotificationService.ShowWarningAsync($"請先儲存{EntityName}後再進行列印");
                return;
            }

            _formattedDocument = await ReportService!.GenerateReportAsync(Entity.Id);
            _reportPreviewImages = await ReportService.RenderToImagesAsync(Entity.Id);
            _reportDocumentName = GetReportDocumentName?.Invoke(Entity) ?? $"{EntityName}-{Entity.Id}";
            _showReportPreviewModal = true;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await NotificationService.ShowErrorAsync("列印處理時發生錯誤");
            LogError("HandlePrintInternal", ex);
        }
    }

    private async Task HandleReportPrintSuccess()
    {
        if (OnPrintSuccess.HasDelegate)
            await OnPrintSuccess.InvokeAsync();
    }

    private async Task HandlePaperSettingChanged(PaperSetting paperSetting)
    {
        try
        {
            if (Entity == null || Entity.Id <= 0 || ReportService == null) return;

            if (_formattedDocument != null)
            {
                _formattedDocument.PageSettings.PageWidth = (float)paperSetting.Width;
                _formattedDocument.PageSettings.PageHeight = (float)paperSetting.Height;
                _formattedDocument.PageSettings.LeftMargin = (float)(paperSetting.LeftMargin ?? 1.0m);
                _formattedDocument.PageSettings.TopMargin = (float)(paperSetting.TopMargin ?? 1.0m);
                _formattedDocument.PageSettings.RightMargin = (float)(paperSetting.RightMargin ?? 1.0m);
                _formattedDocument.PageSettings.BottomMargin = (float)(paperSetting.BottomMargin ?? 1.0m);
            }

            _reportPreviewImages = await ReportService.RenderToImagesAsync(Entity.Id, paperSetting);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await NotificationService.ShowErrorAsync("更新預覽時發生錯誤");
            LogError("HandlePaperSettingChanged", ex);
        }
    }

    private async Task HandlePreview()
    {
        if (OnPreview.HasDelegate)
            await OnPreview.InvokeAsync();
    }

    private async Task HandleRefresh()
    {
        if (IsLoading || IsSubmitting) return;

        try
        {
            await LoadAllData();
        }
        catch (Exception ex)
        {
            await ShowErrorMessage($"重新載入資料時發生錯誤：{ex.Message}");
            LogError("HandleRefresh", ex);
        }
    }

    // ===== Modal 開關 =====

    private async Task CloseModal()
    {
        await CleanupTabNavigationAsync();
        await SetVisible(false);
    }

    private async Task SetVisible(bool visible)
    {
        if (IsVisible != visible)
            await IsVisibleChanged.InvokeAsync(visible);
    }

    /// <summary>
    /// 攔截 BaseModalComponent 發出的 IsVisibleChanged 事件，
    /// 當未儲存修改確認 Modal 正在顯示時，阻擋直接關閉視窗。
    /// </summary>
    private async Task HandleBaseModalVisibleChanged(bool visible)
    {
        if (!visible && _cancelledByUser)
        {
            _cancelledByUser = false;
            return;
        }
        if (IsVisibleChanged.HasDelegate)
            await IsVisibleChanged.InvokeAsync(visible);
    }

    // ===== 輔助方法 =====

    /// <summary>
    /// 檢查實體是否已審核通過
    /// </summary>
    private bool IsEntityApproved()
    {
        if (Entity == null) return false;

        var propertyInfo = GetCachedProperty(typeof(TEntity), "IsApproved");
        if (propertyInfo != null && propertyInfo.PropertyType == typeof(bool))
            return (bool?)propertyInfo.GetValue(Entity) == true;

        return false;
    }

    private string GetModalTitle() =>
        _cachedModalTitle ??= !string.IsNullOrEmpty(ModalTitle)
            ? ModalTitle
            : Id.HasValue ? $"編輯{EntityName}" : $"新增{EntityName}";

    private string GetModalIcon() =>
        _cachedModalIcon ??= Id.HasValue ? "fas fa-edit" : "fas fa-plus";

    /// <summary>
    /// 轉換 ModalSize 到 BaseModalComponent.ModalSize
    /// </summary>
    private BaseModalComponent.ModalSize ConvertToBaseModalSize(ModalSize size) => size switch
    {
        ModalSize.Small      => BaseModalComponent.ModalSize.Small,
        ModalSize.Default    => BaseModalComponent.ModalSize.Default,
        ModalSize.Large      => BaseModalComponent.ModalSize.Large,
        ModalSize.ExtraLarge => BaseModalComponent.ModalSize.ExtraLarge,
        ModalSize.Desktop    => BaseModalComponent.ModalSize.Desktop,
        _                    => BaseModalComponent.ModalSize.Desktop
    };

    private BaseModalComponent.ModalAuditInfo? GetAuditInfo()
    {
        if (Entity == null || Entity.Id <= 0)
            return null;

        return _cachedAuditInfo ??= new BaseModalComponent.ModalAuditInfo
        {
            CreatedAt = Entity.CreatedAt,
            CreatedBy = Entity.CreatedBy ?? "系統",
            UpdatedAt = Entity.UpdatedAt,
            UpdatedBy = Entity.UpdatedBy ?? "系統"
        };
    }

    private string GetModuleCssClass(CustomModule module)
    {
        var css = !string.IsNullOrWhiteSpace(module.Title) ? "mb-3" : "mb-0";
        return string.IsNullOrWhiteSpace(module.CssClass) ? css : $"{css} {module.CssClass}";
    }

    private void ResetState()
    {
        IsLoading = false;
        IsSubmitting = false;
        ErrorMessage = string.Empty;
        _isDirty = false;
        Entity = new();
        editContext = new EditContext(Entity);
        IsApproving = false;
        IsRejecting = false;
        _cachedModalTitle = null;
        _cachedModalIcon = null;
        ResetStatusMessage();
    }

    // ===== 狀態訊息輔助方法 =====

    private bool ShouldShowStatusMessage() => ShowStatusMessage || ShowApprovalSection;

    /// <summary>
    /// BadgeVariant 名稱直接對應 Bootstrap CSS 類別（lowercase）
    /// </summary>
    private static string GetBadgeColorClass(BadgeVariant variant) => variant.ToString().ToLower();

    private void ResetStatusMessage()
    {
        _cachedStatusMessage = null;
        _cachedStatusVariant = BadgeVariant.Info;
        _cachedStatusIcon = "fas fa-info-circle";
    }

    // ===== 通知方法 =====

    private async Task ShowSuccessMessage(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
            await NotificationService.ShowSuccessAsync(message);
    }

    private async Task ShowErrorMessage(string message)
    {
        await NotificationService.ShowErrorAsync(message);
    }

    private void LogError(string method, Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"[GenericEditModalComponent.{method}] 錯誤：{ex.Message}");
    }
}
