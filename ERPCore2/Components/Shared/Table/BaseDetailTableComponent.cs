using ERPCore2.Data;
using ERPCore2.Helpers;
using Microsoft.AspNetCore.Components;

namespace ERPCore2.Components.Shared.Table;

/// <summary>
/// 明細表格組件基底類別
/// 統一管理採購/入庫/入庫退回/報價/訂單/銷貨/銷貨退回等模組共用邏輯：
/// - DataVersion 追蹤（切換上下筆時重載）
/// - SelectedSupplierId 追蹤（切換對象時重載）
/// - _dataLoadCompleted 狀態管理（確保空白行在所有情境下正確觸發）
/// - _hasUndeletableDetails 計算與通知
/// - RefreshEmptyRow() 一致呼叫
/// </summary>
/// <typeparam name="TMainEntity">主單實體（如 PurchaseOrder）</typeparam>
/// <typeparam name="TDetailEntity">明細實體（如 PurchaseOrderDetail）</typeparam>
/// <typeparam name="TItem">UI 列表項目模型（如 ProductItem）</typeparam>
public abstract class BaseDetailTableComponent<TMainEntity, TDetailEntity, TItem> : ComponentBase
    where TMainEntity : BaseEntity
    where TDetailEntity : BaseEntity, new()
    where TItem : class, new()
{
    // ===== 共用參數 =====

    [Parameter] public TMainEntity? MainEntity { get; set; }
    [Parameter] public List<TDetailEntity> ExistingDetails { get; set; } = new();
    [Parameter] public EventCallback<List<TDetailEntity>> OnDetailsChanged { get; set; }
    [Parameter] public bool IsReadOnly { get; set; } = false;
    [Parameter] public bool IsParentLoading { get; set; } = false;
    [Parameter] public bool IsApproved { get; set; } = false;
    [Parameter] public int DataVersion { get; set; } = 0;

    /// <summary>
    /// 對象 ID（廠商 / 客戶）。當此值改變時，自動清空並重載明細。
    /// 對沒有對象概念的模組，保持 null 即可，supplierChanged 分支永遠不觸發。
    /// </summary>
    [Parameter] public int? SelectedSupplierId { get; set; }

    [Parameter] public EventCallback<bool> OnHasUndeletableDetailsChanged { get; set; }

    // ===== 共用狀態（子類別可存取）=====

    protected bool _dataLoadCompleted = true;
    protected bool _hasUndeletableDetails = false;
    public List<TItem> Items { get; protected set; } = new();
    protected InteractiveTableComponent<TItem>? tableComponent;

    // ===== 私有追蹤欄位 =====

    private int _previousDataVersion = 0;
    private int? _previousSelectedSupplierId = null;
    private bool _isLoadingDetails = false;
    private bool _previousTableComponentNull = true;

    /// <summary>
    /// InvokeLoadWithCompletionAsync() 是否已至少完成一次。
    /// 用於防止 OnAfterRenderAsync 在中間渲染（Intermediate Render）時提早呼叫
    /// RefreshEmptyRow()，避免污染 InteractiveTableComponent._previousDataLoadCompleted。
    /// </summary>
    private bool _hasLoadCompleted = false;

    // ===== 抽象方法（子類別必須實作）=====

    /// <summary>
    /// 將 ExistingDetails 對應到 Items 列表。
    /// 注意：Items 已由基底類別 Clear()，此方法只需 Add 項目。
    /// ExistingDetails 為空時可直接 return，不必做任何事。
    /// </summary>
    protected abstract Task LoadItemsFromDetailsAsync();

    /// <summary>
    /// 判斷目前 Items 中是否有不可刪除的明細（已有下游記錄）。
    /// 範例：Items.Any(p => !DetailLockHelper.CanDeleteItem(p, out _, checkReceiving: true))
    /// </summary>
    protected abstract bool ComputeHasUndeletableDetails();

    /// <summary>
    /// 將 Items 轉換回 TDetailEntity 列表（用於通知父元件）。
    /// </summary>
    protected abstract List<TDetailEntity> ConvertItemsToDetails();

    // ===== 虛方法（子類別可選擇性覆寫）=====

    /// <summary>
    /// 當 SelectedSupplierId 改變時執行（如重新載入廠商已知商品目錄）。
    /// 在 InvokeLoadWithCompletionAsync() 之前呼叫。
    /// </summary>
    protected virtual Task OnCounterpartyChangedAsync() => Task.CompletedTask;

    /// <summary>
    /// 資料載入完成後的額外操作（如檢查歷史採購記錄）。
    /// 在 InvokeLoadWithCompletionAsync() 之後呼叫。
    /// </summary>
    protected virtual Task OnAfterLoadAsync() => Task.CompletedTask;

    // ===== 共用生命週期 =====

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        // 偵測 tableComponent 從 null → 非 null 的轉換（例如 @if 條件成立後首次渲染）
        // 必須加上 _hasLoadCompleted 守衛：
        //   若在 OnInitializedAsync 中途（await LoadProductsAsync() 等）產生中間渲染，
        //   tableComponent 會在 InvokeLoadWithCompletionAsync() 執行前就被賦值。
        //   此時若提早呼叫 RefreshEmptyRow()，會污染 InteractiveTableComponent
        //   的 _previousDataLoadCompleted = true，導致後續真正的 false→true 轉換
        //   無法被偵測，空白行永遠不出現。
        //   加上 _hasLoadCompleted 守衛後，只有在資料真正載入完成後才觸發。
        bool tableJustAppeared = _previousTableComponentNull && tableComponent != null;
        _previousTableComponentNull = tableComponent == null;

        if (tableJustAppeared && _hasLoadCompleted)
            tableComponent!.RefreshEmptyRow();

        return Task.CompletedTask;
    }

    protected override async Task OnInitializedAsync()
    {
        _previousSelectedSupplierId = SelectedSupplierId;

        if (IsParentLoading)
            return;

        _previousDataVersion = DataVersion;

        if (SelectedSupplierId.HasValue && SelectedSupplierId.Value > 0)
            await OnCounterpartyChangedAsync();

        await InvokeLoadWithCompletionAsync();

        // OnInitializedAsync 場景：當 tableComponent 在中間渲染時已被賦值（supplier 初始 > 0），
        // OnAfterRenderAsync 因 _hasLoadCompleted 守衛而未呼叫 RefreshEmptyRow()。
        // 此時 InvokeLoadWithCompletionAsync() 已完成，_hasLoadCompleted = true，
        // tableComponent 若已非 null 則在此直接補呼叫；若仍為 null，
        // 後續首次渲染的 OnAfterRenderAsync 會以 tableJustAppeared && _hasLoadCompleted 觸發。
        tableComponent?.RefreshEmptyRow();

        await OnAfterLoadAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        base.OnParametersSet();

        if (_isLoadingDetails)
            return;

        bool versionChanged = DataVersion != _previousDataVersion;

        if (versionChanged)
        {
            _previousDataVersion = DataVersion;
            _previousSelectedSupplierId = SelectedSupplierId;

            _isLoadingDetails = true;
            try
            {
                await InvokeLoadWithCompletionAsync();
                tableComponent?.RefreshEmptyRow();
            }
            finally
            {
                _isLoadingDetails = false;
            }
        }
        else
        {
            bool supplierChanged = _previousSelectedSupplierId != SelectedSupplierId;

            if (supplierChanged)
            {
                _previousSelectedSupplierId = SelectedSupplierId;

                _isLoadingDetails = true;
                try
                {
                    await OnCounterpartyChangedAsync();
                    await InvokeLoadWithCompletionAsync();
                    await OnAfterLoadAsync();
                    tableComponent?.RefreshEmptyRow();
                }
                finally
                {
                    _isLoadingDetails = false;
                }
            }
        }
    }

    // ===== 共用方法 =====

    /// <summary>
    /// 通知父元件明細已變更，並更新不可刪除狀態。
    /// 子類別在任何明細異動後呼叫此方法。
    /// </summary>
    protected async Task NotifyDetailsChangedAsync()
    {
        var details = ConvertItemsToDetails();
        await DetailSyncHelper.SyncToParentAsync(details, OnDetailsChanged);

        bool hasUndeletable = ComputeHasUndeletableDetails();
        if (_hasUndeletableDetails != hasUndeletable)
        {
            _hasUndeletableDetails = hasUndeletable;
            await OnHasUndeletableDetailsChanged.InvokeAsync(hasUndeletable);
            tableComponent?.RefreshEmptyRow();
            StateHasChanged();
        }
    }

    /// <summary>
    /// 公開方法：刷新明細資料（供父元件呼叫，例如子單據儲存後）
    /// </summary>
    public async Task RefreshDetailsAsync()
    {
        await InvokeLoadWithCompletionAsync();
        tableComponent?.RefreshEmptyRow();
        StateHasChanged();
    }

    /// <summary>
    /// 包裝器：管理 _dataLoadCompleted 生命週期 + _hasUndeletableDetails 更新。
    /// 解決 ExistingDetails 為空時 DataLoadCompleted 不轉換導致空白行不出現的根本問題：
    /// 此方法無論 ExistingDetails 是否為空，都會讓 _dataLoadCompleted 走 false→true 轉換。
    /// </summary>
    private async Task InvokeLoadWithCompletionAsync()
    {
        _hasLoadCompleted = false;
        _dataLoadCompleted = false;
        Items.Clear();

        await LoadItemsFromDetailsAsync();

        bool hasUndeletable = ComputeHasUndeletableDetails();
        if (_hasUndeletableDetails != hasUndeletable)
            _hasUndeletableDetails = hasUndeletable;

        _dataLoadCompleted = true;
        _hasLoadCompleted = true;
        StateHasChanged();
    }

    // ===== Reflection 工具方法（供需要反射屬性映射的子類別使用）=====

    protected T? GetPropertyValue<T>(object obj, string propertyName)
    {
        var property = obj.GetType().GetProperty(propertyName);
        if (property == null) return default;
        var value = property.GetValue(obj);
        if (value == null) return default;
        if (typeof(T) == typeof(object)) return (T)value;

        var targetType = typeof(T);
        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlyingType = Nullable.GetUnderlyingType(targetType);
            if (underlyingType != null)
                return (T)Convert.ChangeType(value, underlyingType);
        }

        return (T)Convert.ChangeType(value, targetType);
    }

    protected void SetPropertyValue(object obj, string propertyName, object? value)
    {
        var property = obj.GetType().GetProperty(propertyName);
        if (property == null || !property.CanWrite) return;

        if (value != null && property.PropertyType != value.GetType())
        {
            if (property.PropertyType.IsGenericType &&
                property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var underlyingType = Nullable.GetUnderlyingType(property.PropertyType);
                value = Convert.ChangeType(value, underlyingType!);
            }
            else
            {
                value = Convert.ChangeType(value, property.PropertyType);
            }
        }

        property.SetValue(obj, value);
    }
}
