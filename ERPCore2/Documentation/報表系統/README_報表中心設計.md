# 報表集設計說明

## 更新日期
2026-02-10

---

## 📋 功能概述

報表集提供統一入口，讓使用者瀏覽並選擇特定模組下的所有報表。系統支援兩種報表流程：

1. **需要篩選的報表**：經由 `GenericReportFilterModalComponent` 顯示篩選條件
2. **不需要篩選的報表**：直接執行 ActionRegistry 中註冊的動作

---

## 🏗️ 架構設計

### 檔案結構

```
Models/
├── ReportDefinition.cs              # 報表定義模型
└── ReportCategoryConfig.cs          # 報表分類設定（標題、圖示）

Data/Reports/
└── ReportRegistry.cs                # 報表註冊表（集中管理）

Components/
├── Layout/
│   └── MainLayout.razor             # 主版面配置（處理報表選擇事件）
│
├── Shared/
│   ├── QuickAction/
│   │   └── GenericSearchModalComponent.razor  # Alt+R 報表快捷搜尋
│   │
│   └── Report/
│       ├── GenericReportFilterModalComponent.razor  # 通用篩選 Modal
│       └── FilterTemplateInitializer.cs             # 篩選模板初始化
│
└── Pages/
    └── Reports/
        └── GenericReportIndexPage.razor     # 通用報表集
```

### 組件關係圖

```
┌─────────────────────────────────────────────────────────────────┐
│                         MainLayout                               │
├─────────────────────────────────────────────────────────────────┤
│  入口點 A：導航選單 → 報表集                                   │
│  ┌─────────────────────────────────────────────────────────────┐│
│  │      GenericReportIndexPage (Category=Purchase/Sales/...)   ││
│  │      顯示報表清單，點擊「列印」觸發 OnReportSelected        ││
│  └─────────────────────────────────────────────────────────────┘│
│                              ↓                                   │
├─────────────────────────────────────────────────────────────────┤
│  入口點 B：Alt+R 快捷搜尋                                        │
│  ┌─────────────────────────────────────────────────────────────┐│
│  │      GenericSearchModalComponent                             ││
│  │      搜尋報表，點擊結果觸發 OnItemSelected                   ││
│  └─────────────────────────────────────────────────────────────┘│
│                              ↓                                   │
├─────────────────────────────────────────────────────────────────┤
│  HandleReportSelected(actionId)                                  │
│  ↓ 查詢 FilterTemplateRegistry.HasConfig(reportId)              │
│                              ↓                                   │
│  ┌─────────────────┐    ┌─────────────────────────────────────┐ │
│  │ 有篩選配置       │    │ 無篩選配置                          │ │
│  │ ↓               │    │ ↓                                   │ │
│  │ 開啟篩選 Modal   │    │ actionRegistry.Execute(actionId)   │ │
│  └─────────────────┘    └─────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📖 使用方式

### 1. 新增報表定義

在 `Data/Reports/ReportRegistry.cs` 中加入報表定義：

```csharp
new ReportDefinition
{
    Id = "PO005",                              // 報表 ID（須唯一）
    Name = "採購分析報表",
    Description = "依廠商、商品分析採購統計",
    IconClass = "bi bi-graph-up",
    Category = ReportCategory.Purchase,
    RequiredPermission = "PurchaseOrder.Read",
    ActionId = "PrintPurchaseAnalysis",        // Action 識別碼
    SortOrder = 5,
    IsEnabled = true
}
```

### 2. 決定是否需要篩選

#### 方案 A：需要篩選（推薦）

參考 [README_報表篩選架構設計總綱.md](README_報表篩選架構設計.md) 建立篩選模板。

#### 方案 B：不需要篩選

在 `MainLayout.razor` 的 `OnInitializedAsync` 中註冊 Action：

```csharp
actionRegistry.Register("PrintPurchaseAnalysis", OpenPurchaseAnalysisReport);
```

---

## 🔄 完整流程

```
1. 使用者從導航選單點擊「採購報表集」或按 Alt+R 搜尋
   ↓
2. 選擇報表，觸發 HandleReportSelected(actionId)
   ↓
3. 從 ActionId 反查 ReportId
   ↓
4. 檢查 FilterTemplateRegistry.HasConfig(reportId)
   ↓
   ├─ 有篩選配置 → 設定 currentFilterReportId → 開啟 GenericReportFilterModalComponent
   │  ↓ 使用者設定篩選條件 → 呼叫 RenderBatchToImagesAsync → 開啟預覽 Modal
   │
   └─ 無篩選配置 → 執行 actionRegistry.Execute(actionId)
```

---

## 📊 報表分類常數

```csharp
public static class ReportCategory
{
    public const string Customer = "Customer";    // 客戶報表
    public const string Supplier = "Supplier";    // 廠商報表
    public const string Financial = "Financial";  // 財務報表
    public const string Inventory = "Inventory";  // 庫存報表
    public const string Sales = "Sales";          // 銷售報表
    public const string Purchase = "Purchase";    // 採購報表
}
```

---

## ✅ 目前已實作

### 報表集

| 報表集 | Category 參數 | Action ID |
|----------|---------------|-----------|
| 客戶報表集 | `Customer` | `OpenCustomerReportIndex` |
| 廠商報表集 | `Supplier` | `OpenSupplierReportIndex` |
| 庫存報表集 | `Inventory` | `OpenInventoryReportIndex` |
| 銷售報表集 | `Sales` | `OpenSalesReportIndex` |
| 採購報表集 | `Purchase` | `OpenPurchaseReportIndex` |
| 財務報表集 | `Financial` | `OpenFinancialReportIndex` |

### 已配置篩選的報表

| 報表 ID | 名稱 | 篩選模板 | 說明 |
|---------|------|----------|------|
| AR001 | 應收帳款報表 | AccountsReceivableFilterTemplate | 客戶應收帳款 |
| PO001 | 採購單 | PurchaseOrderBatchFilterTemplate | 報表集進入時顯示篩選 |

> **設計原則**：每個單據類型只有一個報表 ID，入口點決定行為：
> - **EditModal 列印按鈕**：直接單筆列印（不經過篩選）
> - **報表集 / Alt+R 快捷搜尋**：顯示篩選 Modal，可批次列印

---

## 相關檔案

- [README_報表系統總綱.md](README_報表系統總綱.md) - 報表系統入口
- [README_報表篩選架構設計總綱.md](README_報表篩選架構設計.md) - 篩選模板機制
- [README_報表檔設計總綱.md](README_報表檔設計.md) - 報表服務與列印
- [README_報表Index設計總綱.md](README_報表Index設計.md) - Index 批次列印
