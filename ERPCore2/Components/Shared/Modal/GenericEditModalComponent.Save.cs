using ERPCore2.Data;
using ERPCore2.Helpers;
using ERPCore2.Services;
using ERPCore2.Components.Shared.UI.Form;
using Microsoft.AspNetCore.Components.Forms;

namespace ERPCore2.Components.Shared.Modal;

/// <summary>
/// 儲存、取消、欄位變更處理（partial class）
/// </summary>
public partial class GenericEditModalComponent<TEntity, TService>
    where TEntity : BaseEntity, new()
{
    // ===== 儲存處理 =====

    private async Task HandleSave()
    {
        if (IsSubmitting) return;

        try
        {
            IsSubmitting = true;
            StateHasChanged();

            if (Entity == null)
            {
                await ShowErrorMessage("實體資料不存在");
                return;
            }

            // 自動設定審計欄位
            try
            {
                var currentUserName = await CurrentUserHelper.GetCurrentUserFullNameAsync(AuthenticationStateProvider);
                if (!string.IsNullOrEmpty(currentUserName))
                {
                    if (!Id.HasValue)
                        Entity.CreatedBy = currentUserName;
                    Entity.UpdatedBy = currentUserName;
                }
            }
            catch
            {
                // 無法取得使用者資訊時不阻擋儲存流程
            }

            bool success;

            if (UseGenericSave)
                success = await GenericSave(Entity);
            else if (SaveHandler != null)
                success = await SaveHandler(Entity);
            else
            {
                await ShowErrorMessage("未設定儲存處理程序");
                return;
            }

            if (success)
            {
                _isDirty = false;
                await ShowSuccessMessage(SaveSuccessMessage);

                bool wasNewRecord = !Id.HasValue;

                if (wasNewRecord && Entity != null && Entity.Id > 0)
                {
                    bool shouldStayInAddMode = _stayInAddMode || (ShowAddButton && wasNewRecord);

                    if (shouldStayInAddMode)
                    {
                        _stayInAddMode = false;

                        if (OnEntitySaved.HasDelegate && Entity != null)
                            await OnEntitySaved.InvokeAsync(Entity);
                        if (OnSaveSuccess.HasDelegate)
                            await OnSaveSuccess.InvokeAsync();

                        IsSubmitting = false;
                        IsLoading = false;
                        StateHasChanged();

                        await HandleAddClick();
                        return;
                    }
                    else
                    {
                        _lastId = Entity.Id;
                        Id = Entity.Id;

                        if (IdChanged.HasDelegate)
                            await IdChanged.InvokeAsync(Entity.Id);

                        await LoadAllData();
                    }
                }

                if (OnEntitySaved.HasDelegate && Entity != null)
                    await OnEntitySaved.InvokeAsync(Entity);

                if (OnSaveSuccess.HasDelegate)
                    await OnSaveSuccess.InvokeAsync();

                if (CloseOnSave || _forceCloseOnNextSave)
                    await CloseModal();
            }
            else
            {
                if (OnSaveFailure.HasDelegate)
                    await OnSaveFailure.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            await ShowErrorMessage($"儲存時發生錯誤：{ex.Message}");
            LogError("HandleSave", ex);

            if (OnSaveFailure.HasDelegate)
                await OnSaveFailure.InvokeAsync();
        }
        finally
        {
            _forceCloseOnNextSave = false;
            IsSubmitting = false;
            StateHasChanged();
        }
    }

    private async Task HandleCancel()
    {
        try
        {
            if (_isDirty && ShowUnsavedChangesWarning)
            {
                _unsavedChangesConfirmTcs = new TaskCompletionSource<int>();
                _showUnsavedChangesModal = true;
                StateHasChanged();
                var result = await _unsavedChangesConfirmTcs.Task;
                switch (result)
                {
                    case 0: // 繼續編輯
                        _cancelledByUser = true;
                        return;
                    case 2: // 儲存關閉
                        _forceCloseOnNextSave = true;
                        _cancelledByUser = true;
                        await HandleSave();
                        return;
                    // case 1: 確定離開，繼續執行關閉流程
                }
            }

            if (OnCancel.HasDelegate)
                await OnCancel.InvokeAsync();

            await CloseModal();
        }
        catch (Exception ex)
        {
            LogError("HandleCancel", ex);
        }
    }

    // ===== 未儲存修改確認處理 =====

    private void HandleUnsavedChangesLeave()
    {
        _showUnsavedChangesModal = false;
        _unsavedChangesConfirmTcs?.TrySetResult(1);
    }

    private void HandleUnsavedChangesSaveAndClose()
    {
        _showUnsavedChangesModal = false;
        _unsavedChangesConfirmTcs?.TrySetResult(2);
    }

    private void HandleUnsavedChangesCancel()
    {
        _showUnsavedChangesModal = false;
        _unsavedChangesConfirmTcs?.TrySetResult(0);
    }

    // ===== 通用 Save 方法 =====

    private async Task<bool> GenericSave(TEntity entity)
    {
        try
        {
            if (CustomValidator != null && !await CustomValidator(entity))
                return false;

            if (BeforeSave != null)
                await BeforeSave(entity);

            var genericService = Service as IGenericManagementService<TEntity>;
            if (genericService == null)
            {
                await ShowErrorMessage("服務未實作泛型管理介面");
                return false;
            }

            ServiceResult<TEntity> serviceResult = Id.HasValue
                ? await genericService.UpdateAsync(entity)
                : await genericService.CreateAsync(entity);

            var result = new ServiceResult
            {
                IsSuccess = serviceResult.IsSuccess,
                ErrorMessage = serviceResult.ErrorMessage,
                ValidationErrors = serviceResult.ValidationErrors
            };

            if (result.IsSuccess)
            {
                if (AfterSave != null)
                    await AfterSave(entity);
                return true;
            }
            else
            {
                var errorMsg = !string.IsNullOrEmpty(result.ErrorMessage) ? result.ErrorMessage : "儲存失敗";
                await ShowErrorMessage($"{SaveFailureMessage}：{errorMsg}");
                return false;
            }
        }
        catch (Exception ex)
        {
            await ShowErrorMessage($"{SaveFailureMessage}：{ex.Message}");
            LogError("GenericSave", ex);
            return false;
        }
    }

    // ===== 欄位變更處理 =====

    /// <summary>
    /// 供子組件呼叫，標記表單有未儲存變更
    /// </summary>
    public void MarkDirty()
    {
        _isDirty = true;
    }

    private async Task HandleFieldChanged((string PropertyName, object? Value) fieldChange)
    {
        _isDirty = true;

        // AutoComplete 欄位：按鈕狀態同步
        var field = FormFields?.FirstOrDefault(f => f.PropertyName == fieldChange.PropertyName);
        if (field != null &&
            field.FieldType == FormFieldType.AutoComplete &&
            field.ActionButtons != null &&
            field.ActionButtons.Any())
        {
            if (fieldChange.Value == null || string.IsNullOrEmpty(fieldChange.Value.ToString()))
                _lastSearchTerms[fieldChange.PropertyName] = string.Empty;

            field.ActionButtons = ProcessActionButtonsForAutoComplete(field);
            _processedFormFieldsCache = null;
            StateHasChanged();
        }

        // 稅別變更自動處理
        if (fieldChange.PropertyName == "TaxCalculationMethod" && OnTaxMethodChanged != null)
        {
            await OnTaxMethodChanged();
            StateHasChanged();
        }

        // ActionButton 自動更新（透過 ModalManagers）
        if (ModalManagers != null && ModalManagers.ContainsKey(fieldChange.PropertyName))
        {
            var targetField = FormFields?.FirstOrDefault(f => f.PropertyName == fieldChange.PropertyName);
            // IsFilterOnly 欄位（如 FilterProductId）是虛擬欄位，實體中不存在對應屬性
            if (targetField != null && !targetField.IsReadOnly && !targetField.IsFilterOnly)
            {
                int? newId = null;
                if (fieldChange.Value != null && int.TryParse(fieldChange.Value.ToString(), out int id))
                    newId = id;

                var modalManager = ModalManagers[fieldChange.PropertyName];
                var updateMethod = modalManager != null ? GetCachedMethod(modalManager.GetType(), "UpdateFieldActionButtons") : null;
                if (updateMethod != null && modalManager != null)
                {
                    updateMethod.Invoke(modalManager, new object?[] { FormFields, fieldChange.PropertyName, newId });
                    _processedFormFieldsCache = null;
                }
            }
        }

        if (OnFieldChanged != null)
            await OnFieldChanged(fieldChange);
    }
}
