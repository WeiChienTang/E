# 批次列印設計規範

> **版本**：1.0  
> **建立日期**：2025/10/20  
> **適用範圍**：所有單據類型的批次列印功能

## 📋 目錄

- [概述](#概述)
- [架構設計](#架構設計)
- [完整實作步驟](#完整實作步驟)
- [程式碼範本](#程式碼範本)
- [測試檢查清單](#測試檢查清單)
- [常見問題](#常見問題)

---

## 概述

### 設計目標

批次列印功能允許使用者根據多種條件（日期範圍、關聯實體、狀態等）篩選多筆單據，並一次性生成可列印的報表。

### 核心特性

- ✅ **通用條件模型**：`BatchPrintCriteria` 適用於所有單據類型
- ✅ **彈性篩選**：支援日期範圍、多選實體、狀態等多重條件組合
- ✅ **智能分頁**：每個單據自動分頁，使用現有的單筆報表邏輯
- ✅ **效能保護**：內建最大筆數限制、日期範圍驗證
- ✅ **友善體驗**：篩選條件摘要、空結果提示、自動列印

### 技術堆疊

```
前端 (Blazor)
    ↓ JavaScript (batch-print-helpers.js)
    ↓ HTTP POST
API Controller (ReportController)
    ↓
Report Service (生成 HTML)
    ↓
Entity Service (查詢資料)
    ↓
Database (EF Core)
```

---

## 架構設計

### 1. 通用批次列印條件模型

**檔案位置**：`Models/BatchPrintCriteria.cs`

```
BatchPrintCriteria
├── StartDate             // 開始日期
├── EndDate              // 結束日期
├── RelatedEntityIds     // 關聯實體ID列表（廠商/客戶等）
├── Statuses             // 狀態篩選列表
├── CompanyId            // 公司ID
├── WarehouseId          // 倉庫ID
├── DocumentNumberKeyword // 單據編號關鍵字
├── CustomFilters        // 自訂篩選（Dictionary）
├── ReportType           // 報表類型代碼
├── PrintConfigurationId // 列印配置ID
├── SortBy               // 排序欄位
├── SortDirection        // 排序方向
├── MaxResults           // 最大筆數限制
├── IncludeCancelled     // 是否包含已取消單據
├── Validate()           // 驗證方法
└── GetSummary()         // 取得篩選摘要
```

**關鍵設計原則**：

1. **語意清晰**：空列表表示「全部」，有值表示「僅包含」
2. **內建驗證**：自動檢查日期範圍、最大筆數等
3. **彈性擴充**：CustomFilters 支援未來擴充

---

## 完整實作步驟

### 階段一：Service 層擴充

#### 步驟 1.1：擴充 Entity Service 介面

**檔案**：`Services/{模組}/{單據}Service/I{單據}Service.cs`

```csharp
using ERPCore2.Models;

public interface IPurchaseOrderService : IGenericManagementService<PurchaseOrder>
{
    // ... 現有方法 ...
    
    /// <summary>
    /// 根據批次列印條件查詢單據（批次列印專用）
    /// </summary>
    /// <param name="criteria">批次列印篩選條件</param>
    /// <returns>符合條件的單據列表</returns>
    Task<List<PurchaseOrder>> GetByBatchCriteriaAsync(BatchPrintCriteria criteria);
}
```

#### 步驟 1.2：實作批次查詢邏輯

**檔案**：`Services/{模組}/{單據}Service/{單據}Service.cs`

```csharp
public async Task<List<PurchaseOrder>> GetByBatchCriteriaAsync(BatchPrintCriteria criteria)
{
    try
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        // 🔹 建立基礎查詢（包含必要關聯資料）
        IQueryable<PurchaseOrder> query = context.PurchaseOrders
            .Include(po => po.Company)
            .Include(po => po.Supplier)
            .Include(po => po.Warehouse)
            .Include(po => po.ApprovedByUser)
            .AsQueryable();

        // 🔹 日期範圍篩選
        if (criteria.StartDate.HasValue)
        {
            query = query.Where(po => po.OrderDate >= criteria.StartDate.Value.Date);
        }
        if (criteria.EndDate.HasValue)
        {
            var endDate = criteria.EndDate.Value.Date.AddDays(1);
            query = query.Where(po => po.OrderDate < endDate);
        }

        // 🔹 關聯實體篩選（廠商/客戶等）
        if (criteria.RelatedEntityIds != null && criteria.RelatedEntityIds.Any())
        {
            query = query.Where(po => criteria.RelatedEntityIds.Contains(po.SupplierId));
        }

        // 🔹 公司篩選
        if (criteria.CompanyId.HasValue)
        {
            query = query.Where(po => po.CompanyId == criteria.CompanyId.Value);
        }

        // 🔹 倉庫篩選
        if (criteria.WarehouseId.HasValue)
        {
            query = query.Where(po => po.WarehouseId == criteria.WarehouseId.Value);
        }

        // 🔹 狀態篩選（根據實體的狀態欄位調整）
        if (criteria.Statuses != null && criteria.Statuses.Any())
        {
            // 範例：採購單使用布林值表示狀態
            foreach (var status in criteria.Statuses)
            {
                switch (status.ToLower())
                {
                    case "pending":
                    case "待審核":
                        query = query.Where(po => !po.IsApproved && string.IsNullOrEmpty(po.RejectReason));
                        break;
                    case "approved":
                    case "已審核":
                        query = query.Where(po => po.IsApproved);
                        break;
                    case "rejected":
                    case "已駁回":
                        query = query.Where(po => !string.IsNullOrEmpty(po.RejectReason));
                        break;
                }
            }
        }

        // 🔹 單據編號關鍵字搜尋
        if (!string.IsNullOrWhiteSpace(criteria.DocumentNumberKeyword))
        {
            query = query.Where(po => po.PurchaseOrderNumber.Contains(criteria.DocumentNumberKeyword));
        }

        // 🔹 是否包含已取消的單據
        if (!criteria.IncludeCancelled)
        {
            query = query.Where(po => string.IsNullOrEmpty(po.RejectReason));
        }

        // 🔹 排序
        query = criteria.SortDirection == SortDirection.Ascending
            ? query.OrderBy(po => po.OrderDate).ThenBy(po => po.PurchaseOrderNumber)
            : query.OrderByDescending(po => po.OrderDate).ThenBy(po => po.PurchaseOrderNumber);

        // 🔹 限制最大筆數
        if (criteria.MaxResults.HasValue && criteria.MaxResults.Value > 0)
        {
            query = query.Take(criteria.MaxResults.Value);
        }

        // 🔹 執行查詢
        return await query.ToListAsync();
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByBatchCriteriaAsync), GetType(), _logger, new
        {
            Method = nameof(GetByBatchCriteriaAsync),
            ServiceType = GetType().Name,
            Criteria = new
            {
                criteria.StartDate,
                criteria.EndDate,
                RelatedEntityCount = criteria.RelatedEntityIds?.Count ?? 0,
                criteria.MaxResults
            }
        });
        return new List<PurchaseOrder>();
    }
}
```

**⚠️ 注意事項**：

1. **狀態篩選**：根據實體的實際狀態欄位調整（布林值/列舉）
2. **關聯資料**：確保 Include 所有報表需要的導覽屬性
3. **效能優化**：使用 AsQueryable() 延遲執行，最後才 ToListAsync()

---

### 階段二：Report Service 層擴充

#### 步驟 2.1：擴充 Report Service 介面

**檔案**：`Services/Reports/IReportService.cs`

```csharp
public interface IPurchaseOrderReportService
{
    // ... 現有方法 ...
    
    /// <summary>
    /// 批次生成報表（支援多條件篩選）
    /// </summary>
    /// <param name="criteria">批次列印篩選條件</param>
    /// <param name="format">輸出格式（預設 HTML）</param>
    /// <param name="reportPrintConfig">報表列印配置（可選）</param>
    /// <returns>合併後的報表內容（所有單據在同一個 HTML，自動分頁）</returns>
    Task<string> GenerateBatchReportAsync(
        BatchPrintCriteria criteria,
        ReportFormat format = ReportFormat.Html,
        ReportPrintConfiguration? reportPrintConfig = null);
}
```

#### 步驟 2.2：實作批次報表生成

**檔案**：`Services/Reports/{單據}ReportService.cs`

```csharp
public async Task<string> GenerateBatchReportAsync(
    BatchPrintCriteria criteria,
    ReportFormat format = ReportFormat.Html,
    ReportPrintConfiguration? reportPrintConfig = null)
{
    try
    {
        // 🔹 驗證篩選條件
        var validation = criteria.Validate();
        if (!validation.IsValid)
        {
            throw new ArgumentException($"批次列印條件驗證失敗：{validation.GetAllErrors()}");
        }

        // 🔹 根據條件查詢單據
        var entities = await _entityService.GetByBatchCriteriaAsync(criteria);

        if (entities == null || !entities.Any())
        {
            // 返回空結果提示頁面
            return GenerateEmptyResultPage(criteria);
        }

        // 🔹 根據格式生成報表
        return format switch
        {
            ReportFormat.Html => await GenerateBatchHtmlReportAsync(entities, reportPrintConfig, criteria),
            ReportFormat.Excel => throw new NotImplementedException("Excel 格式尚未實作"),
            _ => throw new ArgumentException($"不支援的報表格式: {format}")
        };
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException($"生成批次報表時發生錯誤: {ex.Message}", ex);
    }
}

private async Task<string> GenerateBatchHtmlReportAsync(
    List<PurchaseOrder> entities,
    ReportPrintConfiguration? reportPrintConfig,
    BatchPrintCriteria criteria)
{
    var html = new StringBuilder();

    // 🔹 HTML 文件開始
    html.AppendLine("<!DOCTYPE html>");
    html.AppendLine("<html lang='zh-TW'>");
    html.AppendLine("<head>");
    html.AppendLine("    <meta charset='UTF-8'>");
    html.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
    html.AppendLine($"    <title>批次列印 ({entities.Count} 筆)</title>");
    html.AppendLine("    <link href='/css/print-styles.css' rel='stylesheet' />");
    html.AppendLine("</head>");
    html.AppendLine("<body>");

    // 🔹 批次列印資訊頁（可選）
    html.AppendLine(GenerateBatchPrintInfoPage(entities, criteria));

    // 🔹 載入共用資料（避免重複查詢）
    var allProducts = await _productService.GetAllAsync();
    var productDict = allProducts.ToDictionary(p => p.Id, p => p);
    
    decimal taxRate = 5.0m;
    try
    {
        taxRate = await _systemParameterService.GetTaxRateAsync();
    }
    catch { }

    // 🔹 逐一生成每張單據報表
    for (int i = 0; i < entities.Count; i++)
    {
        var entity = entities[i];

        // 載入該單據的詳細資料
        var details = await _entityService.GetOrderDetailsAsync(entity.Id);
        var relatedEntity = await _relatedService.GetByIdAsync(entity.RelatedEntityId);
        var company = await _companyService.GetByIdAsync(entity.CompanyId);

        // 🔹 重複使用現有的單筆報表生成邏輯
        GenerateSingleReportInBatch(html, entity, details, relatedEntity, company, 
            productDict, taxRate, i + 1, entities.Count);
    }

    // 🔹 列印腳本
    html.AppendLine(GetPrintScript());

    html.AppendLine("</body>");
    html.AppendLine("</html>");

    return html.ToString();
}

private void GenerateSingleReportInBatch(
    StringBuilder html,
    PurchaseOrder entity,
    List<PurchaseOrderDetail> details,
    Supplier? relatedEntity,
    Company? company,
    Dictionary<int, Product> productDict,
    decimal taxRate,
    int currentDoc,
    int totalDocs)
{
    // 🔹 使用現有的通用分頁計算器
    var layout = ReportPageLayout.ContinuousForm();
    var paginator = new ReportPaginator<PurchaseOrderDetailWrapper>(layout);
    
    var wrappedDetails = details
        .Select(d => new PurchaseOrderDetailWrapper(d))
        .ToList();
    
    var pages = paginator.SplitIntoPages(wrappedDetails);

    // 🔹 生成每一頁
    int startRowNum = 0;
    for (int pageNum = 0; pageNum < pages.Count; pageNum++)
    {
        var page = pages[pageNum];
        var pageDetails = page.Items.Select(w => w.Detail).ToList();

        // 重複使用現有的 GeneratePage 方法
        GeneratePage(html, entity, pageDetails, relatedEntity, company, 
            productDict, taxRate, pageNum + 1, pages.Count, page.IsLastPage, startRowNum);
        
        startRowNum += pageDetails.Count;
    }

    // 🔹 每張單據之間加入分頁（除了最後一張）
    if (currentDoc < totalDocs)
    {
        html.AppendLine("    <div style='page-break-after: always;'></div>");
    }
}

private string GenerateBatchPrintInfoPage(List<PurchaseOrder> entities, BatchPrintCriteria criteria)
{
    var html = new StringBuilder();
    
    html.AppendLine("    <div class='batch-print-info-page' style='display: none;'>");
    html.AppendLine("        <div class='info-header'>");
    html.AppendLine("            <h2>批次列印資訊</h2>");
    html.AppendLine($"            <p>列印時間：{DateTime.Now:yyyy/MM/dd HH:mm:ss}</p>");
    html.AppendLine($"            <p>共 {entities.Count} 筆單據</p>");
    html.AppendLine("        </div>");
    html.AppendLine("        <div class='info-criteria'>");
    html.AppendLine($"            <p>篩選條件：{criteria.GetSummary()}</p>");
    html.AppendLine("        </div>");
    html.AppendLine("        <div class='info-list'>");
    html.AppendLine("            <h3>單據清單</h3>");
    html.AppendLine("            <ol>");
    
    foreach (var entity in entities)
    {
        html.AppendLine($"                <li>{entity.DocumentNumber} - {entity.RelatedEntity?.Name ?? "未指定"} - {entity.Date:yyyy/MM/dd}</li>");
    }
    
    html.AppendLine("            </ol>");
    html.AppendLine("        </div>");
    html.AppendLine("    </div>");
    
    return html.ToString();
}

private string GenerateEmptyResultPage(BatchPrintCriteria criteria)
{
    return $@"
<!DOCTYPE html>
<html lang='zh-TW'>
<head>
    <meta charset='UTF-8'>
    <title>批次列印 - 無符合條件的資料</title>
    <link href='/css/print-styles.css' rel='stylesheet' />
</head>
<body>
    <div style='text-align: center; padding: 50px;'>
        <h1>無符合條件的單據</h1>
        <p>篩選條件：{criteria.GetSummary()}</p>
        <p>請調整篩選條件後重新查詢。</p>
    </div>
</body>
</html>";
}
```

**⚠️ 關鍵設計原則**：

1. **重複使用現有邏輯**：不重新發明輪子，使用現有的單筆報表方法
2. **資料預載**：共用資料（如商品字典）只載入一次
3. **自動分頁**：使用 `page-break-after: always` 確保每個單據獨立分頁

---

### 階段三：API Controller 擴充

#### 步驟 3.1：加入批次列印端點

**檔案**：`Controllers/ReportController.cs`

```csharp
/// <summary>
/// 批次生成報表
/// </summary>
/// <param name="criteria">批次列印篩選條件</param>
/// <returns>合併後的報表 HTML</returns>
[HttpPost("purchase-order/batch")]
public async Task<IActionResult> BatchPrintPurchaseOrders(
    [FromBody] BatchPrintCriteria criteria)
{
    try
    {
        _logger.LogInformation("開始批次生成報表 - 條件: {Criteria}", criteria.GetSummary());

        // 載入列印配置（可選）
        ReportPrintConfiguration? printConfig = null;
        if (!string.IsNullOrEmpty(criteria.ReportType))
        {
            printConfig = await _reportPrintConfigurationService
                .GetCompleteConfigurationAsync(criteria.ReportType);
        }

        // 生成報表
        var reportHtml = await _purchaseOrderReportService.GenerateBatchReportAsync(
            criteria,
            ReportFormat.Html,
            printConfig);

        return Content(reportHtml, "text/html; charset=utf-8");
    }
    catch (ArgumentException ex)
    {
        _logger.LogWarning(ex, "批次列印條件驗證失敗");
        return BadRequest(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "批次生成報表時發生錯誤");
        return StatusCode(500, new { message = "生成報表時發生錯誤", detail = ex.Message });
    }
}

/// <summary>
/// 批次生成報表並自動列印
/// </summary>
/// <param name="criteria">批次列印篩選條件</param>
/// <returns>可列印的報表 HTML</returns>
[HttpPost("purchase-order/batch/print")]
public async Task<IActionResult> BatchPrintPurchaseOrdersWithAutoPrint(
    [FromBody] BatchPrintCriteria criteria)
{
    try
    {
        _logger.LogInformation("開始批次列印 - 條件: {Criteria}", criteria.GetSummary());

        // 載入列印配置
        ReportPrintConfiguration? printConfig = null;
        if (!string.IsNullOrEmpty(criteria.ReportType))
        {
            printConfig = await _reportPrintConfigurationService
                .GetCompleteConfigurationAsync(criteria.ReportType);
        }

        // 生成報表
        var reportHtml = await _purchaseOrderReportService.GenerateBatchReportAsync(
            criteria,
            ReportFormat.Html,
            printConfig);

        return Content(reportHtml, "text/html; charset=utf-8");
    }
    catch (ArgumentException ex)
    {
        _logger.LogWarning(ex, "批次列印條件驗證失敗");
        return BadRequest(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "批次列印時發生錯誤");
        return StatusCode(500, new { message = "批次列印時發生錯誤", detail = ex.Message });
    }
}
```

---

### 階段四：前端整合

#### 步驟 4.1：Index 頁面整合

**檔案**：`Components/Pages/{模組}/{單據}Index.razor`

```razor
@page "/{path}/orders"
@using ERPCore2.Components.Shared.Report
@using ERPCore2.Models
@inject I{單據}Service EntityService
@inject IRelatedService RelatedService
@inject INotificationService NotificationService
@inject IJSRuntime JSRuntime
@rendermode InteractiveServer

<GenericIndexPageComponent TEntity="{單據}" 
                          TService="I{單據}Service"
                          Service="@EntityService"
                          EntityBasePath="/{path}/orders"
                          ShowBatchPrintButton="true"
                          OnBatchPrintClick="@HandleBatchPrintAsync"
                          @ref="indexComponent" />

@* 批次列印篩選 Modal *@
<BatchPrintFilterModalComponent IsVisible="@showBatchPrintModal"
                               IsVisibleChanged="@((bool visible) => showBatchPrintModal = visible)"
                               Title="{單據}批次列印條件"
                               OnConfirm="@HandleBatchPrintConfirmAsync"
                               OnCancel="@HandleBatchPrintCancelAsync">
    
    @* 🔹 區塊 1: 關聯實體多選篩選（廠商/客戶等） *@
    <FilterSectionComponent Title="關聯實體篩選 (可多選)" 
                           Badge="@($"{selectedEntities.Count} / {entities.Count}")">
        <MultiSelectFilterComponent TItem="RelatedEntity"
                                   Items="@entities"
                                   @bind-SelectedItems="@selectedEntities"
                                   DisplayProperty="Name"
                                   ValueProperty="Id"
                                   Placeholder="請輸入名稱搜尋..."
                                   EmptyMessage="尚未選擇，留空表示列印所有"
                                   ShowCard="false" />
    </FilterSectionComponent>
    
    @* 🔹 區塊 2: 日期範圍 *@
    <FilterSectionComponent Title="單據日期" 
                           IconClass="bi bi-calendar-range">
        <DateRangeFilterComponent @bind-StartDate="@batchPrintStartDate"
                                 @bind-EndDate="@batchPrintEndDate"
                                 StartDateLabel="起始日期"
                                 EndDateLabel="結束日期"
                                 ShowQuickSelectors="true"
                                 AutoValidate="true"
                                 ShowValidationMessage="true" />
    </FilterSectionComponent>
    
    @* 🔹 區塊 3: 其他篩選條件（可選） *@
    @* 如需要狀態篩選、倉庫篩選等，在此加入 *@
    
</BatchPrintFilterModalComponent>

@code {
    // 組件參考
    private GenericIndexPageComponent<{單據}, I{單據}Service> indexComponent = default!;
    
    // 選項清單
    private List<RelatedEntity> entities = new();
    
    // 批次列印 Modal 相關狀態
    private bool showBatchPrintModal = false;
    private List<RelatedEntity> selectedEntities = new();
    private DateTime? batchPrintStartDate = null;
    private DateTime? batchPrintEndDate = null;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            // 載入關聯實體資料
            await LoadRelatedEntitiesAsync();
        }
        catch (Exception ex)
        {
            await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(OnInitializedAsync), GetType());
            await NotificationService.ShowErrorAsync("初始化頁面失敗");
        }
    }

    private async Task LoadRelatedEntitiesAsync()
    {
        try
        {
            entities = await RelatedService.GetAllAsync();
        }
        catch (Exception ex)
        {
            await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(LoadRelatedEntitiesAsync), GetType());
            await NotificationService.ShowErrorAsync("載入資料失敗");
            entities = new List<RelatedEntity>();
        }
    }

    // 🔹 開啟批次列印 Modal
    private async Task HandleBatchPrintAsync()
    {
        try
        {
            showBatchPrintModal = true;
        }
        catch (Exception ex)
        {
            await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(HandleBatchPrintAsync), GetType());
            await NotificationService.ShowErrorAsync("開啟多筆列印視窗失敗");
        }
    }

    // 🔹 處理批次列印確認
    private async Task HandleBatchPrintConfirmAsync()
    {
        try
        {
            // 組裝批次列印條件
            var criteria = new BatchPrintCriteria
            {
                StartDate = batchPrintStartDate,
                EndDate = batchPrintEndDate,
                RelatedEntityIds = selectedEntities.Select(e => e.Id).ToList(),
                ReportType = "{單據類型代碼}", // 例如：PurchaseOrder
                MaxResults = 100,
                IncludeCancelled = false
            };

            // 驗證篩選條件
            var validation = criteria.Validate();
            if (!validation.IsValid)
            {
                await NotificationService.ShowErrorAsync($"篩選條件錯誤：{validation.GetAllErrors()}");
                return;
            }

            // 序列化條件為 JSON
            var jsonPayload = System.Text.Json.JsonSerializer.Serialize(criteria);

            // 開啟新視窗顯示批次報表
            var apiUrl = "/api/report/{path}/batch?autoprint=true";
            await JSRuntime.InvokeVoidAsync("openBatchPrintWindow", apiUrl, jsonPayload);

            // 顯示成功訊息
            await NotificationService.ShowSuccessAsync($"已開啟批次列印視窗 ({criteria.GetSummary()})");
            
            // 關閉 Modal
            showBatchPrintModal = false;
            
            // 清空選擇
            selectedEntities.Clear();
            batchPrintStartDate = null;
            batchPrintEndDate = null;
        }
        catch (Exception ex)
        {
            await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(HandleBatchPrintConfirmAsync), GetType());
            await NotificationService.ShowErrorAsync("執行批次列印失敗");
        }
    }

    // 🔹 處理批次列印取消
    private async Task HandleBatchPrintCancelAsync()
    {
        try
        {
            showBatchPrintModal = false;
            
            // 清空所有篩選條件
            selectedEntities.Clear();
            batchPrintStartDate = null;
            batchPrintEndDate = null;
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(HandleBatchPrintCancelAsync), GetType());
        }
    }
}
```

**⚠️ 替換內容**：

- `{單據}`：例如 `PurchaseOrder`、`PurchaseReceiving`
- `{path}`：例如 `purchase`、`sales`
- `{單據類型代碼}`：例如 `"PurchaseOrder"`
- `RelatedEntity`：例如 `Supplier`、`Customer`

---

### 階段五：JavaScript 支援（已完成，共用）

**檔案**：`wwwroot/js/batch-print-helpers.js`（已存在，無需重複建立）

**檔案**：`Components/App.razor`（已引入，無需重複操作）

```html
<script src="@Assets["js/batch-print-helpers.js"]"></script>
```

---

## 程式碼範本

### 快速複製範本

#### 1. Service 介面範本

```csharp
// Services/{模組}/I{單據}Service.cs
using ERPCore2.Models;

Task<List<{單據}>> GetByBatchCriteriaAsync(BatchPrintCriteria criteria);
```

#### 2. Service 實作範本

```csharp
// Services/{模組}/{單據}Service.cs
public async Task<List<{單據}>> GetByBatchCriteriaAsync(BatchPrintCriteria criteria)
{
    try
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        IQueryable<{單據}> query = context.{單據複數}
            .Include(e => e.{關聯1})
            .Include(e => e.{關聯2})
            .AsQueryable();

        // 日期篩選
        if (criteria.StartDate.HasValue)
            query = query.Where(e => e.{日期欄位} >= criteria.StartDate.Value.Date);
        if (criteria.EndDate.HasValue)
            query = query.Where(e => e.{日期欄位} < criteria.EndDate.Value.Date.AddDays(1));

        // 關聯實體篩選
        if (criteria.RelatedEntityIds?.Any() == true)
            query = query.Where(e => criteria.RelatedEntityIds.Contains(e.{關聯ID欄位}));

        // 公司篩選
        if (criteria.CompanyId.HasValue)
            query = query.Where(e => e.CompanyId == criteria.CompanyId.Value);

        // 排序
        query = criteria.SortDirection == SortDirection.Ascending
            ? query.OrderBy(e => e.{日期欄位}).ThenBy(e => e.{單據編號欄位})
            : query.OrderByDescending(e => e.{日期欄位}).ThenBy(e => e.{單據編號欄位});

        // 限制筆數
        if (criteria.MaxResults.HasValue && criteria.MaxResults.Value > 0)
            query = query.Take(criteria.MaxResults.Value);

        return await query.ToListAsync();
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByBatchCriteriaAsync), GetType(), _logger);
        return new List<{單據}>();
    }
}
```

#### 3. Report Service 介面範本

```csharp
// Services/Reports/I{單據}ReportService.cs
Task<string> GenerateBatchReportAsync(
    BatchPrintCriteria criteria,
    ReportFormat format = ReportFormat.Html,
    ReportPrintConfiguration? reportPrintConfig = null);
```

#### 4. Report Service 實作範本

```csharp
// Services/Reports/{單據}ReportService.cs
public async Task<string> GenerateBatchReportAsync(
    BatchPrintCriteria criteria,
    ReportFormat format = ReportFormat.Html,
    ReportPrintConfiguration? reportPrintConfig = null)
{
    var validation = criteria.Validate();
    if (!validation.IsValid)
        throw new ArgumentException($"批次列印條件驗證失敗：{validation.GetAllErrors()}");

    var entities = await _{單據}Service.GetByBatchCriteriaAsync(criteria);

    if (!entities?.Any() == true)
        return GenerateEmptyResultPage(criteria);

    return format switch
    {
        ReportFormat.Html => await GenerateBatchHtmlReportAsync(entities, reportPrintConfig, criteria),
        _ => throw new ArgumentException($"不支援的報表格式: {format}")
    };
}
```

#### 5. Controller 端點範本

```csharp
// Controllers/ReportController.cs
[HttpPost("{path}/batch")]
public async Task<IActionResult> BatchPrint{單據複數}(
    [FromBody] BatchPrintCriteria criteria)
{
    try
    {
        var reportHtml = await _{單據}ReportService.GenerateBatchReportAsync(criteria);
        return Content(reportHtml, "text/html; charset=utf-8");
    }
    catch (ArgumentException ex)
    {
        return BadRequest(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "批次生成報表時發生錯誤");
        return StatusCode(500, new { message = "生成報表時發生錯誤" });
    }
}
```

## 常見問題

### Q1：如何處理不同的狀態欄位？

**A**：根據實體的狀態欄位類型調整：

```csharp
// 布林值狀態（如 IsApproved）
if (criteria.Statuses?.Any() == true)
{
    foreach (var status in criteria.Statuses)
    {
        switch (status.ToLower())
        {
            case "approved":
                query = query.Where(e => e.IsApproved);
                break;
            case "pending":
                query = query.Where(e => !e.IsApproved);
                break;
        }
    }
}

// 列舉狀態（如 Status enum）
if (criteria.Statuses?.Any() == true)
{
    var statusEnums = criteria.Statuses
        .Select(s => Enum.TryParse<{單據}Status>(s, out var status) ? status : ({單據}Status?)null)
        .Where(s => s.HasValue)
        .Select(s => s!.Value)
        .ToList();

    if (statusEnums.Any())
        query = query.Where(e => statusEnums.Contains(e.Status));
}
```

### Q2：如何自訂篩選條件？

**A**：使用 `CustomFilters` 字典：

```csharp
// 前端組裝
var criteria = new BatchPrintCriteria
{
    CustomFilters = new Dictionary<string, object>
    {
        { "MinAmount", 1000 },
        { "MaxAmount", 50000 },
        { "IsUrgent", true }
    }
};

// Service 層解析
if (criteria.CustomFilters.TryGetValue("MinAmount", out var minAmountObj) 
    && minAmountObj is decimal minAmount)
{
    query = query.Where(e => e.TotalAmount >= minAmount);
}
```

### Q3：如何限制列印筆數以避免效能問題？

**A**：已內建在 `BatchPrintCriteria.Validate()` 中：

```csharp
// 預設限制
MaxResults = 100 // 前端設定

// 驗證會檢查
if (MaxResults > 1000)
    errors.Add("最大筆數不能超過1000筆");
```

### Q4：如何支援不同的報表格式（PDF、Excel）？

**A**：擴充 `GenerateBatchReportAsync` 的 format 參數：

```csharp
return format switch
{
    ReportFormat.Html => await GenerateBatchHtmlReportAsync(...),
    ReportFormat.Pdf => await GenerateBatchPdfReportAsync(...),
    ReportFormat.Excel => await GenerateBatchExcelReportAsync(...),
    _ => throw new ArgumentException($"不支援的報表格式: {format}")
};
```

### Q5：如何處理報表中的圖片或特殊格式？

**A**：在 `PurchaseOrderDetailWrapper` 類別中加入額外高度因素：

```csharp
public decimal GetExtraHeightFactor()
{
    decimal factor = 0m;
    
    // 如果有圖片，增加高度
    if (!string.IsNullOrEmpty(Detail.ImageUrl))
        factor += 1.5m;
    
    // 如果備註很長，增加高度
    if (Detail.Remarks?.Length > 100)
        factor += 0.5m;
    
    return factor;
}
```

### Q6：如何在批次列印中顯示進度？

**A**：使用 SignalR 或輪詢方式：

```csharp
// 後端（使用 IProgress<T>）
public async Task<string> GenerateBatchReportAsync(
    BatchPrintCriteria criteria,
    IProgress<int> progress = null)
{
    var entities = await GetEntitiesAsync(criteria);
    
    for (int i = 0; i < entities.Count; i++)
    {
        // 生成報表...
        progress?.Report((i + 1) * 100 / entities.Count);
    }
}

// 前端（顯示進度條）
<div class="progress" *ngIf="isGenerating">
    <div class="progress-bar" [style.width]="progress + '%'">
        {{progress}}%
    </div>
</div>
```

---

## 附錄：完整檔案清單

### 新增/修改的檔案

| 檔案路徑 | 說明 | 狀態 |
|---------|------|------|
| `Models/BatchPrintCriteria.cs` | 通用批次列印條件模型 | ✅ 新增 |
| `Services/{模組}/I{單據}Service.cs` | Service 介面擴充 | 🔧 修改 |
| `Services/{模組}/{單據}Service.cs` | Service 實作擴充 | 🔧 修改 |
| `Services/Reports/I{單據}ReportService.cs` | Report Service 介面擴充 | 🔧 修改 |
| `Services/Reports/{單據}ReportService.cs` | Report Service 實作擴充 | 🔧 修改 |
| `Controllers/ReportController.cs` | API 端點擴充 | 🔧 修改 |
| `Components/Pages/{模組}/{單據}Index.razor` | Index 頁面整合 | 🔧 修改 |
| `wwwroot/js/batch-print-helpers.js` | JavaScript 輔助函數 | ✅ 新增（共用） |
| `Components/App.razor` | 引入 JavaScript | 🔧 修改（一次性） |

### 重複使用的組件（無需修改）

- `Components/Shared/Report/BatchPrintFilterModalComponent.razor`
- `Components/Shared/Filters/FilterSectionComponent.razor`
- `Components/Shared/Filters/MultiSelectFilterComponent.razor`
- `Components/Shared/Filters/DateRangeFilterComponent.razor`
- `wwwroot/css/print-styles.css`