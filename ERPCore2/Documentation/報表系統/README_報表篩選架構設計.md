# 報表篩選架構設計說明

## 更新日期
2026-02-17

---

## 📋 設計概述

報表篩選架構採用**模板註冊模式**，提供統一的篩選 UI 管理機制：

- **模板註冊表**：集中管理報表 ID 與篩選模板的對應關係
- **動態載入**：根據 ReportId 自動載入對應的篩選模板組件
- **介面統一**：所有篩選模板實作 `IFilterTemplateComponent` 介面
- **佈局統一**：所有篩選欄位使用 `FilterFieldRow` 組件包裝，確保標題與內容同行佈局一致
- **可擴展**：新增報表只需建立模板組件並註冊即可

---

## 🏗️ 架構圖

```
┌─────────────────────────────────────────────────────────────────┐
│                Layer 1: 通用篩選 Modal 容器                      │
│   GenericReportFilterModalComponent                              │
│   - 接收 ReportId 參數                                           │
│   - 從 FilterTemplateRegistry 取得配置                          │
│   - 使用 DynamicComponent 動態載入篩選模板                      │
│   - 處理確認/取消事件                                            │
│   - 呼叫報表服務並開啟預覽                                       │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                Layer 2: 篩選模板組件                             │
│   IFilterTemplateComponent                                       │
│   - 例如: PurchaseOrderBatchFilterTemplate                       │
│   - 提供篩選 UI（使用原子篩選組件）                              │
│   - 實作 GetCriteria() 返回篩選條件 DTO                         │
│   - 實作 Reset() 重置為預設值                                   │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                Layer 3: 原子篩選組件庫                           │
│   可重用的篩選組件                                               │
│   - FilterFieldRow（佈局包裝：標題 + 內容同行）                 │
│   - SearchSelectFilterComponent<T>（搜尋式多選）                │
│   - DateRangeFilterComponent（日期範圍 + 快速選擇）             │
│   - TextSearchFilterComponent（文字搜尋）                       │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📁 檔案結構

```
Models/Reports/
├── ReportIds.cs                             # 報表 ID 常數（唯一來源）
├── FilterCriteria/                          # 篩選條件 DTO
│   ├── IReportFilterCriteria.cs            # 基礎介面
│   ├── AccountsReceivableCriteria.cs       # 應收帳款篩選條件
│   └── PurchaseOrderBatchPrintCriteria.cs  # 採購單批次篩選條件
│
└── FilterTemplates/                         # 模板配置
    ├── ReportFilterConfig.cs               # 篩選配置模型 + IFilterTemplateComponent
    └── FilterTemplateRegistry.cs           # 模板註冊表（集中管理所有配置）

Components/Shared/Report/
├── GenericReportFilterModalComponent.razor  # 通用篩選 Modal
├── FilterTemplateInitializer.cs             # 模板初始化器
├── FilterFieldRow.razor                     # 篩選欄位行（標題 + 內容同行佈局）
├── FilterFieldRow.razor.css                 # FilterFieldRow 樣式（藍色標題、固定寬度）
├── SearchSelectFilterComponent.razor        # 搜尋式多選（搜尋 → 下拉 → badge 標籤）
├── DateRangeFilterComponent.razor           # 日期範圍（含快速選擇按鈕）
├── TextSearchFilterComponent.razor          # 文字搜尋
│
└── FilterTemplates/                         # 篩選模板組件（24 個）
    ├── EmployeeRosterBatchFilterTemplate.razor
    ├── CustomerRosterBatchFilterTemplate.razor
    ├── SupplierRosterBatchFilterTemplate.razor
    ├── CustomerStatementBatchFilterTemplate.razor
    ├── ... 其他 20 個篩選模板
    └── PurchaseOrderBatchFilterTemplate.razor
```

---

## 🔧 核心介面

### IReportFilterCriteria（篩選條件）

```csharp
public interface IReportFilterCriteria
{
    /// <summary>驗證篩選條件是否有效</summary>
    bool Validate(out string? errorMessage);
    
    /// <summary>轉換為查詢參數字典</summary>
    Dictionary<string, object?> ToQueryParameters();
}
```

### IFilterTemplateComponent（模板組件）

```csharp
public interface IFilterTemplateComponent
{
    /// <summary>取得目前的篩選條件</summary>
    IReportFilterCriteria GetCriteria();
    
