# 欄位數字格式修改記錄

## 修改目的
實現智能數字格式化功能：
- 整數值不顯示小數點（例如：10 而非 10.0）
- 小數值正常顯示（例如：10.5）
- 0 值顯示為空字串

## 修改內容

### 1. 核心組件修改

#### 1.1 InteractiveColumnDefinition.cs
**路徑**: `Components/Shared/BaseModal/BaseTableComponent/InteractiveColumnDefinition.cs`

**新增功能**:
- `IsDisabledFunc` - 動態判斷是否禁用的函數
- `TooltipFunc` - 動態工具提示函數

```csharp
/// <summary>
/// 動態判斷是否禁用的函數（優先於 IsDisabled）
/// </summary>
public Func<object, bool>? IsDisabledFunc { get; set; }

/// <summary>
/// 動態工具提示函數（用於根據項目狀態顯示不同提示）
/// </summary>
public Func<object, string?>? TooltipFunc { get; set; }
```

#### 1.2 InteractiveTableComponent.razor
**路徑**: `Components/Shared/BaseModal/BaseTableComponent/InteractiveTableComponent.razor`

**修改內容**:
- Number 類型加入智能數字格式化邏輯
- 支援 `IsDisabledFunc` 動態禁用
- 支援 `TooltipFunc` 動態提示
- 將 `step` 固定為 `"any"` 允許任意小數輸入

**修改狀態**: ✅ 完成

---

### 2. 使用 InteractiveTableComponent 的組件清單

#### 2.1 報價單相關
| 檔案 | 路徑 | 數字欄位 | 修改狀態 |
|------|------|---------|---------|
| QuotationTable.razor | Components/Shared/BaseModal/Modals/Quotation/ | 報價數量、單價、折扣(%) | ✅ 完成 (2025/11/5) |

**修改詳情**:
- 報價數量: CustomTemplate → Number 類型
- 單價: CustomTemplate → Number 類型
- 折扣(%): CustomTemplate → Number 類型（0-100 範圍限制）
- 程式碼減少: 124 行 → 84 行 (-32%)

---

#### 2.2 採購相關
| 檔案 | 路徑 | 數字欄位 | 修改狀態 |
|------|------|---------|---------|
| PurchaseOrderTable.razor | Components/Shared/BaseModal/Modals/Purchase/ | 數量、入庫量、單價 | ✅ 完成 (2025/11/5) |
| PurchaseReceivingTable.razor | Components/Shared/BaseModal/Modals/Purchase/ | 入庫數量、單價 | ✅ 完成 (2025/11/5) |
| PurchaseReturnTable.razor | Components/Shared/BaseModal/Modals/Purchase/ | 退回數量、單價 | ✅ 完成 (2025/11/5) |

**修改詳情**:
- PurchaseOrderTable: 數量、入庫量、單價 → Number 類型
- PurchaseReceivingTable: 入庫數量、單價 → Number 類型
- PurchaseReturnTable: 退回數量、單價 → Number 類型

---

#### 2.3 銷貨相關
| 檔案 | 路徑 | 數字欄位 | 修改狀態 |
|------|------|---------|---------|
| SalesOrderTable.razor | Components/Shared/BaseModal/Modals/Sales/ | 訂單數量、單價、折扣(%) | ✅ 完成 (2025/11/5) |
| SalesReturnTable.razor | Components/Shared/BaseModal/Modals/Sales/ | 退貨數量、單價、折扣(%) | ✅ 完成 (2025/11/5) |

**修改詳情**:
- SalesOrderTable: 訂單數量、單價、折扣% → Number 類型
- SalesReturnTable: 退貨數量、單價、折扣% → Number 類型（折扣為CustomTemplate，僅格式化）

---

