# 報表列印改版計畫

## 建立日期
2026-03-13　　最後更新：2026-03-14

---

## 📋 背景與原因

### 現況問題（原始）

原報表系統設計了兩種列印方式：

| 按鈕 | 機制 | 問題 |
|------|------|------|
| **列印** | `System.Drawing.Printing`（伺服器端，Windows GDI+） | 列印工作發送到**伺服器**連接的印表機，而非使用者的印表機 |
| **本機列印** | `browserPrintImages` JS（瀏覽器 `window.print()`） | 以 GDI+ 點陣圖渲染後傳給瀏覽器，DPI 不符導致紙張溢出 |

### 根本原因

Blazor Server 是 Web 應用程式，伺服器與使用者電腦是兩台不同的機器。伺服器端列印只能存取伺服器本機印表機，部署後無法使用。

點陣圖路徑的問題：`FormattedPrintService` 以 GDI+ 96 DPI 渲染成固定像素的 PNG 圖片，瀏覽器以公分指定 `@page { size }` 時兩者換算不一致，導致一頁內容被切成多頁（例如中一刀 14cm 高 vs A4 圖片 29.7cm = 3 頁）。

### 技術限制確認

Web 應用程式無法靜默列印到使用者本機印表機——這是瀏覽器的安全設計，`window.print()` 一定會出現對話框。若需要靜默列印，唯一的企業級方案是安裝 QZ Tray 等本地代理程式，但目前不在本次改版範圍內。

---

## 🎯 改版目標

1. **改為 HTML 列印**：`FormattedDocument → HTML 字串 → 瀏覽器原生渲染`，徹底解決 DPI 問題
2. **移除** 無法實際使用的伺服器端列印功能
3. **保留** 紙張設定架構（HTML `@page { size }` 仍需要正確公分）
4. **移除** 印表機設定相關功能（對 Web 列印無意義）

---

## ✅ 不需要修改的範圍

以下架構**完全不受影響**：

- 所有篩選架構（`DynamicFilterTemplate`、`FilterTemplateRegistry`、所有 Criteria 類別）
- 所有 `GenerateReportAsync` 方法（報表資料建構邏輯不變）
- `FormattedPrintService.RenderToImages()` — 仍用於 PDF 匯出
- 紙張設定資料庫表格與服務（`PaperSetting`、`PaperSettingService`、`ReportPrintConfigurationService`）
- `PaperSettingEditModalComponent` — 紙張設定仍然必要
- PDF 匯出、Excel 匯出功能

---

## 📐 新架構說明

### 列印路徑

```
使用者點「列印」
  → HandleBrowserPrint()
    → FormattedHtmlRenderer.RenderBatchToHtml(documents, paper)
      → 產生含 @page { size: Xcm Ycm } 的完整 HTML 字串
    → JS: browserPrintHtml(html)
      → 建立隱藏 iframe → 寫入 HTML → iframe.contentWindow.print()
    → 瀏覽器列印對話框（使用者選印表機 / 份數）
    → 成功後：OnPrintSuccess.InvokeAsync() + CloseModal()
```

### FormattedHtmlRenderer

路徑：`Services/Reports/FormattedHtmlRenderer.cs`

- `RenderToHtml(FormattedDocument, PaperSetting?)` — 單一文件
- `RenderBatchToHtml(IEnumerable<FormattedDocument>, PaperSetting?)` — 批次文件

每份文件獨立保有正確的 `HeaderElements`（表頭）與 `FooterElements`（頁尾），批次時每份之間以 `page-break-after: always` 分頁。

### BatchPreviewResult.Documents

```csharp
public List<FormattedDocument> Documents { get; set; } = new();
```

HTML 列印使用 `Documents`（每份文件獨立）；PDF/Excel 匯出使用 `MergedDocument`（合併文件）。

---

## 🔧 改版進度

### 第一階段：核心列印機制（進行中）

#### ✅ 已完成

