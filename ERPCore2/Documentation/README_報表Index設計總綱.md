# Index 頁面列印實作指南

## 更新日期
2026-02-10

---

## 📋 架構概述

系統提供四個入口點可以實現列印功能，每個入口點有不同的流程特性：

| 入口點 | 流程 | 說明 |
|--------|------|------|
| **快捷鍵查詢 (Alt+R)** | 選擇報表 → 篩選 → 列印 | 全域報表搜尋 |
| **報表中心** | 選擇報表 → 篩選 → 列印 | 分類瀏覽報表 |
| **EditModal 列印** | 直接列印 | 單筆列印當前單據 |
| **Index 列印** | 篩選 → 列印 | 批次列印，不需選擇報表 |

---

## 🏗️ 入口點架構圖

```
┌─────────────────────────────────────────────────────────────────────┐
│                         列印功能入口點                               │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  入口 A：快捷鍵 Alt+R                入口 B：報表中心               │
│  ┌─────────────────────┐            ┌─────────────────────┐        │
│  │ GenericSearchModal  │            │ GenericReportIndex  │        │
│  │ (搜尋報表)           │            │ (報表清單)           │        │
│  └─────────┬───────────┘            └─────────┬───────────┘        │
│            │ OnItemSelected                   │ OnReportSelected   │
│            └────────────┬─────────────────────┘                    │
│                         ↓                                           │
│            ┌────────────────────────────┐                          │
│            │ GenericReportFilterModal   │ ← 根據 ReportId 載入     │
│            │ (篩選 → 預覽 → 列印)       │   對應的 FilterTemplate  │
│            └────────────────────────────┘                          │
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  入口 C：EditModal 列印按鈕          入口 D：Index 批次列印         │
│  ┌─────────────────────┐            ┌─────────────────────┐        │
│  │ GenericEditModal    │            │ GenericIndexPage    │        │
│  │ ShowPrintButton     │            │ ShowBatchPrintButton│        │
│  └─────────┬───────────┘            └─────────┬───────────┘        │
│            │                                   │                    │
│            ↓ 呼叫 ReportService                ↓ 開啟 Modal        │
│  ┌─────────────────────┐            ┌────────────────────────┐     │
│  │ ReportPreviewModal  │            │ GenericReportFilterModal│    │
│  │ (直接預覽當前單據)   │            │ (篩選 → 預覽 → 列印)    │     │
│  └─────────────────────┘            └────────────────────────┘     │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 📦 Index 頁面列印實作步驟

### 前置條件

確認以下項目已完成：
1. ✅ 對應的報表服務已實作（如 `IPurchaseOrderReportService`）
2. ✅ 篩選條件 DTO 已建立（如 `PurchaseOrderBatchPrintCriteria`）
3. ✅ 篩選模板組件已建立（如 `PurchaseOrderBatchFilterTemplate`）
4. ✅ 已在 `FilterTemplateRegistry` 註冊配置

---

### 步驟 1：在 Index 頁面宣告 Modal

```razor
@using ERPCore2.Models.Reports

@* 批次列印篩選 Modal - 使用 GenericReportFilterModalComponent 統一架構 *@
<GenericReportFilterModalComponent IsVisible="@showBatchPrintModal"
                                  IsVisibleChanged="@((bool visible) => showBatchPrintModal = visible)"
                                  ReportId="@ReportIds.PurchaseOrder"
                                  OnPrintSuccess="@HandleBatchPrintSuccess" />
```

**參數說明：**

| 參數 | 說明 |
|------|------|
| `IsVisible` | 控制 Modal 顯示/隱藏 |
| `ReportId` | 報表 ID，對應 FilterTemplateRegistry 中的配置 |
| `OnPrintSuccess` | 列印成功後的回調（選用） |

---

### 步驟 2：設定 Index 頁面參數

```razor
<GenericIndexPageComponent TEntity="PurchaseOrder" 
                          TService="IPurchaseOrderService"
                          ...
                          ShowBatchPrintButton="true"
                          OnBatchPrintClick="@HandleBatchPrintAsync">
</GenericIndexPageComponent>
```

---

### 步驟 3：實作處理方法

```csharp
@code {
    // 批次列印 Modal 狀態
    private bool showBatchPrintModal = false;

    /// <summary>
    /// 批次列印按鈕點擊處理
    /// </summary>
    private async Task HandleBatchPrintAsync()
    {
        try
        {
            // 直接開啟篩選 Modal（使用 GenericReportFilterModalComponent）
            showBatchPrintModal = true;
        }
        catch (Exception ex)
        {
            await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(HandleBatchPrintAsync), GetType());
            await NotificationService.ShowErrorAsync("開啟批次列印視窗失敗");
        }
    }

    /// <summary>
    /// 列印成功後處理（選用）
    /// </summary>
    private void HandleBatchPrintSuccess()
    {
        showBatchPrintModal = false;
        StateHasChanged();
    }
}
```

---

## 🔧 完整範例：PurchaseOrderIndex.razor

```razor
@page "/purchase/orders"
@using ERPCore2.Models.Reports
@inject IPurchaseOrderService PurchaseOrderService
@inject INotificationService NotificationService
@rendermode InteractiveServer

<GenericIndexPageComponent TEntity="PurchaseOrder" 
                          TService="IPurchaseOrderService"
                          Service="@PurchaseOrderService"
                          ...
                          ShowBatchPrintButton="true"
                          OnBatchPrintClick="@HandleBatchPrintAsync">
</GenericIndexPageComponent>

