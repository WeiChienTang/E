# 報表檔案架構說明

## 更新日期
2026-02-06（新增 Excel 匯出功能）

### 變更記錄

| 日期 | 變更內容 |
|------|---------|
| 2026-02-06 | **新增 Excel 匯出**：報表預覽 Modal 新增「轉Excel」按鈕 |
| 2026-02-06 | 新增 `IExcelExportService` 介面和 `ExcelExportService` 實作 |
| 2026-02-06 | 安裝 ClosedXML NuGet 套件 |
| 2026-02-05 | **重大簡化**：移除純文字模式，統一使用格式化列印 |
| 2026-02-05 | 移除舊版 API 控制器（ReportController、PurchaseReportController 等） |
| 2026-02-05 | 簡化 ReportPreviewModalComponent，移除雙模式切換 |

---

## 設計理念

本系統報表採用**統一格式化模式**，使用 `System.Drawing.Graphics` 進行圖形化渲染：

- **表格框線**：支援實線、虛線、雙線等樣式
- **圖片嵌入**：支援 JPG、PNG、BMP 等格式
- **精確排版**：自動計算位置，支援多種對齊方式
- **自動換頁**：內容超過頁面時自動分頁
- **預覽功能**：可渲染為圖片供預覽
- **Excel 匯出**：支援將報表轉換為 .xlsx 檔案

---

## 目錄結構

```
Services/Reports/
├── Interfaces/
│   ├── IFormattedPrintService.cs      ← 格式化列印介面
│   ├── IExcelExportService.cs         ← Excel 匯出介面
│   └── IPurchaseOrderReportService.cs ← 報表服務介面（範本）
├── FormattedPrintService.cs           ← 格式化列印服務實作
├── ExcelExportService.cs              ← Excel 匯出服務實作
├── PurchaseOrderReportService.cs      ← 報表服務實作（範本）
└── ...

Models/Reports/
├── FormattedDocument.cs               ← 格式化報表文件模型
├── TableDefinition.cs                 ← 表格定義
└── ...

Components/Shared/Report/
└── ReportPreviewModalComponent.razor  ← 報表預覽 Modal

wwwroot/js/
└── file-download-helper.js            ← 檔案下載 JS 函數
```

---

## 格式化列印服務（FormattedPrintService）

位置：`Services/Reports/FormattedPrintService.cs`

### 支援的元素類型

| 元素 | 類別 | 說明 |
|------|------|------|
| 文字 | `TextElement` | 支援字型大小、粗體、對齊方式 |
| 表格 | `TableElement` | 支援框線、表頭背景、自動欄寬 |
| 線條 | `LineElement` | 支援實線、虛線、點線、雙線 |
| 圖片 | `ImageElement` | 支援 JPG/PNG/BMP，自動縮放 |
| 間距 | `SpacingElement` | 垂直間距 |
| 分頁 | `PageBreakElement` | 強制換頁 |
| 簽名區 | `SignatureSectionElement` | 簽名欄位 |
| 鍵值對 | `KeyValueRowElement` | 如「單號：PO001」 |

### 使用方式（Fluent API）

```csharp
var document = new FormattedDocument()
    .SetDocumentName("採購單-PO20260205001")
    .SetMargins(1.5f, 1.5f, 1.5f, 1.5f)  // 邊距（公分）
    
    // 標題
    .AddTitle("公司名稱有限公司", fontSize: 14)
    .AddTitle("採 購 單", fontSize: 18, bold: true)
    .AddLine(LineStyle.Double, thickness: 2)
    
    // 基本資訊
    .AddKeyValueRow(("採購單號", "PO20260205001"), ("採購日期", "2026/02/05"))
    .AddLine(LineStyle.Dashed)
    
    // 表格
    .AddTable(table =>
    {
        table.AddColumn("序號", 0.5f, TextAlignment.Center)
             .AddColumn("品名", 3f, TextAlignment.Left)
             .AddColumn("數量", 1f, TextAlignment.Right)
             .AddColumn("單價", 1.2f, TextAlignment.Right)
             .AddColumn("小計", 1.5f, TextAlignment.Right)
             .ShowBorder(true)
             .ShowHeaderBackground(true);
        
        table.AddRow("1", "測試商品A", "10", "100.00", "1,000.00");
        table.AddRow("2", "測試商品B", "5", "200.00", "1,000.00");
    })
    
    // 合計
    .AddText("總計：2,000", alignment: TextAlignment.Right, bold: true)
    
    // 簽名區
    .AddSignatureSection("採購人員", "核准人員", "收貨確認");

// 列印到指定印表機
await _formattedPrintService.PrintByReportIdAsync(document, "PO001", copies: 1);

// 或渲染為圖片預覽
var images = _formattedPrintService.RenderToImages(document);
```

