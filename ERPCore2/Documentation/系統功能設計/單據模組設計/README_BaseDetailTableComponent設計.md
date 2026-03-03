# BaseDetailTableComponent 設計

## 更新日期
2026-03-03

---

## 📋 概述

`BaseDetailTableComponent` 是 7 個明細表格元件的共同抽象基底類別，
將所有模組共用的**生命週期邏輯**集中管理，子類別只需實作業務邏輯。

```
Components/Shared/Table/BaseDetailTableComponent.cs
```

---

## 🎯 設計目的

7 個模組的 Table 元件原本各自重複以下邏輯：
- `DataVersion` 追蹤：父元件遞增 → 重載明細
- `SelectedSupplierId` 追蹤：切換對象（廠商/客戶）→ 清空重載
- `_isLoadingDetails` 防重入保護
- 空白行觸發修正（`_dataLoadCompleted false→true` 轉換）
- `_hasUndeletableDetails` 計算與通知

全部提升至基底類別後，子類別 Table 只需關注業務邏輯。

---

## 📌 共用參數（子類別無需重複宣告）

```csharp
[Parameter] public TMainEntity? MainEntity { get; set; }
[Parameter] public List<TDetailEntity> ExistingDetails { get; set; } = new();
[Parameter] public EventCallback<List<TDetailEntity>> OnDetailsChanged { get; set; }
[Parameter] public bool IsReadOnly { get; set; } = false;
[Parameter] public bool IsParentLoading { get; set; } = false;
[Parameter] public bool IsApproved { get; set; } = false;
[Parameter] public int DataVersion { get; set; } = 0;
[Parameter] public int? SelectedSupplierId { get; set; }    // 也用於 SelectedCustomerId
[Parameter] public EventCallback<bool> OnHasUndeletableDetailsChanged { get; set; }
```

---

## 🔴 抽象方法（子類別必須實作）

```csharp
// 將 ExistingDetails 對應到 Items 列表（Items 已 Clear，只需 Add）
protected abstract Task LoadItemsFromDetailsAsync();

// 判斷是否有不可刪除明細（有下游記錄）
protected abstract bool ComputeHasUndeletableDetails();

// 將 Items 轉換回 TDetailEntity 列表（通知父元件）
protected abstract List<TDetailEntity> ConvertItemsToDetails();
```

### 實作範例（採購單）

```csharp
protected override async Task LoadItemsFromDetailsAsync()
{
    foreach (var detail in ExistingDetails)
    {
        Items.Add(new ProductItem
        {
            ProductId = detail.ProductId,
            Quantity  = detail.OrderQuantity,
            Price     = detail.UnitPrice,
            // ...直接存取屬性，不使用 reflection
        });
    }
}

protected override bool ComputeHasUndeletableDetails()
    => Items.Any(p => !DetailLockHelper.CanDeleteItem(p, out _, checkReceiving: true));

protected override List<PurchaseOrderDetail> ConvertItemsToDetails()
    => Items.Select(p => new PurchaseOrderDetail { ... }).ToList();
```

---

## 🟡 虛方法（子類別可選擇性覆寫）

```csharp
// SelectedSupplierId 改變時（如重新載入廠商已知商品目錄）
protected virtual Task OnCounterpartyChangedAsync() => Task.CompletedTask;

// 資料載入完成後的額外操作（如檢查歷史採購記錄）
protected virtual Task OnAfterLoadAsync() => Task.CompletedTask;
```

---

## 🟢 共用方法（子類別可直接呼叫）

```csharp
// 任何明細異動後呼叫，通知父元件並更新不可刪除狀態
protected async Task NotifyDetailsChangedAsync();

// 公開方法：重載明細（子單據儲存後供父元件呼叫）
public async Task RefreshDetailsAsync();
```

---

## ⚙️ 空白行修正機制

### 問題根源（舊設計）

```csharp
// ❌ 舊設計：ExistingDetails 為空時 early return，_dataLoadCompleted 永遠是 true
private async Task LoadExistingDetailsAsync()
{
    if (!ExistingDetails.Any()) return;  // ← 問題根源：跳過了 false→true 轉換
    _dataLoadCompleted = false;
    // ...
    _dataLoadCompleted = true;
}
```

`InteractiveTableComponent.OnAfterRenderAsync` 偵測 `_dataLoadCompleted false→true` 才補空白行，
若跳過了這個轉換，新增模式下空白行永遠不會出現。

### 修正方案（基底類別）

```csharp
private async Task InvokeLoadWithCompletionAsync()
{
    _dataLoadCompleted = false;             // ← 先設 false
    Items.Clear();
    await LoadItemsFromDetailsAsync();      // ← 子類別填充 Items
    // ...計算 hasUndeletable...
    _dataLoadCompleted = true;              // ← 設 true（即使 Items 為空也會執行）
    StateHasChanged();
}
```

無論 `ExistingDetails` 是否為空，`_dataLoadCompleted` 必然走 `false→true` 轉換，
子類別只需在 `LoadItemsFromDetailsAsync()` 中填充 `Items`，**不需要**手動呼叫 `RefreshEmptyRow()`。

---

## 🔗 繼承方式

```razor
@inherits BaseDetailTableComponent<XxxMainEntity, XxxDetailEntity, XxxTable.XxxItem>
```

### XxxItem 內部型別

```csharp
// 原本（泛型版本）
public TDetailEntity? ExistingDetailEntity { get; set; }

// 改為（具體型別）
public XxxDetailEntity? ExistingDetailEntity { get; set; }
```
