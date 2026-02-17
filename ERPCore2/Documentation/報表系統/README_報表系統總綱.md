# 報表系統設計總綱

## 更新日期
2026-02-10

---

## 📋 概述

ERPCore2 報表系統採用**統一格式化模式**，提供完整的報表產生、預覽、列印與匯出功能。系統支援四個入口點觸發列印，並透過註冊式架構實現高度可擴展性。

---

## 🏗️ 系統架構圖

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           報表系統入口點                                 │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────────┐  │
│  │ 入口 A：Alt+R    │  │ 入口 B：報表中心 │  │ 入口 C：EditModal   │  │
│  │ (快捷鍵搜尋)      │  │ (分類瀏覽)       │  │ (單筆列印)          │  │
│  └────────┬─────────┘  └────────┬─────────┘  └──────────┬───────────┘  │
│           │ 選擇報表            │ 選擇報表              │ 直接列印    │
│           └──────────┬──────────┘                       │             │
│                      ↓                                   ↓             │
│  ┌──────────────────────────────┐    ┌────────────────────────────┐   │
│  │ GenericReportFilterModal     │    │ ReportPreviewModal         │   │
│  │ (篩選 → 預覽 → 列印)         │    │ (直接預覽當前單據)          │   │
│  └─────────────┬────────────────┘    └────────────────────────────┘   │
│                │                                                       │
│  ┌─────────────┴────────────────┐                                      │
│  │ 入口 D：Index 批次列印        │                                      │
│  │ (直接開啟篩選，不需選報表)    │                                      │
│  └──────────────────────────────┘                                      │
│                                                                         │
├─────────────────────────────────────────────────────────────────────────┤
│                           核心服務層                                     │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │ IEntityReportService<T>                                          │   │
│  │ → GenerateReportAsync()         產生 FormattedDocument           │   │
│  │ → RenderToImagesAsync()         渲染為預覽圖片                    │   │
│  │ → RenderBatchToImagesAsync()    批次渲染（使用 BatchReportHelper）│   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                              ↓                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │ FormattedPrintService                                            │   │
│  │ → RenderToImages()              渲染為圖片                        │   │
│  │ → Print()                       列印到印表機                      │   │
│  │ → PrintByReportIdAsync()        使用報表 ID 查詢印表機配置        │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 📚 文件導覽

本報表系統分為四份詳細文件，各司其職：

### 1. 入口點與流程

| 文件 | 說明 | 適用場景 |
|------|------|----------|
| [README_報表中心設計.md](README_報表中心設計.md) | 報表中心與快捷鍵搜尋的設計 | 從選單或 Alt+R 進入報表 |
| [README_報表Index設計.md](README_報表Index設計.md) | Index 頁面批次列印實作 | 從列表頁直接批次列印 |

### 2. 核心架構

| 文件 | 說明 | 適用場景 |
|------|------|----------|
| [README_報表篩選架構設計.md](README_報表篩選架構設計.md) | 篩選模板註冊機制與實作 | 新增報表篩選功能 |
| [README_報表檔設計.md](README_報表檔設計.md) | FormattedDocument、報表服務、列印服務 | 新增報表服務 |

---

## 🔄 四個入口點對照

| 入口點 | 流程 | 需選報表 | 核心元件 |
|--------|------|----------|----------|
| **快捷鍵 Alt+R** | 搜尋 → 篩選 → 預覽 → 列印 | ✅ | GenericSearchModal |
| **報表中心** | 選擇 → 篩選 → 預覽 → 列印 | ✅ | GenericReportIndexPage |
| **EditModal** | 點擊列印 → 預覽 → 列印 | ❌ | GenericEditModalComponent |
| **Index 批次列印** | 點擊列印 → 篩選 → 預覽 → 列印 | ❌ | GenericReportFilterModalComponent |

> **關鍵差異**：EditModal 直接列印當前單據；Index 開啟篩選但不需選擇報表（ReportId 固定）。

---

## 📁 目錄結構總覽

```
Models/Reports/
├── ReportIds.cs                            # 報表 ID 常數（唯一來源）
├── BatchPrintCriteria.cs                   # 批次列印篩選條件
├── FormattedDocument.cs                    # 格式化報表文件模型
├── TableDefinition.cs                      # 表格定義
├── ReportDefinition.cs                     # 報表定義模型
├── FilterCriteria/                         # 篩選條件 DTO
│   ├── IReportFilterCriteria.cs
│   └── PurchaseOrderBatchPrintCriteria.cs
└── FilterTemplates/                        # 模板配置
    ├── ReportFilterConfig.cs               # 篩選配置模型
    └── FilterTemplateRegistry.cs           # 模板註冊表（集中管理所有配置）

Data/Reports/
└── ReportRegistry.cs                       # 報表註冊表（使用 ReportIds 常數）

Helpers/
└── BatchReportHelper.cs                    # 批次報表產生 Helper

Services/Reports/
├── Interfaces/
│   ├── IFormattedPrintService.cs
│   ├── IExcelExportService.cs
│   ├── IEntityReportService.cs
│   └── I[Entity]ReportService.cs
├── FormattedPrintService.cs
├── ExcelExportService.cs
└── [Entity]ReportService.cs

Components/
├── Layout/
│   └── MainLayout.razor                    # 快捷鍵整合入口
├── Shared/Report/
│   ├── ReportPreviewModalComponent.razor   # 報表預覽 Modal
│   ├── GenericReportFilterModalComponent.razor
│   ├── FilterTemplateInitializer.cs
│   └── FilterTemplates/
│       └── [Entity]BatchFilterTemplate.razor
└── Pages/
    ├── Reports/
    │   └── GenericReportIndexPage.razor    # 報表中心
    └── [Module]/
        └── [Entity]Index.razor             # 各模組 Index 頁面
```

