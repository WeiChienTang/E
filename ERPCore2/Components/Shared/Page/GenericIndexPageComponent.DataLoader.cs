using ERPCore2.Data;
using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Services;

namespace ERPCore2.Components.Shared.Page;

/// <summary>
/// 資料載入、篩選、分頁與公開刷新 API（partial class）
/// 優化重點：
///   - LoadPageDataAsync 抽取共用邏輯，消除 InitializePageAsync / RefreshData 重複程式碼
///   - ApplyFilters 改為同步 void，StateHasChanged 由 caller 統一管理
///   - Handle* 事件方法職責單一，不重複呼叫 StateHasChanged
///   - CancellationToken 防止快速連按重新整理時多個載入並行競爭
/// </summary>
public partial class GenericIndexPageComponent<TEntity, TService> : IDisposable
    where TEntity : BaseEntity
    where TService : IGenericManagementService<TEntity>
{
    #region 初始化

    private Task InitializePageAsync() => ExecuteWithLoadingAsync(LoadPageDataAsync, "初始化頁面時發生錯誤");

    /// <summary>統一的 loading 包裝器，消除 InitializePageAsync / RefreshData 重複樣板</summary>
    private async Task ExecuteWithLoadingAsync(Func<Task> action, string errorPrefix)
    {
        isLoading = true;
        StateHasChanged();
        try { await action(); }
        catch (Exception ex) { Console.Error.WriteLine($"{errorPrefix}: {ex.Message}"); }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    #endregion

    #region 資料載入核心（共用邏輯）

    /// <summary>
    /// 共用資料載入流程：初始化基礎資料 → 統計資料 → 主要清單資料
    /// 由 InitializePageAsync 與 RefreshData 共同呼叫，避免重複程式碼
    /// </summary>
    private async Task LoadPageDataAsync()
    {
        if (InitializeBasicData != null)
            await InitializeBasicData();

        if (ShowStatisticsCards && StatisticsDataLoader != null)
            _statisticsData = await StatisticsDataLoader();

        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        // 取消前一次尚未完成的載入，防止並行競爭
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = new CancellationTokenSource();
        var token = _loadCts.Token;

        try
        {
            if (DataLoader != null)
            {
                var data = await DataLoader();

                // 若此次載入已被後續請求取消，直接捨棄結果
                if (token.IsCancellationRequested) return;

                allItems = data;
                ApplyFilters(resetPage: false);
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            if (token.IsCancellationRequested) return;
            Console.Error.WriteLine($"載入資料失敗: {ex.Message}");
            allItems = new List<TEntity>();
            filteredItems = new List<TEntity>();
            StateHasChanged();
        }
    }

    #endregion

    #region 篩選與分頁

    /// <summary>
    /// 套用篩選邏輯並更新分頁快取（同步，StateHasChanged 由 caller 負責）
    /// </summary>
    private void ApplyFilters(bool resetPage = true)
    {
        var query = allItems.AsQueryable();

        query = FilterApplier != null
            ? FilterApplier(searchModel, query)
            : ApplyStandardFilters(searchModel, query);

        filteredItems = query.ToList();
        totalItems    = filteredItems.Count;
        if (resetPage) currentPage = 1;

        UpdatePagedItems();

        if (ShowStatisticsCards && StatisticsCardConfigs?.Any() == true)
            UpdateStatisticsData();
    }

    /// <summary>當沒有提供自訂 FilterApplier 時，套用內建備註篩選</summary>
    private IQueryable<TEntity> ApplyStandardFilters(SearchFilterModel model, IQueryable<TEntity> query)
    {
        if (AutoAddRemarksFilter)
        {
            var remarksFilter = model.GetFilterValue("Remarks")?.ToString();
            if (!string.IsNullOrWhiteSpace(remarksFilter))
                query = query.Where(e => e.Remarks != null && e.Remarks.Contains(remarksFilter));
        }

        return query;
    }

    /// <summary>
    /// 更新基本筆數統計（僅維護通用的 TotalItems）
    /// 業務特定統計（如 TotalOrders）請由 StatisticsDataLoader 提供，不在此硬寫
    /// </summary>
    private void UpdateStatisticsData()
    {
        _statisticsData["TotalItems"] = filteredItems.Count;
    }

    private void UpdatePagedItems()
    {
        var maxPage = totalItems > 0 ? (int)Math.Ceiling((double)totalItems / pageSize) : 1;
        if (currentPage > maxPage)
            currentPage = Math.Max(1, maxPage);

        pagedItems = filteredItems
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    #endregion

    #region 事件處理

    private Task HandleSearch(SearchFilterModel filterModel)
    {
        searchModel = filterModel;
        ApplyFilters();
        StateHasChanged();
        return Task.CompletedTask;
    }

    private Task HandleFilterChanged(SearchFilterModel filterModel)
    {
        searchModel = filterModel;
        return Task.CompletedTask;
    }

    private Task HandlePageChanged(int newPage)
    {
        currentPage = newPage;
        UpdatePagedItems();
        StateHasChanged();
        return Task.CompletedTask;
    }

    private Task HandlePageSizeChanged(int newPageSize)
    {
        pageSize    = newPageSize;
        currentPage = 1;
        UpdatePagedItems();
        StateHasChanged();
        return Task.CompletedTask;
    }

    #endregion

    #region 刷新

    private async Task RefreshData(RefreshMode? overrideMode = null)
    {
        var mode = overrideMode ?? PageRefreshMode;
        if (mode == RefreshMode.ForceReload)
        {
            Navigation.NavigateTo(Navigation.Uri, forceLoad: true);
            return;
        }

        await ExecuteWithLoadingAsync(LoadPageDataAsync, "刷新資料時發生錯誤");
    }

    #endregion

    #region 公開 API

    public Task Refresh() => RefreshData();

    /// <summary>使用指定的刷新方式刷新頁面</summary>
    public Task Refresh(RefreshMode mode) => RefreshData(mode);

    /// <summary>平滑刷新 - 僅重新載入資料，不會造成頁面閃爍</summary>
    public Task SmoothRefresh() => RefreshData(RefreshMode.Smooth);

    /// <summary>強制刷新 - 重新載入整個頁面</summary>
    public Task ForceRefresh() => RefreshData(RefreshMode.ForceReload);

    public Task ReloadData() => LoadDataAsync();

    public void ResetFilters()
    {
        searchModel = new SearchFilterModel();
        ApplyFilters();
        StateHasChanged();
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = null;
    }

    #endregion
}
