# 報表列印改版計畫

## 建立日期
2026-03-13

---

## 📋 背景與原因

### 現況問題

原報表系統設計了兩種列印方式：

| 按鈕 | 機制 | 問題 |
|------|------|------|
| **列印** | `System.Drawing.Printing`（伺服器端，Windows GDI+） | 列印工作發送到**伺服器**連接的印表機，而非使用者的印表機 |
| **本機列印** | `browserPrintImages` JS（瀏覽器 `window.print()`） | JS 函數目前被 `/* */` 整段註解，實際上是壞的 |

### 根本原因

Blazor Server 是 Web 應用程式，伺服器與使用者電腦是兩台不同的機器。伺服器端列印（`System.Drawing.Printing`）只能存取**伺服器本機**安裝的印表機，在實際部署環境中伺服器通常不接任何印表機，導致這個功能實際上無法使用。

### 技術限制確認

Web 應用程式無法靜默列印到使用者本機印表機——這是瀏覽器的安全設計，`window.print()` 一定會出現對話框。若需要靜默列印，唯一的企業級方案是安裝 QZ Tray 等本地代理程式，但目前不在本次改版範圍內。

---

## 🎯 改版目標

1. **移除** 無法實際使用的伺服器端列印功能
2. **修復** 被錯誤註解的 `browserPrintImages` JS 函數
3. **保留** 紙張設定架構（渲染、瀏覽器列印、PDF 匯出均需要）
4. **移除** 印表機設定相關功能（對 Web 列印無意義）

---

## ✅ 不需要修改的範圍

以下架構**完全不受影響**：

- 所有篩選架構（`DynamicFilterTemplate`、`FilterTemplateRegistry`、所有 Criteria 類別）
- `GenericReportFilterModalComponent`
- 所有報表服務（`RenderToImages`、`GenerateReportAsync` 等）
- `FormattedPrintService.RenderToImages()` — 仍用於渲染預覽圖片與 PDF 匯出
- 紙張設定資料庫表格與服務（`PaperSetting`、`PaperSettingService`、`ReportPrintConfigurationService`）
- `PaperSettingEditModalComponent` — 紙張設定仍然必要
- PDF 匯出、Excel 匯出功能

---

## 🔧 改版項目

### 第一階段：核心列印機制修正

#### 1. `wwwroot/js/file-download-helper.js`

**問題**：`browserPrintImages` 函數（第 52–103 行）被 `/* */` 整段註解，導致「本機列印」按鈕點擊後 JS 報錯。

**修改**：取消註解，恢復函數正常運作。

```javascript
// 目前狀態（錯誤）：
/* window.browserPrintImages = function (...) { ... } */

// 修改後：
window.browserPrintImages = function (base64Images, title, widthCm, heightCm) { ... }
```

---

#### 2. `Components/Shared/Report/ReportPreviewModalComponent.razor`

##### 移除項目

| 項目 | 說明 |
|------|------|
| `@inject IPrinterConfigurationService PrinterConfigurationService` | 不再需要查詢印表機清單 |
| 「列印」按鈕（伺服器端） | `HandlePrint()` → `FormattedPrintService.Print()` |
| 印表機下拉選單 | `_selectedPrinterConfigurationId`、`_printerConfigurations` |
| 印表機警告文字 | `Report.SelectPrinterWarning` |
| `IsPrinting` 狀態與列印中 Spinner | 伺服器端列印才需要等待 |
| `HandlePrint()` 方法 | 整個移除 |
| `OnPrinterChanged()` 方法 | 整個移除 |
| 印表機相關 state 變數 | `_printerName`、`_printerConfigurationId`、`_selectedPrinterConfigurationId`、`_printerConfigurations` |

##### 修改項目

| 項目 | 修改內容 |
|------|----------|
| 「本機列印」按鈕 | 改名為「列印」，Variant 改為 Blue（主要操作） |
| `HandleBrowserPrint()` | **⚠️ 重要**：成功後需補上 `CloseModal()` + `OnPrintSuccess.InvokeAsync()`（詳見下方說明） |
| `LoadPrintConfigurations()` | 移除印表機載入邏輯，只保留紙張設定載入 |
| `ReportPrintConfigurationService` 呼叫 | 只讀取 `PaperSettingId`，忽略 `PrinterConfigurationId` |
| 份數設定 | 移除（瀏覽器列印由對話框控制份數） |

##### 保留項目

