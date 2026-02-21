# 報表篩選架構設計說明

## 更新日期
2026-02-21

---

## 📋 設計概述

報表篩選架構採用 **Attribute 驅動的動態模板模式**，所有報表共用單一 `DynamicFilterTemplate.razor`，篩選 UI 由 Criteria 類別屬性上的 `Filter*Attribute` 自動產生：

- **Criteria 驅動**：在 Criteria 屬性上標記 `Filter*Attribute`，即可宣告篩選欄位的類型、標籤、資料來源
- **單一模板**：`DynamicFilterTemplate.razor` 透過反射讀取 Criteria，自動產生 3 欄佈局（基本篩選／日期範圍／快速條件）
- **零 UI 程式碼**：新增報表篩選不需撰寫 `.razor` 模板，只需修改 Criteria 類別
- **介面統一**：`DynamicFilterTemplate` 實作 `IFilterTemplateComponent` 介面
- **可擴展**：新增報表只需建立 Criteria（加 Attribute）並在 Registry 登記即可

---

## 🏗️ 架構圖

```
┌─────────────────────────────────────────────────────────────────┐
│                Layer 1: 通用篩選 Modal 容器                      │
│   GenericReportFilterModalComponent                              │
│   - 接收 ReportId 參數                                           │
│   - 從 FilterTemplateRegistry 取得配置                          │
│   - 使用 DynamicComponent 動態載入 DynamicFilterTemplate        │
│   - 傳入 CriteriaType 參數                                       │
│   - 處理確認/取消事件，呼叫報表服務並開啟預覽                    │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                Layer 2: 通用動態篩選模板（唯一）                 │
│   DynamicFilterTemplate.razor                                    │
│   - 透過反射讀取 CriteriaType 上的 Filter*Attribute             │
│   - 自動分組：基本篩選 / 日期範圍 / 快速條件                    │
│   - 實作 GetCriteria() / Reset()                                │
│   - 用 IServiceProvider 動態載入 FK 資料                        │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                Layer 3: 原子篩選組件庫                           │
│   可重用的篩選組件                                               │
│   - FilterSectionGroup（分欄容器：自動 1-3 欄水平排列）         │
│   - FilterSectionColumn（區段欄：標題 + 欄位直向堆疊）          │
│   - FilterFieldRow（欄位行：標題 + 內容同行）                   │
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
├── FilterAttributes/
│   └── FilterFieldAttributes.cs            # Filter*Attribute 定義
│       ├── FilterGroup (enum)              # Basic=1, Date=2, Quick=3
│       ├── FilterDisplayFormat (enum)      # NameOnly, CodeDashName, CodeOnly
│       ├── FilterDisplayItem (class)       # Id + DisplayName（DynamicFilterTemplate 內部使用）
│       ├── FilterFKAttribute               # List<int> FK 多選
│       ├── FilterEnumAttribute             # List<TEnum> Enum 多選
│       ├── FilterDateRangeAttribute        # DateTime? 日期範圍（標在 Start 屬性）
│       ├── FilterKeywordAttribute          # string? 關鍵字搜尋
│       └── FilterToggleAttribute          # bool Checkbox 切換
├── FilterCriteria/                          # 篩選條件 DTO（實作 IReportFilterCriteria）
│   ├── IReportFilterCriteria.cs
│   └── [Entity]Criteria.cs                 # 屬性上標記 Filter*Attribute
└── FilterTemplates/                         # 模板配置
    ├── ReportFilterConfig.cs               # 篩選配置模型
    └── FilterTemplateRegistry.cs           # 模板註冊表（集中管理所有配置）

Components/Shared/Report/
├── GenericReportFilterModalComponent.razor  # 通用篩選 Modal
├── FilterTemplateInitializer.cs             # 模板初始化器
├── FilterSectionGroup.razor                 # 分欄容器（自動 1-3 欄水平排列）
├── FilterSectionColumn.razor                # 區段欄（標題 + 欄位直向堆疊）
├── FilterFieldRow.razor                     # 篩選欄位行（標題 + 內容同行佈局）
├── SearchSelectFilterComponent.razor        # 搜尋式多選（搜尋 → 下拉 → badge 標籤）
├── DateRangeFilterComponent.razor           # 日期範圍（含快速選擇按鈕）
├── TextSearchFilterComponent.razor          # 文字搜尋
└── FilterTemplates/
    └── DynamicFilterTemplate.razor          # 通用動態篩選模板（所有報表共用）
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
    public string FilterTemplateTypeName { get; set; }     // 永遠指向 DynamicFilterTemplate
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

## 🏷️ Filter*Attribute 說明

### FilterFKAttribute（FK 多選下拉）

標記在 `List<int>` 屬性上，`DynamicFilterTemplate` 會用 `IServiceProvider` 解析指定 Service，呼叫 `GetAllAsync()` 載入選項。

```csharp
[FilterFK(typeof(ICustomerService),
    Group = FilterGroup.Basic,
    Label = "客戶",
    Placeholder = "搜尋客戶...",
    EmptyMessage = "未選擇客戶（查詢全部）",
    DisplayFormat = FilterDisplayFormat.CodeDashName,  // Code - Name 格式
    ExcludeProperty = "IsDisabled",                    // 排除 IsDisabled==true 的項目（可選）
    Order = 1)]
