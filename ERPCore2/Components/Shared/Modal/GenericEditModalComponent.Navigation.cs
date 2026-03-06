using ERPCore2.Data;
using Microsoft.AspNetCore.Components.Forms;

namespace ERPCore2.Components.Shared.Modal;

/// <summary>
/// 記錄導航處理（partial class）
/// </summary>
public partial class GenericEditModalComponent<TEntity, TService>
    where TEntity : BaseEntity, new()
{
    #region 記錄導航處理

    /// <summary>
    /// 載入導航狀態（上一筆和下一筆的 ID，支援循環導航）
    /// 新增模式：只載入最後一筆 ID
    /// 編輯模式：載入完整導航資訊
    /// </summary>
    private async Task LoadNavigationStateAsync()
    {
        var currentId = _currentId ?? Id;

        if (!EnableNavigation || Service == null)
        {
            _previousId = null;
            _nextId = null;
            _firstId = null;
            _lastRecordId = null;
            return;
        }

        try
        {
            var serviceType = Service.GetType();

            // 新增模式：只載入最後一筆 ID（用於返回編輯按鈕）
            if (!currentId.HasValue)
            {
                var getLastMethod = GetCachedMethod(serviceType, "GetLastIdAsync");
                if (getLastMethod != null)
                    _lastRecordId = await (Task<int?>)getLastMethod.Invoke(Service, Array.Empty<object>())!;

                _previousId = null;
                _nextId = null;
                _firstId = null;
                return;
            }

            // 編輯模式：使用快取 MethodInfo，並以 Task.WhenAll 並行執行四個導航查詢
            var getPrev  = GetCachedMethod(serviceType, "GetPreviousIdAsync");
            var getNext  = GetCachedMethod(serviceType, "GetNextIdAsync");
            var getFirst = GetCachedMethod(serviceType, "GetFirstIdAsync");
            var getLast  = GetCachedMethod(serviceType, "GetLastIdAsync");

            var prevTask  = getPrev  != null ? (Task<int?>)getPrev.Invoke(Service,  new object[] { currentId.Value })! : Task.FromResult<int?>(null);
            var nextTask  = getNext  != null ? (Task<int?>)getNext.Invoke(Service,  new object[] { currentId.Value })! : Task.FromResult<int?>(null);
            var firstTask = getFirst != null ? (Task<int?>)getFirst.Invoke(Service, Array.Empty<object>())!            : Task.FromResult<int?>(null);
            var lastTask  = getLast  != null ? (Task<int?>)getLast.Invoke(Service,  Array.Empty<object>())!            : Task.FromResult<int?>(null);

            await Task.WhenAll(prevTask, nextTask, firstTask, lastTask);

            _previousId   = prevTask.Result;
            _nextId       = nextTask.Result;
            _firstId      = firstTask.Result;
            _lastRecordId = lastTask.Result;

            // 循環邏輯：如果沒有上一筆，設為最後一筆；如果沒有下一筆，設為第一筆
            if (!_previousId.HasValue && _lastRecordId.HasValue)
                _previousId = _lastRecordId.Value;

            if (!_nextId.HasValue && _firstId.HasValue)
                _nextId = _firstId.Value;
        }
        catch (Exception ex)
        {
            LogError("LoadNavigationStateAsync", ex);
            _previousId = null;
            _nextId = null;
            _firstId = null;
            _lastRecordId = null;
        }
    }

    /// <summary>
    /// 處理返回編輯按鈕點擊（新增模式專用，返回到最後一筆記錄）
    /// </summary>
    private async Task HandleReturnToLast()
    {
        if (IsLoading || IsSubmitting) return;

        try
        {
            if (_lastRecordId.HasValue)
                await NavigateToRecordAsync(_lastRecordId.Value);
            else
                await ShowErrorMessage($"找不到最後一筆{EntityName}");
        }
        catch (Exception ex)
        {
            await ShowErrorMessage($"返回編輯{EntityName}時發生錯誤");
            LogError("HandleReturnToLast", ex);
        }
    }

    /// <summary>
    /// 處理新增按鈕點擊（編輯模式專用，切換到新增模式）
    /// </summary>
    private async Task HandleAddClick()
    {
        if (IsLoading || IsSubmitting) return;

        try
        {
            _stayInAddMode = true;
            _lastId = -1;  // 設為不可能的值，確保觸發重新載入
            Id = null;
            _currentId = null;

            if (IdChanged.HasDelegate)
                await IdChanged.InvokeAsync(null);

            await LoadAllData();

            // 同步 _lastId 為 null，防止 OnParametersSetAsync 錯誤觸發
            _lastId = null;
        }
        catch (Exception ex)
        {
            await ShowErrorMessage($"切換到新增模式時發生錯誤：{ex.Message}");
            LogError("HandleAddClick", ex);
        }
    }

    /// <summary>
    /// 處理上一筆按鈕點擊（支援循環導航）
    /// </summary>
    private async Task HandlePrevious()
    {
        if (IsLoading || IsSubmitting) return;

        try
        {
            if (_previousId.HasValue)
                await NavigateToRecordAsync(_previousId.Value);
            else
                await ShowErrorMessage($"無法切換到上一筆{EntityName}");
        }
        catch (Exception ex)
        {
            await ShowErrorMessage($"切換到上一筆{EntityName}時發生錯誤");
            LogError("HandlePrevious", ex);
        }
    }

    /// <summary>
    /// 處理下一筆按鈕點擊（支援循環導航）
    /// </summary>
    private async Task HandleNext()
    {
        if (IsLoading || IsSubmitting) return;

        try
        {
            if (_nextId.HasValue)
                await NavigateToRecordAsync(_nextId.Value);
            else
                await ShowErrorMessage($"無法切換到下一筆{EntityName}");
        }
        catch (Exception ex)
        {
            await ShowErrorMessage($"切換到下一筆{EntityName}時發生錯誤");
            LogError("HandleNext", ex);
        }
    }

    /// <summary>
    /// 導航到指定的記錄（方案 A：改為呼叫 DataLoader，統一載入路徑）
    /// </summary>
    private async Task NavigateToRecordAsync(int targetId)
    {
        try
        {
            _isNavigating = true;
            _lastId = targetId;
            _currentId = targetId;

            if (IdChanged.HasDelegate)
                await IdChanged.InvokeAsync(targetId);

            TEntity? loadedEntity = null;

            if (DataLoader != null)
            {
                // 暫時清除導航標記，讓 DataLoader 走正常路徑
                _isNavigating = false;
                loadedEntity = await DataLoader();
            }
            else if (Service != null)
            {
                // 後備方案：如果沒有 DataLoader，使用 Service 直接載入
                var getByIdMethod = GetCachedMethod(Service.GetType(), "GetByIdAsync");
                if (getByIdMethod != null)
                {
                    var getByIdTask = getByIdMethod.Invoke(Service, new object[] { targetId }) as Task<TEntity>;
                    if (getByIdTask != null)
                        loadedEntity = await getByIdTask;
                }
            }

            if (loadedEntity != null)
                await ApplyNavigatedEntityAsync(loadedEntity, targetId);
            else
                await ShowErrorMessage($"找不到指定的{EntityName}");
        }
        catch (Exception ex)
        {
            await ShowErrorMessage($"導航時發生錯誤：{ex.Message}");
            LogError("NavigateToRecordAsync", ex);
        }
        finally
        {
            _isNavigating = false;
        }
    }

    /// <summary>
    /// 導航載入完成後的共用後續操作（更新狀態、快取、觸發事件）
    /// </summary>
    private async Task ApplyNavigatedEntityAsync(TEntity entity, int targetId)
    {
        Entity = entity;
        editContext = new EditContext(Entity);
        UpdateAllActionButtons();
        await LoadStatusMessageData();
        await LoadNavigationStateAsync();

        if (OnEntityLoaded.HasDelegate)
            await OnEntityLoaded.InvokeAsync(targetId);

        StateHasChanged();
    }

    /// <summary>
    /// 取得上一筆按鈕的提示文字
    /// </summary>
    private string GetPreviousButtonTitle()
    {
        if (!_previousId.HasValue)
            return $"沒有上一筆{EntityName}";

        if (_firstId.HasValue && Id.HasValue && Id.Value == _firstId.Value)
            return $"切換到最後一筆{EntityName}（循環）";

        return $"切換到上一筆{EntityName}";
    }

    /// <summary>
    /// 取得下一筆按鈕的提示文字
    /// </summary>
    private string GetNextButtonTitle()
    {
        if (!_nextId.HasValue)
            return $"沒有下一筆{EntityName}";

        if (_lastRecordId.HasValue && Id.HasValue && Id.Value == _lastRecordId.Value)
            return $"切換到第一筆{EntityName}（循環）";

        return $"切換到下一筆{EntityName}";
    }

    #endregion
}