### 介面定義

```csharp
public interface IFormattedPrintService
{
    // 列印到指定印表機
    ServiceResult Print(FormattedDocument document, string printerName, int copies = 1);
    
    // 使用報表配置列印
    Task<ServiceResult> PrintByReportIdAsync(FormattedDocument document, string reportId, int copies = 1);
    
    // 渲染為圖片（用於預覽，使用預設 A4 尺寸）
    List<byte[]> RenderToImages(FormattedDocument document, int pageWidth = 794, int pageHeight = 1123, int dpi = 96);
    
    // 渲染為圖片（用於預覽，根據紙張設定計算尺寸）
    List<byte[]> RenderToImages(FormattedDocument document, PaperSetting paperSetting, int dpi = 96);
    
    // 檢查是否支援
    bool IsSupported();
}
```

---

## Excel 匯出服務（ExcelExportService）

位置：`Services/Reports/ExcelExportService.cs`

使用 **ClosedXML** 套件將 FormattedDocument 轉換為 Excel 檔案（.xlsx 格式）。

### 支援的元素轉換

| FormattedDocument 元素 | Excel 對應 |
|------------------------|------------|
| `TextElement` | 儲存格 + 字型樣式（大小、粗體、對齊） |
| `TableElement` | Excel 表格（邊框、表頭背景色） |
| `KeyValueRowElement` | 鍵值對欄位 |
| `ThreeColumnHeaderElement` | 三欄合併儲存格 |
| `ReportHeaderBlockElement` | 多行標題區塊 |
| `TwoColumnSectionElement` | 左右並排區塊 |
| `LineElement` | 底線樣式 |
| `SpacingElement` | 空白列 |
| `SignatureSectionElement` | 簽名欄位 |
| `PageBreakElement` | Excel 分頁符號 |
| `ImageElement` | 暫不支援（跳過） |

### 介面定義

```csharp
public interface IExcelExportService
{
    /// <summary>
    /// 將 FormattedDocument 匯出為 Excel 檔案
    /// </summary>
    byte[] ExportToExcel(FormattedDocument document);

    /// <summary>
    /// 將 FormattedDocument 匯出為 Excel 檔案（非同步版本）
    /// </summary>
    Task<byte[]> ExportToExcelAsync(FormattedDocument document);

    /// <summary>
    /// 檢查服務是否可用
    /// </summary>
    bool IsSupported();
}
```

### 使用方式

```csharp
@inject IExcelExportService ExcelExportService
@inject IJSRuntime JSRuntime

// 匯出 Excel
var excelBytes = await ExcelExportService.ExportToExcelAsync(formattedDocument);

// 下載檔案（需搭配 file-download-helper.js）
var fileName = $"{formattedDocument.DocumentName}.xlsx";
var base64 = Convert.ToBase64String(excelBytes);
var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
await JSRuntime.InvokeAsync<bool>("downloadFileFromBase64", fileName, base64, contentType);
```

### 相依套件

```xml
<PackageReference Include="ClosedXML" Version="0.105.0" />
```

---

## 報表服務介面

以採購單為例：`Services/Reports/Interfaces/IPurchaseOrderReportService.cs`

