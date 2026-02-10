# Models 資料夾分類總綱說明

## 概述

`Models` 資料夾包含專案中所有的資料傳輸物件 (DTO)、設定類別、列舉及介面。
所有類別皆依功能領域分類至對應的子資料夾，並遵循統一的命名規範。

---

## 命名規範

| 後綴 | 用途 | 範例 |
|------|------|------|
| `Dto` | 資料傳輸物件，用於服務間或 API 資料交換 | `ScheduleItemDto` |
| `Config` | 設定類別，存放元件或功能的配置選項 | `BarcodePrintConfig` |
| `Criteria` | 篩選/查詢條件 | `ProductBarcodePrintCriteria` |
| `Info` | 資訊模型，封裝只讀資料 | `RelatedDocumentInfo` |
| `Item` | 清單項目，通常用於下拉選單或列表 | `BreadcrumbItem` |
| `I` 前綴 | 介面 | `ISearchableItem` |

---

## 資料夾結構

```
Models/
├── GlobalUsings.cs          # 全域 using 宣告與向後相容別名
├── Barcode/                 # 條碼列印相關
├── Configuration/           # 元件設定
├── Documents/               # 關聯單據
├── Enums/                   # 所有列舉型別（從 Data/Enums 遷移）
├── Inventory/               # 庫存相關
├── Navigation/              # 導覽選單
├── Permissions/             # 權限矩陣
├── Printing/                # 印表機資訊
├── Reports/                 # 報表相關
└── Schedule/                # 排程相關
```

---

## 各資料夾說明

### Barcode/
條碼列印功能相關的模型。

| 檔案 | 類別 | 說明 |
|------|------|------|
| `BarcodePrintConfig.cs` | `BarcodePrintConfig` | 條碼列印設定（紙張大小、邊距、份數等） |
| `ProductBarcodeItem.cs` | `ProductBarcodeItem` | 單一商品條碼項目 |
| `ProductBarcodePrintCriteria.cs` | `ProductBarcodePrintCriteria` | 商品條碼列印篩選條件 |

---

### Configuration/
各種元件的設定配置類別。

| 檔案 | 類別 | 說明 |
|------|------|------|
| `DetailFormatConfig.cs` | `DetailFormatConfig` | 明細欄位格式化設定 |
| `StatisticsCardConfig.cs` | `StatisticsCardConfig` | 統計卡片元件設定 |
| `TransactionDetailConfig.cs` | `TransactionDetailConfig` | 交易明細設定 |

---

### Documents/
關聯單據相關的模型。

| 檔案 | 類別/列舉 | 說明 |
|------|-----------|------|
| `RelatedDocument.cs` | `RelatedDocumentInfo` | 關聯單據資訊 |
| `RelatedDocumentsRequest.cs` | `RelatedDocumentsRequest` | 關聯單據查詢請求 |

---

### Enums/
所有列舉型別（從 `Data/Enums` 遷移而來）。

| 檔案 | 列舉 | 說明 |
|------|------|------|
| `BarcodeSize.cs` | `BarcodeSize` | 條碼尺寸選項 |
| `EntityStatusEnum.cs` | `EntityStatus` | 實體狀態（啟用/停用/刪除） |
| `ErrorLevel.cs` | `ErrorLevel` | 錯誤等級 |
| `ErrorSource.cs` | `ErrorSource` | 錯誤來源 |
| `FinancialTransactionTypeEnum.cs` | `FinancialTransactionType` | 財務交易類型 |
| `InventoryEnums.cs` | 多個列舉 | 庫存交易相關列舉集合 |
| `InventoryImpactType.cs` | `InventoryImpactType` | 庫存影響類型 |
| `InventorySourceDocumentTypes.cs` | `InventorySourceDocumentTypes` | 庫存來源單據類型 |
| `InventoryStatus.cs` | `InventoryStatus` | 庫存狀態顯示 |
| `LoadingSize.cs` | `LoadingSize` | 載入大小 |
| `NavigationItemType.cs` | `NavigationItemType` | 導覽項目類型 |
| `PermissionEnum.cs` | `PermissionType` | 權限類型 |
| `PermissionLevel.cs` | `PermissionLevel` | 權限級別 |
| `PricingEnums.cs` | 多個列舉 | 定價相關列舉集合 |
| `PrinterConnectionType.cs` | `PrinterConnectionType` | 印表機連線類型 |
| `ProcurementType.cs` | `ProcurementType` | 採購類型 |
| `ProductionItemStatus.cs` | `ProductionItemStatus` | 生產項目狀態 |
| `PurchaseReceivingStatus.cs` | `PurchaseReceivingStatus` | 進貨單狀態 |
| `RelatedDocumentType.cs` | `RelatedDocumentType` | 關聯單據類型 |
| `SalesReturnStatus.cs` | `SalesReturnStatus` | 銷退狀態 |
| `SetoffDetailType.cs` | `SetoffDetailType` | 沖款明細類型 |
| `SetoffType.cs` | `SetoffType` | 沖款類型 |
| `TaxCalculationMethod.cs` | `TaxCalculationMethod` | 稅金計算方式 |
| `TransactionDetailType.cs` | `TransactionDetailType` | 交易明細類型 |

