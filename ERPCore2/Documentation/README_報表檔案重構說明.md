# 報表檔案重構說明

## 變更日期
2025-01-XX（初版）
2026-02-02（更新目錄結構說明）
2026-02-02（新增報表列印配置自動化機制）

## 重構目的
將分散在多個目錄的報表相關檔案整合至統一的目錄結構，提升程式碼可維護性和可讀性。

---

## 目前完整目錄結構

### 📁 Services/Reports/ （報表服務主目錄）

#### 根目錄檔案（向後相容性重導向 + 服務實作）
| 檔案 | 類型 | 說明 |
|-----|------|------|
| `IReportService.cs` | 重導向 | 向後相容性檔案，使用 `global using` 導向 Interfaces/ |
| `IQuotationReportService.cs` | 重導向 | 向後相容性檔案 |
| `ISalesOrderReportService.cs` | 重導向 | 向後相容性檔案 |
| `ISalesReturnReportService.cs` | 重導向 | 向後相容性檔案 |
| `IPurchaseReturnReportService.cs` | 重導向 | 向後相容性檔案 |
| `IProductBarcodeReportService.cs` | 重導向 | 向後相容性檔案 |
| `ReportService.cs` | 實作 | 通用報表服務實作 |
| `PurchaseOrderReportService.cs` | 實作 | 採購單報表服務實作 |
| `PurchaseReceivingReportService.cs` | 實作 | 進貨單報表服務實作 |
| `PurchaseReturnReportService.cs` | 實作 | 進貨退出單報表服務實作 |
| `QuotationReportService.cs` | 實作 | 報價單報表服務實作 |
| `SalesOrderReportService.cs` | 實作 | 銷貨單報表服務實作 |
| `SalesReturnReportService.cs` | 實作 | 銷貨退回單報表服務實作 |
| `ProductBarcodeReportService.cs` | 實作 | 商品條碼報表服務實作 |

#### 📁 Services/Reports/Interfaces/ （介面定義）
所有報表服務介面的集中位置：
- `IReportService.cs` - 通用報表服務介面
- `IPurchaseOrderReportService.cs` - 採購單報表服務介面
- `IPurchaseReceivingReportService.cs` - 進貨單報表服務介面
- `IPurchaseReturnReportService.cs` - 進貨退出單報表服務介面
- `IQuotationReportService.cs` - 報價單報表服務介面
- `ISalesOrderReportService.cs` - 銷貨單報表服務介面
- `ISalesReturnReportService.cs` - 銷貨退回單報表服務介面
- `IProductBarcodeReportService.cs` - 商品條碼報表服務介面

#### 📁 Services/Reports/Configuration/ （列印配置服務）
報表列印配置服務：
- `IReportPrintConfigurationService.cs` - 列印配置服務介面
- `ReportPrintConfigurationService.cs` - 列印配置服務實作

#### 📁 Services/Reports/Common/ （通用報表建構元件）
報表生成共用元件（Builder Pattern）：
| 檔案 | 說明 |
|-----|------|
| `IReportDetailItem.cs` | 報表明細項目介面（用於分頁計算） |
| `ReportPage.cs` | 報表頁面資訊類別 |
| `ReportPageLayout.cs` | 報表頁面配置定義（尺寸、高度等） |
| `ReportPaginator.cs` | 通用報表分頁計算器 |
| `ReportHeaderBuilder.cs` | 報表表頭建構器（三欄式設計） |
| `ReportInfoSectionBuilder.cs` | 報表資訊區塊建構器 |
| `ReportTableBuilder.cs` | 報表表格建構器（泛型支援） |
| `ReportSummaryBuilder.cs` | 報表統計區建構器 |
| `ReportSignatureBuilder.cs` | 報表簽名區建構器 |

