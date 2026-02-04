# 報表檔案架構說明

## 更新日期
2026-02-03（統一純文字報表架構）

## 設計理念

本系統報表採用**純文字格式**，具有以下優點：
- **簡化列印流程**：使用 `System.Drawing.Printing` 直接列印，不需 HTML 轉換
- **減少依賴**：不需要 Chromium、PDF 閱讀器等額外軟體
- **高相容性**：適用於各種印表機，包括點陣式、標籤機、熱感應印表機
- **易於維護**：格式簡單直觀，使用等寬字型確保對齊

---

## 目錄結構

```
Services/Reports/
├── Interfaces/                      ← 介面定義
│   ├── IReportService.cs
│   ├── IPurchaseOrderReportService.cs
│   ├── IPurchaseReceivingReportService.cs
│   ├── IPurchaseReturnReportService.cs
│   ├── ISalesOrderReportService.cs
│   ├── ISalesReturnReportService.cs
│   ├── IQuotationReportService.cs
│   └── IProductBarcodeReportService.cs
├── Configuration/                   ← 列印配置服務
│   ├── IReportPrintConfigurationService.cs
│   └── ReportPrintConfigurationService.cs
├── PurchaseOrderReportService.cs    ← 報表服務實作（範本）
├── PurchaseReceivingReportService.cs
├── PurchaseReturnReportService.cs
├── SalesOrderReportService.cs
├── SalesReturnReportService.cs
├── QuotationReportService.cs
├── ProductBarcodeReportService.cs
├── PlainTextPrintService.cs         ← 共用列印服務
└── ReportPrintService.cs

Helpers/Common/
└── PlainTextFormatter.cs            ← 純文字格式化工具

Controllers/Reports/
├── BaseReportController.cs          ← 控制器基底類別
├── PurchaseReportController.cs      ← 採購報表控制器
└── SalesReportController.cs         ← 銷貨報表控制器

Data/
└── ReportRegistry.cs                ← 報表定義註冊表

Models/Reports/
├── ReportModels.cs                  ← 報表配置模型
├── ReportDefinition.cs              ← 報表定義
└── BatchPrintCriteria.cs            ← 批次列印條件
```

---

## 核心元件

### PlainTextFormatter（純文字格式化工具）

位置：`Helpers/Common/PlainTextFormatter.cs`

提供純文字格式化的靜態方法，處理中英文混排的寬度計算與對齊：

```csharp
public static class PlainTextFormatter
{
    // 常數
    public const int DefaultLineWidth = 80;  // 預設報表寬度
    public const string PageBreak = "\f";    // 分頁符號

    // 文字對齊
    public static string CenterText(string text, int width);
    public static string PadRight(string text, int width);   // 左對齊
    public static string PadLeft(string text, int width);    // 右對齊

    // 寬度計算（中文=2, 英文=1）
    public static int GetDisplayWidth(string text);
    public static bool IsWideChar(char c);

    // 文字截斷
    public static string TruncateText(string text, int maxWidth, bool ellipsis = false);

    // 分隔線
    public static string Separator(int width);       // ════════════
    public static string ThinSeparator(int width);   // ────────────
    public static string DottedSeparator(int width); // ............

    // 表格格式化
    public static string FormatTableRow(IEnumerable<(string Text, int Width, PlainTextAlignment Alignment)> columns);

    // 金額與數量格式化
    public static string FormatAmount(decimal amount);              // 千分位，無小數
    public static string FormatAmountWithDecimals(decimal amount);  // 千分位，2位小數
    public static string FormatQuantity(decimal quantity);          // 千分位，無小數

    // 日期格式化
    public static string FormatDate(DateTime date);        // yyyy/MM/dd
    public static string FormatDateTime(DateTime dateTime); // yyyy/MM/dd HH:mm

    // 報表區塊建構
    public static string BuildTitleSection(string companyName, string reportTitle, int width);
    public static string BuildSignatureSection(string[] labels, int width);
    public static string BuildTotalLine(string label, decimal amount, string suffix = "");
}

public enum PlainTextAlignment { Left, Right, Center }
```

