# 盤點系統設計文件

## 目錄
1. [概述](#概述)
2. [為什麼需要盤點系統](#為什麼需要盤點系統)
3. [盤點類型](#盤點類型)
4. [資料表設計](#資料表設計)
5. [狀態流程](#狀態流程)
6. [核心功能](#核心功能)
7. [庫存調整邏輯](#庫存調整邏輯)
8. [報表需求](#報表需求)
9. [權限設計](#權限設計)
10. [實作順序建議](#實作順序建議)

---

## 概述

盤點系統用於核對「系統帳面庫存」與「實際庫存」的差異，並提供完整的追蹤記錄。透過盤點系統，可以：
- 發現庫存異常（盤盈/盤虧）
- 追蹤調整原因
- 提供稽核依據
- 分析庫存管理效率

---

## 為什麼需要盤點系統

### 直接修改庫存的問題

| 問題 | 說明 |
|------|------|
| 無法追蹤 | 不知道誰在什麼時候改了庫存 |
| 無差異記錄 | 不知道改了多少、原本是多少 |
| 無原因說明 | 不知道為什麼要調整 |
| 難以稽核 | 會計師無法查核庫存變動依據 |
| 無法分析 | 無法找出經常有差異的商品 |

### 盤點系統的價值

1. **完整追蹤**：記錄盤點時間、人員、差異數量
2. **差異分析**：自動計算盤盈/盤虧金額
3. **審核機制**：盤點結果需審核後才調整庫存
4. **歷史紀錄**：保留每次盤點的完整資料
5. **報表輸出**：產出盤點報表供稽核使用

---

## 盤點類型

| 類型 | 說明 | 適用場景 |
|------|------|----------|
| **全盤** | 盤點整個倉庫的所有商品 | 年度盤點、倉庫交接 |
| **抽盤** | 隨機抽選部分商品盤點 | 日常稽核、高價值商品 |
| **循環盤** | 分批輪流盤點不同品類 | 大量 SKU 的倉庫 |
| **即時盤** | 針對特定商品立即盤點 | 發現異常時 |

---

## 資料表設計

### 盤點單主檔 (InventoryCheck)

```csharp
public class InventoryCheck : BaseEntity
{
    // === 單據資訊 ===
    public string CheckNumber { get; set; }          // 盤點單號（自動產生）
    public DateTime CheckDate { get; set; }          // 盤點日期
    public InventoryCheckType CheckType { get; set; } // 盤點類型（全盤/抽盤/循環盤/即時盤）
    
    // === 倉庫範圍 ===
    public int WarehouseId { get; set; }             // 盤點倉庫
    public Warehouse Warehouse { get; set; }
    public int? WarehouseLocationId { get; set; }    // 盤點庫位（可選，空=整個倉庫）
    public WarehouseLocation? WarehouseLocation { get; set; }
    
    // === 人員資訊 ===
    public int CheckerId { get; set; }               // 盤點人員
    public Employee Checker { get; set; }
    public int? SupervisorId { get; set; }           // 監盤人員（可選）
    public Employee? Supervisor { get; set; }
    public int? ApproverId { get; set; }             // 審核人員
    public Employee? Approver { get; set; }
    
    // === 狀態與時間 ===
    public InventoryCheckStatus Status { get; set; } // 狀態
    public DateTime? ApprovedAt { get; set; }        // 審核時間
    public DateTime? PostedAt { get; set; }          // 過帳時間
    
    // === 統計資訊（過帳時計算） ===
    public int TotalItems { get; set; }              // 盤點品項數
    public int DifferenceItems { get; set; }         // 有差異品項數
    public decimal TotalDifferenceAmount { get; set; } // 差異總金額
    
    // === 備註 ===
    public string? Remarks { get; set; }
    
    // === 關聯 ===
    public ICollection<InventoryCheckDetail> Details { get; set; }
}
```

### 盤點單明細 (InventoryCheckDetail)

```csharp
public class InventoryCheckDetail : BaseEntity
{
    // === 關聯 ===
    public int InventoryCheckId { get; set; }
    public InventoryCheck InventoryCheck { get; set; }
    
    // === 商品資訊 ===
    public int ProductId { get; set; }
    public Product Product { get; set; }
    public int? WarehouseLocationId { get; set; }    // 庫位（如主檔未指定，明細可各自指定）
    public WarehouseLocation? WarehouseLocation { get; set; }
    
    // === 數量資訊 ===
    public int SystemQuantity { get; set; }          // 系統帳面數量（建立時自動帶入）
    public int? ActualQuantity { get; set; }         // 實盤數量（盤點人員輸入）
    public int DifferenceQuantity { get; set; }      // 差異數量（自動計算：實盤 - 系統）
    
    // === 金額資訊 ===
    public decimal UnitCost { get; set; }            // 單位成本（取自庫存的平均成本）
    public decimal DifferenceAmount { get; set; }    // 差異金額（差異數量 × 單位成本）
    
    // === 差異原因 ===
    public InventoryDifferenceReason? DifferenceReason { get; set; } // 差異原因
    public string? DifferenceRemarks { get; set; }   // 差異說明
    
    // === 批號追蹤（如有啟用） ===
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    
    // === 盤點狀態 ===
    public bool IsCounted { get; set; }              // 是否已盤點
    public DateTime? CountedAt { get; set; }         // 盤點時間
}
```

### 列舉定義

```csharp
/// <summary>
/// 盤點類型
/// </summary>
public enum InventoryCheckType
{
    [Display(Name = "全盤")]
    Full = 1,
    
    [Display(Name = "抽盤")]
    Spot = 2,
    
    [Display(Name = "循環盤")]
    Cycle = 3,
    
    [Display(Name = "即時盤")]
    Instant = 4
}

/// <summary>
/// 盤點單狀態
/// </summary>
public enum InventoryCheckStatus
{
    [Display(Name = "草稿")]
    Draft = 0,
    
    [Display(Name = "盤點中")]
    InProgress = 1,
    
    [Display(Name = "待審核")]
    PendingApproval = 2,
    
    [Display(Name = "已審核")]
    Approved = 3,
    
    [Display(Name = "已過帳")]
    Posted = 4,
    
    [Display(Name = "已取消")]
    Cancelled = 99
}

/// <summary>
/// 差異原因
/// </summary>
public enum InventoryDifferenceReason
{
    [Display(Name = "盤點誤差")]
    CountingError = 1,
    
    [Display(Name = "損壞報廢")]
    Damaged = 2,
    
    [Display(Name = "過期報廢")]
    Expired = 3,
    
    [Display(Name = "遺失")]
    Lost = 4,
    
    [Display(Name = "竊盜")]
    Theft = 5,
    
    [Display(Name = "入庫未登錄")]
    UnrecordedReceipt = 6,
    
    [Display(Name = "出庫未登錄")]
    UnrecordedShipment = 7,
    
    [Display(Name = "系統錯誤")]
    SystemError = 8,
    
    [Display(Name = "其他")]
    Other = 99
}
```

---

## 狀態流程

```
┌─────────┐     建立      ┌──────────┐     開始盤點     ┌──────────┐
│  草稿   │ ───────────▶ │  盤點中  │ ◀─────────────▶ │  盤點中  │
│ (Draft) │              │(InProgress)│    輸入實盤數   │(InProgress)│
└─────────┘              └──────────┘                 └──────────┘
                                                            │
                                                            │ 提交審核
                                                            ▼
┌─────────┐     過帳      ┌──────────┐      審核       ┌──────────┐
│ 已過帳  │ ◀─────────── │  已審核  │ ◀───────────── │ 待審核   │
│(Posted) │              │(Approved) │                │(Pending) │
└─────────┘              └──────────┘                 └──────────┘
     │                         │                           │
     │                         │ 退回                      │ 退回
     │                         ▼                           ▼
     │                   ┌──────────┐               ┌──────────┐
     │                   │  盤點中  │               │  盤點中  │
     │                   │(InProgress)│             │(InProgress)│
     │                   └──────────┘               └──────────┘
     │
     ▼
  調整庫存
  寫入異動記錄
```

### 狀態說明

| 狀態 | 可執行動作 | 說明 |
|------|-----------|------|
| 草稿 | 編輯、刪除、開始盤點 | 剛建立的盤點單 |
| 盤點中 | 輸入實盤數、提交審核 | 正在進行盤點作業 |
| 待審核 | 審核通過、退回 | 等待主管審核 |
| 已審核 | 過帳、退回 | 審核通過，可執行過帳 |
| 已過帳 | 列印報表 | 已調整庫存，不可修改 |
| 已取消 | 無 | 已作廢的盤點單 |

---

## 核心功能

### 1. 建立盤點單

```
流程：
1. 選擇倉庫（必填）、庫位（選填）
2. 選擇盤點類型
3. 系統自動產生盤點單號
4. 根據選擇範圍，自動帶入該倉庫/庫位的所有商品及帳面數量
```

### 2. 執行盤點

```
流程：
1. 開啟盤點單
2. 逐一輸入各商品的實盤數量
3. 系統自動計算差異數量與金額
4. 如有差異，必須選擇差異原因
5. 完成後提交審核
```

### 3. 審核盤點

```
流程：
1. 主管審核盤點結果
2. 檢視差異原因是否合理
3. 審核通過 或 退回修改
```

### 4. 過帳調整

```
流程：
1. 執行過帳動作
2. 系統自動調整庫存數量
3. 寫入庫存異動記錄（InventoryMovement）
4. 過帳後不可修改
```

---

## 庫存調整邏輯

### 過帳時的處理

```csharp
// 虛擬碼示意
public async Task PostInventoryCheckAsync(int inventoryCheckId)
{
    var check = await GetInventoryCheckWithDetailsAsync(inventoryCheckId);
    
    foreach (var detail in check.Details.Where(d => d.DifferenceQuantity != 0))
    {
        // 1. 調整庫存數量
        await AdjustInventoryStockAsync(
            productId: detail.ProductId,
            warehouseId: check.WarehouseId,
            warehouseLocationId: detail.WarehouseLocationId,
            adjustQuantity: detail.DifferenceQuantity  // 正數=盤盈，負數=盤虧
        );
        
        // 2. 寫入庫存異動記錄
        await CreateInventoryMovementAsync(new InventoryMovement
        {
            ProductId = detail.ProductId,
            WarehouseId = check.WarehouseId,
            MovementType = detail.DifferenceQuantity > 0 
                ? MovementType.AdjustIn   // 盤盈
                : MovementType.AdjustOut, // 盤虧
            Quantity = Math.Abs(detail.DifferenceQuantity),
            ReferenceType = "InventoryCheck",
            ReferenceId = check.Id,
            ReferenceNumber = check.CheckNumber,
            Remarks = $"盤點調整：{detail.DifferenceReason?.GetDisplayName()}"
        });
    }
    
    // 3. 更新盤點單狀態
    check.Status = InventoryCheckStatus.Posted;
    check.PostedAt = DateTime.Now;
    
    await SaveChangesAsync();
}
```

### 異動類型擴充

需要在 `MovementType` 列舉中新增：

```csharp
[Display(Name = "盤點盤盈")]
AdjustIn = 7,

[Display(Name = "盤點盤虧")]
AdjustOut = 8,
```

---

## 報表需求

### 1. 盤點清單（盤點前列印）

用途：提供給盤點人員攜帶至現場盤點

| 項次 | 商品編號 | 商品名稱 | 庫位 | 單位 | 帳面數量 | 實盤數量 | 差異 | 備註 |
|------|---------|---------|------|------|---------|---------|------|------|
| 1 | P001 | 商品A | A-01 | 個 | 100 | _____ | _____ | |

### 2. 盤點差異報表（盤點後產出）

用途：顯示有差異的商品，供主管審核

| 商品編號 | 商品名稱 | 帳面數量 | 實盤數量 | 差異 | 單位成本 | 差異金額 | 差異原因 |
|---------|---------|---------|---------|------|---------|---------|---------|
| P001 | 商品A | 100 | 95 | -5 | 50.00 | -250.00 | 損壞報廢 |

### 3. 盤點彙總報表

用途：統計分析用

```
盤點單號：IC-20260115-001
盤點日期：2026-01-15
倉庫：主倉庫

盤點品項數：150
有差異品項：12
無差異品項：138

盤盈品項：3     盤盈金額：+1,500.00
盤虧品項：9     盤虧金額：-8,200.00
淨差異金額：-6,700.00
```

---

## 權限設計

| 功能 | 倉管人員 | 倉管主管 | 財務人員 |
|------|---------|---------|---------|
| 建立盤點單 | ✅ | ✅ | ❌ |
| 執行盤點 | ✅ | ✅ | ❌ |
| 審核盤點 | ❌ | ✅ | ✅ |
| 過帳調整 | ❌ | ✅ | ❌ |
| 取消盤點 | ❌ | ✅ | ❌ |
| 查看報表 | ✅ | ✅ | ✅ |

---

## 實作順序建議

### 第一階段：基礎功能 ✅ 已完成

1. **資料表（已存在）**
   - `StockTaking`（盤點單主檔）- `Data/Entities/Inventory/StockTaking.cs`
   - `StockTakingDetail`（盤點單明細）- `Data/Entities/Inventory/StockTakingDetail.cs`
   - 列舉定義 - `Data/Enums/InventoryEnums.cs`

2. **Service（已存在）**
   - `IStockTakingService` - `Services/Inventory/IStockTakingService.cs`
   - `StockTakingService` - `Services/Inventory/StockTakingService.cs`

3. **UI 頁面（新建立）**
   - 盤點單列表頁 - `Components/Pages/Warehouse/StockTakingIndex.razor`
   - 盤點單編輯頁 - `Components/Pages/Warehouse/StockTakingEditModalComponent.razor`
   - 盤點明細表格 - `Components/Pages/Warehouse/StockTakingTable.razor`
   - 欄位配置 - `Components/FieldConfiguration/StockTakingFieldConfiguration.cs`

### 第二階段：核心流程

4. **自動帶入庫存**
   - 建立盤點單時，自動帶入指定倉庫的商品與帳面數量

5. **差異計算**
   - 輸入實盤數量後，自動計算差異

6. **狀態流轉**
   - 實作狀態變更邏輯

### 第三階段：過帳與報表

7. **過帳功能**
   - 調整庫存
   - 寫入異動記錄

8. **報表輸出**
   - 盤點清單
   - 差異報表

### 第四階段：進階功能

9. **條碼掃描**（選配）
   - 支援掃描商品條碼快速輸入

10. **行動裝置支援**（選配）
    - 響應式設計或獨立 APP

---

## 與現有系統整合

### 關聯的現有元件

| 元件/資料表 | 關聯說明 |
|------------|---------|
| InventoryStock | 讀取帳面數量、過帳時調整 |
| InventoryStockDetail | 細部庫位的庫存調整 |
| InventoryMovement | 過帳時寫入異動記錄 |
| Product | 商品資訊 |
| Warehouse | 倉庫資訊 |
| WarehouseLocation | 庫位資訊 |
| Employee | 盤點人員、審核人員 |

### 現有元件可複用

- `InteractiveTableComponent`：盤點明細表格
- `GenericFormComponent`：盤點單表單
- `GenericButtonComponent`：各種按鈕
- `RelatedDocumentsHelper`：關聯單據顯示

---

## 注意事項

1. **盤點期間的庫存變動**
   - 建議盤點期間暫停該倉庫的進出貨作業
   - 或在過帳時檢查是否有新的異動，提示使用者

2. **大量資料處理**
   - 商品數量多時，考慮分頁載入
   - 過帳時使用批次處理

3. **並行盤點**
   - 同一倉庫不應同時有多張進行中的盤點單
   - 建立時檢查是否有未完成的盤點

4. **成本計算**
   - 差異金額使用盤點當時的平均成本
   - 過帳後成本不隨後續進貨變動

---

## 已實作檔案結構

```
ERPCore2/
├── Components/
│   ├── FieldConfiguration/
│   │   └── StockTakingFieldConfiguration.cs     # 盤點單欄位配置
│   │
│   └── Pages/
│       └── Warehouse/
│           ├── StockTakingIndex.razor           # 盤點單列表頁
│           ├── StockTakingEditModalComponent.razor  # 盤點單編輯 Modal
│           └── StockTakingTable.razor           # 盤點明細表格（InteractiveTableComponent）
│
├── Data/
│   ├── Entities/
│   │   └── Inventory/
│   │       ├── StockTaking.cs                   # 盤點單主檔實體（已存在）
│   │       └── StockTakingDetail.cs             # 盤點單明細實體（已存在）
│   │
│   └── Enums/
│       └── InventoryEnums.cs                    # 盤點相關列舉（已存在）
│           # - StockTakingTypeEnum（盤點類型）
│           # - StockTakingStatusEnum（盤點狀態）
│           # - StockTakingDetailStatusEnum（明細狀態）
│
└── Services/
    └── Inventory/
        ├── IStockTakingService.cs               # 盤點服務介面（已存在）
        └── StockTakingService.cs                # 盤點服務實作（已存在）
```

---

## 使用方式

### 存取盤點管理頁面

網址路徑：`/stocktakings`

### 盤點流程操作

1. **新增盤點單**
   - 點擊「新增」按鈕
   - 選擇倉庫（必填）和庫位（選填）
   - 選擇盤點類型（預設為全盤）
   - 可使用「帶入倉庫商品」快速載入該倉庫的所有庫存商品

2. **執行盤點**
   - 在明細表格中輸入「實盤數量」
   - 系統自動計算差異數量與差異金額
   - 輸入實盤數量後，該項目自動標記為「已盤點」

3. **儲存與審核**
   - 儲存盤點單後，可繼續編輯或提交審核
   - 審核通過後可執行過帳，調整庫存

### StockTakingTable 組件使用說明

```razor
<StockTakingTable @ref="detailTableComponent"
                  Details="@stockTakingDetails"
                  DetailsChanged="@HandleDetailsChanged"
                  IsReadOnly="@isReadOnly"
                  SelectedWarehouseId="@selectedWarehouseId"
                  Products="@products"
                  WarehouseLocations="@warehouseLocations" />
```

**參數說明：**

| 參數 | 類型 | 說明 |
|------|------|------|
| `Details` | `List<StockTakingDetail>` | 盤點明細資料 |
| `DetailsChanged` | `EventCallback<List<StockTakingDetail>>` | 明細變更事件 |
| `IsReadOnly` | `bool` | 是否唯讀模式 |
| `SelectedWarehouseId` | `int?` | 選擇的倉庫 ID |
| `Products` | `List<Product>` | 可選擇的商品清單 |
| `WarehouseLocations` | `List<WarehouseLocation>` | 可選擇的庫位清單 |
| `AllowAddNewRow` | `bool` | 是否允許新增行（預設 true）|
| `ShowFooter` | `bool` | 是否顯示頁腳統計（預設 true）|

**公開方法：**

| 方法 | 說明 |
|------|------|
| `RefreshDetailsAsync()` | 重新載入明細資料 |
| `ValidateAsync()` | 驗證明細資料完整性 |

---

## 12. 常見問題與修復模式

### 12.1 InteractiveTableComponent 空白行不顯示問題

#### 問題描述

**症狀：** 第一次選擇倉庫時，Table 顯示「請在商品欄位搜尋選擇商品，或使用下方「帶入倉庫商品」按鈕批次載入」的提示訊息，但關閉 Modal 後再次開啟則能正常顯示可輸入的空白行。

#### 根本原因

`InteractiveTableComponent` 使用 `DataLoadCompleted` 參數來控制空白行的建立時機。關鍵邏輯位於元件的 `OnAfterRenderAsync` 方法：

```csharp
// InteractiveTableComponent.razor 核心邏輯
if (EnableAutoEmptyRow && DataLoadCompleted && !_previousDataLoadCompleted)
{
    // 只有在 DataLoadCompleted 從 false 變成 true 時才建立空白行
    EnsureEmptyRow();
}
_previousDataLoadCompleted = DataLoadCompleted;
```

**問題：** 如果 `_dataLoadCompleted` 初始值為 `true`，且在資料載入過程中從未設為 `false`，則條件 `!_previousDataLoadCompleted` 永遠為 `false`，導致永遠不會建立空白行。

#### 正確的修復模式

在 `OnParametersSetAsync` 中，於載入資料**之前**將 `_dataLoadCompleted` 設為 `false`，載入完成**之後**再設為 `true`：

```csharp
private bool _dataLoadCompleted = false;  // 初始值設為 false

protected override async Task OnParametersSetAsync()
{
    // 偵測參數變化
    bool warehouseChanged = _previousWarehouseId != SelectedWarehouseId;
    bool productsJustLoaded = _previousProductsCount == 0 && Products?.Count > 0;
    
    if (warehouseChanged || productsJustLoaded)
    {
        // ✅ 關鍵：載入前設為 false
        _dataLoadCompleted = false;
        
        // 執行非同步載入
        await LoadAvailableProductsAsync();
        
        if (warehouseChanged)
        {
            DetailItems.Clear();
            await LoadExistingDetailsAsync();
        }
        
        // ✅ 關鍵：載入後設為 true（觸發狀態轉換）
        _dataLoadCompleted = true;
        StateHasChanged();
    }
    
    // 更新追蹤變數
    _previousWarehouseId = SelectedWarehouseId;
    _previousProductsCount = Products?.Count ?? 0;
}
```

#### 重點摘要

| 項目 | 說明 |
|------|------|
| 核心概念 | `InteractiveTableComponent` 偵測的是**狀態轉換**，不是**狀態值** |
| 關鍵條件 | `DataLoadCompleted && !_previousDataLoadCompleted` 必須同時為 `true` |
| 修復方式 | 在每次載入資料前後明確設定 `_dataLoadCompleted = false → true` |
| 適用場景 | 任何使用 `InteractiveTableComponent` 且需要自動顯示空白行的明細表 |

#### 其他注意事項

1. **防止重複載入：** 使用 `_isLoadingProducts` 旗標防止並行載入
2. **正確偵測首次載入：** 使用 `_previousProductsCount == 0 && Products?.Count > 0` 偵測 Products 首次傳入
3. **StateHasChanged 時機：** 在 `_dataLoadCompleted = true` 之後呼叫，確保 UI 正確更新

