# DirtyCheck 修改規範 — EditModal 與 Table 元件

> 本文件說明「未儲存變更警告（_isDirty）」機制的架構，以及新增或修改 EditModal / Table 元件時必須遵守的規範。

---

## 一、問題背景

`GenericEditModalComponent` 使用 `_isDirty` 旗標追蹤表單是否有未儲存的變更，當使用者嘗試關閉 Modal 時，若 `_isDirty == true` 則顯示警告。

**原始設計缺陷**：`_isDirty` 只在 `HandleFieldChanged`（主表單欄位變更）時被設為 `true`。Table 明細的變更透過 `NotifyDetailsChanged()` → `OnDetailsChanged` 回呼傳遞，**完全繞過** `HandleFieldChanged`，導致明細有變更但關閉 Modal 時不顯示警告。

---

## 二、修正方案架構

採用 **Option A + 安全網** 兩層設計：

```
使用者編輯 Table 儲存格
        │
        ├─ 有 OnInputChanged → [TableComponent].Handler → NotifyDetailsChanged()
        │                                                        │
        └─ 無 OnInputChanged → InteractiveTableComponent.OnDataChanged → NotifyDetailsChanged()
                                                                 │
                                                    [EditModalComponent].HandleDetailsChanged()
                                                                 │
                                                    editModalComponent?.MarkDirty()   ← 第一層
                                                                 │
                                                         _isDirty = true ✅
```

### 第一層：GenericEditModalComponent.MarkDirty()

```csharp
// GenericEditModalComponent.razor
public void MarkDirty()
{
    _isDirty = true;
}
```

### 第二層：InteractiveTableComponent.OnDataChanged

```razor
<!-- InteractiveTableComponent.razor -->
[Parameter] public EventCallback OnDataChanged { get; set; }
```

觸發時機（任一儲存格編輯都會觸發）：
- `HandleInputChange` — 文字/數字輸入
- `HandleSelectionChange` — 下拉選單
- `HandleCheckboxChange` — 勾選框
- `HandleBuiltInDelete` — 內建刪除按鈕
- `HandleKeyboardNavigation` Enter 鍵選取
- `HandleSearchableSelectItemClick` — 搜尋選取
- `HandleTableDrop` — 拖放排序

---

## 三、已修改的元件清單

### 3-1 EditModal 元件（HandleDetailsChanged 加入 MarkDirty）

| 元件 | HandleDetailsChanged 方法 |
|------|--------------------------|
| `PurchaseOrderEditModalComponent.razor` | `HandleDetailsChanged` |
| `PurchaseReceivingEditModalComponent.razor` | `HandleReceivingDetailsChanged` |
| `PurchaseReturnEditModalComponent.razor`（若有） | 視情況 |
| `SalesOrderEditModalComponent.razor` | `HandleDetailsChanged` |
| `SalesDeliveryEditModalComponent.razor` | `HandleDeliveryDetailsChanged` |
| `QuotationEditModalComponent.razor` | `HandleQuotationDetailsChanged` |
| `SetoffDocumentEditModalComponent.razor` | `HandleProductDetailsChanged` |
| `StockTakingEditModalComponent.razor` | `HandleDetailsChanged` |
| `MaterialIssueEditModalComponent.razor` | `HandleDetailsChanged` |
| `InventoryStockEditModalComponent.razor` | `HandleStockDetailsChanged` |
| `ProductCompositionEditModalComponent.razor` | `HandleDetailsChanged` |

**每個 HandleDetailsChanged 的第一行必須是：**
```csharp
editModalComponent?.MarkDirty();
```

### 3-2 Table 元件（綁定 OnDataChanged）

| Table 元件 | OnDataChanged 綁定 |
|-----------|-------------------|
| `PurchaseOrderTable.razor` | `@NotifyDetailsChanged` |
| `PurchaseReceivingTable.razor` | `@NotifyChange` |
| `PurchaseReturnTable.razor` | `@NotifyDetailsChanged` |
| `SalesOrderTable.razor` | `@NotifyDetailsChanged` |
| `SalesDeliveryTable.razor` | `@NotifyDetailsChanged` |
| `QuotationTable.razor` | `@NotifyDetailsChanged` |
| `SalesReturnTable.razor` | `@NotifyDetailsChanged` |
| `StockTakingTable.razor` | `@NotifyDetailsChanged` |
| `InventoryStockTable.razor` | `@NotifyDetailsChanged` |
| `MaterialIssueTable.razor` | `@SyncToDetails` |
| `SetoffProductTable.razor` | `@NotifyDetailsChanged` |
| `ProductCompositionTable.razor` | `@NotifyItemsChanged` |

---

## 四、⚠️ 新增元件時的必要步驟

### 新增 EditModal 元件（含 Table 明細）

**步驟 1**：確認 `<GenericEditModalComponent>` 有 `@ref`：
```razor
<GenericEditModalComponent ... @ref="editModalComponent" ...>
```

**步驟 2**：宣告對應型別的 ref：
```csharp
private GenericEditModalComponent<MyEntity, IMyEntityService>? editModalComponent;
```