    /// <summary>重置篩選條件為預設值</summary>
    void Reset();
}
```

### ReportFilterConfig（篩選配置）

```csharp
public class ReportFilterConfig
{
    public string ReportId { get; set; }                   // 報表 ID
    public string FilterTemplateTypeName { get; set; }     // 模板組件完整類別名稱
    public Type CriteriaType { get; set; }                 // 篩選條件 DTO 類型
    public Type? ReportServiceType { get; set; }           // 報表服務類型
    public string PreviewTitle { get; set; }               // 預覽標題
    public string FilterTitle { get; set; }                // 篩選 Modal 標題
    public string IconClass { get; set; }                  // 圖示類別
    public Func<IReportFilterCriteria, string>? GetDocumentName { get; set; }
    
    // 延遲解析模板類型
    public Type GetFilterTemplateType() { ... }
}
```

---

## 📖 新增報表篩選步驟

### 1. 建立篩選條件 DTO

```csharp
// Models/Reports/FilterCriteria/CustomerStatementCriteria.cs
public class CustomerStatementCriteria : IReportFilterCriteria
{
    public int CustomerId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    public bool Validate(out string? errorMessage)
    {
        if (CustomerId <= 0)
        {
            errorMessage = "請選擇客戶";
            return false;
        }
        errorMessage = null;
        return true;
    }
    
    public Dictionary<string, object?> ToQueryParameters()
    {
        return new Dictionary<string, object?>
        {
            ["customerId"] = CustomerId,
            ["startDate"] = StartDate,
            ["endDate"] = EndDate
        };
    }
}
```

### 2. 建立篩選模板組件

所有篩選欄位必須使用 `FilterFieldRow` 組件包裝，確保佈局一致：

```razor
@* Components/Shared/Report/FilterTemplates/CustomerStatementFilterTemplate.razor *@
@using ERPCore2.Models.Reports.FilterTemplates
@implements IFilterTemplateComponent
@inject ICustomerService CustomerService

<div>
    <FilterFieldRow Label="指定客戶">
        <SearchSelectFilterComponent TItem="Customer"
                                   Items="@customers"
                                   @bind-SelectedItems="@selectedCustomers"
                                   DisplayProperty="CompanyName"
                                   ValueProperty="Id"
                                   Placeholder="搜尋客戶..."
                                   EmptyMessage="未選擇客戶（查詢全部客戶）" />
    </FilterFieldRow>

    <FilterFieldRow Label="日期範圍">
        <DateRangeFilterComponent @bind-StartDate="startDate"
                                  @bind-EndDate="endDate"
                                  ShowQuickSelectors="true"
                                  AutoValidate="true"
                                  ShowValidationMessage="true" />
    </FilterFieldRow>

    <FilterFieldRow Label="關鍵字">
        <div class="d-flex align-items-center gap-2">
            <input type="text" class="form-control" placeholder="搜尋..."
                   @bind="keyword" />
            <div class="form-check text-nowrap">
                <input class="form-check-input" type="checkbox" id="activeOnly" @bind="activeOnly">
                <label class="form-check-label" for="activeOnly">僅啟用</label>
            </div>
        </div>
    </FilterFieldRow>
</div>