#### 2.4 倉庫相關
| 檔案 | 路徑 | 數字欄位 | 修改狀態 |
|------|------|---------|---------|
| InventoryStockTable.razor | Components/Shared/BaseModal/Modals/Warehouse/ | 現有庫存、平均成本、最低/最高警戒線 | ✅ 完成 (2025/11/5) |
| StockLevelAlertModalComponent.razor | Components/Shared/BaseModal/Modals/Warehouse/ | 最低量、最高量 | ✅ 完成 (原本就是 Number) |
| StockAlertViewModalComponent.razor | Components/Shared/BaseModal/Modals/Warehouse/ | - | ➖ 僅顯示，無需修改 |

**修改詳情**:
- InventoryStockTable: 現有庫存、平均成本、警戒線 → Number 類型
- StockLevelAlertModalComponent: 已使用 Number 類型

---

#### 2.5 供應商相關
| 檔案 | 路徑 | 數字欄位 | 修改狀態 |
|------|------|---------|---------|
| SupplierProductTable.razor | Components/Shared/BaseModal/Modals/Supplier/ | 報價、交期、最小訂購量 | ➖ 使用泛型委託，保持 CustomTemplate |

---

#### 2.6 沖銷相關
| 檔案 | 路徑 | 數字欄位 | 修改狀態 |
|------|------|---------|---------|
| SetoffPaymentTable.razor | Components/Shared/BaseModal/Modals/Setoff/ | 收/付款金額、折讓金額 | ✅ 完成 (2025/11/5) |
| SetoffProductTable.razor | Components/Shared/BaseModal/Modals/Setoff/ | 本次沖款、本次折讓 | ➖ 有退貨負數邏輯，保持 CustomTemplate |
| SetoffPrepaymentTable.razor | Components/Shared/BaseModal/Modals/Setoff/ | 金額 | ➖ 有轉帳類型判斷邏輯，保持 CustomTemplate |

**修改詳情**:
- SetoffPaymentTable: 收/付款金額、折讓金額 → Number 類型

---

#### 2.7 其他
| 檔案 | 路徑 | 數字欄位 | 修改狀態 |
|------|------|---------|---------|
| MaterialIssueTable.razor | Components/Shared/BaseModal/Modals/MaterialIssue/ | 領貨數量 | ✅ 完成 (2025/11/5) |
| ProductCompositionTable.razor | Components/Shared/BaseModal/Modals/Product/ | 所需數量 | ✅ 完成 (2025/11/5) |
| SupplierProductTable.razor | Components/Shared/BaseModal/Modals/Supplier/ | 報價、交期、最小訂購量 | ➖ 使用泛型委託，保持 CustomTemplate |
| ProductBarcodePrintTable.razor | Components/Shared/BaseModal/Modals/Product/ | - | ➖ 無數字欄位 |

**修改詳情**:
- 領料單: 領貨數量 → Number 類型
- 商品組成: 所需數量 → Number 類型

---

## 修改範本

### 使用 Number 類型的標準寫法

```csharp
// 數量欄位範例
columns.Add(new InteractiveColumnDefinition
{ 
    Title = "數量", 
    PropertyName = "Quantity",  // 綁定實體屬性
    ColumnType = InteractiveColumnType.Number,
    Width = "100px",
    Tooltip = "輸入數量",
    CellCssClass = "text-end",  // 靠右對齊
    MinValue = 0,
    // 動態禁用判斷（可選）
    IsDisabledFunc = item =>
    {
        var myItem = (MyItemType)item;
        return myItem.SomeCondition;
    },
    // 動態提示（可選）
    TooltipFunc = item =>
    {
        var myItem = (MyItemType)item;
        if (myItem.SomeCondition)
            return "此項目已鎖定，無法修改";
        return "輸入數量";
    },
    // 輸入事件處理
    OnInputChanged = EventCallback.Factory.Create<(object, string?)>(this, async args =>
    {
        var myItem = (MyItemType)args.Item1;
        await OnQuantityInput(myItem, args.Item2);
    })
});
```

### CustomTemplate 改為 Number 的對照

