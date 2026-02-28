# 嚴重 Bug 調查紀錄

**日期：** 2026-02-28
**狀態：** ✅ 已修正（兩個獨立問題）
**影響：** 登入後畫面卡住；採購/銷貨相關索引頁面 EF Connection[20004] 錯誤

---

## 症狀描述

輸入帳號密碼後按下登入，畫面卡住不動，無法進入系統主頁。
後台 Log 出現：

```
Microsoft.EntityFrameworkCore.Database.Connection[20004]
    An error occurred using the connection to database 'ERPCore2' on server '.\SQLEXPRESS'

System.InvalidOperationException: The configured execution strategy
'SqlServerRetryingExecutionStrategy' does not support user-initiated transactions.
```

---

## 根本原因

**`Data/ServiceRegistration.cs` 中的 `EnableRetryOnFailure()` 造成全面性錯誤。**

```csharp
// 問題設定（已移除）
.EnableRetryOnFailure(
    maxRetryCount: 1,
    maxRetryDelay: TimeSpan.FromSeconds(2),
    errorNumbersToAdd: new[] { 233 })
```

`EnableRetryOnFailure()` 即使 `maxRetryCount` 很小，也會強制套用
`SqlServerRetryingExecutionStrategy`。此 strategy 的限制：

1. **禁止直接呼叫 `BeginTransactionAsync()`** → 所有交易拋出 `InvalidOperationException`
2. **Split Query 在特定情況下與此 strategy 衝突** → `DashboardService.GetEmployeePanelsAsync()` 失敗

**受影響範圍：** 全系統共 **50 個** `BeginTransactionAsync()` 呼叫全部失效，包括：
- `CompanyService.SetDefaultCompanyAsync`
- `SetoffDocumentService`（2處）
- `MaterialIssueService`（4處）
- `StockTakingService`（4處）
- `PurchaseOrderService`、`PurchaseReceivingService`、`PurchaseReturnService` 等
- `SalesOrderService`、`SalesDeliveryService`、`SalesReturnService` 等
- `InventoryStockService`（9處）

**Git 溯源：** commit `a522c14`（錯誤的開始）修改了 `ServiceRegistration.cs`，
同時修改 `DashboardService.cs` 新增 `GetEnabledModuleKeysAsync()`，
該方法呼叫的查詢在 `SqlServerRetryingExecutionStrategy` 下失敗，造成登入卡住。

---

## 修正方案

**移除 `EnableRetryOnFailure()`**（`Data/ServiceRegistration.cs`）

- `maxRetryCount` 不論設多少，只要呼叫 `EnableRetryOnFailure()`，就會套用 `SqlServerRetryingExecutionStrategy`
- 移除後，EF Core 恢復使用預設的 `NonRetryingExecutionStrategy`，完整支援所有交易
- **一行修改，修復全系統 50+ 個交易點**

---

## 調查過程（逐步縮小）

| 步驟 | 測試內容 | 結果 |
|------|---------|------|
| 1 | 新增 Debug API 端點測試 DB 連線 | ✅ DB 正常，問題不在連線本身 |
| 2 | 移除全部權限審查 | ❌ 問題依然存在 |
| 3 | 跳過 MainLayout 預載 + AuthStateProvider DB 驗證 | ❌ 問題依然存在 |
| 4 | 跳過 Home.razor 所有 DB 呼叫 | ✅ 可以登入 → 問題在首頁 DB |
| 5 | 保留 CompanyService，跳過 DashboardService | ✅ 可以登入 → 問題在 DashboardService |
| 6 | git diff 分析 DashboardService 變更 | 發現新增 `GetEnabledModuleKeysAsync()` |
| 7 | 追查 `EnableRetryOnFailure()` 影響 | **找到根本原因** |

---

## 第二個問題：採購/銷貨索引頁面 EF Connection[20004]

### 症狀