@code {
    private List<Customer> customers = new();
    private List<Customer> selectedCustomers = new();
    private DateTime? startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    private DateTime? endDate = DateTime.Today;
    private string? keyword;
    private bool activeOnly = true;

    protected override async Task OnInitializedAsync()
    {
        customers = await CustomerService.GetAllAsync();
    }

    public IReportFilterCriteria GetCriteria()
    {
        return new CustomerStatementCriteria
        {
            CustomerIds = selectedCustomers.Select(c => c.Id).ToList(),
            StartDate = startDate,
            EndDate = endDate
        };
    }

    public void Reset()
    {
        selectedCustomers = new();
        startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        endDate = DateTime.Today;
        keyword = null;
        activeOnly = true;
        StateHasChanged();
    }
}
```

#### 篩選模板 UI 規範

| 規範 | 說明 |
|------|------|
| **佈局包裝** | 每個欄位必須用 `<FilterFieldRow Label="...">` 包裝 |
| **多選欄位** | 使用 `SearchSelectFilterComponent`（搜尋 → 下拉 → badge 標籤） |
| **日期範圍** | 使用 `DateRangeFilterComponent`，必須設定 `ShowQuickSelectors="true"` |
| **關鍵字 + Checkbox** | 放在同一個 `FilterFieldRow` 內，用 `d-flex align-items-center gap-2` 排列 |
| **Checkbox 群組** | 用 `<FilterFieldRow Label="選項">` 包裝，內部用 `d-flex gap-3` 排列 |
| **Checkbox label** | 使用 `form-check-label`（不加 `small` class） |

### 3. 在 FilterTemplateRegistry 註冊篩選配置

```csharp
// Models/Reports/FilterTemplates/FilterTemplateRegistry.cs
public static void Initialize()
{
    // ... 現有配置 ...
    
    // 新增配置
    RegisterConfig(new ReportFilterConfig
    {
        ReportId = ReportIds.CustomerStatement,
        FilterTemplateTypeName = "ERPCore2.Components.Shared.Report.FilterTemplates.CustomerStatementFilterTemplate",
        CriteriaType = typeof(CustomerStatementCriteria),
        ReportServiceType = typeof(ICustomerStatementReportService),
        PreviewTitle = "客戶對帳單預覽",
        FilterTitle = "客戶對帳單篩選條件",
        IconClass = "bi-file-earmark-ruled",
        GetDocumentName = criteria =>
        {
            var c = (CustomerStatementCriteria)criteria;
            return $"客戶對帳單-{DateTime.Now:yyyyMMdd}";
        }
    });
}
```

> **重要**：`FilterTemplateTypeName` 必須是完整的類別名稱（含命名空間），系統會在執行時期延遲解析。

### 4. 在 ReportRegistry 新增報表定義

```csharp
// Data/Reports/ReportRegistry.cs
new ReportDefinition
{
    Id = "AR002",
    Name = "客戶對帳單",
    Description = "列印客戶交易對帳明細",
    IconClass = "bi bi-file-earmark-ruled",
    Category = ReportCategory.Customer,
    RequiredPermission = "Customer.Read",
    ActionId = "PrintCustomerStatement",
    SortOrder = 2,
    IsEnabled = true
}
```

---

## 🔄 完整流程

```
1. 應用程式啟動時
   ↓ MainLayout.OnInitializedAsync()
   ↓ FilterTemplateInitializer.EnsureInitialized()
   ↓ 註冊所有模板類型到 FilterTemplateRegistry
   
2. 使用者從報表中心選擇報表或按 Alt+R 搜尋
   ↓ GenericReportIndexPage 或 GenericSearchModalComponent
   ↓ 觸發 OnReportSelected / OnItemSelected(ActionId)
   
3. MainLayout.HandleReportSelected(actionId)
   ↓ 從 ActionId 查找對應的 ReportId
   ↓ 檢查 FilterTemplateRegistry.HasConfig(reportId)
   
4. 如果有篩選配置：
   ↓ currentFilterReportId = reportId
   ↓ 開啟 GenericReportFilterModalComponent
   
5. GenericReportFilterModalComponent 根據 ReportId 載入配置
   ↓ FilterTemplateRegistry.GetConfig(reportId)
   ↓ DynamicComponent 動態渲染對應的篩選模板組件
   
6. 使用者填寫篩選條件，按下「預覽列印」
   ↓ 從 DynamicComponent 取得 IFilterTemplateComponent
   ↓ 呼叫 GetCriteria() → Validate()
   
7. 轉換篩選條件並呼叫報表服務
   ↓ criteria.ToBatchPrintCriteria()
   ↓ ReportService.RenderBatchToImagesAsync(batchCriteria)
   ↓ 使用 BatchReportHelper 產生批次預覽圖片（含紙張設定）
   
8. 設定預覽資料，開啟 ReportPreviewModalComponent
   ↓ previewImages = result.PreviewImages
   ↓ formattedDocument = result.MergedDocument
   ↓ 根據 ReportId 載入預設印表機和紙張配置
   
9. 使用者變更紙張設定（可選）
   ↓ OnPaperSettingChanged 事件觸發
   ↓ 更新 batchCriteria.PaperSetting
   ↓ 重新呼叫 RenderBatchToImagesAsync 產生新預覽
   