### PlainTextPrintService（共用列印服務）

位置：`Services/Reports/PlainTextPrintService.cs`

封裝 Windows 列印功能，提供純文字直接列印：

```csharp
public interface IPlainTextPrintService
{
    /// <summary>執行純文字列印</summary>
    ServiceResult PrintText(string textContent, string printerName, string documentName, float fontSize = 10);

    /// <summary>使用報表配置列印（自動查詢印表機設定）</summary>
    Task<ServiceResult> PrintTextByReportIdAsync(string textContent, string reportId, string documentName);

    /// <summary>檢查是否支援直接列印</summary>
    bool IsDirectPrintSupported();
}
```

列印參數：
- 字型：Courier New（等寬字型）
- 預設字型大小：10pt
- 列印後等待時間：2000ms

---

## 報表服務介面

以採購單為例：`Services/Reports/Interfaces/IPurchaseOrderReportService.cs`

```csharp
public interface IPurchaseOrderReportService
{
    // 生成純文字報表
    Task<string> GeneratePlainTextReportAsync(int purchaseOrderId);
    Task<string> GenerateBatchPlainTextReportAsync(BatchPrintCriteria criteria);

    // 直接列印
    Task<ServiceResult> DirectPrintAsync(int purchaseOrderId, string printerName);
    Task<ServiceResult> DirectPrintByReportIdAsync(int purchaseOrderId, string reportId);
    Task<ServiceResult> DirectPrintBatchAsync(BatchPrintCriteria criteria, string reportId);
}
```

---

## 報表服務實作範本

以 `PurchaseOrderReportService.cs` 為範本：

### 1. 類別結構

```csharp
public class PurchaseOrderReportService : IPurchaseOrderReportService
{
    private readonly IPurchaseOrderService _purchaseOrderService;
    private readonly ISupplierService _supplierService;
    private readonly IProductService _productService;
    private readonly ICompanyService _companyService;
    private readonly ISystemParameterService _systemParameterService;
    private readonly IReportPrintConfigurationService _reportPrintConfigService;
    private readonly IPrinterConfigurationService _printerConfigService;
    private readonly IPlainTextPrintService _plainTextPrintService;
    private readonly ILogger<PurchaseOrderReportService>? _logger;

    public PurchaseOrderReportService(
        IPurchaseOrderService purchaseOrderService,
        ISupplierService supplierService,
        IProductService productService,
        ICompanyService companyService,
        ISystemParameterService systemParameterService,
        IReportPrintConfigurationService reportPrintConfigService,
        IPrinterConfigurationService printerConfigService,
        IPlainTextPrintService plainTextPrintService,
        ILogger<PurchaseOrderReportService>? logger = null)
    {
        _purchaseOrderService = purchaseOrderService;
        _supplierService = supplierService;
        _productService = productService;
        _companyService = companyService;
        _systemParameterService = systemParameterService;
        _reportPrintConfigService = reportPrintConfigService;
        _printerConfigService = printerConfigService;
        _plainTextPrintService = plainTextPrintService;
        _logger = logger;
    }
}
```

### 2. 生成純文字報表

```csharp
public async Task<string> GeneratePlainTextReportAsync(int purchaseOrderId)
{
    // 載入資料
    var purchaseOrder = await _purchaseOrderService.GetByIdAsync(purchaseOrderId);
    if (purchaseOrder == null)
        throw new ArgumentException($"找不到採購單 ID: {purchaseOrderId}");

    var orderDetails = await _purchaseOrderService.GetOrderDetailsAsync(purchaseOrderId);

    Supplier? supplier = purchaseOrder.SupplierId > 0
        ? await _supplierService.GetByIdAsync(purchaseOrder.SupplierId)
        : null;

    Company? company = purchaseOrder.CompanyId > 0
        ? await _companyService.GetByIdAsync(purchaseOrder.CompanyId)
        : null;

    var allProducts = await _productService.GetAllAsync();
    var productDict = allProducts.ToDictionary(p => p.Id, p => p);

    // 生成純文字內容
    return GeneratePlainTextContent(purchaseOrder, orderDetails, supplier, company, productDict);
}
```

