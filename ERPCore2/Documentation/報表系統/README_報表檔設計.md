# 報表檔案架構說明

## 更新日期
2026-03-14

---

## 設計理念

本系統報表採用**統一格式化模式**，以 `FormattedDocument` Fluent API 建構報表結構：

- **表格框線**：支援實線、虛線、雙線等樣式
- **圖片嵌入**：支援 JPG、PNG、BMP 等格式
- **精確排版**：自動計算位置，支援多種對齊方式
- **自動換頁**：內容超過頁面時自動分頁
- **列印**：`FormattedHtmlRenderer` 將文件轉為含 `@page { size }` 的 HTML，由瀏覽器原生排版（跨平台）
- **預覽**：`FormattedPrintService` 使用 GDI+（Windows 專屬）渲染為 PNG 圖片
- **PDF 匯出**：GDI+ 150 DPI 重新渲染後由 PuppeteerSharp 合併為 PDF
- **Excel 匯出**：ClosedXML 轉換，跨平台

---

## 目錄結構

```
Helpers/
└── BatchReportHelper.cs                    # 批次報表產生 Helper

Services/Reports/
├── Interfaces/
│   ├── IFormattedPrintService.cs           # GDI+ 渲染介面（預覽 / PDF）
│   ├── IExcelExportService.cs              # Excel 匯出介面
│   ├── IPdfExportService.cs                # PDF 匯出介面
│   ├── IEntityReportService.cs             # 報表服務泛型介面 + BatchPreviewResult（含 Documents）
│   └── I[Entity]ReportService.cs           # 各報表服務介面
├── FormattedPrintService.cs                # GDI+ 渲染實作（預覽 96 DPI / PDF 150 DPI）
├── FormattedHtmlRenderer.cs               # HTML 列印渲染（跨平台靜態類別）
├── ExcelExportService.cs                   # Excel 匯出服務實作
├── PdfExportService.cs                     # PDF 匯出服務實作（PuppeteerSharp）
└── [Entity]ReportService.cs               # 各報表服務實作

Models/Reports/
├── ReportIds.cs                            # 報表 ID 常數（唯一來源）
├── BatchPrintCriteria.cs                   # 批次列印篩選條件（含 PaperSetting）
├── FormattedDocument.cs                    # 格式化報表文件模型（含 MergeFrom 方法）
├── TableDefinition.cs                      # 表格定義
├── FilterCriteria/                         # 篩選條件 DTO
│   ├── IReportFilterCriteria.cs
│   └── PurchaseOrderBatchPrintCriteria.cs
└── FilterTemplates/                        # 篩選模板配置
    ├── ReportFilterConfig.cs               # 篩選配置模型
    └── FilterTemplateRegistry.cs           # 模板註冊表（集中管理所有配置）

Components/Shared/Report/
├── ReportPreviewModalComponent.razor       # 報表預覽 Modal（列印/PDF/Excel）
├── GenericReportFilterModalComponent.razor # 通用篩選 Modal（呼叫 RenderBatchToImagesAsync）
├── FilterTemplateInitializer.cs            # 篩選模板初始化
└── FilterTemplates/                        # 篩選模板組件
    ├── DynamicFilterTemplate.razor         # 通用動態篩選（所有報表共用）
    └── ProductBarcodeBatchFilterTemplate.razor  # 條碼列印專用（唯一例外）
```

---

## 報表列印方式

系統提供兩種報表列印入口：

### 方式一：EditModal 內建列印（單筆）

適用於：編輯畫面中列印單一單據

```razor
<GenericEditModalComponent TEntity="PurchaseOrder" 
                          TService="IPurchaseOrderService"
                          ShowPrintButton="true"
                          ReportService="@PurchaseOrderReportService"
                          ReportId="PO001"
                          ReportPreviewTitle="採購單預覽"
                          GetReportDocumentName="@(e => $"採購單-{e.Code}")" />
```

### 方式二：報表中心篩選列印（批次）

適用於：從報表中心選擇條件後批次列印

流程：報表中心 → 篩選 Modal → 預覽 → 列印

參考 [README_報表篩選架構設計.md](README_報表篩選架構設計.md)

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
    .SetMargins(1.5f, 1.5f, 1.5f, 1.5f)
    
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
        
        table.AddRow("1", "測試品項A", "10", "100.00", "1,000.00");
    })
    
    // 簽名區
    .AddSignatureSection("採購人員", "核准人員", "收貨確認");

// 渲染為圖片（預覽 / PDF 用）
var images = _formattedPrintService.RenderToImages(document);