| 項目 | 說明 |
|------|------|
| `FormattedHtmlRenderer.cs` | 新增靜態類別，`FormattedDocument → HTML` |
| `file-download-helper.js` | 新增 `browserPrintHtml(html)`、`previewHtmlInNewTab(html)` |
| `BatchPreviewResult.Documents` | 新增欄位；`BatchPreviewResult.Success()` 新增第 4 個選用參數 |
| `BatchReportHelper` | 內部新增 `allDocuments` 收集，傳入 `Success()` |
| `ReportPreviewModalComponent` | 簡化 UI（移除印表機選擇、份數設定）；`HandleBrowserPrint` 改為 HTML 優先，圖片列印為退路 |
| `ReportPreviewModalComponent.razor.css` | 重寫為簡化版（`.report-action-*`，移除舊兩欄面板樣式） |
| `GenericReportFilterModalComponent` | 移除 `OnPaperSettingChanged`；新增 `documentList` 欄位；傳 `Documents="@documentList"` |
| `GenericEditModalComponent` | 移除 `OnPaperSettingChanged` |
| **HTML 列印確認可用** | `@page { size: 24.10cm 14.00cm }` 正確生成，瀏覽器列印視窗成功開啟 |

#### ⏳ 待完成

---

**1-A：`ReportPreviewModalComponent.HandleBrowserPrint` 補上成功回呼**

目前 HTML 列印成功後未關閉 Modal、未觸發 `OnPrintSuccess`。需補上：

```csharp
if (OnPrintSuccess.HasDelegate)
    await OnPrintSuccess.InvokeAsync();
await CloseModal();
```

呼叫鏈：
```
ReportPreviewModal.OnPrintSuccess
  → GenericReportFilterModal.HandlePrintSuccess   → 關閉 FilterModal
    → MainLayout.HandleFilterPrintSuccess          → 清空 currentFilterReportId
    → Index頁面.HandleBatchPrintSuccess            → 關閉 showBatchPrintModal
  → GenericEditModal.HandleReportPrintSuccess     → 呼叫 GenericEditModal.OnPrintSuccess
```

---

**1-B：補齊各報表服務的 `Documents` 參數**

> 觀察到的現象：`[Print] Documents=-1` — 表示 `result.Documents` 為 null 或空，HTML 列印目前依賴 `FormattedDocument` fallback 運作。需明確填入 `Documents`，完成後移除 fallback。

報表服務分為三類：

**類型 A — 逐筆批次型（需確認）**

理論上 `BatchReportHelper.RenderBatchToImagesAsync` 已自動收集 `allDocuments`，但實際 log 顯示 `Documents=-1`。需確認 `BatchReportHelper` 的 `allDocuments` 是否正確傳入 `Success()`。

| 服務 | 說明 |
|------|------|
| `PurchaseOrderReportService` | 採購單 |
| `PurchaseReceivingReportService` | 採購收貨 |
| `PurchaseReturnReportService` | 採購退貨 |
| `SalesOrderReportService` | 銷售訂單 |
| `SalesDeliveryReportService` | 銷售出貨 |
| `SalesReturnReportService` | 銷售退貨 |
| `QuotationReportService` | 報價單 |
| `SetoffDocumentReportService` | 沖銷單 |

**類型 B — 彙總型（需修改，共 29 個服務、42 個呼叫點）**

將所有資料彙整進單一 `FormattedDocument`，呼叫 `Success()` 時需傳入第 4 個參數：

```csharp
return BatchPreviewResult.Success(images, document, count, new List<FormattedDocument> { document });
```