| 項目 | 說明 |
|------|------|
| 紙張設定下拉 | 控制渲染尺寸與 `@page { size }` |
| 邊距設定 | 已套用進渲染圖片，仍有意義 |
| 縮放控制 | 預覽用途 |
| `OnPrintSuccess` 參數 | 保留，改為「瀏覽器列印視窗成功開啟後觸發」（語義改變，行為一致） |
| `OnPrintFailure` 參數 | 保留，當 `browserPrintImages` 回傳 false 時觸發 |
| PDF 匯出、Excel 匯出 | 完全不動 |

##### ⚠️ `HandleBrowserPrint` Modal 關閉問題（關鍵）

目前 `HandleBrowserPrint()` 成功後**沒有**關閉 Modal 也沒有觸發 `OnPrintSuccess`，但 `HandlePrint()`（伺服器列印）有。這導致改版後點列印雖然開啟瀏覽器視窗，但所有 Modal 不會關閉。

呼叫鏈確認（透過 grep 驗證）：

```
ReportPreviewModal.OnPrintSuccess
  → GenericReportFilterModal.HandlePrintSuccess   → 關閉 FilterModal
    → MainLayout.HandleFilterPrintSuccess          → 清空 currentFilterReportId
    → Index頁面.HandleBatchPrintSuccess            → 關閉 showBatchPrintModal
  → GenericEditModal.HandleReportPrintSuccess     → 呼叫 GenericEditModal.OnPrintSuccess
```

**修改方式**：在 `HandleBrowserPrint()` 的成功分支加入：

```csharp
// 瀏覽器列印視窗成功開啟 → 觸發成功事件並關閉 Modal
if (OnPrintSuccess.HasDelegate)
    await OnPrintSuccess.InvokeAsync();
await CloseModal();
```

---

### 第二階段：修改印表機相關設定元件

> ⚠️ 此階段影響範圍較大，建議確認第一階段正常運作後再進行。

#### 2-1. `ReportPrintConfigurationEditModalComponent`（修改，不移除）

目前表單包含 `PrinterConfigurationId`（印表機）和 `PaperSettingId`（紙張）兩個欄位，改版後只需要紙張。

| 需修改項目 | 說明 |
|-----------|------|
| 移除 `PrinterConfigurationId` FormFieldDefinition | 從表單欄位移除 |
| 移除 `@inject IPrinterConfigurationService` | 不再載入印表機清單 |
| 移除 `availablePrinters`、`printerConfigurationOptions` 變數 | 相關資料集合 |
| 移除 `AutoCompleteConfig` 中印表機設定 | `AddField(nameof(...PrinterConfigurationId), ...)` |
| 移除 Section 中的 `rpc => rpc.PrinterConfigurationId` | 表單區段設定 |

> 資料庫的 `PrinterConfigurationId` 欄位**不需要** Migration，保留即可，只移除 UI 層的讀寫。

---

#### 2-2. `ReportPrintSettingAlertModalComponent`（修改，不移除）

此元件用於「批次設定未設定紙張的報表」，目前還包含印表機設定，需要清除印表機部分：

| 需修改項目 | 說明 |
|-----------|------|
| 移除 UI 中印表機批次填入下拉選單 | `batchPrinterId`、相關 `<select>` |
| 移除表格欄位中「印表機」欄 | `InteractiveColumnDefinition` for `PrinterConfigurationId` |
| 移除 `@inject IPrinterConfigurationService` | 不再載入印表機清單 |
| 修改 `GetReportsWithoutPrinterOrPaperSettingAsync` 呼叫語義 | 改為只查「未設定紙張」的報表 |
| 修改 `BatchUpdatePrinterAndPaperSettingsAsync` 呼叫 | 只傳 `PaperSettingId`，`printerConfigurationId` 傳 `null` |

---

#### 2-3. `IReportPrintConfigurationService` 相關方法（修改簽名）

| 方法 | 修改說明 |
|------|---------|
| `GetReportsWithoutPrinterOrPaperSettingAsync()` | 改名為 `GetReportsWithoutPaperSettingAsync()`，只查未設紙張 |
| `BatchUpdatePrinterAndPaperSettingsAsync(...)` | 改名為 `BatchUpdatePaperSettingsAsync(...)`，移除 `printerConfigurationId` 參數 |

---

#### 2-4. 完全移除的項目（完整清單）

透過全域搜尋確認，以下項目可完全移除：

**UI 元件**

| 項目 | 路徑 | 說明 |
|------|------|------|
| `PrinterConfigurationEditModalComponent` | `Components/Pages/Systems/` | 管理伺服器印表機 |
| `InstalledPrinterListModalComponent` | `Components/Pages/Systems/` | 列出伺服器安裝的印表機 |
| `PrinterConfigurationIndex.razor` | `Components/Pages/Systems/` | 印表機管理頁面 |
| `PrinterConfigurationFieldConfiguration.cs` | `Components/FieldConfiguration/` | 印表機 FieldConfiguration |