10. 使用者確認後按「列印」
    ↓ 列印成功，關閉所有 Modal
```

---

## 🧩 原子篩選組件

### FilterFieldRow（佈局包裝）

統一所有篩選模板的「標題 + 內容」同行佈局。未來修改佈局樣式只需改此組件。

| 參數 | 類型 | 說明 |
|------|------|------|
| `Label` | `string?` | 標題文字（藍色、固定寬度 80-120px） |
| `ChildContent` | `RenderFragment` | 內容區域（佔滿剩餘寬度） |
| `CssClass` | `string?` | 額外 CSS 類別 |

樣式特性（定義在 `FilterFieldRow.razor.css`）：
- 標題：`color: #0d6efd`、`font-weight: 600`、`font-size: 1rem`
- 佈局：`display: flex`、`align-items: flex-start`、`gap: 0.5rem`
- 間距：`margin-bottom: 0.5rem`

### SearchSelectFilterComponent（搜尋式多選）

取代舊的 `MultiSelectFilterComponent`，改為「搜尋 → 下拉選擇 → badge 標籤顯示」模式。

| 參數 | 類型 | 說明 |
|------|------|------|
| `Items` | `List<TItem>` | 可選擇的項目清單 |
| `SelectedItems` | `List<TItem>` | 已選擇的項目（雙向綁定） |
| `DisplayProperty` | `string` | 顯示文字的屬性名稱 |
| `ValueProperty` | `string` | 值的屬性名稱（預設 `"Id"`） |
| `Placeholder` | `string` | 搜尋框提示文字 |
| `EmptyMessage` | `string` | 未選擇時的提示訊息 |
| `MaxDropdownItems` | `int` | 下拉最多顯示筆數（預設 50） |

### DateRangeFilterComponent（日期範圍）

| 參數 | 類型 | 說明 |
|------|------|------|
| `StartDate` / `EndDate` | `DateTime?` | 起訖日期（雙向綁定） |
| `ShowQuickSelectors` | `bool` | 顯示快速選擇按鈕（今天、本週、本月等） |
| `AutoValidate` | `bool` | 自動驗證日期範圍 |
| `ShowValidationMessage` | `bool` | 顯示驗證訊息 |

### TextSearchFilterComponent（文字搜尋）

| 參數 | 類型 | 說明 |
|------|------|------|
| `Value` | `string?` | 搜尋文字（雙向綁定） |
| `Label` | `string` | 標籤文字 |
| `Placeholder` | `string` | 輸入框提示文字 |

---

## ✅ 已實作的篩選模板

共 24 個篩選模板，全部使用 `FilterFieldRow` + `SearchSelectFilterComponent` 統一佈局。

| 分類 | 報表 ID | 模板組件 | 篩選欄位 |
|------|---------|----------|----------|
| 人資 | HR001 | EmployeeRosterBatchFilterTemplate | 員工、部門、職位、狀態、權限組、到職/離職/生日日期、關鍵字 |
| 客戶 | AR001 | AccountsReceivableFilterTemplate | 客戶、日期範圍、帳款狀態 |
| 客戶 | AR002 | CustomerStatementBatchFilterTemplate | 客戶、日期範圍、交易類型 |
| 客戶 | AR003 | AccountsReceivableSetoffBatchFilterTemplate | 客戶、日期範圍、單號 |
| 客戶 | AR004 | CustomerTransactionBatchFilterTemplate | 客戶、日期範圍、選項 |
| 客戶 | AR005 | CustomerRosterBatchFilterTemplate | 客戶、業務負責人、關鍵字 |
| 客戶 | AR006 | CustomerSalesAnalysisBatchFilterTemplate | 客戶、日期範圍、選項 |
| 廠商 | AP002 | SupplierStatementBatchFilterTemplate | 廠商、日期範圍、選項 |
| 廠商 | AP003 | AccountsPayableSetoffBatchFilterTemplate | 廠商、日期範圍、單號 |
| 廠商 | AP004 | SupplierRosterBatchFilterTemplate | 廠商、關鍵字 |
| 銷售 | SO001 | QuotationBatchFilterTemplate | 客戶、日期範圍、單號 |
| 銷售 | SO002 | SalesOrderBatchFilterTemplate | 客戶、日期範圍、單號 |
| 銷售 | SO004 | SalesDeliveryBatchFilterTemplate | 客戶、日期範圍、單號 |
| 銷售 | SO005 | SalesReturnBatchFilterTemplate | 客戶、日期範圍、單號 |
| 採購 | PO001 | PurchaseOrderBatchFilterTemplate | 廠商、日期範圍、單號 |
| 採購 | PO002 | PurchaseReceivingBatchFilterTemplate | 廠商、日期範圍、單號 |
| 採購 | PO003 | PurchaseReturnBatchFilterTemplate | 廠商、日期範圍、單號 |
| 庫存 | IV002 | InventoryStatusBatchFilterTemplate | 倉庫、商品分類、關鍵字 |
| 庫存 | IV003 | StockTakingDifferenceBatchFilterTemplate | 倉庫、日期範圍、關鍵字 |
| 生產 | PD001 | ProductionScheduleBatchFilterTemplate | 客戶、日期範圍、生產狀態 |
| 生產 | PD002 | BOMBatchFilterTemplate | 成品、關鍵字 |
| 產品 | PD004 | ProductListBatchFilterTemplate | 商品分類、採購類型、關鍵字 |
| 車輛 | VH001 | VehicleListBatchFilterTemplate | 車型、關鍵字 |
| 車輛 | VH002 | VehicleMaintenanceBatchFilterTemplate | 車輛、日期範圍、關鍵字 |