### 3. 純文字內容生成（核心方法）

```csharp
private static string GeneratePlainTextContent(
    PurchaseOrder purchaseOrder,
    List<PurchaseOrderDetail> orderDetails,
    Supplier? supplier,
    Company? company,
    Dictionary<int, Product> productDict)
{
    var sb = new StringBuilder();
    const int lineWidth = PlainTextFormatter.DefaultLineWidth;

    // === 標題區 ===
    sb.Append(PlainTextFormatter.BuildTitleSection(
        company?.CompanyName ?? "公司名稱",
        "採 購 單",
        lineWidth));

    // === 基本資訊區 ===
    sb.AppendLine($"採購單號：{purchaseOrder.Code,-20} 採購日期：{PlainTextFormatter.FormatDate(purchaseOrder.OrderDate)}");
    sb.AppendLine($"交貨日期：{PlainTextFormatter.FormatDate(purchaseOrder.ExpectedDeliveryDate)}");
    sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));
    sb.AppendLine($"廠商名稱：{supplier?.CompanyName ?? ""}");
    sb.AppendLine($"聯 絡 人：{supplier?.ContactPerson ?? "",-20} 統一編號：{supplier?.TaxNumber ?? ""}");
    sb.AppendLine($"送貨地址：{company?.Address ?? ""}");
    sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));

    // === 明細表頭 ===
    sb.AppendLine(FormatTableRow("序號", "品名", "數量", "單位", "單價", "小計", "備註"));
    sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));

    // === 明細內容 ===
    int rowNum = 1;
    foreach (var detail in orderDetails)
    {
        var product = productDict.GetValueOrDefault(detail.ProductId);
        var productName = PlainTextFormatter.TruncateText(product?.Name ?? "", 28);
        var remarks = PlainTextFormatter.TruncateText(detail.Remarks ?? "", 8);

        sb.AppendLine(FormatTableRow(
            rowNum.ToString(),
            productName,
            PlainTextFormatter.FormatQuantity(detail.OrderQuantity),
            "個",
            PlainTextFormatter.FormatAmountWithDecimals(detail.UnitPrice),
            PlainTextFormatter.FormatAmountWithDecimals(detail.SubtotalAmount),
            remarks
        ));
        rowNum++;
    }

    sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));

    // === 合計區 ===
    var taxMethodText = purchaseOrder.TaxCalculationMethod switch
    {
        TaxCalculationMethod.TaxExclusive => "外加稅",
        TaxCalculationMethod.TaxInclusive => "內含稅",
        TaxCalculationMethod.NoTax => "免稅",
        _ => ""
    };

    sb.AppendLine(PlainTextFormatter.BuildTotalLine("小　計", purchaseOrder.TotalAmount));
    sb.AppendLine(PlainTextFormatter.BuildTotalLine("稅　額", purchaseOrder.PurchaseTaxAmount, taxMethodText));
    sb.AppendLine(PlainTextFormatter.BuildTotalLine("總　計", purchaseOrder.PurchaseTotalAmountIncludingTax));

    // === 備註 ===
    if (!string.IsNullOrWhiteSpace(purchaseOrder.Remarks))
    {
        sb.AppendLine(PlainTextFormatter.ThinSeparator(lineWidth));
        sb.AppendLine($"備註：{purchaseOrder.Remarks}");
    }

    sb.AppendLine(PlainTextFormatter.Separator(lineWidth));

    // === 簽名區 ===
    sb.Append(PlainTextFormatter.BuildSignatureSection(
        new[] { "採購人員", "核准人員", "收貨確認" },
        lineWidth));

    return sb.ToString();
}
```

### 4. 表格行格式化（各報表自訂）

