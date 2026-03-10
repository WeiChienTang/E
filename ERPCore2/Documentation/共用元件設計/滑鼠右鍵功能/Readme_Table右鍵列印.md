# Table 右鍵列印（Row Print）

## 相關檔案

| 檔案 | 說明 |
|------|------|
| `Components/Shared/Page/GenericIndexPageComponent.razor.cs` | `RowPrintService`、`RowPrintReportId`、`OnRowPrint` 參數；`HandleContextMenuPrintAsync` 方法 |
| `Components/Shared/Page/GenericIndexPageComponent.razor` | `ReportPreviewModalComponent` 渲染（由 `_showRowPrintModal` 控制） |
| `Services/Reports/Interfaces/IEntityReportService.cs` | 報表服務標準介面，所有支援列印的服務必須實作 |
| `Components/Shared/Report/ReportPreviewModalComponent.razor` | 報表預覽 Modal（顯示圖片、列印、匯出） |

---

## 功能說明

在 Index 頁面的表格中，對任一列按右鍵可直接開啟該筆資料的報表預覽，無需先進入 EditModal。

**原有流程：**
```
點擊 row → EditModal 開啟 → 點擊列印 → ReportPreviewModal
```

**右鍵列印流程：**
```
右鍵 row → 選單出現「列印」→ 自動生成預覽圖片 → ReportPreviewModal 直接開啟
```

---

## GenericIndexPageComponent 參數

| 參數 | 類型 | 預設 | 說明 |
|------|------|------|------|
| `RowPrintService` | `IEntityReportService<TEntity>?` | `null` | 報表服務實例；設定後自動在右鍵選單加入「列印」項目，由元件內部全權管理預覽 Modal |
| `RowPrintReportId` | `string` | `""` | 報表 ID（使用 `ReportIds.*` 常數），用於讀取列印機配置 |
| `OnRowPrint` | `EventCallback<TEntity>` | — | 自訂列印邏輯的 callback（escape hatch）；`RowPrintService` 未設定時才生效 |

### 優先順序

`RowPrintService` 優先於 `OnRowPrint`：
- `RowPrintService != null` → 由元件內部呼叫服務並開啟預覽 Modal
- `RowPrintService == null && OnRowPrint.HasDelegate` → 委派給父頁面 callback 自行處理

---

## 內部狀態（GenericIndexPageComponent）

| 欄位 | 類型 | 說明 |
|------|------|------|
| `_showRowPrintModal` | `bool` | 預覽 Modal 顯示狀態 |
| `_rowPrintImages` | `List<byte[]>?` | 預覽圖片（PNG，逐頁） |
| `_rowPrintDocument` | `FormattedDocument?` | 格式化文件（用於列印與匯出） |
| `_rowPrintTitle` | `string` | Modal 標題與文件名稱（來自 `GetEntityDisplayName`） |

---

## 內部方法（GenericIndexPageComponent）

### `HandleContextMenuPrintAsync(TEntity entity)`

由右鍵選單的「列印」項目觸發，執行順序：

1. 呼叫 `RowPrintService.RenderToImagesAsync(entity.Id)` 取得預覽圖片
2. 呼叫 `RowPrintService.GenerateReportAsync(entity.Id)` 取得格式化文件
3. 設定 `_showRowPrintModal = true`，開啟 `ReportPreviewModalComponent`
4. 若發生例外，透過 `NotificationService.ShowErrorAsync` 顯示錯誤通知

---

## 自動生成選單邏輯

`BuildEffectiveContextMenuItems` 在以下條件加入「列印」項目：

```
RowPrintService != null  OR  OnRowPrint.HasDelegate
```

選單項目順序（當三者同時存在時）：

```
編輯  →  列印  →  ─────  →  刪除
```

---

## 使用示範

以客戶頁面為例，套用右鍵列印只需：

**1. 在 Index 頁面注入服務**

```razor
@inject ICustomerDetailReportService CustomerDetailReportService
```

**2. 傳入兩個參數給 GenericIndexPageComponent**

```razor
<GenericIndexPageComponent TEntity="Customer"
    ...
    RowPrintService="@CustomerDetailReportService"
    RowPrintReportId="@ReportIds.CustomerDetail" />
```

其餘（狀態管理、handler、ReportPreviewModalComponent）均由 `GenericIndexPageComponent` 自動處理，頁面不需任何額外程式碼。

---

## 適用條件

- 報表服務必須實作 `IEntityReportService<TEntity>` 介面（`GenerateReportAsync` + `RenderToImagesAsync`）
- `RowPrintReportId` 必須使用 `ReportIds.*` 常數，不得直接寫死字串（規範同整個報表系統）
- `GetEntityDisplayName` 參數（GenericIndexPageComponent 已有）將作為預覽 Modal 的標題與文件名稱

---

## Localization Keys

| Key | 說明 |
|-----|------|
| `Button.Print` | 右鍵選單「列印」項目文字 |

---

## 已套用頁面

| Index 頁面 | 報表服務 | ReportId |
|-----------|---------|----------|
| `CustomerIndex.razor` | `ICustomerDetailReportService` | `ReportIds.CustomerDetail` |
| `SupplierIndex.razor` | `ISupplierDetailReportService` | `ReportIds.SupplierDetail` |
| `EmployeeIndex.razor` | `IEmployeeDetailReportService` | `ReportIds.EmployeeDetail` |
| `PurchaseOrderIndex.razor` | `IPurchaseOrderReportService` | `ReportIds.PurchaseOrder` |
| `PurchaseReceivingIndex.razor` | `IPurchaseReceivingReportService` | `ReportIds.PurchaseReceiving` |
| `PurchaseReturnIndex.razor` | `IPurchaseReturnReportService` | `ReportIds.PurchaseReturn` |
| `QuotationIndex.razor` | `IQuotationReportService` | `ReportIds.Quotation` |
| `SalesOrderIndex.razor` | `ISalesOrderReportService` | `ReportIds.SalesOrder` |
| `SalesDeliveryIndex.razor` | `ISalesDeliveryReportService` | `ReportIds.SalesDelivery` |
| `SalesReturnIndex.razor` | `ISalesReturnReportService` | `ReportIds.SalesReturn` |
| `ProductIndex.razor` | `IProductDetailReportService` | `ReportIds.ProductDetail` |
| `ProductCompositionIndex.razor` | `IBOMReportService` | `ReportIds.BOMReport` |
| `VehicleMaintenanceIndex.razor` | `IVehicleMaintenanceReportService` | `ReportIds.VehicleMaintenance` |
| `WasteRecordIndex.razor` | `IWasteRecordReportService` | `ReportIds.WasteRecord` |
| `StockTakingIndex.razor` | `IStockTakingDifferenceReportService` | `ReportIds.InventoryCount` |
| `AccountItemIndex.razor` | `IAccountItemListReportService` | `ReportIds.AccountItemList` |
| `InventoryStockIndex.razor` | `IInventoryStatusReportService` | `ReportIds.InventoryStatus` |
| `VehicleIndex.razor` | `IVehicleListReportService` | `ReportIds.VehicleList` |