---

## 🔧 報表 ID 命名規則

| 前綴 | 分類 | 範例 |
|------|------|------|
| AR | 客戶報表 | AR001（應收帳款） |
| AP | 廠商報表 | AP001 |
| PO | 採購報表 | PO001（採購單）、PO002（進貨單）、PO003（進貨退出單） |
| SO | 銷售報表 | SO001（報價單）、SO002（銷貨單）、SO004（出貨單）、SO005（銷貨退回單） |
| IV | 庫存報表 | IV001 |
| FN | 財務報表 | FN001 |

### 使用 ReportIds 常數

所有報表 ID 均定義於 `Models/Reports/ReportIds.cs`，**禁止**在程式碼中直接寫死字串：

```csharp
// ✅ 正確：使用常數
@using ERPCore2.Models.Reports
<GenericReportFilterModalComponent ReportId="@ReportIds.PurchaseOrder" ... />

// ❌ 錯誤：直接寫死
<GenericReportFilterModalComponent ReportId="PO001" ... />
```

---

## ✅ 已實作項目

### 報表服務

| 服務 | 報表 ID | 狀態 |
|------|---------|------|
| PurchaseOrderReportService | PO001 | ✅ 完成 |
| PurchaseReceivingReportService | PO002 | ✅ 完成 |
| PurchaseReturnReportService | PO003 | ✅ 完成 |
| QuotationReportService | SO001 | ✅ 完成 |
| SalesOrderReportService | SO002 | ✅ 完成 |
| SalesDeliveryReportService | SO004 | ✅ 完成 |
| SalesReturnReportService | SO005 | ✅ 完成 |

### 篩選模板

| 報表 ID | 模板 | 狀態 |
|---------|------|------|
| AR001 | AccountsReceivableFilterTemplate | ✅ 完成 |
| PO001 | PurchaseOrderBatchFilterTemplate | ✅ 完成 |

### Index 列印（使用 GenericReportFilterModalComponent）

| Index 頁面 | 報表 ID | 狀態 |
|-----------|---------|------|
| PurchaseOrderIndex | PO001 | ✅ 完成 |
| PurchaseReceivingIndex | PR001 | ⏳ 待實作 |
| PurchaseReturnIndex | PRT001 | ⏳ 待實作 |
| SalesOrderIndex | SO001 | ⏳ 待實作 |
| QuotationIndex | QT001 | ⏳ 待實作 |

---

## 📋 新增報表快速指南

### 情境 A：在 EditModal 新增列印功能

1. 確認報表服務已實作 `IEntityReportService<T>`
2. 在 EditModalComponent 設定參數：

```razor
<GenericEditModalComponent 
    ShowPrintButton="true"
    ReportService="@ReportService"
    ReportId="PO001"
    ReportPreviewTitle="採購單預覽"
    GetReportDocumentName="@(e => $"採購單-{e.Code}")" />
```

📖 詳見 [README_報表檔設計.md](README_報表檔設計.md)

---

### 情境 B：在 Index 頁面新增批次列印

1. 確認報表已在 FilterTemplateRegistry 註冊
2. 在 Index 頁面放置 GenericReportFilterModalComponent：

```razor
<GenericIndexPageComponent 
    ShowBatchPrintButton="true"
    OnBatchPrintClick="@HandleBatchPrintAsync" />

<GenericReportFilterModalComponent 
    IsVisible="@showBatchPrintModal"
    IsVisibleChanged="@((bool v) => showBatchPrintModal = v)"
    ReportId="PO001" />
```

📖 詳見 [README_報表Index設計.md](README_報表Index設計.md)

---

### 情境 C：新增全新報表（含篩選）

1. 建立篩選條件 DTO（實作 `IReportFilterCriteria`）
2. 建立篩選模板組件（實作 `IFilterTemplateComponent`）
3. 在 FilterTemplateInitializer 註冊模板類型
4. 在 FilterTemplateRegistry 註冊配置
5. 在 ReportRegistry 註冊報表定義

📖 詳見 [README_報表篩選架構設計.md](README_報表篩選架構設計.md)

---

### 情境 D：新增報表服務

1. 建立介面（繼承 `IEntityReportService<T>`）
2. 實作服務（使用 `FormattedDocument` Fluent API）
3. 在 ServiceRegistration.cs 註冊服務
4. 在 ReportRegistry.cs 註冊報表定義

📖 詳見 [README_報表檔設計.md](README_報表檔設計.md)

---

## ⚠️ 注意事項

1. **Windows 專屬**：`FormattedPrintService` 使用 `System.Drawing.Printing`，僅支援 Windows
2. **Excel 匯出跨平台**：`ExcelExportService` 使用 ClosedXML，支援所有平台
3. **單一報表 ID 原則**：每個單據類型只有一個報表 ID，入口點決定流程
4. **篩選模板初始化**：`FilterTemplateInitializer.EnsureInitialized()` 在 MainLayout 啟動時自動呼叫

---

## 相關檔案

- [README_報表中心設計.md](README_報表中心設計.md) - 報表中心入口
- [README_報表篩選架構設計.md](README_報表篩選架構設計.md) - 篩選模板機制
- [README_報表檔設計.md](README_報表檔設計.md) - 報表服務與列印
- [README_報表Index設計.md](README_報表Index設計.md) - Index 批次列印
- [README_快捷鍵設計.md](../共用元件設計/README_快捷鍵設計.md) - Alt+R 快捷鍵