```csharp
/// <summary>
/// 格式化表格行 - 採購單專用格式
/// 欄位配置：序號(4) | 品名(30) | 數量(8) | 單位(6) | 單價(10) | 小計(12) | 備註(10)
/// </summary>
private static string FormatTableRow(string col1, string col2, string col3, string col4, string col5, string col6, string col7)
{
    return PlainTextFormatter.FormatTableRow(new (string, int, PlainTextAlignment)[]
    {
        (col1, 4, PlainTextAlignment.Left),
        (col2, 30, PlainTextAlignment.Left),
        (col3, 8, PlainTextAlignment.Right),
        (col4, 6, PlainTextAlignment.Left),
        (col5, 10, PlainTextAlignment.Right),
        (col6, 12, PlainTextAlignment.Right),
        (col7, 10, PlainTextAlignment.Left)
    });
}
```

### 5. 直接列印方法

```csharp
[SupportedOSPlatform("windows6.1")]
public async Task<ServiceResult> DirectPrintAsync(int purchaseOrderId, string printerName)
{
    try
    {
        var textContent = await GeneratePlainTextReportAsync(purchaseOrderId);

        _logger?.LogInformation("開始直接列印採購單 {OrderId}，印表機：{PrinterName}", purchaseOrderId, printerName);

        // 使用共用列印服務
        var printResult = _plainTextPrintService.PrintText(textContent, printerName, $"採購單-{purchaseOrderId}");

        if (printResult.IsSuccess)
            _logger?.LogInformation("採購單 {OrderId} 列印完成", purchaseOrderId);

        return printResult;
    }
    catch (Exception ex)
    {
        _logger?.LogError(ex, "直接列印採購單 {OrderId} 時發生錯誤", purchaseOrderId);
        return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
    }
}

[SupportedOSPlatform("windows6.1")]
public async Task<ServiceResult> DirectPrintByReportIdAsync(int purchaseOrderId, string reportId)
{
    try
    {
        var textContent = await GeneratePlainTextReportAsync(purchaseOrderId);

        _logger?.LogInformation("開始列印採購單 {OrderId}，使用配置：{ReportId}", purchaseOrderId, reportId);

        // 使用共用列印服務（自動查詢印表機配置）
        return await _plainTextPrintService.PrintTextByReportIdAsync(textContent, reportId, $"採購單-{purchaseOrderId}");
    }
    catch (Exception ex)
    {
        _logger?.LogError(ex, "使用配置列印採購單 {OrderId} 時發生錯誤", purchaseOrderId, reportId);
        return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
    }
}
```

---

## 純文字報表輸出範例

```
================================================================================
                            公司名稱有限公司
                              採 購 單
================================================================================
採購單號：PO-20260203-001      採購日期：2026/02/03
交貨日期：2026/02/10
--------------------------------------------------------------------------------
廠商名稱：測試廠商股份有限公司
聯 絡 人：張先生              統一編號：12345678
送貨地址：台北市信義區xxx路xxx號
--------------------------------------------------------------------------------
序號 品名                           數量     單位     單價       小計     備註
--------------------------------------------------------------------------------
1    測試商品A                        10     個     100.00    1,000.00
2    測試商品B                         5     個     200.00    1,000.00
3    這是一個很長的商品名稱會被..       3     個     500.00    1,500.00
--------------------------------------------------------------------------------
                                                  小　計：       3,500
                                                  稅　額：         175 (外加稅)
                                                  總　計：       3,675
================================================================================

採購人員：________________    核准人員：________________    收貨確認：________________

================================================================================
```

---

## 前端組件呼叫方式

以 `PurchaseOrderEditModalComponent.razor` 為範本：

### 1. 注入服務

```razor
@using ERPCore2.Services.Reports
@inject IPurchaseOrderReportService PurchaseOrderReportService
@inject IPrinterConfigurationService PrinterConfigurationService
@inject IReportPrintConfigurationService ReportPrintConfigurationService
```

### 2. 列印按鈕處理