// HTML 列印（由 ReportPreviewModalComponent 呼叫）
var html = FormattedHtmlRenderer.RenderBatchToHtml(new List<FormattedDocument> { document }, paperSetting);
```

### 介面定義

```csharp
public interface IFormattedPrintService
{
    // 渲染為 PNG 圖片陣列（預覽 96 DPI / PDF 150 DPI）
    List<byte[]> RenderToImages(FormattedDocument document, int pageWidth = 794, int pageHeight = 1123, int dpi = 96);
    List<byte[]> RenderToImages(FormattedDocument document, PaperSetting paperSetting, int dpi = 96);

    // 已停用（伺服器端列印已移除）
    Task<ServiceResult> PrintByReportIdAsync(FormattedDocument document, string reportId, int copies = 1);

    bool IsSupported();
}
```

### FormattedHtmlRenderer（HTML 列印，跨平台）

```csharp
// 靜態類別，無需注入
public static class FormattedHtmlRenderer
{
    // 批次文件 → HTML（每份文件間 page-break-after: always）
    public static string RenderBatchToHtml(IEnumerable<FormattedDocument> documents, PaperSetting? paper = null);

    // 單一文件 → HTML
    public static string RenderToHtml(FormattedDocument document, PaperSetting? paper = null);
}
```

> **`@page { size }` 注意**：自定義尺寸只能寫 `size: Xcm Ycm`，**不可**在長度後加 `landscape/portrait`（那是預定義紙張名稱的語法）。方向由寬高大小決定。

---

## Excel 匯出服務（ExcelExportService）

位置：`Services/Reports/ExcelExportService.cs`

使用 **ClosedXML** 套件將 FormattedDocument 轉換為 Excel 檔案。

### 支援的元素轉換

| FormattedDocument 元素 | Excel 對應 |
|------------------------|------------|
| `TextElement` | 儲存格 + 字型樣式 |
| `TableElement` | Excel 表格（邊框、表頭背景色） |
| `KeyValueRowElement` | 鍵值對欄位 |
| `LineElement` | 底線樣式 |
| `SpacingElement` | 空白列 |
| `SignatureSectionElement` | 簽名欄位 |
| `PageBreakElement` | Excel 分頁符號 |

### 相依套件

```xml
<PackageReference Include="ClosedXML" Version="0.105.0" />
```

---

## PDF 匯出服務（PdfExportService）

位置：`Services/Reports/PdfExportService.cs`

使用 **PuppeteerSharp** 將高解析度預覽圖片（150 DPI）組合為 PDF 檔案，再觸發瀏覽器下載。

### 流程

1. `ReportPreviewModalComponent` 呼叫 `FormattedPrintService.RenderToImages(document, paper, dpi: 150)` 重新渲染高品質圖片
2. 呼叫 `PdfExportService.ExportToPdfAsync(images, widthCm, heightCm)` 產生 PDF bytes
3. 透過 `downloadFileFromBase64` JS 函式觸發瀏覽器下載

### 介面定義

```csharp
public interface IPdfExportService
{
    Task<byte[]> ExportToPdfAsync(List<byte[]> pageImages, double widthCm, double heightCm);
}
```

---

## ReportPreviewModalComponent 匯出行為

位置：`Components/Shared/Report/ReportPreviewModalComponent.razor`

預覽 Modal 的「下載 PDF」與「匯出 Excel」按鈕均使用**進度條 Overlay**提供視覺回饋，取代舊版的全域 Loading 遮罩。

### 進度條設計

進度條以半透明白色 Overlay 覆蓋整個 `.report-modal-layout`，中央顯示白色卡片：

```
┌─────────────────────────────────────┐
│  正在渲染頁面...                      │
│  ████████████░░░░░░░░  60%          │  ← 橘色（PDF）/ 綠色（Excel）
└─────────────────────────────────────┘
```

### PDF 下載進度階段

| 階段 | 進度 | 訊息 |
|------|------|------|
| 初始化 | 5% | 準備中... |
| 高解析度渲染前 | 15% | 正在渲染頁面... |
| PDF 產生中 | 60% | 正在產生 PDF... |
| 下載中 | 85% | 正在下載... |
| 完成（800ms 後自動關閉）| 100% | 下載完成！ |

> 進度條顏色：`bg-warning`（橘色，對應「下載PDF」按鈕顏色）

### Excel 下載進度階段

| 階段 | 進度 | 訊息 |
|------|------|------|
| 產生 Excel 中 | 10% | 正在產生 Excel... |
| 下載中 | 75% | 正在下載... |
| 完成（500ms 後自動關閉）| 100% | 下載完成！ |

> 進度條顏色：`bg-success`（綠色，對應「匯出Excel」按鈕顏色）

### 實作說明

進度條透過 `SetExportProgress(value, message)` helper 更新，每次呼叫都會 `StateHasChanged()` + `await Task.Delay(10)` 確保 UI 刷新後再執行下一步：

```csharp
private async Task SetExportProgress(int value, string message)
{
    _exportProgressValue = value;
    _exportProgressMessage = message;
    StateHasChanged();
    await Task.Delay(10);
}
```

> **注意**：`RenderToImages` 為同步方法，執行期間 UI 無法更新。因此 15% → 60% 的跳躍發生在同步渲染完成後。

---

## 報表服務介面

### IEntityReportService（泛型介面）

```csharp
public interface IEntityReportService<TEntity> where TEntity : BaseEntity
{
    Task<FormattedDocument> GenerateReportAsync(int entityId);
    Task<List<byte[]>> RenderToImagesAsync(int entityId);
    Task<List<byte[]>> RenderToImagesAsync(int entityId, PaperSetting paperSetting);
    Task<ServiceResult> DirectPrintAsync(int entityId, string reportId, int copies = 1);
    Task<ServiceResult> DirectPrintBatchAsync(BatchPrintCriteria criteria, string reportId);
    Task<BatchPreviewResult> RenderBatchToImagesAsync(BatchPrintCriteria criteria);
}
```

### BatchPreviewResult（批次預覽結果）

```csharp
public class BatchPreviewResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public List<byte[]> PreviewImages { get; set; }        // 所有頁面的預覽圖片（96 DPI）
    public FormattedDocument? MergedDocument { get; set; } // 合併文件（Excel / PDF 匯出用）
    public List<FormattedDocument> Documents { get; set; } // 獨立文件清單（HTML 列印用，每份保有正確表頭/頁尾）
    public int DocumentCount { get; set; }                 // 符合條件的單據數量
    public int TotalPages { get; set; }                    // 總頁數

    // documents 為選用參數；Type B 服務傳入 new List<FormattedDocument> { document }
    public static BatchPreviewResult Success(List<byte[]> images, FormattedDocument? document, int documentCount,
                                             List<FormattedDocument>? documents = null);
    public static BatchPreviewResult Failure(string errorMessage);
}
```

### 已實作的報表服務

| 服務 | 報表 ID | 類型 |
|------|---------|------|
| PurchaseOrderReportService | PO001 | A（BatchReportHelper） |
| PurchaseReceivingReportService | PO002 | A |
| PurchaseReturnReportService | PO003 | A |
| QuotationReportService | SO001 | A |
| SalesOrderReportService | SO002 | A |
| SalesDeliveryReportService | SO004 | A |
| SalesReturnReportService | SO005 | A |
| SetoffDocumentReportService | SO006 | A |
| CustomerRosterReportService | AR001 | B（單一合併文件） |
| SupplierRosterReportService | AP002 | B |
| EmployeeRosterReportService | HR001 | B |
| CustomerDetailReportService | AR002 | B |
| SupplierDetailReportService | AP004 | B |
| EmployeeDetailReportService | HR002 | B |
| ProductDetailReportService | PD001 | B |
| ProductListReportService | PD002 | B |
| ProductBarcodeReportService | PD003 | C（物件初始化語法） |
| AccountItemListReportService | FN005 | B |
| 其餘 21 個服務 | — | B |

> **類型說明**：A — 逐筆批次，BatchReportHelper 自動收集 `Documents`；B — 彙總型，`Success()` 需傳第 4 參數 `new List<FormattedDocument> { document }`；C — 物件初始化，需明確設定 `Documents` 屬性。

---

## BatchReportHelper（批次報表 Helper）

位置：`Helpers/BatchReportHelper.cs`

提供通用的批次預覽邏輯，避免各報表服務重複實作。支援紙張設定（`PaperSetting`）動態變更。

### 使用方式

```csharp
// 在報表服務中使用
public async Task<BatchPreviewResult> RenderBatchToImagesAsync(BatchPrintCriteria criteria)
{
    var entities = await _entityService.GetByBatchCriteriaAsync(criteria);

    return await BatchReportHelper.RenderBatchToImagesAsync(
        entities,
        (id, _) => GenerateReportAsync(id),  // 委派接受 (id, PaperSetting?)
        _formattedPrintService,
        "採購單",                        // 實體顯示名稱
        criteria.PaperSetting,            // 紙張設定（可為 null）
        criteria.GetSummary(),
        _logger);
}
```

### Helper 方法簽名（支援紙張設定）

```csharp
public static async Task<BatchPreviewResult> RenderBatchToImagesAsync<TEntity>(
    IEnumerable<TEntity> entities,
    Func<TEntity, int> getEntityId,
    Func<int, PaperSetting?, Task<FormattedDocument>> generateReportAsync,
    IFormattedPrintService formattedPrintService,
    string entityDisplayName,
    PaperSetting? paperSetting = null,
    string? criteriaMessage = null,
    ILogger? logger = null)
