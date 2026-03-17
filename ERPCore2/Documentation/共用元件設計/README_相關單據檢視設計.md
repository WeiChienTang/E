# 相關單據檢視設計

## 更新日期
2026-03-07

---

## 概述

`Components/Shared/RelatedDocument/` 提供「相關單據檢視」解決方案，用於顯示與特定項目關聯的單據清單（如入庫、退貨、沖款記錄等）。採用**組件化設計**，將顯示邏輯、配置與範本分離，便於擴充新的單據類型。

---

## 檔案結構

| 檔案 | 路徑 | 說明 |
|------|------|------|
| 主要 Modal | `Components/Shared/RelatedDocument/RelatedDocumentsModalComponent.razor` | 自動依單據類型分組顯示 |
| 庫存異動 Modal | `Components/Shared/RelatedDocument/InventoryTransactionRelatedModal.razor` | 庫存異動專用 Modal |
| 區塊子元件 | `Components/Shared/RelatedDocument/Components/RelatedDocumentSectionComponent.razor` | 單一類型的單據區塊 |
| 區塊配置 | `Components/Shared/RelatedDocument/Config/DocumentSectionConfig.cs` | 各單據類型的標題、圖示、顏色配置 |
| 詳細欄位範本 | `Components/Shared/RelatedDocument/Templates/*.razor` | 各類型詳細欄位 RenderFragment |
| 資料模型 | `Models/Documents/RelatedDocument.cs` | `RelatedDocumentInfo` 定義 |
| 類型枚舉 | `Models/Enums/RelatedDocumentType.cs` | `RelatedDocumentType` 枚舉 |
| 輔助方法 | `Helpers/RelatedDocumentsHelper.cs` | 服務層輔助方法 |

---

## RelatedDocumentsModalComponent 參數

| 參數 | 型別 | 說明 |
|------|------|------|
| `IsVisible` | `bool` | Modal 是否顯示 |
| `IsVisibleChanged` | `EventCallback<bool>` | 顯示狀態變更事件 |
| `ProductName` | `string` | 顯示於標題的品項名稱 |
| `RelatedDocuments` | `List<RelatedDocumentInfo>?` | 相關單據資料清單（null 時顯示載入中） |
| `IsLoading` | `bool` | 載入狀態 |
| `OnDocumentClick` | `EventCallback<RelatedDocumentInfo>` | 點擊單據列觸發 |
| `OnAddNew` | `EventCallback` | 點擊新增按鈕觸發 |

---

## RelatedDocumentSectionComponent 參數

| 參數 | 型別 | 說明 |
|------|------|------|
| `Documents` | `List<RelatedDocumentInfo>` | 該類型的單據清單 |
| `Config` | `DocumentSectionConfig` | 區塊顯示配置（標題、圖示、顏色） |
| `DetailsTemplate` | `RenderFragment<RelatedDocumentInfo>?` | 自訂詳細欄位範本（選填） |
| `OnDocumentClick` | `EventCallback<RelatedDocumentInfo>` | 點擊事件回呼 |

---

## DocumentSectionConfig 屬性

| 屬性 | 型別 | 說明 |
|------|------|------|
| `Title` | `string` | 區塊標題 |
| `Icon` | `string` | Bootstrap Icons 類別名稱 |
| `TextColor` | `string` | 文字顏色（Bootstrap 色彩名稱） |
| `BadgeColor` | `string` | Badge 背景顏色 |
| `ShowAddButton` | `bool` | 是否顯示新增按鈕 |
| `AddButtonText` | `string` | 新增按鈕文字 |

---

## RelatedDocumentInfo 欄位

| 欄位 | 型別 | 說明 |
|------|------|------|
| `DocumentId` | `int` | 單據 ID |
| `DocumentType` | `RelatedDocumentType` | 單據類型 |
| `DocumentNumber` | `string` | 單據編號 |
| `DocumentDate` | `DateTime` | 單據日期 |
| `Quantity` | `decimal?` | 相關數量 |
| `UnitPrice` | `decimal?` | 單價 |
| `Amount` | `decimal?` | 相關金額 |
| `CurrentAmount` | `decimal?` | 本次金額 |
| `TotalAmount` | `decimal?` | 累計金額 |
| `Remarks` | `string?` | 備註 |
| `Icon` | `string`（計算屬性） | 根據 `DocumentType` 自動取得圖示 |
| `BadgeColor` | `string`（計算屬性） | 根據 `DocumentType` 自動取得顏色 |
| `TypeDisplayName` | `string`（計算屬性） | 類型顯示名稱 |

---

## 支援的單據類型

| 枚舉值 | 說明 | 圖示 | 顏色 |
|--------|------|------|------|
| `ProductComposition` | 品項物料清單 (BOM) | `diagram-3` | purple |
| `SalesOrder` | 銷貨訂單 | `cart-check` | primary |
| `ReceivingDocument` | 入庫記錄 | `box-seam` | info |
| `ReturnDocument` | 退貨記錄 | `arrow-return-left` | warning |
| `SetoffDocument` | 沖款記錄 | `cash-coin` | success |
| `DeliveryDocument` | 出貨記錄 | `truck` | info |
| `ProductionSchedule` | 生產排程 | `calendar-check` | dark |
| `SupplierRecommendation` | 供應商推薦 | `shop` | success |
| `InventoryTransaction` | 庫存異動記錄 | `arrow-left-right` | secondary |

---

## 重要設計規則

### 1. 自動分組

`RelatedDocumentsModalComponent` 內部依 `DocumentType` 自動分組，呼叫端只需傳入扁平的 `List<RelatedDocumentInfo>`，無需預先分類。

### 2. 懶載入模式

開啟 Modal 時先設 `IsLoading = true`、`RelatedDocuments = null`，非同步取得資料後再更新 `RelatedDocuments` 並設 `IsLoading = false`，避免阻塞 UI。

### 3. 點擊事件由呼叫端處理

`OnDocumentClick` 傳回 `RelatedDocumentInfo`，呼叫端自行決定開啟對應 EditModal 或導航行為；元件本身不持有路由邏輯。

### 4. Templates 目錄的範本為可選

若不指定 `DetailsTemplate`，`RelatedDocumentSectionComponent` 使用預設顯示格式。詳細欄位需自訂時，在 `Templates/` 建立對應的 `.razor` 範本並在主 Modal 的 `GetDetailsTemplate` 方法中註冊。

---

## 擴充新單據類型快速參考

1. 在 `Models/Enums/RelatedDocumentType.cs` 新增枚舉值
2. 在 `Config/DocumentSectionConfig.cs` 的 `GetConfig` 方法新增對應配置（Title / Icon / TextColor / BadgeColor）
3. （選填）在 `Templates/` 建立詳細欄位 `.razor` 範本，參數為 `[Parameter] RelatedDocumentInfo Document`
4. （選填）在 `RelatedDocumentsModalComponent.GetDetailsTemplate` 方法中將新類型對應至範本

---

## 相關文件

- [README_共用元件設計總綱.md](README_共用元件設計總綱.md)