| 服務 | 說明 | 呼叫點數 |
|------|------|---------|
| `CustomerRosterReportService` | 客戶名冊 | 2 |
| `SupplierRosterReportService` | 供應商名冊 | 2 |
| `EmployeeRosterReportService` | 員工名冊 | 2 |
| `CustomerDetailReportService` | 客戶明細 | 2 |
| `SupplierDetailReportService` | 供應商明細 | 2 |
| `EmployeeDetailReportService` | 員工明細 | 2 |
| `ProductDetailReportService` | 產品明細 | 2 |
| `ProductListReportService` | 產品清單 | 2 |
| `CustomerComplaintReportService` | 客訴紀錄 | 2 |
| `WasteRecordReportService` | 廢棄物紀錄 | 2 |
| `VehicleListReportService` | 車輛清單 | 2 |
| `VehicleMaintenanceReportService` | 車輛保養 | 2 |
| `AccountItemListReportService` | 科目清單 | 2 |
| `BOMReportService` | BOM 表 | 2 |
| `TrialBalanceReportService` | 試算表 | 1 |
| `BalanceSheetReportService` | 資產負債表 | 1 |
| `IncomeStatementReportService` | 損益表 | 1 |
| `GeneralLedgerReportService` | 總分類帳 | 1 |
| `SubsidiaryLedgerReportService` | 明細分類帳 | 1 |
| `DetailAccountBalanceReportService` | 明細科目餘額 | 1 |
| `CustomerStatementReportService` | 客戶對帳單 | 1 |
| `SupplierStatementReportService` | 供應商對帳單 | 1 |
| `CustomerTransactionReportService` | 客戶交易紀錄 | 1 |
| `CustomerSalesAnalysisReportService` | 客戶銷售分析 | 1 |
| `InventoryStatusReportService` | 庫存狀況 | 1 |
| `ProductionScheduleReportService` | 生產排程 | 1 |
| `StockTakingDifferenceReportService` | 盤點差異 | 1 |
| `MaterialRequirementsReportService` | 物料需求 | 1 |
| `MaterialScrapReportService` | 物料報廢 | 1 |

**類型 C — 特殊型（條碼列印）**

`ProductBarcodeReportService` 使用物件初始化語法，需補上 `Documents` 屬性：

```csharp
return new BatchPreviewResult
{
    IsSuccess = true,
    PreviewImages = previewImages,
    MergedDocument = document,
    Documents = new List<FormattedDocument> { document },
    DocumentCount = products.Count,
    TotalPages = previewImages.Count
};
```

---

**1-C：移除 `HandleBrowserPrint` 的圖片列印退路（在 1-A 與 1-B 完成後）**

完成 1-A 與 1-B 後，`Documents` 皆已填入，下列區塊成為死碼，整段移除：

```csharp
// 移除以下區塊（ReportPreviewModalComponent.HandleBrowserPrint）：
if (PreviewImages == null || !PreviewImages.Any())
    return;
var (widthCm, heightCm) = GetActualPaperDimensions();
var base64 = PreviewImages.Select(img => Convert.ToBase64String(img)).ToArray();
var imgSuccess = await JSRuntime.InvokeAsync<bool>(
    "browserPrintImages", base64, DocumentName, widthCm, heightCm);
if (!imgSuccess)
    await NotificationService.ShowWarningAsync("列印初始化失敗，請稍後再試");
```

---

### 第二階段：移除印表機相關功能（待第一階段確認正常後執行）

> ⚠️ 此階段影響範圍較大，建議確認第一階段全部正常運作後再進行。

#### 2-1. `ReportPrintConfigurationEditModalComponent`（修改）

| 需移除項目 | 說明 |
|-----------|------|
| `PrinterConfigurationId` FormFieldDefinition | 從表單欄位移除 |
| `@inject IPrinterConfigurationService` | 不再載入印表機清單 |
| `availablePrinters`、`printerConfigurationOptions` 變數 | |
| `AutoCompleteConfig` 中印表機設定 | |
| Section 中的 `rpc => rpc.PrinterConfigurationId` | |

> 資料庫的 `PrinterConfigurationId` 欄位**不需要** Migration，保留即可，只移除 UI 層的讀寫。

#### 2-2. `ReportPrintSettingAlertModalComponent`（修改）

| 需修改項目 | 說明 |
|-----------|------|
| 移除印表機批次填入下拉選單 | `batchPrinterId`、相關 `<select>` |
| 移除表格欄位中「印表機」欄 | `InteractiveColumnDefinition` for `PrinterConfigurationId` |
| 移除 `@inject IPrinterConfigurationService` | |
| 修改查詢語義 | `GetReportsWithoutPrinterOrPaperSettingAsync` → 只查「未設定紙張」 |
| 修改批次更新呼叫 | `BatchUpdatePrinterAndPaperSettingsAsync` → 只傳 `PaperSettingId` |