### 📁 Controllers/Reports/ （報表控制器）
報表 API 控制器：
- `BaseReportController.cs` - 報表控制器基底類別（提供共用邏輯）
- `PurchaseReportController.cs` - 採購相關報表控制器
- `SalesReportController.cs` - 銷貨相關報表控制器

### 📁 Models/Reports/ （報表模型）
報表相關模型類別：
- `ReportModels.cs` - 報表配置類別（ReportConfiguration、ReportField、ReportHeaderSection 等）
- `ReportDefinition.cs` - 報表定義類別
- `BatchPrintCriteria.cs` - 批次列印條件類別

### 📁 Models/ 根目錄（向後相容性）
保留向後相容性的檔案：
- `ReportModels.cs` - 使用 `global using` 重導向至 Models/Reports/
- `ReportDefinition.cs` - 報表定義
- `BatchPrintCriteria.cs` - 批次列印條件

---

## 向後相容性

為確保現有程式碼無需修改，已保留舊檔案並添加重導向：

### 舊介面檔案（Services/Reports/*.cs）
使用 `global using` 語句重導向至新的 Interfaces 目錄：
```csharp
// ============================================================================
// 向後相容性檔案 - 已遷移至 Services/Reports/Interfaces/
// 此檔案保留以維持現有程式碼的相容性，建議逐步更新 using 語句至新位置
// ============================================================================
global using ERPCore2.Services.Reports.Interfaces;
```

### 舊模型檔案（Models/ReportModels.cs）
- 使用 `global using` 導入 `ERPCore2.Models.Reports` 命名空間
- 保留 `SortDirection` 枚舉在 `ERPCore2.Models` 命名空間，以支援 `Models.SortDirection` 語法

### 舊服務檔案（Services/Systems/ReportPrintConfigurationService.cs）
使用 `global using` 語句重導向至新的 Configuration 目錄：
```csharp
global using ERPCore2.Services.Reports.Configuration;
```

---

## 命名空間對照

| 舊命名空間 | 新命名空間 |
|-----------|-----------|
| `ERPCore2.Services.Reports` (介面) | `ERPCore2.Services.Reports.Interfaces` |
| `ERPCore2.Services` (ReportPrintConfigurationService) | `ERPCore2.Services.Reports.Configuration` |
| `ERPCore2.Models` (報表模型) | `ERPCore2.Models.Reports` |

---

## ServiceRegistration.cs 配置

已更新服務註冊以使用完整命名空間（位於 `Data/ServiceRegistration.cs`）：
```csharp
// 報表列印配置服務
services.AddScoped<ERPCore2.Services.Reports.Configuration.IReportPrintConfigurationService, 
                  ERPCore2.Services.Reports.Configuration.ReportPrintConfigurationService>();

// 報表服務 - 介面位於 ERPCore2.Services.Reports.Interfaces
services.AddScoped<ERPCore2.Services.Reports.Interfaces.IReportService, ReportService>();
services.AddScoped<ERPCore2.Services.Reports.Interfaces.IPurchaseOrderReportService, PurchaseOrderReportService>();
services.AddScoped<ERPCore2.Services.Reports.Interfaces.IPurchaseReceivingReportService, PurchaseReceivingReportService>();
services.AddScoped<ERPCore2.Services.Reports.Interfaces.IPurchaseReturnReportService, PurchaseReturnReportService>();
services.AddScoped<ERPCore2.Services.Reports.Interfaces.ISalesOrderReportService, SalesOrderReportService>();
services.AddScoped<ERPCore2.Services.Reports.Interfaces.ISalesReturnReportService, SalesReturnReportService>();
services.AddScoped<ERPCore2.Services.Reports.Interfaces.IQuotationReportService, QuotationReportService>();
services.AddScoped<ERPCore2.Services.Reports.Interfaces.IProductBarcodeReportService, ProductBarcodeReportService>();
```

---

## 報表服務架構說明