```csharp
/// <summary>
/// 處理列印按鈕點擊
/// </summary>
private async Task HandlePrint()
{
    try
    {
        // 檢查是否已儲存
        if (!canPrint)
        {
            await NotificationService.ShowWarningAsync("請先儲存採購單");
            return;
        }

        // 檢查 Entity 是否已載入
        if (editModalComponent?.Entity == null)
        {
            await NotificationService.ShowWarningAsync("無法列印：採購單資料不存在");
            return;
        }

        // 檢查審核狀態（如有需要）
        if (isApprovalEnabled && !editModalComponent.Entity.IsApproved)
        {
            await NotificationService.ShowWarningAsync("此採購單尚未審核通過，無法列印");
            return;
        }

        // 執行列印
        await HandleDirectPrint(null);
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync("列印處理時發生錯誤");
    }
}

/// <summary>
/// 執行直接列印
/// </summary>
private async Task HandleDirectPrint(ReportPrintConfiguration? printConfig)
{
    try
    {
        if (editModalComponent?.Entity == null)
        {
            await NotificationService.ShowWarningAsync("無法列印：採購單資料不存在");
            return;
        }

        // 取得印表機名稱
        string? printerName = null;

        // 方式 1：從傳入的列印配置取得
        if (printConfig?.PrinterConfigurationId.HasValue == true)
        {
            var printerConfig = await PrinterConfigurationService.GetByIdAsync(printConfig.PrinterConfigurationId.Value);
            printerName = printerConfig?.Name;
        }

        // 方式 2：從報表配置取得（使用預設報表 ID）
        if (string.IsNullOrEmpty(printerName))
        {
            var reportConfig = await ReportPrintConfigurationService.GetByReportIdAsync("PO001");
            if (reportConfig?.PrinterConfigurationId.HasValue == true)
            {
                var printerConfig = await PrinterConfigurationService.GetByIdAsync(reportConfig.PrinterConfigurationId.Value);
                printerName = printerConfig?.Name;
            }
        }

        if (string.IsNullOrEmpty(printerName))
        {
            await NotificationService.ShowWarningAsync("請先設定採購單的列印印表機");
            return;
        }

        // 直接呼叫服務列印（不經過 API）
        var result = await PurchaseOrderReportService.DirectPrintAsync(
            editModalComponent.Entity.Id,
            printerName
        );

        if (result.IsSuccess)
        {
            await NotificationService.ShowSuccessAsync($"已送出列印到印表機：{printerName}");
        }
        else
        {
            await NotificationService.ShowErrorAsync($"列印失敗：{result.ErrorMessage}");
        }
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync("列印執行時發生錯誤");
    }
}
```

---

## API 控制器

位置：`Controllers/Reports/PurchaseReportController.cs`

```csharp
[Route("api/purchase-report")]
[ApiController]
public class PurchaseReportController : BaseReportController
{
    private readonly IPurchaseOrderReportService _purchaseOrderReportService;

    /// <summary>
    /// 取得採購單純文字報表（預覽用）
    /// </summary>
    [HttpGet("order/{id}")]
    public async Task<IActionResult> GetPurchaseOrderReport(int id)
    {
        try
        {
            var plainText = await _purchaseOrderReportService.GeneratePlainTextReportAsync(id);
            var html = WrapPlainTextAsHtml(plainText, $"採購單報表 - {id}");
            return Content(html, "text/html; charset=utf-8");
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 採購單直接列印
    /// </summary>
    [HttpPost("order/{id}/direct")]
    public async Task<IActionResult> DirectPrintPurchaseOrder(int id, [FromQuery] string reportId = "PO001")
    {
        try
        {
            var result = await _purchaseOrderReportService.DirectPrintByReportIdAsync(id, reportId);

            if (result.IsSuccess)
                return Ok(new { success = true, message = "列印成功" });
            else
                return BadRequest(new { success = false, message = result.ErrorMessage });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "列印時發生錯誤", detail = ex.Message });
        }
    }
}
```