public List<int> CustomerIds { get; set; } = new();
```

| 參數 | 說明 | 預設值 |
|------|------|--------|
| `ServiceType` | 用於載入選項的 Service 介面型別（必填） | — |
| `Group` | 顯示在哪個欄位群組 | `FilterGroup.Basic` |
| `Label` | FilterFieldRow 標籤文字 | `""` |
| `Placeholder` | 搜尋框提示文字 | `"搜尋..."` |
| `EmptyMessage` | 未選擇時的提示訊息 | `"未選擇（查詢全部）"` |
| `DisplayFormat` | 顯示名稱格式（NameOnly / CodeDashName / CodeOnly） | `NameOnly` |
| `ExcludeProperty` | Entity 上的 bool 屬性名稱，為 true 時排除該筆資料 | `null` |
| `Order` | 群組內排列順序 | `0` |

### FilterEnumAttribute（Enum 多選下拉）

標記在 `List<TEnum>` 屬性上，自動讀取 `[Display(Name)]` 產生選項。

```csharp
[FilterEnum(typeof(OrderStatus),
    Group = FilterGroup.Basic,
    Label = "訂單狀態",
    Order = 2)]
public List<OrderStatus> Statuses { get; set; } = new();
```

### FilterDateRangeAttribute（日期範圍）

**只標記在 Start 屬性**，End 屬性由命名規則自動推導（`XxxStart` → `XxxEnd`）。

```csharp
[FilterDateRange(Group = FilterGroup.Date, Label = "訂單日期", Order = 1)]
public DateTime? OrderDateStart { get; set; }
public DateTime? OrderDateEnd { get; set; }   // 不加 Attribute，自動配對
```

若 End 屬性命名不符規則，可手動指定：

```csharp
[FilterDateRange(Label = "日期", EndPropertyName = "DateTo")]
public DateTime? DateFrom { get; set; }
public DateTime? DateTo { get; set; }
```

### FilterKeywordAttribute（關鍵字搜尋）

標記在 `string?` 屬性上。

```csharp
[FilterKeyword(Group = FilterGroup.Quick, Label = "關鍵字", Placeholder = "搜尋單號、備注...", Order = 1)]
public string? Keyword { get; set; }
```

### FilterToggleAttribute（Checkbox 切換）

標記在 `bool` 屬性上。

```csharp
[FilterToggle(Group = FilterGroup.Quick, Label = "顯示條件", CheckboxLabel = "僅顯示啟用", DefaultValue = true, Order = 2)]
public bool ActiveOnly { get; set; } = true;
```

---

## 📖 新增報表篩選步驟

### 1. 建立篩選條件 Criteria 並加上 Attribute

```csharp
// Models/Reports/FilterCriteria/SomeReportCriteria.cs
using ERPCore2.Models.Reports.FilterAttributes;
using ERPCore2.Services;

public class SomeReportCriteria : IReportFilterCriteria
{
    [FilterFK(typeof(ICustomerService),
        Group = FilterGroup.Basic,
        Label = "客戶",
        Placeholder = "搜尋客戶...",
        EmptyMessage = "未選擇客戶（查詢全部）",
        Order = 1)]
    public List<int> CustomerIds { get; set; } = new();

    [FilterDateRange(Group = FilterGroup.Date, Label = "日期範圍", Order = 1)]
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [FilterToggle(Group = FilterGroup.Quick, Label = "顯示條件", CheckboxLabel = "排除已取消", DefaultValue = true, Order = 1)]
    public bool ExcludeCancelled { get; set; } = true;

    /// <summary>紙張設定（不加 Attribute，不顯示在 UI）</summary>
    public PaperSetting? PaperSetting { get; set; }