> **設計原則**：每個單據類型只有一個報表 ID，入口點決定行為：
> - **EditModal**：直接單筆列印（不經過 HandleReportSelected）
> - **報表中心 / Alt+R**：經由 HandleReportSelected 檢查是否有篩選配置

---

## 📝 新增報表篩選 Checklist

1. ☐ 在 `ReportIds.cs` 新增報表 ID 常數
2. ☐ 建立篩選條件 DTO（`Models/Reports/FilterCriteria/`）
   - 實作 `IReportFilterCriteria` 介面
   - 實作 `ToBatchPrintCriteria()` 方法
3. ☐ 建立篩選模板組件（`Components/Shared/Report/FilterTemplates/`）
   - 建立 `.razor` 檔案（實作 `IFilterTemplateComponent`）
   - **所有欄位使用 `FilterFieldRow` 包裝**
   - **多選欄位使用 `SearchSelectFilterComponent`**
   - **日期欄位設定 `ShowQuickSelectors="true"`**
4. ☐ 在 `FilterTemplateRegistry.cs` 的 `Initialize()` 中註冊配置
   - 設定 `FilterTemplateTypeName` 為完整類別名稱
5. ☐ 在 `ReportRegistry.cs` 中確認報表 `IsEnabled = true`
6. ☐ 報表服務實作 `RenderBatchToImagesAsync`（使用 `BatchReportHelper`）

---

## ⚠️ 注意事項

1. **模板組件必須實作 `IFilterTemplateComponent`**：否則 Modal 無法取得篩選條件
2. **所有篩選欄位必須用 `FilterFieldRow` 包裝**：確保佈局一致，未來統一修改樣式
3. **多選欄位使用 `SearchSelectFilterComponent`**：不要使用舊的 `MultiSelectFilterComponent`
4. **FilterTemplateInitializer 在 MainLayout 啟動時呼叫**：確保在使用前完成初始化
5. **驗證邏輯放在 Criteria 的 Validate() 方法**：不要在模板組件中處理
6. **篩選條件須實作 `ToBatchPrintCriteria()`**：用於轉換為報表服務可用的批次篩選條件
7. **報表服務使用 `BatchReportHelper`**：避免重複實作批次預覽邏輯，只需專注於資料查詢
8. **紙張變更會觸發重新渲染**：GenericReportFilterModalComponent 處理 OnPaperSettingChanged 事件，更新 BatchPrintCriteria.PaperSetting 並重新產生預覽

---

## 相關檔案

- [README_報表系統總綱.md](README_報表系統總綱.md) - 報表系統入口
- [README_報表中心設計.md](README_報表中心設計.md) - 報表中心入口
- [README_報表檔設計總綱.md](README_報表檔設計.md) - 報表服務與列印
- [README_Index列印實作指南.md](README_報表Index設計.md) - Index 批次列印