**步驟 3**：在 `HandleDetailsChanged`（或任何 Table 回呼）的**第一行**加入：
```csharp
private async Task HandleDetailsChanged(List<MyDetail> details)
{
    editModalComponent?.MarkDirty();   // ← 必須加這行
    myDetails = details;
    // ...其餘邏輯
}
```

> ⚠️ **警告**：若忘記加 `MarkDirty()`，使用者修改明細後關閉 Modal 時**不會**顯示未儲存警告，可能導致資料遺失。

---

### 新增 Table 元件（用於 EditModal 內）

**步驟 1**：在 `<InteractiveTableComponent>` 標籤加入 `OnDataChanged` 綁定：
```razor
<InteractiveTableComponent TItem="MyItem"
                           OnDataChanged="@NotifyDetailsChanged"
                           Items="@MyItems"
                           ...>
```

**步驟 2**：確認 Table 元件有對應的通知方法（名稱依各 Table 而異）：

| 通知方法常見命名 | 說明 |
|----------------|------|
| `NotifyDetailsChanged()` | 最常見，直接呼叫 `OnDetailsChanged.InvokeAsync()` |
| `NotifyChange()` | 部分 Table 使用（如 PurchaseReceivingTable） |
| `NotifyItemsChanged()` | 用於 ItemsChanged 回呼模式 |
| `SyncToDetails()` | 先整理資料再通知（如 MaterialIssueTable） |

> ⚠️ **警告**：若 `OnDataChanged` 綁定的方法不存在，會導致**編譯錯誤**。請確認方法名稱正確。

**步驟 3**：確認通知方法最終會觸發 EditModal 的 `HandleDetailsChanged`，形成完整鏈路：
```
OnDataChanged → NotifyDetailsChanged() → OnDetailsChanged.InvokeAsync()
→ EditModal.HandleDetailsChanged() → editModalComponent?.MarkDirty()
```

> ⚠️ **警告**：若 Table 元件的 `OnDetailsChanged` 參數沒有在 EditModal 中設定，`MarkDirty()` 永遠不會被呼叫。

---

## 五、常見錯誤與排查

### 問題：修改明細後關閉 Modal，沒有出現未儲存警告

排查清單（依序確認）：

- [ ] `GenericEditModalComponent` 是否開啟了 `ShowUnsavedChangesWarning`（預設應為 true）
- [ ] `EditModal` 的 `HandleDetailsChanged` 第一行是否有 `editModalComponent?.MarkDirty()`
- [ ] `Table` 的 `<InteractiveTableComponent>` 是否綁定了 `OnDataChanged="@NotifyDetailsChanged"`（或對應方法）
- [ ] Table 的 `OnDataChanged` 綁定的方法名稱是否正確（不存在會編譯錯誤，綁錯方法則靜默失敗）
- [ ] EditModal 的 `<TableComponent>` 是否有綁定 `OnDetailsChanged="@HandleDetailsChanged"`

---

### 問題：`OnDataChanged` 造成方法被呼叫兩次

**說明**：若 Table 欄位已設定 `OnInputChanged`，當使用者編輯時：
1. `OnInputChanged` → `NotifyDetailsChanged()` → 第一次呼叫 `HandleDetailsChanged`
2. `OnDataChanged` → `NotifyDetailsChanged()` → 第二次呼叫 `HandleDetailsChanged`

**是否有害**：通常**無害**（`MarkDirty()` 和金額重算都是冪等操作）。

**若要避免**：在各 Table 的通知方法加入防抖（debounce）或旗標防止重入，但目前的系統規模不需要。

---

## 六、SetoffDocumentEditModalComponent 特殊說明

`SetoffDocumentEditModalComponent` 有**多個** Table（SetoffProductTable、SetoffPaymentTable、SetoffPrepaymentTable），各有自己的 `HandleXxxDetailsChanged`：

```csharp
private void HandleProductDetailsChanged(List<SetoffProductDetail> details)
{
    editModalComponent?.MarkDirty();   // ← 已加入
    // ...
}
```

> `SetoffPaymentTable` 和 `SetoffPrepaymentTable` 若也需要 dirty 追蹤，請確認它們的回呼方法也有 `MarkDirty()`。

---

## 七、不需要修改的 Table 元件

以下 Table 元件雖使用 `InteractiveTableComponent`，但**不在 EditModal 的明細管理路徑中**，不需要 `OnDataChanged` 綁定（或已有其他 dirty 機制）：

- `CustomerTransactionTable.razor` — 唯讀查詢
- `CustomerVisitTable.razor` — 獨立頁面
- `BatchApprovalTable.razor` — 批次操作
- `ProductBarcodePrintTable.razor` — 列印功能
- `VehicleTable.razor` — 獨立管理
- `JournalEntryLineTable.razor` — 另有自己的 EditModal 機制
- `SupplierProductTable.razor`、`ProductSupplierTable.razor` — Tab 內嵌，視情況處理

> 若上述 Table 未來被嵌入支援 dirty 的 EditModal，請補充 `OnDataChanged` 綁定。
