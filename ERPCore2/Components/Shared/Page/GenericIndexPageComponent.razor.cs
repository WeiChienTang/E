using ERPCore2.Data;
using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Models.Documents;
using ERPCore2.Models.Enums;
using ERPCore2.Services;
using ERPCore2.Services.Reports.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;

namespace ERPCore2.Components.Shared.Page;

/// <summary>
/// 泛型 Index 頁面基底組件（partial class 主檔）
/// 負責：enum 定義、所有 [Parameter]、私有欄位、公開屬性、生命週期、快取管理、文字 helper
/// </summary>
public partial class GenericIndexPageComponent<TEntity, TService>
    where TEntity : BaseEntity
    where TService : IGenericManagementService<TEntity>
{
    // ===== 刷新方式枚舉 =====

    /// <summary>頁面刷新方式</summary>
    public enum RefreshMode
    {
        /// <summary>平滑刷新 - 僅重新載入資料，不會造成頁面閃爍</summary>
        Smooth,
        /// <summary>強制刷新 - 重新載入整個頁面，會造成短暫閃爍但確保完全重置</summary>
        ForceReload
    }

    // ===== 參數 =====

    // 權限 / 模組 / Debug
    [Parameter] public string RequiredPermission { get; set; } = "";
    [Parameter] public string RequiredModule { get; set; } = "";
    [Parameter] public string? DebugPageName { get; set; }

    // 頁面行為
    [Parameter] public RefreshMode PageRefreshMode { get; set; } = RefreshMode.Smooth;

    // 頁面基本文字
    [Parameter] public string PageTitle { get; set; } = "資料管理";
    [Parameter] public string PageSubtitle { get; set; } = "管理資料";
    [Parameter] public string AddButtonText { get; set; } = "";
    [Parameter] public string AddButtonTitle { get; set; } = "";
    [Parameter] public string SearchSectionTitle { get; set; } = "";
    [Parameter] public string EmptyMessage { get; set; } = "";
    [Parameter] public string ActionsHeader { get; set; } = "";

    // 動作按鈕
    [Parameter] public bool ShowDefaultActions { get; set; } = true;
    [Parameter] public bool ShowAddButton { get; set; } = true;
    [Parameter] public RenderFragment? CustomActionButtons { get; set; }

    /// <summary>
    /// 自訂額外按鈕（追加在預設按鈕列尾端，不會取代預設按鈕）。
    /// 與 CustomActionButtons 不同：CustomActionButtons 會完全替換按鈕列，
    /// 而 CustomIndexButtons 只會在預設按鈕列後方追加。
    /// </summary>
    [Parameter] public RenderFragment? CustomIndexButtons { get; set; }

    // 匯出
    [Parameter] public bool ShowExportExcelButton { get; set; } = false;
    [Parameter] public bool ShowExportPdfButton { get; set; } = false;
    [Parameter] public EventCallback OnExportExcelClick { get; set; }
    [Parameter] public EventCallback OnExportPdfClick { get; set; }

    // 批次操作
    [Parameter] public bool ShowBatchPrintButton { get; set; } = false;
    [Parameter] public EventCallback OnBatchPrintClick { get; set; }
    [Parameter] public bool ShowBarcodePrintButton { get; set; } = false;
    [Parameter] public EventCallback OnBarcodePrintClick { get; set; }
    [Parameter] public bool ShowBatchApprovalButton { get; set; } = false;
    [Parameter] public EventCallback OnBatchApprovalClick { get; set; }
    [Parameter] public bool ShowImportScheduleButton { get; set; } = false;
    [Parameter] public EventCallback OnImportScheduleClick { get; set; }
    [Parameter] public bool ShowBatchDeleteButton { get; set; } = false;
    [Parameter] public bool IsBatchDeleteDisabled { get; set; } = false;
    [Parameter] public EventCallback OnBatchDeleteClick { get; set; }

    // 內建批次刪除 Modal（設定後自動接管 OnBatchDeleteClick，無需在頁面自行管理 modal 狀態）
    [Parameter] public List<InteractiveColumnDefinition>? BatchDeleteColumnDefinitions { get; set; }
    [Parameter] public string BatchDeleteTitle { get; set; } = "";
    [Parameter] public EventCallback<List<TEntity>> OnBatchDelete { get; set; }

    // 麵包屑
    [Parameter] public List<BreadcrumbItem> BreadcrumbItems { get; set; } = new();

    // 統計卡片
    [Parameter] public bool ShowStatisticsCards { get; set; } = false;
    [Parameter] public List<StatisticsCardConfig>? StatisticsCardConfigs { get; set; }
    [Parameter] public Func<Task<Dictionary<string, object>>>? StatisticsDataLoader { get; set; }

    // 導航 URL
    [Parameter] public string EntityBasePath { get; set; } = "";
    [Parameter] public string CreateUrl { get; set; } = "";
    [Parameter] public string DetailUrl { get; set; } = "";
    [Parameter] public string EditUrl { get; set; } = "";

    // 服務與資料
    [Parameter] public TService Service { get; set; } = default!;
    [Parameter] public Func<Task<List<TEntity>>> DataLoader { get; set; } = default!;
    [Parameter] public Func<Task> InitializeBasicData { get; set; } = default!;

    /// <summary>
    /// 伺服器端分頁載入器（opt-in）。
    /// 設定後優先使用，DataLoader 將被忽略。
    /// 參數：(篩選條件, 頁碼, 每頁筆數) → (本頁資料, 總筆數)
    /// </summary>
    [Parameter] public Func<SearchFilterModel, int, int, Task<(List<TEntity> Items, int TotalCount)>>? ServerDataLoader { get; set; }

    // 篩選與表格定義
    [Parameter] public List<SearchFilterDefinition> FilterDefinitions { get; set; } = new();
    [Parameter] public List<TableColumnDefinition> ColumnDefinitions { get; set; } = new();
    [Parameter] public Func<SearchFilterModel, IQueryable<TEntity>, IQueryable<TEntity>> FilterApplier { get; set; } = default!;

    // 自動標準欄位
    [Parameter] public bool AutoAddRemarksColumn { get; set; } = true;
    [Parameter] public bool AutoAddCreatedAtColumn { get; set; } = false;
    [Parameter] public bool AutoAddRemarksFilter { get; set; } = true;
    [Parameter] public string RemarksColumnTitle { get; set; } = "";
    [Parameter] public string CreatedAtColumnTitle { get; set; } = "";
    [Parameter] public string RemarksFilterTitle { get; set; } = "";

    // 搜尋組件
    [Parameter] public bool AutoSearch { get; set; } = true;
    [Parameter] public bool ShowSearchButton { get; set; } = true;
    [Parameter] public int SearchDelayMs { get; set; } = 500;

    // 表格組件
    [Parameter] public bool ShowActions { get; set; } = true;
    [Parameter] public bool EnableRowClick { get; set; } = true;
    [Parameter] public bool EnableSorting { get; set; } = false;
    [Parameter] public bool IsStriped { get; set; } = true;
    [Parameter] public bool IsHoverable { get; set; } = true;
    [Parameter] public bool IsBordered { get; set; } = false;
    [Parameter] public TableSize TableSize { get; set; } = TableSize.Normal;
    [Parameter] public List<ContextMenuItem<TEntity>>? ContextMenuItems { get; set; }
    [Parameter] public bool EnableMultiSelect { get; set; } = false;
    [Parameter] public Func<TEntity, bool>? IsMultiSelectItemSelectable { get; set; }
    [Parameter] public bool EnablePagination { get; set; } = true;
    [Parameter] public bool ShowPageSizeSelector { get; set; } = true;
    /// <summary>
    /// 每頁顯示筆數。預設 0 表示使用使用者偏好設定（由 IUserPreferenceContext 提供）；
    /// 明確傳入正整數時以傳入值為準，忽略使用者偏好。
    /// </summary>
    [Parameter] public int DefaultPageSize { get; set; } = 0;

    // 刪除功能
    [Parameter] public string EntityName { get; set; } = "資料";
    [Parameter] public Func<TEntity, string> GetEntityDisplayName { get; set; } = entity => entity.Id.ToString();
    [Parameter] public string DeleteSuccessMessage { get; set; } = "";
    [Parameter] public string DeleteConfirmMessage { get; set; } = "";
    [Parameter] public bool EnableStandardActions { get; set; } = true;
    [Parameter] public bool ShowViewButton { get; set; } = false;
    [Parameter] public bool ShowEditButton { get; set; } = false;
    [Parameter] public bool ShowDeleteButton { get; set; } = true;
    [Parameter] public RenderFragment<TEntity>? CustomActionsTemplate { get; set; }

    /// <summary>
    /// 判斷特定實體是否可以刪除的函數。
    /// 如果返回 false，則該實體的刪除按鈕將被隱藏。
    /// 預設所有實體都可以刪除，除非明確指定判斷邏輯。
    /// 例如：entity => entity.CreatedBy != "System"
    /// </summary>
    [Parameter] public Func<TEntity, bool>? CanDelete { get; set; }

    /// <summary>
    /// 是否啟用預設的系統資料保護機制。
    /// 當啟用時，CreatedBy 為 "System" 的資料將不可刪除。預設值：true。
    /// </summary>
    [Parameter] public bool EnableSystemDataProtection { get; set; } = true;

    [Parameter] public Func<TEntity, Task<bool>>? CustomDeleteHandler { get; set; }
    [Parameter] public EventCallback OnAddClick { get; set; }
    [Parameter] public EventCallback<TEntity> OnRowClick { get; set; }
    [Parameter] public EventCallback<TEntity> OnRowPrint { get; set; }

    /// <summary>
    /// 設定後自動在右鍵選單加入「列印」項目，由元件內部管理預覽 Modal。
    /// 需搭配 RowPrintReportId 使用。
    /// </summary>
    [Parameter] public IEntityReportService<TEntity>? RowPrintService { get; set; }

    /// <summary>報表 ID，用於讀取列印配置（如 ReportIds.CustomerDetail）。</summary>
    [Parameter] public string RowPrintReportId { get; set; } = "";

    [Parameter] public RenderFragment<TEntity>? ActionsTemplate { get; set; }

    // ===== 私有欄位 =====

    // 資料
    private List<TEntity> allItems = new();
    private List<TEntity> filteredItems = new();
    private List<TEntity> pagedItems = new();
    private Dictionary<string, object> _statisticsData = new();

    // 快取定義
    private List<TableColumnDefinition> _cachedColumnDefinitions = new();
    private List<SearchFilterDefinition> _cachedFilterDefinitions = new();
    private List<ContextMenuItem<TEntity>>? _cachedEffectiveContextMenuItems;

    // 快取失效追蹤（條件式重建，避免每次 OnParametersSet 都重建）
    private List<TableColumnDefinition>? _prevColumnDefs;
    private List<SearchFilterDefinition>? _prevFilterDefs;
    private List<ContextMenuItem<TEntity>>? _prevContextMenuItems;
    private IEntityReportService<TEntity>? _prevRowPrintService;
    private bool   _prevAutoRemarksColumn;
    private bool   _prevAutoRemarksFilter;
    private bool   _prevAutoCreatedAt;
    private bool   _prevShowActions;
    private bool   _prevEnableStandardActions;
    private string _prevActionsHeader                       = "";
    private string _prevRemarksColumnTitle                  = "";
    private string _prevCreatedAtColumnTitle                = "";
    private string _prevRemarksFilterTitle                  = "";
    private RenderFragment<TEntity>? _prevActionsTemplate;
    private RenderFragment<TEntity>? _prevCustomActionsTemplate;

    // 並行載入保護
    private CancellationTokenSource? _loadCts;

    // 草稿 Tab 參數
    /// <summary>是否顯示正式/草稿切換按鈕（預設 false）</summary>
    [Parameter] public bool ShowDraftTab { get; set; } = false;

    // 內建批次刪除 Modal 狀態
    private bool _showInternalBatchDeleteModal = false;
    private bool _isInternalBatchDeleting = false;

    // 右鍵選單刪除確認 Modal 狀態
    private bool _showContextMenuDeleteModal = false;
    private TEntity? _contextMenuDeleteTarget = null;

    // 操作欄刪除確認 Modal 狀態
    private bool _showActionDeleteModal = false;
    private TEntity? _actionDeleteTarget = null;

    // 多選狀態
    private HashSet<TEntity> _selectedItems = new();
    private bool _showMultiDeleteModal = false;

    // 右鍵列印預覽 Modal 狀態（由 RowPrintService 驅動）
    private bool _showRowPrintModal = false;
    private List<byte[]>? _rowPrintImages;
    private FormattedDocument? _rowPrintDocument;
    private string _rowPrintTitle = "";

    // 篩選 / 分頁 / 狀態
    private SearchFilterModel searchModel = new();
    private int currentPage = 1;
    private int pageSize = 20;
    private int totalItems = 0;
    private bool isLoading = true;
    private bool _isModuleEnabled = true;
    private bool _isSuperAdmin = false;

    // Debug 量測（僅 SuperAdmin 可見）
    private long   _debugQueryMs    = -1;   // 最後一次查詢耗時（ms）
    private int    _debugPageCount  = 0;    // 本頁載入筆數
    private int    _debugTotalCount = 0;    // 符合條件總筆數
    private string _debugMode       = "";   // "server" / "client"

    // 草稿 Tab 內部狀態
    private bool _showingDrafts = false;
    private int _draftCount = 0;

    // 手機版篩選 sidebar 狀態
    private bool _mobileFilterOpen = false;

    /// <summary>計算目前有幾個作用中的篩選條件（用於書籤把手 badge）</summary>
    private int _activeFilterCount =>
        searchModel.TextFilters.Values.Count(v => !string.IsNullOrEmpty(v)) +
        searchModel.NumberFilters.Values.Count(v => v.HasValue) +
        searchModel.NumberRangeFilters.Values.Count(v => v != null && (v.Min.HasValue || v.Max.HasValue)) +
        searchModel.DateFilters.Values.Count(v => v.HasValue) +
        searchModel.DateRangeFilters.Values.Count(v => v != null && (v.StartDate.HasValue || v.EndDate.HasValue)) +
        searchModel.DateTimeFilters.Values.Count(v => v.HasValue) +
        searchModel.DateTimeRangeFilters.Values.Count(v => v != null && (v.StartDateTime.HasValue || v.EndDateTime.HasValue)) +
        searchModel.SelectFilters.Values.Count(v => !string.IsNullOrEmpty(v)) +
        searchModel.MultiSelectFilters.Values.Count(v => v?.Any() == true) +
        searchModel.BooleanFilters.Values.Count(v => v == true);

    // ===== 公開屬性 =====

    public SearchFilterModel SearchModel => searchModel;
    public List<TEntity> PagedItems => pagedItems;
    public List<TEntity> FilteredItems => filteredItems;
    public int CurrentPage => currentPage;
    public int PageSize => pageSize;
    public int TotalItems => totalItems;
    public bool IsLoading => isLoading;

    // ===== 生命週期 =====

    protected override void OnParametersSet()
    {
        // 僅在真正影響 definitions 的參數變更時重建快取，避免每次 re-render 都重建 List
        bool needsRebuild =
            !ReferenceEquals(ColumnDefinitions,     _prevColumnDefs)           ||
            !ReferenceEquals(FilterDefinitions,     _prevFilterDefs)           ||
            !ReferenceEquals(ContextMenuItems,      _prevContextMenuItems)     ||
            !ReferenceEquals(RowPrintService,       _prevRowPrintService)      ||
            !ReferenceEquals(ActionsTemplate,       _prevActionsTemplate)      ||
            !ReferenceEquals(CustomActionsTemplate, _prevCustomActionsTemplate)||
            AutoAddRemarksColumn   != _prevAutoRemarksColumn                   ||
            AutoAddRemarksFilter   != _prevAutoRemarksFilter                   ||
            AutoAddCreatedAtColumn != _prevAutoCreatedAt                       ||
            ShowActions            != _prevShowActions                         ||
            EnableStandardActions  != _prevEnableStandardActions               ||
            ActionsHeader          != _prevActionsHeader                       ||
            RemarksColumnTitle     != _prevRemarksColumnTitle                  ||
            CreatedAtColumnTitle   != _prevCreatedAtColumnTitle                ||
            RemarksFilterTitle     != _prevRemarksFilterTitle;

        if (!needsRebuild) return;

        _prevColumnDefs            = ColumnDefinitions;
        _prevFilterDefs            = FilterDefinitions;
        _prevAutoRemarksColumn     = AutoAddRemarksColumn;
        _prevAutoRemarksFilter     = AutoAddRemarksFilter;
        _prevAutoCreatedAt         = AutoAddCreatedAtColumn;
        _prevShowActions           = ShowActions;
        _prevEnableStandardActions = EnableStandardActions;
        _prevActionsHeader         = ActionsHeader;
        _prevRemarksColumnTitle    = RemarksColumnTitle;
        _prevCreatedAtColumnTitle  = CreatedAtColumnTitle;
        _prevRemarksFilterTitle        = RemarksFilterTitle;
        _prevActionsTemplate           = ActionsTemplate;
        _prevCustomActionsTemplate     = CustomActionsTemplate;
        _prevContextMenuItems          = ContextMenuItems;
        _prevRowPrintService           = RowPrintService;
        RebuildDefinitionsCache();
    }

    protected override async Task OnInitializedAsync()
    {
        pageSize = DefaultPageSize > 0 ? DefaultPageSize : UserPreferenceContext.DefaultPageSize;

        // 一次性取得 SuperAdmin 狀態，避免重複查詢 DB
        bool isSuperAdmin = (!string.IsNullOrEmpty(DebugPageName) || !string.IsNullOrWhiteSpace(RequiredModule))
            ? await NavigationPermissionService.IsCurrentEmployeeSuperAdminAsync()
            : false;
        _isSuperAdmin = isSuperAdmin;

        // 公司層級模組檢查（阻擋直接輸入網址存取）
        // 只有 IsSuperAdmin=true 的帳號可繞過，System.Admin 權限持有者亦受模組限制
        if (!string.IsNullOrWhiteSpace(RequiredModule))
        {
            var isEnabled = await CompanyModuleService.IsModuleEnabledAsync(RequiredModule);
            _isModuleEnabled = isEnabled || isSuperAdmin;
        }

        if (_isModuleEnabled)
            await InitializePageAsync();
    }

    // ===== 快取管理 =====

    private void RebuildDefinitionsCache()
    {
        _cachedColumnDefinitions = BuildFinalColumnDefinitions();
        _cachedFilterDefinitions = BuildFinalFilterDefinitions();
        _cachedEffectiveContextMenuItems = BuildEffectiveContextMenuItems();
    }

    /// <summary>
    /// 建立有效的右鍵選單項目。
    /// - 若外部明確傳入 ContextMenuItems（含空列表），直接使用（空列表 = 停用選單）。
    /// - 否則自動從 OnRowClick / ShowDeleteButton 生成預設選單，所有 Index 頁面無需修改。
    /// </summary>
    private List<ContextMenuItem<TEntity>>? BuildEffectiveContextMenuItems()
    {
        // 外部明確設定 → 尊重呼叫端意圖（空列表代表不要選單）
        if (ContextMenuItems != null)
            return ContextMenuItems.Count > 0 ? ContextMenuItems : null;

        // 自動生成
        var items = new List<ContextMenuItem<TEntity>>();

        if (EnableRowClick && OnRowClick.HasDelegate)
        {
            items.Add(new()
            {
                Label     = L["Button.Edit"].ToString(),
                IconClass = "fas fa-edit",
                OnClick   = async entity => await HandleRowClick(entity)
            });
        }

        if (RowPrintService != null || OnRowPrint.HasDelegate)
        {
            items.Add(new()
            {
                Label     = L["Button.Print"].ToString(),
                IconClass = "fas fa-print",
                OnClick   = async entity => await HandleContextMenuPrintAsync(entity)
            });
        }

        if (ShowDeleteButton)
        {
            if (items.Any())
                items.Add(new() { IsDivider = true });

            items.Add(new()
            {
                Label      = L["Button.Delete"].ToString(),
                IconClass  = "fas fa-trash",
                CssClass   = "text-danger",
                IsDisabled = entity => !IsEntityDeletable(entity),
                OnClick    = entity =>
                {
                    _contextMenuDeleteTarget = entity;
                    _showContextMenuDeleteModal = true;
                    StateHasChanged();
                    return Task.CompletedTask;
                }
            });
        }

        return items.Any() ? items : null;
    }

    private async Task HandleContextMenuPrintAsync(TEntity entity)
    {
        // 優先使用內建服務；若未設定則委派給 OnRowPrint callback
        if (RowPrintService != null)
        {
            try
            {
                _rowPrintTitle   = GetEntityDisplayName(entity);
                _rowPrintImages  = await RowPrintService.RenderToImagesAsync(entity.Id);
                _rowPrintDocument = await RowPrintService.GenerateReportAsync(entity.Id);
                _showRowPrintModal = true;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                await NotificationService.ShowErrorAsync($"列印預覽失敗：{ex.Message}");
            }
        }
        else if (OnRowPrint.HasDelegate)
        {
            await OnRowPrint.InvokeAsync(entity);
        }
    }

    private Task HandleSelectionChanged(HashSet<TEntity> selected)
    {
        _selectedItems = selected;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private List<TableColumnDefinition> BuildFinalColumnDefinitions()
    {
        var columns = new List<TableColumnDefinition>(ColumnDefinitions);

        if (AutoAddRemarksColumn && !columns.Any(c => c.PropertyName == "Remarks"))
        {
            var col = TableColumnDefinition.Text(
                string.IsNullOrEmpty(RemarksColumnTitle) ? L["Label.Remarks"] : RemarksColumnTitle,
                nameof(BaseEntity.Remarks));
            col.HeaderStyle  = "width: 150px;";
            col.CellCssClass = "text-truncate";
            columns.Add(col);
        }

        if (AutoAddCreatedAtColumn && !columns.Any(c => c.PropertyName == "CreatedAt"))
        {
            var col = TableColumnDefinition.Date(
                string.IsNullOrEmpty(CreatedAtColumnTitle) ? L["Label.CreatedAt"] : CreatedAtColumnTitle,
                nameof(BaseEntity.CreatedAt), "yyyy/MM/dd");
            col.HeaderStyle  = "width: 100px; text-align: center;";
            col.CellCssClass = "text-center";
            columns.Add(col);
        }

        if (ShowActions && EnableStandardActions && !columns.Any(c => c.PropertyName == "Actions"))
        {
            var actionsTemplate = GetFinalActionsTemplate();
            columns.Add(new TableColumnDefinition
            {
                Title          = string.IsNullOrEmpty(ActionsHeader) ? L["Label.Actions"] : ActionsHeader,
                PropertyName   = "Actions",
                DataType       = ColumnDataType.Html,
                HeaderCssClass = "text-center",
                CellCssClass   = "text-center",
                HeaderStyle    = "width: 60px;",
                CustomTemplate = item => actionsTemplate((TEntity)item)
            });
        }

        return columns;
    }

    private List<SearchFilterDefinition> BuildFinalFilterDefinitions()
    {
        var filters = new List<SearchFilterDefinition>(FilterDefinitions);

        if (AutoAddRemarksFilter && !filters.Any(f => f.PropertyName == "Remarks"))
        {
            filters.Add(new SearchFilterDefinition
            {
                Name        = "Remarks",
                Label       = string.IsNullOrEmpty(RemarksFilterTitle) ? L["Label.Remarks"] : RemarksFilterTitle,
                Type        = SearchFilterType.Text,
                Placeholder = L["Placeholder.Remarks"]
            });
        }

        return filters;
    }

    // ===== 智能文字產生 =====

    private string GetAddButtonText()      => !string.IsNullOrWhiteSpace(AddButtonText)      ? AddButtonText      : string.Format(L["Button.AddEntity"],     EntityName);
    private string GetAddButtonTitle()     => !string.IsNullOrWhiteSpace(AddButtonTitle)     ? AddButtonTitle     : string.Format(L["Button.AddEntityTitle"], EntityName);
    private string GetSearchSectionTitle() => !string.IsNullOrWhiteSpace(SearchSectionTitle) ? SearchSectionTitle : string.Format(L["Label.SearchAndManage"], EntityName);
    private string GetEmptyMessage()       => !string.IsNullOrWhiteSpace(EmptyMessage)       ? EmptyMessage       : string.Format(L["Message.EntityNoResults"], EntityName);

    // ===== Debug Badge =====

    private async Task CopyDebugPageNameAsync()
    {
        if (!string.IsNullOrEmpty(DebugPageName))
            await JSRuntime.InvokeVoidAsync("DebugHelper.copyText", DebugPageName);
    }
}