@* 批次列印篩選 Modal *@
<GenericReportFilterModalComponent IsVisible="@showBatchPrintModal"
                                  IsVisibleChanged="@((bool visible) => showBatchPrintModal = visible)"
                                  ReportId="@ReportIds.PurchaseOrder"
                                  OnPrintSuccess="@HandleBatchPrintSuccess" />

@code {
    private bool showBatchPrintModal = false;

    private async Task HandleBatchPrintAsync()
    {
        try
        {
            showBatchPrintModal = true;
        }
        catch (Exception ex)
        {
            await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(HandleBatchPrintAsync), GetType());
            await NotificationService.ShowErrorAsync("開啟批次列印視窗失敗");
        }
    }

    private void HandleBatchPrintSuccess()
    {
        showBatchPrintModal = false;
        StateHasChanged();
    }
}
```

---

## 📊 報表 ID 對照表

> **重要**：所有報表 ID 定義於 `Models/Reports/ReportIds.cs`。使用程式碼時必須使用常數，禁止直接寫死字串。

| 模組 | Index 頁面 | 報表 ID 常數 | 篩選模板 |
|------|-----------|---------|----------|
| 採購 | PurchaseOrderIndex | `ReportIds.PurchaseOrder` | PurchaseOrderBatchFilterTemplate |
| 採購 | PurchaseReceivingIndex | `ReportIds.PurchaseReceiving` | （待建立） |
| 採購 | PurchaseReturnIndex | `ReportIds.PurchaseReturn` | （待建立） |
| 銷售 | SalesOrderIndex | `ReportIds.SalesOrder` | （待建立） |
| 銷售 | SalesReturnIndex | `ReportIds.SalesReturn` | （待建立） |
| 銷售 | QuotationIndex | `ReportIds.Quotation` | （待建立） |

---

## ⚠️ 常見錯誤與解決方案

### 錯誤 1：找不到報表配置

**症狀**：篩選 Modal 顯示「找不到報表 ID「XXX」的篩選配置」

**解決方案**：
1. 確認 `FilterTemplateRegistry.cs` 的 `Initialize()` 中已註冊配置
2. 確認 `FilterTemplateTypeName` 是完整的類別名稱

```csharp
// FilterTemplateRegistry.cs
RegisterConfig(new ReportFilterConfig
{
    ReportId = ReportIds.PurchaseOrder,
    FilterTemplateTypeName = "ERPCore2.Components.Shared.Report.FilterTemplates.PurchaseOrderBatchFilterTemplate",
    // ...
});
```

### 錯誤 2：列印結果為空

**症狀**：預覽時沒有任何報表

**解決方案**：
1. 確認篩選條件 DTO 的 `ToQueryParameters()` 正確轉換條件
2. 確認報表服務的 `GetByCriteriaAsync()` 正確處理篩選條件

### 錯誤 3：預覽圖片產生失敗

**症狀**：Loading 結束後沒有顯示預覽

**解決方案**：
1. 確認 `ReportFilterConfig.ReportServiceType` 設定正確
2. 確認報表服務實作了 `RenderBatchToImagesAsync()` 方法

---

## 🔄 流程圖

```
Index 批次列印流程：

1. 點擊「批次列印」按鈕
   ↓
2. 開啟 GenericReportFilterModalComponent
   ↓ 傳入 ReportId（如 "PO001"）
   ↓
3. Modal 從 FilterTemplateRegistry 載入配置
   ↓
4. 動態載入篩選模板組件（如 PurchaseOrderBatchFilterTemplate）
   ↓
5. 使用者設定篩選條件
   ↓
6. 點擊「預覽列印」
   ↓
7. 從模板取得 IReportFilterCriteria
   ↓
8. 轉換為 BatchPrintCriteria
   ↓
9. 呼叫報表服務 RenderBatchToImagesAsync
   ↓
10. 開啟 ReportPreviewModalComponent 顯示預覽
   ↓
11. 點擊「列印」或「儲存」完成操作
```

---

## 📋 實作檢查清單

新增一個 Index 列印功能時，確認以下項目：

- [ ] **報表 ID 常數**
  - 在 `ReportIds.cs` 新增常數

- [ ] **篩選條件 DTO**
  - 建立 `XxxBatchPrintCriteria.cs` 繼承 `IReportFilterCriteria`
  - 實作 `Validate()` 和 `ToQueryParameters()` 方法

- [ ] **篩選模板組件**
  - 建立 `XxxBatchFilterTemplate.razor` 實作 `IFilterTemplateComponent`
  - 實作 `GetCriteria()` 和 `Reset()` 方法

- [ ] **註冊配置**
  - 在 `FilterTemplateRegistry.cs` 的 `Initialize()` 中新增配置
  - 設定 `FilterTemplateTypeName` 為完整類別名稱
  - 確認 `ReportServiceType` 設定正確

- [ ] **報表服務**
  - 確認服務實作了批次渲染方法

- [ ] **Index 頁面**
  - 設定 `ShowBatchPrintButton="true"`
  - 設定 `OnBatchPrintClick="@HandleBatchPrintAsync"`
  - 放置 `GenericReportFilterModalComponent` 並使用 `ReportId="@ReportIds.Xxx"`

---

## 相關檔案

- [README_報表系統總綱.md](README_報表系統總綱.md) - 報表系統入口
- [README_報表篩選架構設計總綱.md](../Components/Shared/Report/README_報表篩選架構設計總綱.md) - 篩選模板機制
- [README_報表中心設計.md](../Components/Pages/Reports/README_報表中心設計.md) - 報表中心入口
- [README_報表檔設計總綱.md](README_報表檔設計總綱.md) - 報表服務與列印
