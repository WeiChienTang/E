# 庫存異動主明細重構設計文件

## 目錄
1. [概述](#概述)
2. [現有架構分析](#現有架構分析)
3. [新架構設計](#新架構設計)
4. [資料表設計](#資料表設計)
5. [影響範圍](#影響範圍)
6. [實作步驟](#實作步驟)
7. [資料遷移策略](#資料遷移策略)
8. [測試計畫](#測試計畫)
9. [2026-01-19 重大修復記錄](#2026-01-19-重大修復記錄)
10. [技術要點備忘](#技術要點備忘)

---

## 概述

### 背景
現有的 `InventoryTransaction` 採用平面結構，每個商品的庫存異動都是獨立的一筆記錄。當一張單據（如採購進貨單）有 100 筆商品時，會產生 100 筆獨立的異動記錄，造成以下問題：

1. **資料冗餘**：相同單據資訊重複儲存
2. **查詢效率低**：需要透過 `TransactionNumber` 群組才能看到完整單據
3. **追蹤困難**：無法直接關聯到來源單據
4. **不符合業務邏輯**：實際上一張單據應該對應一筆主檔 + 多筆明細

### 目標
重構為主/明細（Master/Detail）結構，與採購單、銷貨單等單據設計保持一致。

---

## 現有架構分析

### 現有資料表結構

```
InventoryTransaction (平面結構)
├── Id
├── TransactionNumber: "PR-001"
├── TransactionType
├── TransactionDate
├── ProductId          ← 每筆記錄一個商品
├── WarehouseId
├── WarehouseLocationId
├── Quantity
├── UnitCost
├── StockBefore
├── StockAfter
├── Remarks
└── ...
```

### 現有問題示例

採購進貨單 `PR-001` 有 3 個商品：

| Id | TransactionNumber | ProductId | Quantity | 問題 |
|----|-------------------|-----------|----------|------|
| 1 | PR-001 | 商品A | 10 | 單據資訊重複 |
| 2 | PR-001 | 商品B | 20 | 單據資訊重複 |
| 3 | PR-001 | 商品C | 30 | 單據資訊重複 |

---

## 新架構設計

### 新資料表結構

```
InventoryTransaction (主檔)
├── Id
├── TransactionNumber: "PR-001"
├── TransactionType
├── TransactionDate
├── SourceDocumentType: "PurchaseReceiving"  ← 新增
├── SourceDocumentId: 123                     ← 新增
├── WarehouseId
├── TotalQuantity                             ← 新增
├── TotalAmount                               ← 新增
├── Remarks
└── Details: [                                ← 新增關聯
      InventoryTransactionDetail (明細檔)
      ├── Id, TransactionId
      ├── ProductId, WarehouseLocationId
      ├── Quantity, UnitCost
      ├── StockBefore, StockAfter
      └── ...
    ]
```

### 新架構示例

**主檔 (InventoryTransaction)**

| Id | TransactionNumber | SourceDocumentType | SourceDocumentId | TotalQuantity |
|----|-------------------|-------------------|------------------|---------------|
| 1 | PR-001 | PurchaseReceiving | 123 | 60 |

**明細檔 (InventoryTransactionDetail)**

| Id | TransactionId | ProductId | Quantity | StockBefore | StockAfter |
|----|---------------|-----------|----------|-------------|------------|
| 1 | 1 | 商品A | 10 | 0 | 10 |
| 2 | 1 | 商品B | 20 | 5 | 25 |
| 3 | 1 | 商品C | 30 | 10 | 40 |

---

## 資料表設計

### InventoryTransaction（主檔）修改

```csharp
public class InventoryTransaction : BaseEntity
{
    // === 原有欄位（保留） ===
    [Required]
    [MaxLength(30)]
    public string TransactionNumber { get; set; } = string.Empty;
    
    [Required]
    public InventoryTransactionTypeEnum TransactionType { get; set; }
    
    [Required]
    public DateTime TransactionDate { get; set; } = DateTime.Now;
    
    [Required]
    public int WarehouseId { get; set; }
    
    
    // === 新增欄位 ===
    
    /// <summary>
    /// 來源單據類型（PurchaseReceiving、SalesDelivery、StockTaking 等）
    /// </summary>
    [MaxLength(50)]
    public string? SourceDocumentType { get; set; }
    
    /// <summary>
    /// 來源單據 ID
    /// </summary>
    public int? SourceDocumentId { get; set; }
    
    /// <summary>
    /// 總數量（所有明細的數量加總）
    /// </summary>
    public decimal TotalQuantity { get; set; }
    
    /// <summary>
    /// 總金額（所有明細的金額加總）
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// 經辦人員
    /// </summary>
    public int? EmployeeId { get; set; }
    
    // === 移除的欄位（移至明細） ===
    // - ProductId（移至明細）
    // - WarehouseLocationId（移至明細）
    // - Quantity（移至明細，主檔保留 TotalQuantity）
    // - UnitCost（移至明細）
    // - StockBefore（移至明細）
    // - StockAfter（移至明細）
    // - 批號相關欄位（移至明細）
    
    // === 導航屬性 ===
    public Warehouse Warehouse { get; set; } = null!;
    public Employee? Employee { get; set; }
    public ICollection<InventoryTransactionDetail> Details { get; set; } = new List<InventoryTransactionDetail>();
}
```

### InventoryTransactionDetail（明細檔）新增

```csharp
public class InventoryTransactionDetail : BaseEntity
{
    // === 關聯欄位 ===
    [Required]
    public int InventoryTransactionId { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
    public int? WarehouseLocationId { get; set; }
    
    // === 數量與金額 ===
    [Required]
    public decimal Quantity { get; set; }
    
    public decimal? UnitCost { get; set; }
    
    public decimal Amount { get; set; }
    
    // === 庫存追蹤 ===
    public decimal StockBefore { get; set; }
    
    public decimal StockAfter { get; set; }
    
    // === 批號追蹤 ===
    [MaxLength(50)]
    public string? BatchNumber { get; set; }
    
    public DateTime? BatchDate { get; set; }
    
    public DateTime? ExpiryDate { get; set; }
    
    // === 來源明細關聯（選填） ===
    public int? SourceDetailId { get; set; }
    
    // === 備註 ===
    public string? Remarks { get; set; }
    
    // === 導航屬性 ===
    public InventoryTransaction InventoryTransaction { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public WarehouseLocation? WarehouseLocation { get; set; }
}
```

### 來源單據類型列舉

```csharp
/// <summary>
/// 庫存異動來源單據類型
/// </summary>
public static class InventorySourceDocumentTypes
{
    public const string PurchaseReceiving = "PurchaseReceiving";       // 採購進貨
    public const string PurchaseReturn = "PurchaseReturn";             // 採購退貨
    public const string SalesDelivery = "SalesDelivery";               // 銷貨出貨
    public const string SalesReturn = "SalesReturn";                   // 銷貨退回
    public const string StockTaking = "StockTaking";                   // 盤點調整
    public const string MaterialIssue = "MaterialIssue";               // 領料出庫
    public const string Transfer = "Transfer";                         // 調撥
    public const string Adjustment = "Adjustment";                     // 手動調整
    public const string Initial = "Initial";                           // 期初建立
}
```

---

## 影響範圍

### Entity 層

| 檔案 | 修改類型 | 說明 |
|------|----------|------|
| `InventoryTransaction.cs` | 修改 | 移除商品欄位，新增來源單據欄位 |
| `InventoryTransactionDetail.cs` | 新增 | 明細檔實體 |
| `AppDbContext.cs` | 修改 | 新增 DbSet、設定關聯 |

### Service 層

| 檔案 | 修改類型 | 說明 |
|------|----------|------|
| `IInventoryTransactionService.cs` | 修改 | 調整介面方法 |
| `InventoryTransactionService.cs` | 修改 | 重構所有異動建立邏輯 |
| `IInventoryStockService.cs` | 修改 | 調整介面方法 |
| `InventoryStockService.cs` | 修改 | 調整異動記錄建立邏輯 |
| `PurchaseReceivingService.cs` | 修改 | 調整進貨異動建立 |
| `PurchaseReturnService.cs` | 修改 | 調整退貨異動建立 |
| `SalesDeliveryService.cs` | 修改 | 調整出貨異動建立 |
| `SalesReturnService.cs` | 修改 | 調整退回異動建立 |
| `StockTakingService.cs` | 修改 | 調整盤點異動建立 |
| `MaterialIssueService.cs` | 修改 | 調整領料異動建立 |

### UI 層

| 檔案 | 修改類型 | 說明 |
|------|----------|------|
| `InventoryTransactionEditModalComponent.razor` | 修改 | 支援主/明細編輯 |
| `InventoryTransactionTable.razor` | 新增 | 明細表格元件 |
| `InventoryTransactionFieldConfiguration.cs` | 修改 | 欄位配置調整 |
| `InventoryTransactionIndex.razor` | 修改 | 列表顯示調整 |

### Migration

| 檔案 | 說明 |
|------|------|
| 新增 Migration | 資料表結構變更 + 資料遷移 |

---

## 實作步驟

### 第一階段：Entity 層修改

- [ ] 1.1 新增 `InventoryTransactionDetail.cs` 明細檔實體
- [ ] 1.2 修改 `InventoryTransaction.cs` 主檔結構
- [ ] 1.3 新增 `InventorySourceDocumentTypes.cs` 常數類別
- [ ] 1.4 修改 `AppDbContext.cs` 設定關聯
- [ ] 1.5 建立 Migration 並執行

### 第二階段：Service 層修改

- [ ] 2.1 修改 `IInventoryTransactionService.cs` 介面
- [ ] 2.2 重構 `InventoryTransactionService.cs`
- [ ] 2.3 修改 `IInventoryStockService.cs` 介面
- [ ] 2.4 修改 `InventoryStockService.cs` 異動記錄建立邏輯
- [ ] 2.5 修改 `PurchaseReceivingService.cs`
- [ ] 2.6 修改 `PurchaseReturnService.cs`
- [ ] 2.7 修改 `SalesDeliveryService.cs`
- [ ] 2.8 修改 `SalesReturnService.cs`
- [ ] 2.9 修改 `StockTakingService.cs`
- [ ] 2.10 修改 `MaterialIssueService.cs`

### 第三階段：UI 層修改

- [ ] 3.1 新增 `InventoryTransactionTable.razor`（明細表格元件，參考下方設計）
- [ ] 3.2 修改 `InventoryTransactionEditModalComponent.razor`
- [ ] 3.3 修改 `InventoryTransactionFieldConfiguration.cs`
- [ ] 3.4 修改 `InventoryTransactionIndex.razor`

---

## InventoryTransactionTable 設計

### 設計原則

1. **純唯讀顯示**：所有欄位都只能閱讀，不提供編輯功能
2. **統一風格**：套用 `InteractiveTableComponent` 統一 UI
3. **參考設計**：依照 `PurchaseOrderTable.razor` 的結構設計

### 元件結構

```razor
@* 庫存異動明細表格組件 - 使用 InteractiveTableComponent 統一UI *@
@* 注意：此元件為純唯讀顯示，不提供編輯功能 *@

@using ERPCore2.Helpers
@inject IProductService ProductService

<div class="card border-0 shadow-sm">
    <div class="card-body p-0">        
        <InteractiveTableComponent @ref="tableComponent"
                                  TItem="InventoryTransactionDetailItem"
                                  Items="@DetailItems"
                                  ColumnDefinitions="@GetColumnDefinitions()"
                                  ShowRowNumbers="true"
                                  ShowActions="false"
                                  ShowBuiltInActions="false"
                                  EnableAutoEmptyRow="false"
                                  IsReadOnly="true"
                                  EmptyMessage="沒有異動明細" />
    </div>
    
    <div class="card-footer">
        <div class="d-flex justify-content-between">
            <span>共 @DetailItems.Count 筆明細</span>
            <span class="fw-bold">總數量：@GetTotalQuantity().ToString("N2")</span>
        </div>
    </div>
</div>

@code {
    // ===== 參數 =====
    [Parameter] public List<InventoryTransactionDetail> Details { get; set; } = new();
    [Parameter] public List<Product> Products { get; set; } = new();
    
    // ===== InteractiveTableComponent 參考 =====
    private InteractiveTableComponent<InventoryTransactionDetailItem>? tableComponent;
    
    private List<InventoryTransactionDetailItem> DetailItems { get; set; } = new();
    
    protected override void OnParametersSet()
    {
        LoadDetails();
    }
    
    private void LoadDetails()
    {
        DetailItems = Details.Select(d => new InventoryTransactionDetailItem
        {
            Detail = d,
            Product = Products.FirstOrDefault(p => p.Id == d.ProductId)
        }).ToList();
    }
    
    private decimal GetTotalQuantity()
    {
        return DetailItems.Sum(d => d.Detail?.Quantity ?? 0);
    }
    
    // ===== 欄位定義（全部唯讀） =====
    private List<InteractiveColumnDefinition> GetColumnDefinitions()
    {
        return new List<InteractiveColumnDefinition>
        {
            // 商品編號
            new()
            {
                Title = "商品編號",
                PropertyName = "ProductCode",
                ColumnType = InteractiveColumnType.Display,
                Width = "120px",
                Tooltip = "商品編號",
                IsReadOnly = true
            },
            // 商品名稱
            new()
            {
                Title = "商品名稱",
                PropertyName = "ProductName",
                ColumnType = InteractiveColumnType.Display,
                Width = "200px",
                Tooltip = "商品名稱",
                IsReadOnly = true
            },
            // 庫位
            new()
            {
                Title = "庫位",
                PropertyName = "WarehouseLocationName",
                ColumnType = InteractiveColumnType.Display,
                Width = "100px",
                Tooltip = "存放庫位",
                IsReadOnly = true
            },
            // 數量
            new()
            {
                Title = "數量",
                PropertyName = "QuantityDisplay",
                ColumnType = InteractiveColumnType.Display,
                Width = "100px",
                Tooltip = "異動數量（正數為入庫，負數為出庫）",
                TextAlign = "right",
                IsReadOnly = true
            },
            // 單位
            new()
            {
                Title = "單位",
                PropertyName = "UnitName",
                ColumnType = InteractiveColumnType.Display,
                Width = "60px",
                Tooltip = "計量單位",
                IsReadOnly = true
            },
            // 單價
            new()
            {
                Title = "單價",
                PropertyName = "UnitCostDisplay",
                ColumnType = InteractiveColumnType.Display,
                Width = "100px",
                Tooltip = "單位成本",
                TextAlign = "right",
                IsReadOnly = true
            },
            // 金額
            new()
            {
                Title = "金額",
                PropertyName = "AmountDisplay",
                ColumnType = InteractiveColumnType.Display,
                Width = "120px",
                Tooltip = "異動金額",
                TextAlign = "right",
                IsReadOnly = true
            },
            // 異動前庫存
            new()
            {
                Title = "異動前",
                PropertyName = "StockBeforeDisplay",
                ColumnType = InteractiveColumnType.Display,
                Width = "100px",
                Tooltip = "異動前庫存數量",
                TextAlign = "right",
                IsReadOnly = true
            },
            // 異動後庫存
            new()
            {
                Title = "異動後",
                PropertyName = "StockAfterDisplay",
                ColumnType = InteractiveColumnType.Display,
                Width = "100px",
                Tooltip = "異動後庫存數量",
                TextAlign = "right",
                IsReadOnly = true
            },
            // 批號
            new()
            {
                Title = "批號",
                PropertyName = "BatchNumber",
                ColumnType = InteractiveColumnType.Display,
                Width = "100px",
                Tooltip = "批號",
                IsReadOnly = true,
                HideOnMobile = true
            },
            // 備註
            new()
            {
                Title = "備註",
                PropertyName = "Remarks",
                ColumnType = InteractiveColumnType.Display,
                Width = "150px",
                Tooltip = "明細備註",
                IsReadOnly = true,
                HideOnMobile = true
            }
        };
    }
    
    /// <summary>
    /// 明細顯示項目（供 InteractiveTableComponent 使用）
    /// </summary>
    public class InventoryTransactionDetailItem
    {
        public InventoryTransactionDetail? Detail { get; set; }
        public Product? Product { get; set; }
        
        // 顯示屬性
        public string ProductCode => Product?.Code ?? string.Empty;
        public string ProductName => Product?.Name ?? string.Empty;
        public string UnitName => Product?.Unit?.Name ?? string.Empty;
        public string WarehouseLocationName => Detail?.WarehouseLocation?.Name ?? string.Empty;
        
        // 數字顯示（格式化）
        public string QuantityDisplay => (Detail?.Quantity ?? 0).ToString("N2");
        public string UnitCostDisplay => (Detail?.UnitCost ?? 0).ToString("N2");
        public string AmountDisplay => (Detail?.Amount ?? 0).ToString("N2");
        public string StockBeforeDisplay => (Detail?.StockBefore ?? 0).ToString("N2");
        public string StockAfterDisplay => (Detail?.StockAfter ?? 0).ToString("N2");
        
        // 直接屬性
        public string? BatchNumber => Detail?.BatchNumber;
        public string? Remarks => Detail?.Remarks;
    }
}
```

### 欄位說明

| 欄位 | 寬度 | 說明 | 備註 |
|------|------|------|------|
| 商品編號 | 120px | 商品的編號 | 唯讀 |
| 商品名稱 | 200px | 商品的名稱 | 唯讀 |
| 庫位 | 100px | 存放的庫位名稱 | 唯讀 |
| 數量 | 100px | 異動數量（正入負出） | 唯讀、右對齊 |
| 單位 | 60px | 計量單位 | 唯讀 |
| 單價 | 100px | 單位成本 | 唯讀、右對齊 |
| 金額 | 120px | 異動金額 | 唯讀、右對齊 |
| 異動前 | 100px | 異動前庫存 | 唯讀、右對齊 |
| 異動後 | 100px | 異動後庫存 | 唯讀、右對齊 |
| 批號 | 100px | 批號資訊 | 唯讀、手機隱藏 |
| 備註 | 150px | 明細備註 | 唯讀、手機隱藏 |

### 與 PurchaseOrderTable 的差異

| 項目 | PurchaseOrderTable | InventoryTransactionTable |
|------|-------------------|---------------------------|
| 用途 | 編輯採購明細 | 顯示異動明細 |
| 編輯模式 | 可編輯 | **純唯讀** |
| 自動空行 | `EnableAutoEmptyRow="true"` | `EnableAutoEmptyRow="false"` |
| 操作按鈕 | 刪除/查看 | **無** |
| SearchableSelect | 有（商品搜尋） | **無** |
| 數量輸入 | 可輸入 | 純顯示 |
| 價格輸入 | 可輸入 | 純顯示 |

### 關鍵設定

```razor
<InteractiveTableComponent
    ...
    ShowActions="false"              <!-- 不顯示操作欄 -->
    ShowBuiltInActions="false"       <!-- 不顯示內建操作按鈕 -->
    EnableAutoEmptyRow="false"       <!-- 不自動新增空行 -->
    IsReadOnly="true"                <!-- 設為唯讀模式 -->
    ...
/>
```

### 在 EditModal 中使用

```razor
@* InventoryTransactionEditModalComponent.razor *@

<GenericEditModalComponent TEntity="InventoryTransaction" ...>
    <CustomModules>
        @if (entity?.Id > 0)
        {
            <div class="mt-3">
                <h6 class="mb-2">
                    <i class="bi bi-list-ul me-1"></i>
                    異動明細
                </h6>
                <InventoryTransactionTable 
                    Details="@entity.Details?.ToList() ?? new()"
                    Products="@Products" />
            </div>
        }
    </CustomModules>
</GenericEditModalComponent>
```

### 第四階段：測試與驗證

- [ ] 4.1 測試採購進貨流程
- [ ] 4.2 測試採購退貨流程
- [ ] 4.3 測試銷貨出貨流程
- [ ] 4.4 測試銷貨退回流程
- [ ] 4.5 測試盤點調整流程
- [ ] 4.6 測試領料出庫流程
- [ ] 4.7 測試刪除回滾功能

---

## 資料遷移策略

### 方案：清空現有資料

由於系統仍在測試階段，建議：

1. **備份現有資料**（以防萬一）
2. **清空 InventoryTransaction 表**
3. **建立新的表結構**
4. **從頭開始建立測試資料**

### Migration 腳本（概念）

```sql
-- 1. 備份現有資料
SELECT * INTO InventoryTransaction_Backup FROM InventoryTransactions;

-- 2. 刪除現有資料
DELETE FROM InventoryTransactions;

-- 3. 修改主表結構
ALTER TABLE InventoryTransactions
    DROP COLUMN ProductId,
    DROP COLUMN WarehouseLocationId,
    DROP COLUMN Quantity,
    DROP COLUMN UnitCost,
    DROP COLUMN StockBefore,
    DROP COLUMN StockAfter,
    DROP COLUMN TransactionBatchNumber,
    DROP COLUMN TransactionBatchDate,
    DROP COLUMN TransactionExpiryDate,
    DROP COLUMN InventoryStockId,
    DROP COLUMN InventoryStockDetailId;

ALTER TABLE InventoryTransactions
    ADD SourceDocumentType NVARCHAR(50) NULL,
    ADD SourceDocumentId INT NULL,
    ADD TotalQuantity DECIMAL(18,4) NOT NULL DEFAULT 0,
    ADD TotalAmount DECIMAL(18,4) NOT NULL DEFAULT 0,
    ADD EmployeeId INT NULL;

-- 4. 建立明細表
CREATE TABLE InventoryTransactionDetails (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    InventoryTransactionId INT NOT NULL,
    ProductId INT NOT NULL,
    WarehouseLocationId INT NULL,
    Quantity DECIMAL(18,4) NOT NULL,
    UnitCost DECIMAL(18,4) NULL,
    Amount DECIMAL(18,4) NOT NULL DEFAULT 0,
    StockBefore DECIMAL(18,4) NOT NULL DEFAULT 0,
    StockAfter DECIMAL(18,4) NOT NULL DEFAULT 0,
    BatchNumber NVARCHAR(50) NULL,
    BatchDate DATETIME NULL,
    ExpiryDate DATETIME NULL,
    SourceDetailId INT NULL,
    Remarks NVARCHAR(MAX) NULL,
    Status INT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (InventoryTransactionId) REFERENCES InventoryTransactions(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id),
    FOREIGN KEY (WarehouseLocationId) REFERENCES WarehouseLocations(Id)
);
```

---

## 測試計畫

### 功能測試

| 測試項目 | 測試內容 | 預期結果 |
|----------|----------|----------|
| 採購進貨 | 進貨 3 個商品 | 建立 1 筆主檔 + 3 筆明細 |
| 採購退貨 | 退貨 2 個商品 | 建立 1 筆主檔 + 2 筆明細，庫存減少 |
| 銷貨出貨 | 出貨 5 個商品 | 建立 1 筆主檔 + 5 筆明細，庫存減少 |
| 銷貨退回 | 退回 1 個商品 | 建立 1 筆主檔 + 1 筆明細，庫存增加 |
| 盤點調整 | 調整 10 個商品 | 建立 1 筆主檔 + 10 筆明細 |
| 領料出庫 | 領料 4 個商品 | 建立 1 筆主檔 + 4 筆明細，庫存減少 |

### 刪除回滾測試

| 測試項目 | 測試內容 | 預期結果 |
|----------|----------|----------|
| 刪除進貨單 | 刪除已入庫的進貨單 | 庫存還原，異動記錄刪除 |
| 刪除出貨單 | 刪除已出庫的出貨單 | 庫存還原，異動記錄刪除 |
| 刪除盤點單 | 刪除已調整的盤點單 | 庫存還原，異動記錄刪除 |

### 查詢測試

| 測試項目 | 測試內容 | 預期結果 |
|----------|----------|----------|
| 依單據查詢 | 查詢某進貨單的異動 | 顯示主檔 + 所有明細 |
| 依商品查詢 | 查詢某商品的異動歷史 | 顯示所有相關明細 |
| 依日期查詢 | 查詢某日期區間的異動 | 顯示所有相關主檔 |

---

## 注意事項

1. **向下相容**：新架構需要相容現有的業務流程
2. **交易一致性**：所有庫存異動必須在交易中執行
3. **效能考量**：為常用查詢欄位建立索引
4. **錯誤處理**：異動失敗時需要完整回滾

---

## 時間估計

| 階段 | 預估時間 |
|------|----------|
| Entity 層修改 | 1-2 小時 |
| Service 層修改 | 3-4 小時 |
| UI 層修改 | 2-3 小時 |
| 測試與驗證 | 2-3 小時 |
| **總計** | **8-12 小時** |

---

## 版本記錄

| 版本 | 日期 | 說明 |
|------|------|------|
| 1.0 | 2026-01-19 | 初版設計文件 |
| 2.0 | 2026-01-20 | 完成 Entity 層和 Service 層實作 |
| 2.1 | 2026-01-19 | 修復重大 Bug：倉庫位置 ID null 比較問題、刪除重複扣減問題、關聯查看功能 |

---

## 實作完成狀態

### ✅ 已完成項目

#### 第一階段：Entity 層（已完成）
- [x] 建立 `InventoryTransactionDetail.cs` 明細檔實體
- [x] 建立 `InventorySourceDocumentTypes.cs` 來源單據類型常數
- [x] 修改 `InventoryTransaction.cs` 主檔結構
  - 移除：ProductId, Quantity, UnitCost, StockBefore, StockAfter, 批號欄位
  - 新增：SourceDocumentType, SourceDocumentId, TotalQuantity, TotalAmount, EmployeeId
  - 新增：Details 集合導航屬性
- [x] 修改 `AppDbContext.cs` 設定關聯
- [x] 修改 `InventoryStockDetail.cs` 導航屬性
- [x] 建立資料庫遷移

#### 第二階段：Service 層（已完成）
- [x] 重寫 `InventoryTransactionFieldConfiguration.cs`
- [x] 重寫 `InventoryStockService.cs`
  - AddStockAsync 新增 sourceDocumentType/Id/DetailId 參數
  - ReduceStockAsync 新增 sourceDocumentType/Id/DetailId 參數
  - 新增 GetOrCreateTransactionAsync 輔助方法
  - 更新 RevertStockToOriginalAsync
  - 更新 ReduceStockFromSpecificBatchAsync
- [x] 更新 `IInventoryStockService.cs` 介面
- [x] 重寫 `InventoryTransactionService.cs`
  - 所有查詢方法改用 Details 集合
  - 將異動建立方法標記為過時
  - 新增 GetBySourceDocumentAsync 方法
- [x] 更新 `IInventoryTransactionService.cs` 介面
- [x] 修復 `SalesDeliveryService.cs` - UpdateInventoryByDifferenceAsync
- [x] 修復 `PurchaseReceivingService.cs` - UpdateInventoryByDifferenceAsync
- [x] 修復 `SalesReturnService.cs` - UpdateInventoryByDifferenceAsync
- [x] 修復 `MaterialIssueService.cs` - UpdateInventoryByDifferenceAsync
- [x] 修復 `PurchaseReceivingDetailService.cs` - GetRelatedInventoryTransactionsAsync

#### 第三階段：UI 層（已完成）
- [x] 更新 `InventoryTransactionIndex.razor`
- [x] 重寫 `InventoryTransactionEditModalComponent.razor`
  - 使用新的主/明細顯示結構
  - 新增 DetailTableContent 渲染明細表格
- [x] 新增 `InventoryTransactionRelatedModal.razor` - 關聯查看 Modal
- [x] 新增 `InventoryTransactionDetailsTemplate.razor` - 明細顯示模板
- [x] 更新 `InventoryTransactionTable.razor` - 加入關聯查看按鈕

#### 第四階段：Bug 修復（2026-01-19 完成）
- [x] 修復倉庫位置 ID null 比較問題（6 處）
- [x] 修復刪除時重複扣減庫存問題
- [x] 修復異動類型分組錯誤問題
- [x] 修復 ShowBuiltInActions vs ShowActions 按鈕不顯示問題

### ⏳ 待執行項目

#### 資料庫遷移
- [ ] 執行 `dotnet ef database update` 套用遷移
- [ ] 驗證資料庫結構變更

#### 測試驗證
- [ ] 測試採購進貨流程
- [ ] 測試銷貨出貨流程
- [ ] 測試盤點調整流程
- [ ] 測試刪除回滾功能

---

## 主要程式碼變更摘要

### 新建檔案
| 檔案 | 說明 |
|------|------|
| `Data/Entities/Inventory/InventoryTransactionDetail.cs` | 異動明細實體 |
| `Data/Entities/Inventory/InventorySourceDocumentTypes.cs` | 來源單據類型常數 |
| `Components/Shared/RelatedDocument/InventoryTransactionRelatedModal.razor` | 庫存異動關聯查看 Modal |
| `Components/Shared/RelatedDocument/InventoryTransactionDetailsTemplate.razor` | 庫存異動明細顯示模板 |

### 修改檔案
| 檔案 | 變更說明 |
|------|----------|
| `InventoryTransaction.cs` | 移除商品欄位，新增來源單據和彙總欄位 |
| `InventoryStockDetail.cs` | 導航屬性改為 InventoryTransactionDetail |
| `AppDbContext.cs` | 新增 DbSet 和關聯配置 |
| `InventoryStockService.cs` | 所有庫存操作改建立主檔+明細；**修復 6 處倉庫位置 null 比較 Bug** |
| `InventoryTransactionService.cs` | 查詢改用明細，新增 `GetRelatedTransactionsAsync()` 方法 |
| `InventoryTransactionFieldConfiguration.cs` | 欄位配置改用主檔欄位 |
| `InventoryTransactionIndex.razor` | 載入方法和參數更新，移除關聯功能（改由 Table 處理） |
| `InventoryTransactionEditModalComponent.razor` | 新增明細表格顯示，傳遞 TransactionNumber 給 Table |
| `InventoryTransactionTable.razor` | 新增 `ShowBuiltInActions`、`CustomActionsTemplate`、關聯查看 Modal |
| `SalesDeliveryService.cs` | 差異更新改用 Details |
| `PurchaseReceivingService.cs` | 差異更新改用 Details；**新增 _DEL 重複處理檢查** |
| `SalesReturnService.cs` | 差異更新改用 Details |
| `MaterialIssueService.cs` | 差異更新改用 Details |
| `PurchaseReceivingDetailService.cs` | 關聯查詢改用 Details |
| `RelatedDocument.cs` | 新增 `RelatedDocumentType.InventoryTransaction` |
| `DocumentSectionConfig.cs` | 新增 InventoryTransaction 配置 |

---

## 2026-01-19 重大修復記錄

### 🐛 Bug 1：倉庫位置 ID null 比較問題

#### 問題描述
庫存調減失敗，錯誤訊息：「庫存調減失敗：可用庫存不足」，但實際上有足夠庫存。

#### 根本原因
在 `InventoryStockService.cs` 中，查詢倉庫位置時使用了錯誤的 null 比較邏輯：

```csharp
// ❌ 錯誤寫法：當 locationId 為 null 時，會匹配任何位置的庫存
.FirstOrDefault(d => (locationId == null || d.WarehouseLocationId == locationId))

// 當 locationId == null 時：
// - (null == null) = true，短路運算直接返回 true
// - 結果：匹配到第一個庫存記錄（可能是 locationId=1 的空庫存）
// - 正確的 locationId=null 庫存（21件）反而沒被選中
```

#### 解決方案
修改為精確匹配：

```csharp
// ✅ 正確寫法：精確匹配倉庫位置（包含 null == null 的情況）
.FirstOrDefault(d => d.WarehouseLocationId == locationId)
```

#### 受影響的方法（共 6 處）
| 方法名稱 | 行號 | 說明 |
|----------|------|------|
| `GetByProductWarehouseAsync` (1) | 488 | 取得商品倉庫庫存 |
| `GetByProductWarehouseAsync` (2) | 520 | 取得商品倉庫庫存（多載） |
| `GetAvailableStockAsync` | 575 | 取得可用庫存 |
| `ReduceStockAsync` | 1061 | 庫存調減（核心修復） |
| `TransferStockAsync` | 1221 | 庫存調撥 |
| `AdjustStockAsync` | 1280 | 庫存調整 |

---

### 🐛 Bug 2：刪除時重複扣減庫存問題

#### 問題描述
刪除進貨單時，系統顯示「庫存回退失敗」，且同一商品被重複扣減。

#### 根本原因
`ReduceStockAsync` 方法內部有自己的 Transaction，當外層刪除失敗時：
1. 第一次調用成功（Transaction 已 Commit）
2. 外層失敗重試
3. 第二次調用又扣減一次

#### 解決方案
在 `PurchaseReceivingService.cs` 中，扣減庫存前先檢查是否已有 `_DEL` 記錄：

```csharp
// 檢查是否已經處理過（防止重複扣減）
var existingDelTransaction = await context.InventoryTransactions
    .Include(t => t.Details)
    .FirstOrDefaultAsync(t => 
        t.TransactionNumber == delTransactionNumber && 
        t.TransactionType == InventoryTransactionTypeEnum.Adjustment &&
        t.Details.Any(d => d.ProductId == detail.ProductId));

if (existingDelTransaction != null)
{
    ConsoleHelper.WriteWarning($"跳過已處理的商品: {detail.Product?.Name}, 交易編號: {delTransactionNumber}");
    continue;  // 已經處理過，跳過
}
```

---

### 🐛 Bug 3：異動類型分組錯誤

#### 問題描述
編輯進貨單後，調整記錄被錯誤地分組到其他類型（如 PurchaseReceiving）下。

#### 根本原因
`GetOrCreateTransactionAsync` 只匹配 `transactionNumber`，沒有同時匹配 `transactionType`。

#### 解決方案
修改 `GetOrCreateTransactionAsync` 同時匹配編號和類型：

```csharp
// ✅ 正確：同時匹配交易編號 + 交易類型
var existingTransaction = await context.InventoryTransactions
    .FirstOrDefaultAsync(t => t.TransactionNumber == transactionNumber && 
                             t.TransactionType == transactionType);
```

---

### ✨ 新功能：庫存異動關聯查看

#### 功能說明
在庫存異動列表中新增「查看關聯」按鈕，可查看原始交易和所有調整記錄的關係。

#### 新建檔案
| 檔案 | 說明 |
|------|------|
| `Components/Shared/RelatedDocument/InventoryTransactionRelatedModal.razor` | 關聯查看 Modal |
| `Components/Shared/RelatedDocument/InventoryTransactionDetailsTemplate.razor` | 明細顯示模板 |

#### 修改檔案
| 檔案 | 變更 |
|------|------|
| `InventoryTransactionTable.razor` | 新增 `ShowBuiltInActions`、`CustomActionsTemplate`、Modal 整合 |
| `InventoryTransactionIndex.razor` | 移除舊的關聯功能（改由 Table 處理） |
| `InventoryTransactionService.cs` | 新增 `GetRelatedTransactionsAsync()` 方法 |
| `RelatedDocument.cs` | 新增 `RelatedDocumentType.InventoryTransaction` |
| `DocumentSectionConfig.cs` | 新增 InventoryTransaction 配置 |

#### UI 使用注意
```razor
@* InventoryTransactionTable.razor 關鍵設定 *@
<InteractiveTableComponent 
    ShowBuiltInActions="true"              @* 必須設為 true *@
    CustomActionsTemplate="@GetCustomActionsTemplate()" />

@* 注意：CustomActionsTemplate 需要 ShowBuiltInActions="true" 才會顯示 *@
@* 而非 ShowActions="true" + ActionsTemplate *@
```

---

### 🔧 設計決策：統一使用 Adjustment 類型

#### 決策說明
編輯單據產生的所有庫存調整，統一使用 `InventoryTransactionTypeEnum.Adjustment` 類型。

#### 好處
1. 每張單據最多只有 2 筆異動主檔：
   - 原始（PurchaseReceiving/SalesDelivery 等）
   - 調整（Adjustment）
2. 查詢關聯記錄更簡單
3. 報表統計更清晰

#### 實作方式
```csharp
// 刪除時使用 Adjustment 類型
var delResult = await _inventoryStockService.ReduceStockAsync(
    productId: detail.ProductId.Value,
    warehouseId: entity.WarehouseId.Value,
    quantity: detail.Quantity.Value,
    transactionType: InventoryTransactionTypeEnum.Adjustment,  // 統一使用 Adjustment
    transactionNumber: delTransactionNumber,
    ...
);
```

---

## 技術要點備忘

### InteractiveTableComponent 操作按鈕模式

| 參數組合 | 效果 |
|----------|------|
| `ShowActions="true"` + `ActionsTemplate` | 完全自訂操作欄 |
| `ShowBuiltInActions="true"` + `CustomActionsTemplate` | 內建按鈕 + 自訂按鈕 |
| `ShowBuiltInActions="true"` | 只有內建按鈕（編輯/刪除） |

### ConsoleHelper 除錯工具

```csharp
ConsoleHelper.WriteError("錯誤訊息");      // 紅色
ConsoleHelper.WriteWarning("警告訊息");    // 黃色
ConsoleHelper.WriteSuccess("成功訊息");    // 綠色
ConsoleHelper.WriteInfo("一般資訊");       // 藍色
ConsoleHelper.WriteDebug("除錯資訊");      // 灰色
ConsoleHelper.WriteTitle("標題");          // 青色
```

### null 比較注意事項

```csharp
// ❌ 危險：會匹配任何值
.Where(x => (param == null || x.Field == param))

// ✅ 安全：精確匹配（包含 null）
.Where(x => x.Field == param)
```