```csharp
public interface IPurchaseOrderReportService
{
    /// <summary>生成報表文件</summary>
    Task<FormattedDocument> GenerateReportAsync(int purchaseOrderId);

    /// <summary>渲染為圖片（用於預覽，使用預設 A4 尺寸）</summary>
    Task<List<byte[]>> RenderToImagesAsync(int purchaseOrderId);

    /// <summary>渲染為圖片（用於預覽，根據紙張設定計算尺寸）</summary>
    Task<List<byte[]>> RenderToImagesAsync(int purchaseOrderId, PaperSetting paperSetting);

    /// <summary>直接列印</summary>
    Task<ServiceResult> DirectPrintAsync(int purchaseOrderId, string reportId, int copies = 1);

    /// <summary>批次列印</summary>
    Task<ServiceResult> DirectPrintBatchAsync(BatchPrintCriteria criteria, string reportId);
}
```

---

## 報表預覽 Modal

位置：`Components/Shared/Report/ReportPreviewModalComponent.razor`

### 功能按鈕

| 按鈕 | 說明 |
|------|------|
| **轉Excel** | 將報表轉換為 .xlsx 檔案並下載 |
| **列印** | 發送到選定的印表機列印 |
| **取消** | 關閉預覽視窗 |

### 參數說明

| 參數 | 類型 | 說明 |
|------|------|------|
| `IsVisible` | `bool` | Modal 是否顯示 |
| `Title` | `string` | Modal 標題 |
| `PreviewImages` | `List<byte[]>?` | 預覽圖片列表（PNG 格式） |
| `FormattedDocument` | `FormattedDocument?` | 格式化文件（列印用） |
| `ReportId` | `string` | 報表 ID（用於查詢印表機配置） |
| `DocumentName` | `string` | 列印文件名稱 |
| `OnPrintSuccess` | `EventCallback` | 列印成功事件 |
| `OnPrintFailure` | `EventCallback<string>` | 列印失敗事件 |
| `OnCancel` | `EventCallback` | 取消事件 |
| `OnPaperSettingChanged` | `EventCallback<PaperSetting>` | 紙張設定變更事件（用於重新渲染預覽） |

### 使用方式

```razor
@using ERPCore2.Services.Reports.Interfaces
@using ERPCore2.Models.Reports
@using ERPCore2.Data.Entities
@inject IPurchaseOrderReportService PurchaseOrderReportService
@inject IFormattedPrintService FormattedPrintService

@* 報表預覽 Modal *@
<ReportPreviewModalComponent IsVisible="@_showReportPreviewModal"
                             IsVisibleChanged="@((bool visible) => _showReportPreviewModal = visible)"
                             Title="採購單預覽"
                             PreviewImages="@_reportPreviewImages"
                             FormattedDocument="@_formattedDocument"
                             ReportId="PO001"
                             DocumentName="@_reportDocumentName"
                             OnPrintSuccess="@HandleReportPrintSuccess"
                             OnPaperSettingChanged="@HandlePaperSettingChanged"
                             OnCancel="@(() => _showReportPreviewModal = false)" />

@code {
    private bool _showReportPreviewModal = false;
    private List<byte[]>? _reportPreviewImages = null;
    private FormattedDocument? _formattedDocument = null;
    private string _reportDocumentName = string.Empty;
    private int _currentEntityId;

    private async Task OpenReportPreviewModal(int entityId, string entityCode)
    {
        _currentEntityId = entityId;
        
        // 生成報表文件
        _formattedDocument = await PurchaseOrderReportService.GenerateReportAsync(entityId);
        
        // 渲染為預覽圖片（使用預設紙張，Modal 開啟後會根據紙張設定重新渲染）
        _reportPreviewImages = await PurchaseOrderReportService.RenderToImagesAsync(entityId);
        
        _reportDocumentName = $"採購單-{entityCode}";
        _showReportPreviewModal = true;
        StateHasChanged();
    }
    
    /// <summary>
    /// 紙張設定變更時重新渲染預覽圖片
    /// </summary>
    private async Task HandlePaperSettingChanged(PaperSetting paperSetting)
    {
        if (_formattedDocument != null)
        {
            _reportPreviewImages = FormattedPrintService.RenderToImages(_formattedDocument, paperSetting);
            StateHasChanged();
        }
    }
}
```