#### 2-3. `IReportPrintConfigurationService` 方法改名

| 原方法名 | 新方法名 | 說明 |
|---------|---------|------|
| `GetReportsWithoutPrinterOrPaperSettingAsync()` | `GetReportsWithoutPaperSettingAsync()` | 只查未設紙張 |
| `BatchUpdatePrinterAndPaperSettingsAsync(...)` | `BatchUpdatePaperSettingsAsync(...)` | 移除 `printerConfigurationId` 參數 |

需同步更新：介面、實作類別、所有呼叫端。

#### 2-4. 完全移除的項目

**UI 元件**

| 項目 | 路徑 |
|------|------|
| `PrinterConfigurationEditModalComponent` | `Components/Pages/Systems/` |
| `InstalledPrinterListModalComponent` | `Components/Pages/Systems/` |
| `PrinterConfigurationIndex.razor` | `Components/Pages/Systems/` |
| `PrinterConfigurationFieldConfiguration.cs` | `Components/FieldConfiguration/` |

**Services**

| 項目 | 路徑 |
|------|------|
| `IPrinterConfigurationService` / `PrinterConfigurationService` | `Services/Systems/` |
| `IPrinterTestService` / `PrinterTestService` | `Services/Systems/` |
| `IReportPrintService` / `ReportPrintService` | `Services/Reports/`（PuppeteerSharp 版本，已是死碼） |
| `IPlainTextPrintService` / `PlainTextPrintService` | `Services/Reports/`（已是死碼） |
| `ReportPrintHelper.cs` | `Helpers/Common/`（已是死碼） |

**設定與導覽**

| 項目 | 路徑 |
|------|------|
| `NavigationConfig.cs` 印表機導覽項目 | `Nav.PrinterConfigurations` 條目 |
| `QuickActionModalHost.razor` `NewPrinterConfiguration` 條目 | |
| `PermissionRegistry.PrinterConfiguration` 類別 | `Models/PermissionRegistry.cs` |
| `ServiceRegistration.cs` 三個 Service 的 `AddScoped` | `IPrinterConfigurationService`、`IReportPrintService`、`IPlainTextPrintService` |

**資料庫 Migration**

1. 移除 `ReportPrintConfiguration.PrinterConfigurationId` 欄位（先移除 FK）
2. 移除 `PrinterConfigurations` 資料表
3. 移除 `ReportPrintConfiguration` Entity 中的 `PrinterConfigurationId` 屬性與導覽屬性
4. 移除 `AppDbContext` 中的 `PrinterConfigurations DbSet` 與相關 FK 設定
5. 執行 `dotnet ef migrations add RemovePrinterConfiguration` 並 `dotnet ef database update`

---

## 📊 改版後使用者操作流程

```
開啟篩選 Modal → 設定條件 → 按「預覽列印」
→ 開啟預覽 Modal → 選擇紙張（系統自動帶入預設）→ 按「列印」
→ 瀏覽器列印視窗開啟（紙張尺寸已預設）→ 使用者確認印表機 → 列印
→ Modal 自動關閉
```

按鈕區域（4 個）：
```
[匯出 Excel] [下載 PDF] [列印] [取消]
```

---

## ⚠️ 注意事項

1. **紙張設定仍然重要**：HTML `@page { size: Xcm Ycm }` 直接使用紙張公分，必須保留紙張設定
2. **`FormattedPrintService.RenderToImages()` 仍保留**：PDF 匯出仍需要，只移除伺服器端 `Print()` 的呼叫端
3. **第二階段 Service 方法改名**：需同步更新介面、實作、呼叫端，三處同步修改
4. **類型 A 服務的 `Documents` 需確認**：log 顯示 `Documents=-1`，需確認 `BatchReportHelper` 的 `allDocuments` 收集是否正確運作

---

## 📁 相關文件

- [README_報表系統總綱.md](README_報表系統總綱.md) - 報表系統架構
- [README_報表檔設計.md](README_報表檔設計.md) - 報表服務與列印服務
- [README_報表篩選架構設計.md](README_報表篩選架構設計.md) - 篩選架構（不受影響）
