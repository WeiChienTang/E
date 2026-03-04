# 主從式進銷存模組設計總綱

## 更新日期
2026-03-04（庫存異動審核整合）

---

## 📋 概述

ERPCore2 有 7 個**主從式進銷存模組**，每個模組由「主表單（進銷存頭）」加「明細表格」組成，
兩者顯示在同一頁面，不需切換 Tab。

所有 7 個模組共用相同的元件架構與設計模式，差異僅在業務邏輯細節。

---

## 📦 適用模組

| 模組 | EditModal | Table 元件 | 狀態 |
|------|-----------|------------|------|
| 採購單 | `PurchaseOrderEditModalComponent` | `PurchaseOrderTable` | ✅ 完成 |
| 入庫單 | `PurchaseReceivingEditModalComponent` | `PurchaseReceivingTable` | ✅ 完成 |
| 入庫退回 | `PurchaseReturnEditModalComponent` | `PurchaseReturnTable` | ✅ 完成 |
| 報價單 | `QuotationEditModalComponent` | `QuotationTable` | ✅ 完成 |
| 訂單 | `SalesOrderEditModalComponent` | `SalesOrderTable` | ✅ 完成 |
| 銷貨單 | `SalesDeliveryEditModalComponent` | `SalesDeliveryTable` | ✅ 完成 |
| 銷貨退回 | `SalesReturnEditModalComponent` | `SalesReturnTable` | ✅ 完成 |

---

## 🏗️ UI 版面結構

```
┌─────────────────────────────────────────────────────┐
│  GenericEditModalComponent（Modal 框架）             │
│                                                     │
│  FormHeaderContent（鎖定警告訊息）                   │
│  CustomActionButtons（轉單 / 複製訊息按鈕）           │
│                                                     │
│  ┌─────────────────────────────────────────────┐    │
│  │  表單欄位（FormFields + FormSections）        │    │
│  │  BasicInfo: 單號、對象、公司、日期            │    │
│  │  AmountInfo: 稅別、小計、稅額、含稅合計       │    │
│  │  AdditionalInfo: 備註                        │    │
│  │  ApprovalInfo: 審核狀態、審核者、審核時間     │    │
│  └─────────────────────────────────────────────┘    │
│                                                     │
│  ┌─────────────────────────────────────────────┐    │
│  │  CustomModules（Order=1）                    │    │
│  │  XxxTable（明細表格）                        │    │
│  │  - 商品搜尋 / 數量 / 單位 / 單價             │    │
│  │  - 稅率 / 小計 / 狀態 / 備註                 │    │
│  └─────────────────────────────────────────────┘    │
│                                                     │
│  ShowApprovalSection（審核區塊，依設定顯示）          │
│  ShowPrintButton（列印 / PDF）                       │
└─────────────────────────────────────────────────────┘
```

---

## 🧱 元件層次結構

```
GenericEditModalComponent（Modal 框架）
    └── XxxEditModalComponent（主組件，協調資料流）
            └── XxxTable（明細表格）
                    ├── BaseDetailTableComponent（抽象基底：生命週期 + 共用邏輯）
                    └── InteractiveTableComponent（UI 原語：表格渲染 + 空白行）
```

| 層次 | 元件 | 職責 |
|------|------|------|
| UI 原語 | `InteractiveTableComponent<TItem>` | 表格渲染、空白行管理、行選取、批量操作 |
| 生命週期協調 | `BaseDetailTableComponent<TMainEntity, TDetailEntity, TItem>` | DataVersion 追蹤、SelectedSupplierId 追蹤、空白行觸發修正、不可刪除狀態通知 |
| 業務邏輯 | `XxxTable.razor` | 商品搜尋、欄位定義、Lock 判斷、金額計算、特殊業務規則 |
| 資料協調 | `XxxEditModalComponent.razor` | 主檔 + 明細協調、儲存、轉單、金額匯總 |

---

## 📚 子文件導覽

| 文件 | 說明 | 適用場景 |
|------|------|----------|
| [README_BaseDetailTableComponent設計.md](README_BaseDetailTableComponent設計.md) | 抽象基底類別的參數、方法與空白行機制 | 理解共用生命週期邏輯 |
| [README_表單設計規範.md](README_表單設計規範.md) | 元件職責、欄位鎖定、金額計算、審核 | 修改現有模組行為 |
| [README_進銷存模組實作指南.md](README_進銷存模組實作指南.md) | Step-by-step 新增或遷移模組 | 新增模組時參考 |

---

## 📁 檔案位置

```
Components/Shared/Table/
└── BaseDetailTableComponent.cs      ← 抽象基底（7 個模組共用）

Components/Pages/Purchase/
├── PurchaseOrderEditModalComponent.razor
├── PurchaseOrderTable.razor
├── PurchaseReceivingEditModalComponent.razor
├── PurchaseReceivingTable.razor
├── PurchaseReturnEditModalComponent.razor
└── PurchaseReturnTable.razor

Components/Pages/Sales/
├── QuotationEditModalComponent.razor
├── QuotationTable.razor
├── SalesOrderEditModalComponent.razor
├── SalesOrderTable.razor
├── SalesDeliveryEditModalComponent.razor
├── SalesDeliveryTable.razor
├── SalesReturnEditModalComponent.razor
└── SalesReturnTable.razor
```

---

## 📝 已棄用的 Tab 元件

下列 Tab 元件仍存在於 codebase，但不再被任何 EditModal 渲染（`AdditionalSections` 已移除）：

```
Components/Pages/Purchase/PurchaseOrderEditModal/
├── PurchaseOrderReceivingTab.razor   ← 不再渲染
└── PurchaseOrderReturnTab.razor      ← 不再渲染

Components/Pages/Purchase/PurchaseReceivingEditModal/
├── PurchaseReceivingReturnTab.razor  ← 不再渲染
├── PurchaseReceivingSetoffTab.razor  ← 不再渲染
└── PurchaseReceivingOrderTab.razor   ← 不再渲染

（其餘 Sales 模組類似，各有 2-3 個棄用 Tab 元件）
```

> 這些元件仍可透過 `LoadAsync(id)` / `Clear()` 獨立運作，如未來需要在 Drawer 顯示相關記錄，可直接復用。