### 列印流程

```
使用者點擊「列印」→ GenerateReportAsync() → RenderToImagesAsync() 
→ ReportPreviewModal 顯示圖片預覽 → 使用者可調整紙張/邊距 
→ OnPaperSettingChanged 重新渲染預覽 → FormattedPrintService.Print()
```

### Excel 匯出流程

```
使用者點擊「轉Excel」→ ExcelExportService.ExportToExcelAsync(FormattedDocument)
→ 生成 .xlsx byte[] → JSRuntime.InvokeAsync("downloadFileFromBase64") → 下載檔案
```

---

## 報表服務實作範本

以 `PurchaseOrderReportService.cs` 為範本：

```csharp
public class PurchaseOrderReportService : IPurchaseOrderReportService
{
    private readonly IPurchaseOrderService _purchaseOrderService;
    private readonly ISupplierService _supplierService;
    private readonly IProductService _productService;
    private readonly ICompanyService _companyService;
    private readonly IFormattedPrintService _formattedPrintService;

    public PurchaseOrderReportService(
        IPurchaseOrderService purchaseOrderService,
        ISupplierService supplierService,
        IProductService productService,
        ICompanyService companyService,
        IFormattedPrintService formattedPrintService)
    {
        _purchaseOrderService = purchaseOrderService;
        _supplierService = supplierService;
        _productService = productService;
        _companyService = companyService;
        _formattedPrintService = formattedPrintService;
    }

    public async Task<FormattedDocument> GenerateReportAsync(int purchaseOrderId)
    {
        // 載入資料
        var purchaseOrder = await _purchaseOrderService.GetByIdAsync(purchaseOrderId);
        if (purchaseOrder == null)
            throw new ArgumentException($"找不到採購單 ID: {purchaseOrderId}");

        var orderDetails = await _purchaseOrderService.GetOrderDetailsAsync(purchaseOrderId);
        var supplier = purchaseOrder.SupplierId > 0
            ? await _supplierService.GetByIdAsync(purchaseOrder.SupplierId)
            : null;
        var company = purchaseOrder.CompanyId > 0
            ? await _companyService.GetByIdAsync(purchaseOrder.CompanyId)
            : null;
        
        var allProducts = await _productService.GetAllAsync();
        var productDict = allProducts.ToDictionary(p => p.Id, p => p);

        // 建構 FormattedDocument
        return BuildFormattedDocument(purchaseOrder, orderDetails, supplier, company, productDict);
    }

    public async Task<List<byte[]>> RenderToImagesAsync(int purchaseOrderId)
    {
        var document = await GenerateReportAsync(purchaseOrderId);
        return _formattedPrintService.RenderToImages(document);
    }
    
    public async Task<List<byte[]>> RenderToImagesAsync(int purchaseOrderId, PaperSetting paperSetting)
    {
        var document = await GenerateReportAsync(purchaseOrderId);
        return _formattedPrintService.RenderToImages(document, paperSetting);
    }

    public async Task<ServiceResult> DirectPrintAsync(int purchaseOrderId, string reportId, int copies = 1)
    {
        var document = await GenerateReportAsync(purchaseOrderId);
        return await _formattedPrintService.PrintByReportIdAsync(document, reportId, copies);
    }

    private FormattedDocument BuildFormattedDocument(
        PurchaseOrder purchaseOrder,
        List<PurchaseOrderDetail> orderDetails,
        Supplier? supplier,
        Company? company,
        Dictionary<int, Product> productDict)
    {
        var doc = new FormattedDocument()
            .SetDocumentName($"採購單-{purchaseOrder.Code}")
            .SetMargins(1.5f, 1.5f, 1.5f, 1.5f);

        // 標題區
        doc.AddTitle(company?.CompanyName ?? "公司名稱", fontSize: 14, bold: true)
           .AddTitle("採 購 單", fontSize: 18, bold: true)
           .AddLine(LineStyle.Double, 2);

        // 基本資訊區
        doc.AddSpacing(10)
           .AddKeyValueRow(
               ("採購單號", purchaseOrder.Code ?? ""),
               ("採購日期", purchaseOrder.OrderDate.ToString("yyyy/MM/dd")))
           .AddLine(LineStyle.Dashed)
           .AddKeyValueRow(("廠商名稱", supplier?.CompanyName ?? ""));

        // 明細表格
        doc.AddTable(table =>
        {
            table.AddColumn("序號", 0.5f, TextAlignment.Center)
                 .AddColumn("品名", 3f, TextAlignment.Left)
                 .AddColumn("數量", 1f, TextAlignment.Right)
                 .AddColumn("單價", 1.2f, TextAlignment.Right)
                 .AddColumn("小計", 1.5f, TextAlignment.Right)
                 .ShowBorder(true)
                 .ShowHeaderBackground(true);

            int rowNum = 1;
            foreach (var detail in orderDetails)
            {
                table.AddRow(
                    rowNum.ToString(),
                    detail.ProductName ?? "",
                    detail.OrderQuantity.ToString("N0"),
                    detail.UnitPrice.ToString("N2"),
                    detail.SubtotalAmount.ToString("N2")
                );
                rowNum++;
            }
        });

        // 合計與簽名區
        doc.AddSpacing(10)
           .AddText($"總計：{purchaseOrder.PurchaseTotalAmountIncludingTax:N0}", 
                    fontSize: 12, alignment: TextAlignment.Right, bold: true)
           .AddSpacing(30)
           .AddSignatureSection("採購人員", "核准人員", "收貨確認");

        return doc;
    }
}
```