---

### Inventory/
庫存管理相關的模型。

| 檔案 | 類別 | 說明 |
|------|------|------|
| `OrderInventoryCheckItem.cs` | `OrderInventoryCheckItem` | 訂單庫存檢查項目 |
| `OrderInventoryCheckResult.cs` | `OrderInventoryCheckResult` | 訂單庫存檢查結果 |
| `SupplierRecommendationDto.cs` | `SupplierRecommendationDto` | 供應商推薦資訊 |

---

### Navigation/
導覽選單與麵包屑相關的模型。

| 檔案 | 類別/介面 | 說明 |
|------|-----------|------|
| `BreadcrumbItem.cs` | `BreadcrumbItem` | 麵包屑項目 |
| `ISearchableItem.cs` | `ISearchableItem` | 可搜尋項目介面 |
| `NavigationItem.cs` | `NavigationItem` | 導覽項目 |
| `NavigationMenuItem.cs` | `NavigationMenuItem` | 導覽選單項目 |

---

### Permissions/
權限管理相關的模型。

| 檔案 | 類別 | 說明 |
|------|------|------|
| `ModulePermissionMatrix.cs` | `ModulePermissionMatrix` | 模組權限矩陣 |

---

### Printing/
列印功能相關的模型。

| 檔案 | 類別 | 說明 |
|------|------|------|
| `InstalledPrinterInfo.cs` | `InstalledPrinterInfo` | 已安裝印表機資訊 |

---

### Reports/
報表功能相關的模型，含子資料夾。

| 檔案/資料夾 | 類別/列舉 | 說明 |
|-------------|-----------|------|
| `BatchPrintCriteria.cs` | `BatchPrintCriteria` | 批次列印條件 |
| `FormattedDocument.cs` | `FormattedDocument` | 格式化文件 |
| `ReportCategoryConfig.cs` | `ReportCategoryConfig` | 報表分類設定 |
| `ReportDefinition.cs` | `ReportDefinition` | 報表定義 |
| `ReportModels.cs` | 多個類別 | 報表相關模型集合 |
| `FilterCriteria/` | - | 篩選條件子資料夾 |
| `FilterTemplates/` | - | 篩選範本子資料夾 |

---

### Schedule/
排程功能相關的 DTO。

| 檔案 | 類別 | 說明 |
|------|------|------|
| `ImportScheduleItemDto.cs` | `ImportScheduleItemDto` | 匯入排程項目 |
| `PendingScheduleDetailDto.cs` | `PendingScheduleDetailDto` | 待處理排程明細 |
| `PendingScheduleOrderDto.cs` | `PendingScheduleOrderDto` | 待處理排程訂單 |
| `ScheduleItemDto.cs` | `ScheduleItemDto` | 排程項目 |

---

## 全域 Using 與向後相容

`GlobalUsings.cs` 檔案提供：

1. **全域命名空間引用** - 所有 Models 子資料夾的命名空間皆自動引入，無需在每個檔案中手動 `using`。

2. **向後相容別名** - 保留舊類別名稱的相容性：

| 舊名稱 | 新名稱 | 說明 |
|--------|--------|------|
| `BarcodePrintSettings` | `BarcodePrintConfig` | 統一使用 Config 後綴 |
| `SupplierRecommendation` | `SupplierRecommendationDto` | 統一使用 Dto 後綴 |
| `RelatedDocument` | `RelatedDocumentInfo` | 避免與命名空間衝突 |
| `ERPCore2.Data.Enums.*` | `ERPCore2.Models.Enums.*` | 所有 Enum 已遷移至 Models |

---

## 維護指引

1. **新增檔案時**：
   - 依功能領域放入對應資料夾
   - 遵循命名規範（Dto、Config、Criteria 等後綴）
   - 使用對應的命名空間（如 `ERPCore2.Models.Barcode`）

2. **重命名類別時**：
   - 在 `GlobalUsings.cs` 新增向後相容別名
   - 更新本文件的對應表格

3. **新增資料夾時**：
   - 在 `GlobalUsings.cs` 加入該命名空間的 `global using` 宣告
   - 更新本文件的資料夾結構說明