```

### 簡化版本（實體須繼承 BaseEntity）

```csharp
public static Task<BatchPreviewResult> RenderBatchToImagesAsync<TEntity>(
    IEnumerable<TEntity> entities,
    Func<int, PaperSetting?, Task<FormattedDocument>> generateReportAsync,
    IFormattedPrintService formattedPrintService,
    string entityDisplayName,
    PaperSetting? paperSetting = null,
    string? criteriaMessage = null,
    ILogger? logger = null) where TEntity : BaseEntity
```

> **向下相容**：保留舊版本的方法簽名（不帶 PaperSetting），內部自動包裝為新版本。

---

## FormattedDocument 合併功能

用於批次列印時將多個報表合併為單一文件：

```csharp
// 合併另一個文件的內容
mergedDocument.AddPageBreak();
mergedDocument.MergeFrom(document);
```

---

## GenericEditModalComponent 列印整合

位置：`Components/Shared/PageTemplate/GenericEditModalComponent.razor`

### 參數說明

| 參數 | 類型 | 說明 |
|------|------|------|
| `ShowPrintButton` | `bool` | 是否顯示列印按鈕 |
| `ReportService` | `IEntityReportService<TEntity>` | 報表服務 |
| `ReportId` | `string` | 報表 ID（用於查詢紙張設定） |
| `ReportPreviewTitle` | `string` | 報表預覽 Modal 標題 |
| `GetReportDocumentName` | `Func<TEntity, string>` | 產生列印文件名稱的函數 |
| `OnPrintSuccess` | `EventCallback` | 列印成功後的回調事件 |

### 已整合的組件

| 組件 | 報表 ID | 文件名稱 |
|------|---------|----------|
| QuotationEditModalComponent | SO001 | 報價單-{Code} |
| SalesOrderEditModalComponent | SO002 | 銷貨單-{Code} |
| SalesDeliveryEditModalComponent | SO004 | 出貨單-{Code} |
| SalesReturnEditModalComponent | SO005 | 銷貨退回單-{Code} |
| PurchaseOrderEditModalComponent | PO001 | 採購單-{Code} |
| PurchaseReceivingEditModalComponent | PO002 | 進貨單-{Code} |
| PurchaseReturnEditModalComponent | PO003 | 進貨退出單-{Code} |

---

## 新增報表服務步驟

1. **建立介面**（繼承 `IEntityReportService<T>`）
2. **建立服務實作**（參考 `PurchaseOrderReportService.cs`）
3. **在 ServiceRegistration.cs 註冊服務**
4. **在 ReportRegistry.cs 註冊報表定義**
5. **（可選）建立篩選模板**（參考 [README_報表篩選架構設計.md](README_報表篩選架構設計.md)）

### 報表 ID 命名規則

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

1. **GDI+ Windows 專屬**：`FormattedPrintService.RenderToImages()` 使用 `System.Drawing`，僅支援 Windows。用於螢幕預覽（96 DPI）與 PDF 匯出（150 DPI）。
2. **列印已改 HTML，跨平台**：`FormattedHtmlRenderer` 是純靜態類別，無 GDI+ 依賴，可在 Linux/macOS 執行。
3. **Excel 匯出跨平台**：`ExcelExportService` 使用 ClosedXML，支援所有平台。
4. **紙張設定**：透過 `ReportPrintConfiguration` 資料表為每個報表指定 `PaperSetting`，`ReportPreviewModalComponent` 在開啟時靜默載入並傳入 `FormattedHtmlRenderer`。
5. **PrintByReportIdAsync 已停用**：介面方法保留但標記為 obsolete，所有列印改走 `FormattedHtmlRenderer → browserPrintHtml`，不再使用此方法。

---

## 相關檔案

- [README_報表系統總綱.md](README_報表系統總綱.md) - 報表系統入口
- [README_報表篩選架構設計.md](README_報表篩選架構設計.md) - 篩選模板機制
- [README_報表中心設計.md](README_報表中心設計.md) - 報表中心入口
- [README_報表Index設計.md](README_報表Index設計.md) - Index 批次列印