---

## 報表列印配置

### ReportRegistry（報表定義）

位置：`Data/ReportRegistry.cs`

```csharp
public static class ReportRegistry
{
    public static List<ReportDefinition> GetAllReports()
    {
        return new List<ReportDefinition>
        {
            new ReportDefinition
            {
                Id = "PO001",
                Name = "採購單",
                Description = "採購單列印報表",
                Category = ReportCategory.Purchase,
                RequiredPermission = "PurchaseOrder.Read",
                IsEnabled = true
            },
            // ... 其他報表定義
        };
    }
}
```

### ReportPrintConfiguration（資料表）

使用者可在「報表列印配置」頁面設定每個報表的印表機和紙張：

| 欄位 | 說明 |
|------|------|
| ReportId | 報表識別碼（對應 ReportRegistry.Id） |
| ReportName | 報表名稱 |
| PrinterConfigurationId | 印表機設定 FK |
| PaperSettingId | 紙張設定 FK |

### 自動建立配置

`ReportPrintConfigurationSeeder` 在應用程式啟動時：
1. 讀取 `ReportRegistry.GetAllReports()`
2. 自動建立尚未存在的 `ReportPrintConfiguration` 記錄
3. 已存在的配置不會被覆蓋

---

## 新增報表步驟

### 步驟 1：定義報表

在 `Data/ReportRegistry.cs` 新增：

```csharp
new ReportDefinition
{
    Id = "XX001",
    Name = "新報表名稱",
    Description = "報表說明",
    Category = ReportCategory.Sales,
    RequiredPermission = "Entity.Read",
    IsEnabled = true
}
```

### 步驟 2：建立介面

在 `Services/Reports/Interfaces/` 新增 `INewReportService.cs`：

```csharp
public interface INewReportService
{
    Task<string> GeneratePlainTextReportAsync(int entityId);
    Task<string> GenerateBatchPlainTextReportAsync(BatchPrintCriteria criteria);
    Task<ServiceResult> DirectPrintAsync(int entityId, string printerName);
    Task<ServiceResult> DirectPrintByReportIdAsync(int entityId, string reportId);
    Task<ServiceResult> DirectPrintBatchAsync(BatchPrintCriteria criteria, string reportId);
}
```

### 步驟 3：建立實作

在 `Services/Reports/` 新增 `NewReportService.cs`，參考 `PurchaseOrderReportService` 的結構。

### 步驟 4：註冊服務

在 `Data/ServiceRegistration.cs` 新增：

```csharp
services.AddScoped<ERPCore2.Services.Reports.Interfaces.INewReportService, NewReportService>();
```

### 步驟 5：建立控制器端點

在適當的控制器新增端點，或建立新控制器繼承 `BaseReportController`。

### 步驟 6：重新啟動應用程式

Seeder 會自動在「報表列印配置」頁面建立新報表的配置記錄。

---

## 平台限制

| 項目 | 說明 |
|------|------|
| 直接列印功能 | 僅支援 Windows 平台（使用 System.Drawing.Printing） |
| 服務註冊 | 使用 `OperatingSystem.IsWindowsVersionAtLeast(6, 1)` 檢查 |
| 屬性標記 | 使用 `[SupportedOSPlatform("windows6.1")]` 標記方法 |

---

## 現有報表清單

| 報表 ID | 報表名稱 | 服務類別 | 狀態 |
|---------|---------|---------|------|
| PO001 | 採購單 | PurchaseOrderReportService | ✅ 已實作 |
| PR001 | 進貨單 | PurchaseReceivingReportService | ✅ 已實作 |
| PT001 | 進貨退出單 | PurchaseReturnReportService | ✅ 已實作 |
| SO001 | 銷貨單 | SalesOrderReportService | ✅ 已實作 |
| SR001 | 銷貨退回單 | SalesReturnReportService | ✅ 已實作 |
| QT001 | 報價單 | QuotationReportService | ✅ 已實作 |