### 分層架構
```
Controllers/Reports/
    └── BaseReportController.cs          ← 控制器基底類別（處理 HTTP 請求）
         ├── PurchaseReportController.cs ← 採購報表控制器
         └── SalesReportController.cs    ← 銷貨報表控制器

Services/Reports/
    ├── Interfaces/                      ← 介面定義層
    │    └── I*ReportService.cs
    ├── *ReportService.cs                ← 服務實作層
    ├── Configuration/                   ← 列印配置服務
    └── Common/                          ← 共用元件層（Builder Pattern）
         ├── ReportPageLayout.cs         ← 頁面尺寸配置
         ├── ReportPaginator.cs          ← 智能分頁計算
         ├── ReportHeaderBuilder.cs      ← 表頭建構
         ├── ReportInfoSectionBuilder.cs ← 資訊區建構
         ├── ReportTableBuilder.cs       ← 表格建構
         ├── ReportSummaryBuilder.cs     ← 統計區建構
         └── ReportSignatureBuilder.cs   ← 簽名區建構

Models/Reports/
    ├── ReportModels.cs                  ← 報表配置模型
    ├── ReportDefinition.cs              ← 報表定義
    └── BatchPrintCriteria.cs            ← 批次列印條件
```

### Common 元件使用模式（Builder Pattern）
報表服務使用 Builder Pattern 組裝報表各區塊：

```csharp
// 1. 定義頁面配置
var layout = ReportPageLayout.ContinuousForm();

// 2. 使用分頁器分割明細
var paginator = new ReportPaginator<DetailItem>(layout);
var pages = paginator.SplitIntoPages(details);

// 3. 為每頁建構 HTML
foreach (var page in pages)
{
    // 表頭
    var header = new ReportHeaderBuilder()
        .SetCompanyInfo(taxId, phone, fax)
        .SetCompanyName(companyName)
        .SetReportTitle("採購單")
        .SetPageInfo($"第 {pageNum} 頁，共 {totalPages} 頁")
        .Build();

    // 資訊區
    var info = new ReportInfoSectionBuilder()
        .AddRow("採購單號", orderNo, "採購日期", date)
        .AddRow("廠商名稱", supplierName)
        .Build();

    // 表格
    var table = new ReportTableBuilder<DetailItem>()
        .AddIndexColumn()
        .AddTextColumn("品名", "30%", d => d.ProductName)
        .AddQuantityColumn("數量", "10%", d => d.Quantity)
        .AddAmountColumn("金額", "15%", d => d.Amount)
        .Build(page.Items);

    // 統計區（僅最後一頁）
    if (page.IsLastPage)
    {
        var summary = new ReportSummaryBuilder()
            .SetRemarks(remarks)
            .AddAmountItem("小計", subtotal)
            .AddAmountItem("稅額", tax)
            .AddAmountItem("總計", total)
            .Build();

        var signature = new ReportSignatureBuilder()
            .AddSignatures("採購人員", "核准人員", "廠商簽收")
            .Build();
    }
}
```

### IReportDetailItem 介面
所有報表明細項目必須實作此介面以支援智能分頁：
```csharp
public interface IReportDetailItem
{
    /// <summary>取得備註內容（用於高度計算）</summary>
    string GetRemarks();

    /// <summary>取得額外高度因素（mm）</summary>
    decimal GetExtraHeightFactor() => 0m;
}
```

---

## 新程式碼建議使用方式

### 使用介面時
```csharp
using ERPCore2.Services.Reports.Interfaces;

public class MyComponent
{
    [Inject] private IPurchaseOrderReportService ReportService { get; set; }
}
```

### 使用模型時
```csharp
using ERPCore2.Models.Reports;

var config = new ReportConfiguration
{
    Title = "報表標題"
};
```

### 使用配置服務時
```csharp
using ERPCore2.Services.Reports.Configuration;

public class MyService
{
    private readonly IReportPrintConfigurationService _configService;
}
```

