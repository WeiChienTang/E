# 報表篩選架構設計說明

## 更新日期
2026-02-10

---

## 📋 設計概述

報表篩選架構採用**模板註冊模式**，提供統一的篩選 UI 管理機制：

- **模板註冊表**：集中管理報表 ID 與篩選模板的對應關係
- **動態載入**：根據 ReportId 自動載入對應的篩選模板組件
- **介面統一**：所有篩選模板實作 `IFilterTemplateComponent` 介面
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
│   - DateRangeFilterComponent（日期範圍）                        │
│   - MultiSelectFilterComponent<T>（多選）                       │
│   - FilterSectionComponent（區塊容器）                          │
│   - 更多...                                                      │
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
├── FilterSectionComponent.razor             # 區塊容器
├── DateRangeFilterComponent.razor           # 日期範圍
├── MultiSelectFilterComponent.razor         # 多選
│
└── FilterTemplates/                         # 篩選模板組件
    ├── AccountsReceivableFilterTemplate.razor
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

```razor
@* Components/Shared/Report/FilterTemplates/CustomerStatementFilterTemplate.razor *@
@using ERPCore2.Models.Reports.FilterTemplates
@implements IFilterTemplateComponent

<FilterSectionComponent Title="基本條件">
    <div class="mb-3">
        <label class="form-label fw-bold">選擇客戶</label>
        <select class="form-select" @bind="customerId">
            <option value="0">-- 請選擇 --</option>
            @foreach (var customer in customers)
            {
                <option value="@customer.Id">@customer.CompanyName</option>
            }
        </select>
    </div>
</FilterSectionComponent>

<FilterSectionComponent Title="日期範圍">
    <DateRangeFilterComponent @bind-StartDate="@startDate"
                             @bind-EndDate="@endDate"
                             ShowQuickSelectors="true" />
</FilterSectionComponent>

@code {
    private int customerId;
    private DateTime? startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
    private DateTime? endDate = DateTime.Now;
    private List<Customer> customers = new();
    
    public IReportFilterCriteria GetCriteria()
    {
        return new CustomerStatementCriteria
        {
            CustomerId = customerId,
            StartDate = startDate,
            EndDate = endDate
        };
    }
    
    public void Reset()
    {
        customerId = 0;
        startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        endDate = DateTime.Now;
        StateHasChanged();
    }
}
```

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

## ✅ 已實作的篩選模板

| 報表 ID | 模板組件 | 篩選條件 DTO | 說明 |
|---------|----------|--------------|------|
| AR001 | AccountsReceivableFilterTemplate | AccountsReceivableCriteria | 應收帳款報表 |
| PO001 | PurchaseOrderBatchFilterTemplate | PurchaseOrderBatchPrintCriteria | 採購單（報表中心進入） |

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
4. ☐ 在 `FilterTemplateRegistry.cs` 的 `Initialize()` 中註冊配置
   - 設定 `FilterTemplateTypeName` 為完整類別名稱
5. ☐ 在 `ReportRegistry.cs` 中確認報表 `IsEnabled = true`
6. ☐ 報表服務實作 `RenderBatchToImagesAsync`（使用 `BatchReportHelper`）

---

## ⚠️ 注意事項

1. **模板組件必須實作 `IFilterTemplateComponent`**：否則 Modal 無法取得篩選條件
2. **FilterTemplateInitializer 在 MainLayout 啟動時呼叫**：確保在使用前完成初始化
3. **驗證邏輯放在 Criteria 的 Validate() 方法**：不要在模板組件中處理
4. **篩選條件須實作 `ToBatchPrintCriteria()`**：用於轉換為報表服務可用的批次篩選條件
5. **報表服務使用 `BatchReportHelper`**：避免重複實作批次預覽邏輯，只需專注於資料查詢
6. **紙張變更會觸發重新渲染**：GenericReportFilterModalComponent 處理 OnPaperSettingChanged 事件，更新 BatchPrintCriteria.PaperSetting 並重新產生預覽

---

## 相關檔案

- [README_報表系統總綱.md](README_報表系統總綱.md) - 報表系統入口
- [README_報表中心設計.md](README_報表中心設計.md) - 報表中心入口
- [README_報表檔設計總綱.md](README_報表檔設計.md) - 報表服務與列印
- [README_Index列印實作指南.md](README_報表Index設計.md) - Index 批次列印