修正 `EnableRetryOnFailure` 後，登入問題解決，但導覽至以下頁面仍出現 `EF Connection[20004]`：
- 採購訂單索引、進貨索引、進貨退出索引
- 銷貨訂單索引、銷貨出貨索引、銷貨退回索引

### 根本原因：Blazor 組件循環引用

**原始碼中 EditModal 組件無條件互相嵌套**，造成無限遞歸渲染：

```
PurchaseOrderEditModal → PurchaseReceivingEditModal（無條件）
                         ↑ 也嵌套 PurchaseOrderEditModal（無條件）
                         → 無限遞歸！
```

相同問題存在於銷貨：
```
SalesOrderEditModal → SalesDeliveryEditModal（無條件）
                       ↑ 也嵌套 SalesOrderEditModal（無條件）
                       → 無限遞歸！
```

每個組件實例的 `OnInitializedAsync` 都會查詢資料庫（`SystemParameterService.IsPurchaseOrderApprovalEnabledAsync()` 等），導致連線池耗盡 → `EF Connection[20004]`。

**Git 溯源：** commit `a522c14`（錯誤的開始）同時引入了 `EnableRetryOnFailure` 和這些循環嵌套的 EditModal 結構。

### 修正方案：`@if` Guard 模式

**為所有嵌套 EditModal 加上 `@if` 條件渲染**，只在需要顯示時才實例化組件：

```razor
@* 修正前（無條件渲染，造成無限遞歸） *@
<PurchaseReceivingEditModalComponent @ref="purchaseReceivingEditModal"
                                     IsVisible="@showPurchaseReceivingModal" ... />

@* 修正後（僅在顯示時渲染，打破循環） *@
@if (showPurchaseReceivingModal)
{
    <PurchaseReceivingEditModalComponent @ref="purchaseReceivingEditModal"
                                         IsVisible="@showPurchaseReceivingModal" ... />
}
```

**使用 `@ref` 調用公開方法時需先等待渲染：**

```csharp
// @if guard 下 @ref 在組件渲染前為 null，需先設 flag 等待 Blazor 渲染
if (!showPurchaseReceivingModal)
{
    showPurchaseReceivingModal = true;
    await Task.Yield(); // 讓 Blazor 完成組件渲染
}
await purchaseReceivingEditModal!.ShowAddModalWithPrefilledOrder(supplierId, orderId);
```

### 修正的檔案

**採購模組：**
- `Components/Pages/Purchase/PurchaseOrderEditModalComponent.razor`
- `Components/Pages/Purchase/PurchaseReceivingEditModalComponent.razor`
- `Components/Pages/Purchase/PurchaseReturnEditModalComponent.razor`

**銷貨模組：**
- `Components/Pages/Sales/SalesOrderEditModalComponent.razor`
- `Components/Pages/Sales/SalesDeliveryEditModalComponent.razor`
- `Components/Pages/Sales/SalesReturnEditModalComponent.razor`

---

## 教訓

### Bug 1：`EnableRetryOnFailure()`

- EF Core 的 `EnableRetryOnFailure()` 是針對 **雲端/不穩定網路** 設計的功能，在本機 SQL Server 環境下完全不需要
- 一旦啟用，**所有** `BeginTransactionAsync()` 呼叫都必須改用 `CreateExecutionStrategy().ExecuteAsync()` 包裝，否則全部失效
- 這個 API 的設計違反直覺——小小的一行配置會靜默地破壞全系統所有交易

### Bug 2：Blazor 組件循環引用

- **相關單據 Tab 化** 功能讓 EditModal 互相能開啟對方，設計上合理，但實作時必須使用 `@if` guard
- 沒有 `@if` guard 的嵌套組件，**即使 `IsVisible=false`**，Blazor 仍會在頁面載入時建立並初始化所有組件實例
- `@if` guard 的副作用：`@ref` 在組件首次顯示前為 `null`，呼叫公開方法前需先等待渲染（`await Task.Yield()`）
- 詳細規範見 `Readme_相關單據Tab化_各模組修改計畫.md` 的「循環引用警告」章節