---

## 新增其他報表服務

複製 `PurchaseOrderReportService.cs` 並修改：

1. 將類別名稱改為對應的報表服務名稱（如 `SalesOrderReportService`）
2. 修改注入的服務（如 `ISalesOrderService`）
3. 修改 `BuildFormattedDocument` 方法，根據報表需求調整內容
4. 在 `ServiceRegistration.cs` 中註冊新服務
5. **在 `Data/Reports/ReportRegistry.cs` 中註冊報表定義**（重要！）

### 報表註冊表（ReportRegistry.cs）

新增報表服務時，**必須**在 `ReportRegistry.cs` 中新增對應的報表定義，否則系統無法正確識別報表：

```csharp
new ReportDefinition
{
    Id = "XX001",              // 報表識別碼（須唯一）
    Name = "報表名稱",
    Description = "報表描述說明",
    IconClass = "bi bi-xxx",   // Bootstrap Icons 類別
    Category = ReportCategory.Sales,  // 分類：Customer、Supplier、Sales、Purchase、Inventory、Financial
    RequiredPermission = "Entity.Read", // 權限要求
    ActionId = "PrintXxx",     // 動作識別碼
    SortOrder = 1,             // 排序順序
    IsEnabled = true           // 是否啟用
},
```

**報表 ID 命名規則：**
| 前綴 | 分類 | 範例 |
|------|------|------|
| AR | 客戶報表 | AR001、AR002 |
| AP | 廠商報表 | AP001、AP002 |
| PO | 採購報表 | PO001、PO002 |
| SO | 銷售報表 | SO001、SO002 |
| IV | 庫存報表 | IV001、IV002 |
| FN | 財務報表 | FN001、FN002 |

---

## 注意事項

1. **Windows 專屬**：`FormattedPrintService` 使用 `System.Drawing.Printing`，僅支援 Windows
2. **Excel 匯出跨平台**：`ExcelExportService` 使用 ClosedXML，支援所有平台
3. **預覽與列印一致**：`RenderToImages` 和 `Print` 使用相同的渲染邏輯
4. **SQL 配置印表機**：透過 `ReportPrintConfiguration` 資料表設定預設印表機
