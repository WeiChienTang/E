using ERPCore2.Data;
using ERPCore2.Helpers;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace ERPCore2.Components.Shared.Modal;

/// <summary>
/// 生命週期、資料載入、公開 API（partial class）
/// </summary>
public partial class GenericEditModalComponent<TEntity, TService>
    where TEntity : BaseEntity, new()
{
    // ===== Tab 導航處理 =====

    private void HandleTabChanged(int index)
    {
        _activeTabIndex = index;
        StateHasChanged();
    }

    // ===== 生命週期方法 =====

    /// <summary>
    /// 當 Modal 已關閉且上次也是關閉狀態時，跳過整棵渲染樹的 diff，
    /// 避免父組件每次 StateHasChanged() 都觸發不必要的子組件渲染。
    /// </summary>
    protected override bool ShouldRender() => IsVisible || _renderedVisible;

    protected override Task OnInitializedAsync()
    {
        editContext = new EditContext(Entity);

        // 從 Circuit-scoped IUserPreferenceContext 讀取（由 MainLayout 於登入後寫入，無需 DB 查詢）
        _prefUnsavedWarning = UserPreferenceContext.ShowUnsavedChangesWarning;

        return Task.CompletedTask;
    }

    protected override async Task OnParametersSetAsync()
    {
        // 早期返回：如果 Modal 一直關閉且參數未變更，直接返回
        if (!IsVisible && !_lastVisible && _lastId == Id && !_isNavigating)
            return;

        // 關鍵修復：如果 _currentId 存在且與傳入的 Id 不同，
        // 說明是導航後的狀態，不應該被外部參數覆蓋
        if (_currentId.HasValue && _currentId != Id && !_isNavigating && IsVisible)
        {
            if (IdChanged.HasDelegate)
                await IdChanged.InvokeAsync(_currentId);

            _lastId = _currentId;
            return;
        }

        // 同步 _currentId 與 Id（除非正在導航中）
        if (!_isNavigating)
            _currentId = Id;

        if (IsVisible)
        {
            if (!_lastVisible)
            {
                // Modal 從關閉變成開啟
                _lastVisible = true;
                _lastId = Id;
                await LoadAllData();
            }
            else if (_lastId != Id)
            {
                // Modal 已開啟但 Id 變更
                _lastId = Id;
                await LoadAllData();
            }
        }
        else
        {
            if (_lastVisible)
            {
                // Modal 從開啟變成關閉
                _lastVisible = false;
                ResetState();
            }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        _renderedVisible = IsVisible;

        if (IsVisible && firstRender)
            await SetupTabNavigationAsync();
    }

    // ===== Tab 鍵導航 =====

    private async Task SetupTabNavigationAsync()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("setupButtonTabNavigation");
        }
        catch (Exception ex)
        {
            LogError("SetupTabNavigation", ex);
        }
    }

    private async Task CleanupTabNavigationAsync()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("cleanupButtonTabNavigation");
        }
        catch (Exception ex)
        {
            LogError("CleanupTabNavigation", ex);
        }
    }

    // ===== 資料載入方法 =====

    private async Task LoadAllData()
    {
        // 取消上一次尚未完成的載入，建立新的 CancellationToken
        _loadCts.Cancel();
        _loadCts.Dispose();
        _loadCts = new CancellationTokenSource();
        var token = _loadCts.Token;

        await _loadingSemaphore.WaitAsync();
        try
        {
            if (token.IsCancellationRequested) return;

            IsLoading = true;
            ErrorMessage = string.Empty;
            StateHasChanged();

            if (AdditionalDataLoader != null)
            {
                await AdditionalDataLoader();
                if (token.IsCancellationRequested) return;
            }

            // 關鍵修復：如果正在導航，跳過 DataLoader（Entity 已由 NavigateToRecordAsync 直接載入）
            if (!_isNavigating && DataLoader != null)
            {
                var loadedEntity = await DataLoader();
                if (token.IsCancellationRequested) return;

                if (loadedEntity != null)
                {
                    Entity = loadedEntity;
                    editContext = new EditContext(Entity);
                    UpdateAllActionButtons();

                    if (OnEntityLoaded.HasDelegate && Entity.Id > 0)
                        await OnEntityLoaded.InvokeAsync(Entity.Id);
                }
            }
            else if (_isNavigating)
            {
                UpdateAllActionButtons();
            }

            if (token.IsCancellationRequested) return;
            await LoadStatusMessageData();

            if (token.IsCancellationRequested) return;
            await LoadNavigationStateAsync();
        }
        catch (Exception ex)
        {
            if (!token.IsCancellationRequested)
            {
                ErrorMessage = $"載入資料時發生錯誤：{ex.Message}";
                LogError("LoadAllData", ex);
            }
        }
        finally
        {
            if (!token.IsCancellationRequested)
            {
                IsLoading = false;
                _isDirty = false;
                _processedFormFieldsCache = null;
                _cachedAuditInfo = null;
                _cachedModalTitle = null;
                _cachedModalIcon = null;
                StateHasChanged();
            }
            _loadingSemaphore.Release();
        }
    }

    /// <summary>
    /// 載入狀態訊息資料（整合審核和其他所有狀態訊息）
    /// </summary>
    private async Task LoadStatusMessageData()
    {
        try
        {
            if (!ShouldShowStatusMessage() || Entity == null || Entity.Id <= 0)
                return;

            if (GetStatusMessage != null)
            {
                var result = await GetStatusMessage();
                if (result.HasValue)
                {
                    _cachedStatusMessage = result.Value.Message;
                    _cachedStatusVariant = result.Value.Variant;
                    _cachedStatusIcon = result.Value.IconClass;
                }
            }
            else if (!string.IsNullOrEmpty(StatusMessage))
            {
                _cachedStatusMessage = StatusMessage;
                _cachedStatusVariant = StatusBadgeVariant;
                _cachedStatusIcon = StatusIconClass;
            }
        }
        catch (Exception ex)
        {
            LogError("LoadStatusMessageData", ex);
        }
    }

    // ===== 公開 API 方法 =====

    /// <summary>
    /// 開啟新增模式 Modal（設定 Id = null 並開啟）
    /// </summary>
    public async Task ShowAddModal()
    {
        Id = null;
        if (IdChanged.HasDelegate)
            await IdChanged.InvokeAsync(null);
        IsVisible = true;
        if (IsVisibleChanged.HasDelegate)
            await IsVisibleChanged.InvokeAsync(true);
    }

    /// <summary>
    /// 開啟編輯模式 Modal（設定 Id 並開啟）
    /// </summary>
    public async Task ShowEditModal(int entityId)
    {
        Id = entityId;
        if (IdChanged.HasDelegate)
            await IdChanged.InvokeAsync(entityId);
        IsVisible = true;
        if (IsVisibleChanged.HasDelegate)
            await IsVisibleChanged.InvokeAsync(true);
    }

    /// <summary>
    /// 以程式方式設定 AutoComplete 欄位的值與顯示文字
    /// </summary>
    public async Task SetAutoCompleteFieldValueAsync(string propertyName, string value, string displayText)
    {
        if (genericFormComponent != null)
            await genericFormComponent.SetAutoCompleteValueAsync(propertyName, value, displayText);
    }

    /// <summary>
    /// 重新載入實體資料
    /// </summary>
    public async Task RefreshEntityAsync()
    {
        if (DataLoader != null && Entity != null && Entity.Id > 0)
        {
            var reloadedEntity = await DataLoader();
            if (reloadedEntity != null)
            {
                Entity = reloadedEntity;
                editContext = new EditContext(Entity);
                _cachedAuditInfo = null;
                await LoadStatusMessageData();
                StateHasChanged();
            }
        }
    }

    /// <summary>
    /// 重新載入額外資料並刷新 AutoComplete 欄位顯示
    /// </summary>
    public async Task RefreshAutoCompleteFieldsAsync()
    {
        try
        {
            if (AdditionalDataLoader != null)
                await AdditionalDataLoader();

            UpdateAllActionButtons();
            _processedFormFieldsCache = null;
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            LogError("RefreshAutoCompleteFields", ex);
        }
    }

    // ===== 資源釋放 =====

    public void Dispose()
    {
        _unsavedChangesConfirmTcs?.TrySetCanceled();
        _loadCts.Cancel();
        _loadCts.Dispose();
        _loadingSemaphore.Dispose();
    }
}