    public bool Validate(out string? errorMessage) { ... }
    public Dictionary<string, object?> ToQueryParameters() { ... }
}
```

### 2. 在 FilterTemplateRegistry 登記配置

```csharp
// Models/Reports/FilterTemplates/FilterTemplateRegistry.cs
RegisterConfig(new ReportFilterConfig
{
    ReportId = ReportIds.SomeReport,
    FilterTemplateTypeName = "ERPCore2.Components.Shared.Report.FilterTemplates.DynamicFilterTemplate",
    CriteriaType = typeof(SomeReportCriteria),
    ReportServiceType = typeof(ISomeReportService),
    PreviewTitle = "某報表預覽",
    FilterTitle = "某報表篩選條件",
    IconClass = "bi-file-earmark-text",
    GetDocumentName = criteria => $"某報表-{DateTime.Now:yyyyMMddHHmm}"
});
```

### 3. 在 ReportRegistry 確認報表已啟用

```csharp
new ReportDefinition
{
    Id = "XX001",
    Name = "某報表",
    IsEnabled = true,
    ...
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
   ↓ DynamicComponent 渲染 DynamicFilterTemplate，傳入 CriteriaType

6. DynamicFilterTemplate 初始化
   ↓ 反射掃描 CriteriaType 屬性上的 Filter*Attribute
   ↓ 並行呼叫所有 FK Service.GetAllAsync() 載入選項
   ↓ 建立 3 欄 UI（基本篩選 / 日期範圍 / 快速條件）

7. 使用者填寫篩選條件，按下「預覽列印」
   ↓ 從 DynamicComponent 取得 IFilterTemplateComponent
   ↓ 呼叫 GetCriteria() → Validate()

8. 轉換篩選條件並呼叫報表服務
   ↓ criteria.ToBatchPrintCriteria()
   ↓ ReportService.RenderBatchToImagesAsync(batchCriteria)
   ↓ 使用 BatchReportHelper 產生批次預覽圖片（含紙張設定）

9. 設定預覽資料，開啟 ReportPreviewModalComponent
   ↓ previewImages = result.PreviewImages
   ↓ formattedDocument = result.MergedDocument
   ↓ 根據 ReportId 載入預設印表機和紙張配置

10. 使用者確認後按「列印」
    ↓ 列印成功，關閉所有 Modal
```

---

## 🧩 原子篩選組件

### FilterSectionGroup（分欄容器）

將多個 `FilterSectionColumn` 水平並排，自動依欄數決定寬度。

| 參數 | 類型 | 說明 |
|------|------|------|
| `ChildContent` | `RenderFragment` | 放入 `FilterSectionColumn` |

佈局行為（CSS flex-wrap，定義在 `FilterSectionGroup.razor.css`）：

| 容器寬度 | 呈現效果 |
|---|---|
| ≥ 900px | 最多 3 欄並排（每欄 flex-basis 280px） |
| 600–900px | 2 欄並排 |
| ≤ 768px | 強制折成單欄 |

### FilterSectionColumn（區段欄）

代表一個分組欄，內部欄位直向堆疊，可設定標題與圖示。

| 參數 | 類型 | 說明 |
|------|------|------|
| `Title` | `string?` | 區段標題（選填） |
| `Icon` | `string?` | Bootstrap Icons CSS 類別（選填），例如 `"bi bi-people"` |
| `ChildContent` | `RenderFragment` | 放入 `FilterFieldRow` 欄位 |

**DynamicFilterTemplate 自動使用的區段分類：**

| 區段名稱 | FilterGroup | Icon | 適用欄位類型 |
|---|---|---|---|
| 基本篩選 | `Basic` | `bi bi-funnel` | FilterFK、FilterEnum |
| 日期範圍 | `Date` | `bi bi-calendar-range` | FilterDateRange |
| 快速條件 | `Quick` | `bi bi-search` | FilterKeyword、FilterToggle |

### FilterFieldRow（欄位行）

統一所有篩選欄位的「標題 + 內容」同行佈局。

| 參數 | 類型 | 說明 |
|------|------|------|
| `Label` | `string?` | 標題文字（藍色、固定寬度 80-120px） |
| `ChildContent` | `RenderFragment` | 內容區域（佔滿剩餘寬度） |
| `CssClass` | `string?` | 額外 CSS 類別 |

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

---

## ✅ 已實作的篩選配置

共 26 個篩選配置，全部使用 `DynamicFilterTemplate` 自動產生 UI。

| 分類 | 報表 ID | Criteria 類別 | 篩選欄位摘要 |
|------|---------|---------------|------------|
| 人資 | HR001 | EmployeeRosterCriteria | 員工、部門、職位、在職狀態、權限組、到職/離職/生日日期、關鍵字、僅在職 |
| 人資 | HR002 | EmployeeRosterCriteria | 同 HR001 |
| 客戶 | AR001 | AccountsReceivableCriteria | 客戶、日期範圍、帳款狀態 |
| 客戶 | AR002 | CustomerStatementCriteria | 客戶、日期範圍、交易類型 |
| 客戶 | AR003 | SetoffDocumentBatchPrintCriteria | 客戶、日期範圍、單號 |
| 客戶 | AR004 | CustomerTransactionCriteria | 客戶、日期範圍、選項 |
| 客戶 | AR005 | CustomerRosterCriteria | 客戶、業務負責人、關鍵字 |
| 客戶 | AR006 | CustomerSalesAnalysisCriteria | 客戶、日期範圍、選項 |
| 廠商 | AP002 | SupplierStatementCriteria | 廠商、日期範圍、選項 |
| 廠商 | AP003 | SetoffDocumentBatchPrintCriteria | 廠商、日期範圍、單號 |
| 廠商 | AP004 | SupplierRosterCriteria | 廠商、關鍵字 |
| 銷售 | SO001 | QuotationBatchPrintCriteria | 客戶、日期範圍、單號 |
| 銷售 | SO002 | SalesOrderBatchPrintCriteria | 客戶、日期範圍、單號 |
| 銷售 | SO004 | SalesDeliveryBatchPrintCriteria | 客戶、日期範圍、單號 |
| 銷售 | SO005 | SalesReturnBatchPrintCriteria | 客戶、日期範圍、單號 |
| 採購 | PO001 | PurchaseOrderBatchPrintCriteria | 廠商、日期範圍、單號 |
| 採購 | PO002 | PurchaseReceivingBatchPrintCriteria | 廠商、日期範圍、單號 |
| 採購 | PO003 | PurchaseReturnBatchPrintCriteria | 廠商、日期範圍、單號 |
| 庫存 | IV002 | InventoryStatusCriteria | 倉庫、商品分類、關鍵字 |
| 庫存 | IV003 | StockTakingDifferenceCriteria | 倉庫、日期範圍、關鍵字 |
| 生產 | PD001 | ProductionScheduleCriteria | 客戶、日期範圍、生產狀態 |
| 生產 | PD002 | BOMReportCriteria | 成品、關鍵字 |
| 產品 | PD004 | ProductListBatchPrintCriteria | 商品分類、採購類型、關鍵字 |
| 產品 | PD005 | ProductBarcodeBatchPrintCriteria | 商品分類、關鍵字 |
| 車輛 | VH001 | VehicleListCriteria | 車型、關鍵字 |
| 車輛 | VH002 | VehicleMaintenanceCriteria | 車輛、日期範圍、關鍵字 |

---

## 📝 新增報表篩選 Checklist

1. ☐ 在 `ReportIds.cs` 新增報表 ID 常數
2. ☐ 建立篩選條件 Criteria（`Models/Reports/FilterCriteria/`）
   - 實作 `IReportFilterCriteria` 介面（`Validate()` + `ToQueryParameters()`）
   - 在每個篩選屬性上加對應的 `Filter*Attribute`
   - `PaperSetting?` 不加 Attribute
3. ☐ 在 `FilterTemplateRegistry.cs` 的 `Initialize()` 中新增配置
   - `FilterTemplateTypeName` = `"ERPCore2.Components.Shared.Report.FilterTemplates.DynamicFilterTemplate"`
4. ☐ 在 `ReportRegistry.cs` 中確認報表 `IsEnabled = true`
5. ☐ 報表服務實作 `RenderBatchToImagesAsync`（使用 `BatchReportHelper`）

---

## ⚠️ 注意事項

1. **Criteria 屬性上加 Attribute 即可**：不需要建立 FilterTemplate.razor，`DynamicFilterTemplate` 自動讀取並產生 UI
2. **FilterDateRange 只標在 Start 屬性**：End 屬性由命名規則自動推導（`HireDateStart` → `HireDateEnd`）；若命名不符可用 `EndPropertyName = "..."` 手動指定
3. **PaperSetting 不加 Attribute**：不需要篩選 UI 的屬性不加任何 Attribute，系統自動略過
4. **FilterFK 需要 ServiceType**：`typeof(ICustomerService)` 等，系統用 `IServiceProvider` 在執行期解析並呼叫 `GetAllAsync()`
5. **FilterTemplateInitializer 在 MainLayout 啟動時呼叫**：確保在使用前完成初始化
6. **驗證邏輯放在 Criteria 的 Validate() 方法**：不要在其他地方處理
7. **篩選條件須實作 `ToBatchPrintCriteria()`**：用於轉換為報表服務可用的批次篩選條件
8. **報表服務使用 `BatchReportHelper`**：避免重複實作批次預覽邏輯

---

## 相關檔案

- [README_報表系統總綱.md](README_報表系統總綱.md) - 報表系統入口
- [README_報表中心設計.md](README_報表中心設計.md) - 報表中心入口
- [README_報表檔設計總綱.md](README_報表檔設計.md) - 報表服務與列印
- [README_Index列印實作指南.md](README_報表Index設計.md) - Index 批次列印