### 使用 Common 元件時
```csharp
using ERPCore2.Services.Reports.Common;

// 建立頁面配置
var layout = ReportPageLayout.ContinuousForm();

// 使用各種 Builder
var header = new ReportHeaderBuilder();
var table = new ReportTableBuilder<MyDetailItem>();
var summary = new ReportSummaryBuilder();
var signature = new ReportSignatureBuilder();
```

---

## 注意事項

1. **SortDirection 枚舉位置**：保留在 `ERPCore2.Models` 命名空間，因為許多現有程式碼使用 `Models.SortDirection` 語法。

2. **逐步遷移**：現有程式碼可繼續使用舊的命名空間，建議在修改相關檔案時逐步更新至新命名空間。

3. **未來清理**：當所有程式碼都更新至新命名空間後，可移除舊的重導向檔案。

4. **服務實作位置**：報表服務實作檔案（`*ReportService.cs`）保留在 `Services/Reports/` 根目錄，介面檔案則在 `Interfaces/` 子目錄。

5. **Common 元件**：新增報表時，應優先使用 Common 目錄下的 Builder 元件，確保報表風格一致。

---

## 報表列印配置自動化機制

### 概述
系統透過 `ReportRegistry` 集中管理所有報表定義，並在應用程式啟動時自動建立對應的列印配置。

### 相關檔案
| 檔案 | 位置 | 說明 |
|-----|------|------|
| `ReportRegistry.cs` | `Data/` | 靜態報表定義註冊表 |
| `ReportDefinition.cs` | `Models/Reports/` | 報表定義模型 |
| `ReportPrintConfiguration.cs` | `Data/Entities/Systems/` | 報表列印配置實體 |
| `ReportPrintConfigurationSeeder.cs` | `Data/SeedDataManager/Seeders/` | 自動建立列印配置的種子資料器 |

### 資料流程
```
ReportRegistry (靜態定義)              ReportPrintConfiguration (資料表)
┌────────────────────────────┐        ┌─────────────────────────────────────┐
│ Id: AR001                  │        │ ReportId: AR001                     │
│ Name: 應收帳款報表          │   →    │ ReportName: 應收帳款報表             │
│ Description: ...           │        │ PrinterConfigurationId: null        │
│ IsEnabled: true/false      │        │ PaperSettingId: null                │
└────────────────────────────┘        └─────────────────────────────────────┘
        ↓                                      ↓
   程式碼定義                            資料庫儲存（可編輯印表機/紙張）
```

### ReportPrintConfiguration 實體欄位
```csharp
public class ReportPrintConfiguration : BaseEntity
{
    public string ReportId { get; set; }              // 報表識別碼（對應 ReportRegistry.Id）
    public string ReportName { get; set; }            // 報表名稱（對應 ReportRegistry.Name）
    public int? PrinterConfigurationId { get; set; }  // 印表機設定 FK
    public int? PaperSettingId { get; set; }          // 紙張設定 FK
}
```

### Seeder 運作邏輯
`ReportPrintConfigurationSeeder` 在每次應用程式啟動時執行：
1. 讀取 `ReportRegistry.GetAllReports()` 取得所有報表定義
2. 查詢資料庫中已存在的 `ReportId`
3. 對於尚未存在的報表，自動建立 `ReportPrintConfiguration` 記錄
4. 已存在的配置不會被覆蓋（保留使用者設定的印表機/紙張）

```csharp
// Seeder 核心邏輯
foreach (var report in allReports)
{
    if (existingReportIds.Contains(report.Id))
        continue;  // 已存在，跳過
    
    // 建立新配置（預設無印表機、無紙張）
    var config = new ReportPrintConfiguration
    {
        ReportId = report.Id,
        ReportName = report.Name,
        PrinterConfigurationId = null,
        PaperSettingId = null,
        Status = report.IsEnabled ? EntityStatus.Active : EntityStatus.Inactive
    };
}
```

---