**Services**

| 項目 | 路徑 | 說明 |
|------|------|------|
| `IPrinterConfigurationService` / `PrinterConfigurationService` | `Services/Systems/` | 印表機設定 CRUD |
| `IPrinterTestService` / `PrinterTestService` | `Services/Systems/` | 測試印表機連線 |
| `IReportPrintService` / `ReportPrintService` | `Services/Reports/` | 另一個伺服器端列印服務（PuppeteerSharp 版本），無 Component 使用，已是死碼 |
| `IPlainTextPrintService` / `PlainTextPrintService` | `Services/Reports/` | 純文字列印服務，無 Component 使用，已是死碼 |
| `ReportPrintHelper.cs` | `Helpers/Common/` | 報表列印輔助類別，無 Component 使用，已是死碼 |

**設定與導覽**

| 項目 | 路徑 | 說明 |
|------|------|------|
| `NavigationConfig.cs` 印表機導覽項目 | `Data/Navigation/` | `Nav.PrinterConfigurations` 條目 |
| `QuickActionModalHost.razor` `NewPrinterConfiguration` 條目 | `Components/Shared/Dashboard/` | 快速操作入口 |
| `PermissionRegistry.PrinterConfiguration` 類別 | `Models/PermissionRegistry.cs` | 印表機權限類別與定義 |
| `ServiceRegistration.cs` 中三個 Service 的 `AddScoped` | `Data/ServiceRegistration.cs` | `IPrinterConfigurationService`、`IReportPrintService`、`IPlainTextPrintService` |

**資料庫 Migration（已確認執行）**

目前仍在測試中，確認執行完整 Migration：

1. 移除 `ReportPrintConfiguration.PrinterConfigurationId` 欄位（先移除 FK）
2. 移除 `PrinterConfigurations` 資料表
3. 移除 `ReportPrintConfiguration` Entity 中的 `PrinterConfigurationId` 屬性與導覽屬性
4. 移除 `AppDbContext` 中的 `PrinterConfigurations DbSet` 與相關 FK 設定
5. 執行 `dotnet ef migrations add RemovePrinterConfiguration` 並 `dotnet ef database update`

---

## 📊 改版前後對照

### 使用者操作流程

**改版前**：
```
開啟預覽 Modal → 選擇印表機 → 選擇紙張 → 按「列印」→ 靜默送印到伺服器印表機
（實際上伺服器沒有印表機，列印失敗）
```

**改版後**：
```
開啟預覽 Modal → 選擇紙張（系統自動帶入預設） → 按「列印」
→ 瀏覽器列印視窗開啟（紙張尺寸已預設）→ 使用者確認印表機 → 列印
```

### 按鈕區域變化

**改版前**（4 個操作按鈕）：
```
[匯出 Excel] [下載 PDF] [本機列印] [列印] [取消]
```

**改版後**（3 個操作按鈕）：
```
[匯出 Excel] [下載 PDF] [列印] [取消]
```

---

## ⚠️ 注意事項

1. **紙張設定仍然重要**：即使改為瀏覽器列印，紙張設定仍控制報表的渲染尺寸與 `@page { size }` CSS，必須保留
2. **`HandleBrowserPrint` 必須補上 Modal 關閉邏輯**：否則改版後按列印不會關閉任何 Modal
3. **`OnPrintSuccess` 語義改變可接受**：改為「瀏覽器列印視窗成功開啟後觸發」，所有父元件的 callback 只是關閉 Modal，行為上一致
4. **份數設定**：瀏覽器列印的份數在瀏覽器對話框中設定，不需要在 Modal 中重複設定，可移除
5. **`FormattedPrintService` 仍保留**：`RenderToImages()` 方法仍被 PDF 匯出使用，只移除 `Print()` 的呼叫端，不移除服務本身
6. **第二階段的 Service 方法改名**：`GetReportsWithoutPrinterOrPaperSettingAsync` 和 `BatchUpdatePrinterAndPaperSettingsAsync` 需同步更新介面、實作、和呼叫端

---

## 📁 相關文件

- [README_報表系統總綱.md](README_報表系統總綱.md) - 報表系統架構
- [README_報表檔設計.md](README_報表檔設計.md) - 報表服務與列印服務
- [README_報表篩選架構設計.md](README_報表篩選架構設計.md) - 篩選架構（不受影響）