**改之前（CustomTemplate）**:
```csharp
columns.Add(new InteractiveColumnDefinition
{ 
    Title = "數量", 
    PropertyName = "",
    ColumnType = InteractiveColumnType.Custom,
    CustomTemplate = item => 
    {
        var myItem = (MyItemType)item;
        var value = myItem.Quantity > 0 ? myItem.Quantity.ToString() : "";
        
        return @<input type="number" class="form-control text-end" 
                       value="@value"
                       @oninput="(e) => OnQuantityInput(myItem, e.Value?.ToString())"
                       min="0" 
                       step="1" />;
    }
});
```

**改之後（Number 類型）**:
```csharp
columns.Add(new InteractiveColumnDefinition
{ 
    Title = "數量", 
    PropertyName = "Quantity",
    ColumnType = InteractiveColumnType.Number,
    Width = "100px",
    CellCssClass = "text-end",
    MinValue = 0,
    OnInputChanged = EventCallback.Factory.Create<(object, string?)>(this, async args =>
    {
        var myItem = (MyItemType)args.Item1;
        await OnQuantityInput(myItem, args.Item2);
    })
});
```

---

## 優勢總結

### 1. 編號簡化
- 減少重複的格式化邏輯
- CustomTemplate 平均減少 30-40% 編號

### 2. 一致性
- 所有數字欄位使用相同的格式化規則
- 統一的用戶體驗

### 3. 可維護性
- 集中管理數字格式化邏輯
- 修改一處即可影響全系統

### 4. 擴展性
- `IsDisabledFunc` 支援動態禁用控制
- `TooltipFunc` 支援動態提示訊息
- 未來新增功能更容易

---

## 修改進度追蹤

- ✅ 已完成: 12/16 (75.0%)
- ➖ 保持 CustomTemplate (有特殊邏輯): 4/16 (25.0%)

**最後更新**: 2025/11/5

---

## 注意事項

1. **CustomTemplate 保留情境**:
   - 需要顯示唯讀狀態的特殊樣式
   - 需要在輸入框旁顯示額外資訊
   - 有複雜的條件式渲染邏輯

2. **Number 類型適用情境**:
   - 標準的數字輸入欄位
   - 需要動態禁用控制
   - 需要智能數字格式化

3. **測試重點**:
   - 整數值是否正確顯示（不含小數點）
   - 小數值是否正確顯示
   - 動態禁用是否正常運作
   - 事件處理是否正確觸發

---

## 修改記錄

| 日期 | 檔案 | 修改者 | 說明 |
|------|------|--------|------|
| 2025/11/5 | InteractiveColumnDefinition.cs | - | 新增 IsDisabledFunc 和 TooltipFunc |
| 2025/11/5 | InteractiveTableComponent.razor | - | Number 類型加入智能格式化 |
| 2025/11/5 | QuotationTable.razor | - | 數量、單價、折扣改用 Number 類型 |
| 2025/11/5 | SalesOrderTable.razor | - | 訂單數量、單價、折扣改用 Number 類型 |
| 2025/11/5 | SalesReturnTable.razor | - | 退貨數量、單價改用 Number 類型 |
| 2025/11/5 | PurchaseOrderTable.razor | - | 數量、入庫量、單價改用 Number 類型 |
| 2025/11/5 | PurchaseReceivingTable.razor | - | 入庫數量、單價改用 Number 類型 |
| 2025/11/5 | PurchaseReturnTable.razor | - | 退回數量、單價改用 Number 類型 |
| 2025/11/5 | InventoryStockTable.razor | - | 現有庫存、平均成本、警戒線改用 Number 類型 |
| 2025/11/5 | MaterialIssueTable.razor | - | 領貨數量改用 Number 類型 |
| 2025/11/5 | ProductCompositionTable.razor | - | 所需數量改用 Number 類型 |
| 2025/11/5 | SetoffPaymentTable.razor | - | 收/付款金額、折讓金額改用 Number 類型 |