## 新增報表完整步驟

### 步驟 1：在 ReportRegistry 定義報表
在 `Data/ReportRegistry.cs` 的 `GetAllReports()` 方法中新增：
```csharp
new ReportDefinition
{
    Id = "XX001",                    // 唯一識別碼（建議格式：類別代碼 + 序號）
    Name = "新報表名稱",
    Description = "報表說明",
    IconClass = "bi bi-file-text",   // Bootstrap Icons
    Category = ReportCategory.Sales, // 報表分類
    RequiredPermission = "Entity.Read",
    ActionId = "OpenNewReport",
    SortOrder = 1,
    IsEnabled = true                 // false = 尚未實作
}
```

### 步驟 2：建立報表服務介面
在 `Services/Reports/Interfaces/` 新增 `I{ReportName}ReportService.cs`

### 步驟 3：建立報表服務實作
在 `Services/Reports/` 新增 `{ReportName}ReportService.cs`，使用 Common 元件組裝報表

### 步驟 4：註冊服務
在 `Data/ServiceRegistration.cs` 新增：
```csharp
services.AddScoped<ERPCore2.Services.Reports.Interfaces.I{ReportName}ReportService, {ReportName}ReportService>();
```

### 步驟 5：建立控制器端點
在適當的控制器新增端點，或建立新控制器繼承 `BaseReportController`

### 步驟 6：重新啟動應用程式
- Seeder 會自動偵測新報表並建立 `ReportPrintConfiguration` 記錄
- 在「報表列印配置」頁面 (`/reportPrintConfigurations`) 可看到新報表
- 使用者可編輯設定印表機和紙張

### 注意事項
| 操作 | 結果 |
|------|------|
| 在 ReportRegistry 新增報表 | 重啟後自動建立配置 ✅ |
| 修改 ReportRegistry 的 Name | 不影響已存在的配置（以 ReportId 比對） |
| 刪除 ReportRegistry 的報表 | 不會自動刪除資料庫配置（需手動處理） |
| 修改已存在配置的印表機/紙張 | 不會被 Seeder 覆蓋 ✅ |

---

## 報表列印配置服務 API

### 透過 ReportId 取得配置
```csharp
// 推薦使用 ReportId 查詢（精確匹配）
var config = await _configService.GetByReportIdAsync("AR001");

// 也支援透過 ReportName 查詢（向下相容）
var config = await _configService.GetCompleteConfigurationAsync("應收帳款報表");
```

### 在控制器中使用
```csharp
[HttpGet("order/{id}")]
public async Task<IActionResult> GetReport(
    int id,
    [FromQuery] string? reportType = null)  // 傳入 ReportId 如 "PO001"
{
    ReportPrintConfiguration? printConfig = null;
    if (!string.IsNullOrEmpty(reportType))
    {
        printConfig = await _configService.GetByReportIdAsync(reportType);
    }
    
    // 使用 printConfig 中的印表機/紙張設定...
}
```

---

## 新增報表服務步驟（舊版，保留相容性）

1. **建立介面**
   - 在 `Services/Reports/Interfaces/` 新增 `I{ReportName}ReportService.cs`
   
2. **建立實作**
   - 在 `Services/Reports/` 新增 `{ReportName}ReportService.cs`
   - 使用 Common 元件組裝報表

3. **註冊服務**
   - 在 `Data/ServiceRegistration.cs` 新增：
   ```csharp
   services.AddScoped<ERPCore2.Services.Reports.Interfaces.I{ReportName}ReportService, {ReportName}ReportService>();
   ```

4. **（可選）新增向後相容性檔案**
   - 在 `Services/Reports/` 新增 `I{ReportName}ReportService.cs` 重導向檔案

5. **建立控制器端點**
   - 在適當的控制器（`PurchaseReportController.cs` 或 `SalesReportController.cs`）新增端點
   - 或建立新的控制器繼承 `BaseReportController`
