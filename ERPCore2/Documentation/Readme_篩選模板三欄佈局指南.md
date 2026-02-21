# 篩選模板三欄佈局指南

## 背景

原本所有篩選模板的欄位都是單欄直向堆疊。
本次新增 `FilterSectionGroup` + `FilterSectionColumn` 兩個組件，讓篩選欄位可依功能類型分欄水平並排，改善空間利用率與可讀性。

---

## 新增組件說明

### `FilterSectionGroup`

水平排列容器，直接包裹所有 `FilterSectionColumn`，不需要任何參數。

```razor
<FilterSectionGroup>
    ...
</FilterSectionGroup>
```

**佈局行為（CSS flex-wrap）：**

| 容器寬度 | 呈現效果 |
|---|---|
| ≥ 900px | 最多 3 欄並排（每欄 ≥ 280px） |
| 600–900px | 2 欄並排 |
| ≤ 768px | 強制折成單欄 |

### `FilterSectionColumn`

代表一個分組欄，內部欄位直向堆疊。

| 參數 | 型別 | 說明 |
|---|---|---|
| `Title` | `string?` | 區段標題（選填，建議加上） |
| `Icon` | `string?` | Bootstrap Icons CSS 類別（選填） |
| `ChildContent` | `RenderFragment?` | 放入 `FilterFieldRow` 欄位 |

```razor
<FilterSectionColumn Title="基本篩選" Icon="bi bi-people">
    <FilterFieldRow Label="...">...</FilterFieldRow>
</FilterSectionColumn>
```

---

## 欄位分類慣例

| 欄名 | 建議圖示 | 放入的欄位類型 |
|---|---|---|
| **基本篩選** | `bi bi-people` / `bi bi-box-seam` / `bi bi-building` | SearchSelect（客戶、供應商、員工、商品、部門 …） |
| **日期範圍** | `bi bi-calendar-range` | DateRangeFilterComponent |
| **快速條件** | `bi bi-search` | 關鍵字 input、Checkbox（僅啟用、僅在職 …）、簡單 Select |

> 圖示可自行調整，無硬性規定。

---

## 欄數決策原則

| 模板中含有的欄位種類 | 建議欄數 | 說明 |
|---|---|---|
| SearchSelect + 日期 + 關鍵字 | **3 欄** | 三類各一欄 |
| SearchSelect + 日期（無獨立關鍵字欄） | **2 欄** | 基本篩選 + 日期範圍 |
| 僅 SearchSelect / 僅關鍵字 | **1 欄** | 整個放進單一 FilterSectionColumn |
| 類型混雜不易歸類 | **2 欄** | 左欄主要篩選，右欄次要篩選 |

---

## 完整改法步驟

以下以**三欄模板**為例，逐步說明改法。

### Before（原始寫法）

```razor
<div>
    <FilterFieldRow Label="指定客戶">
        <SearchSelectFilterComponent ... />
    </FilterFieldRow>

    <FilterFieldRow Label="業務負責人">
        <SearchSelectFilterComponent ... />
    </FilterFieldRow>

    <FilterFieldRow Label="訂單日期">
        <DateRangeFilterComponent ... />
    </FilterFieldRow>

    <FilterFieldRow Label="關鍵字">
        <input type="text" class="form-control" @bind="keyword" />
    </FilterFieldRow>

    <FilterFieldRow Label="僅啟用">
        <div class="form-check">
            <input class="form-check-input" type="checkbox" @bind="activeOnly">
            <label class="form-check-label">僅啟用</label>
        </div>
    </FilterFieldRow>
</div>
```

### After（三欄寫法）

```razor
<FilterSectionGroup>

    @* ===== 欄 1：主要篩選 ===== *@
    <FilterSectionColumn Title="基本篩選" Icon="bi bi-people">
        <FilterFieldRow Label="指定客戶">
            <SearchSelectFilterComponent ... />
        </FilterFieldRow>

        <FilterFieldRow Label="業務負責人">
            <SearchSelectFilterComponent ... />
        </FilterFieldRow>
    </FilterSectionColumn>

    @* ===== 欄 2：日期範圍 ===== *@
    <FilterSectionColumn Title="日期範圍" Icon="bi bi-calendar-range">
        <FilterFieldRow Label="訂單日期">
            <DateRangeFilterComponent ... />
        </FilterFieldRow>
    </FilterSectionColumn>

    @* ===== 欄 3：快速條件 ===== *@
    <FilterSectionColumn Title="快速條件" Icon="bi bi-search">
        <FilterFieldRow Label="關鍵字">
            <input type="text" class="form-control" @bind="keyword" />
        </FilterFieldRow>

        <FilterFieldRow Label="顯示條件">
            <div class="form-check">
                <input class="form-check-input" type="checkbox" @bind="activeOnly">
                <label class="form-check-label">僅啟用</label>
            </div>
        </FilterFieldRow>
    </FilterSectionColumn>

</FilterSectionGroup>
```

### After（二欄寫法，無獨立快速條件欄）

```razor
<FilterSectionGroup>

    <FilterSectionColumn Title="基本篩選" Icon="bi bi-box-seam">
        <FilterFieldRow Label="指定客戶">
            <SearchSelectFilterComponent ... />
        </FilterFieldRow>
        <FilterFieldRow Label="關鍵字">
            <input type="text" class="form-control" @bind="keyword" />
        </FilterFieldRow>
        <FilterFieldRow Label="顯示條件">
            <div class="form-check">
                <input class="form-check-input" type="checkbox" @bind="activeOnly">
                <label class="form-check-label">僅啟用</label>
            </div>
        </FilterFieldRow>
    </FilterSectionColumn>

    <FilterSectionColumn Title="日期範圍" Icon="bi bi-calendar-range">
        <FilterFieldRow Label="訂單日期">
            <DateRangeFilterComponent ... />
        </FilterFieldRow>
    </FilterSectionColumn>

</FilterSectionGroup>
```

---

## 各模板建議配置

### 已完成

| 模板 | 欄數 | 欄 1 | 欄 2 | 欄 3 |
|---|---|---|---|---|
| EmployeeRosterBatchFilterTemplate | 3 | 基本篩選（員工/部門/職位/狀態/權限組） | 日期範圍（到職/離職/生日） | 快速條件（關鍵字/在職） |

---

### 待套用（依複雜度排序）

#### 名冊類（Roster）→ 建議 2~3 欄

| 模板 | 建議欄數 | 欄 1 | 欄 2 | 欄 3 |
|---|---|---|---|---|
| CustomerRosterBatchFilterTemplate | 2 | 基本篩選（客戶/業務） | 快速條件（關鍵字/僅啟用） | — |
| SupplierRosterBatchFilterTemplate | 2 | 基本篩選（供應商） | 快速條件（關鍵字/僅啟用） | — |
| VehicleListBatchFilterTemplate | 視欄位 | 基本篩選 | 快速條件 | — |

#### 對帳單 / 交易類（Statement / Transaction）→ 建議 2 欄

| 模板 | 建議欄數 | 欄 1 | 欄 2 |
|---|---|---|---|
| CustomerStatementBatchFilterTemplate | 2 | 基本篩選（客戶） | 日期範圍 |
| SupplierStatementBatchFilterTemplate | 2 | 基本篩選（供應商） | 日期範圍 |
| CustomerTransactionBatchFilterTemplate | 2 | 基本篩選（客戶/類型） | 日期範圍 |

#### 銷售類（Sales）→ 建議 2~3 欄

| 模板 | 建議欄數 | 欄 1 | 欄 2 | 欄 3 |
|---|---|---|---|---|
| SalesOrderBatchFilterTemplate | 3 | 基本篩選（客戶） | 日期範圍 | 快速條件（單號/關鍵字） |
| SalesDeliveryBatchFilterTemplate | 3 | 基本篩選（客戶） | 日期範圍 | 快速條件 |
| SalesReturnBatchFilterTemplate | 3 | 基本篩選（客戶） | 日期範圍 | 快速條件 |
| QuotationBatchFilterTemplate | 3 | 基本篩選（客戶） | 日期範圍 | 快速條件 |
| CustomerSalesAnalysisBatchFilterTemplate | 2 | 基本篩選（客戶） | 日期範圍 |

#### 採購類（Purchase）→ 建議 2~3 欄

| 模板 | 建議欄數 | 欄 1 | 欄 2 | 欄 3 |
|---|---|---|---|---|
| PurchaseOrderBatchFilterTemplate | 3 | 基本篩選（供應商） | 日期範圍 | 快速條件 |
| PurchaseReceivingBatchFilterTemplate | 3 | 基本篩選（供應商） | 日期範圍 | 快速條件 |
| PurchaseReturnBatchFilterTemplate | 3 | 基本篩選（供應商） | 日期範圍 | 快速條件 |

#### 商品 / 庫存類（Product / Inventory）→ 建議 1~2 欄

| 模板 | 建議欄數 | 欄 1 | 欄 2 |
|---|---|---|---|
| ProductListBatchFilterTemplate | 2 | 基本篩選（分類/採購類型/僅啟用） | 快速條件（關鍵字） |
| ProductBarcodeBatchFilterTemplate | 2 | 基本篩選（商品/分類） | 快速條件 |
| InventoryStatusBatchFilterTemplate | 2 | 基本篩選（倉庫/分類） | 快速條件（關鍵字/零庫存） |
| BOMBatchFilterTemplate | 視欄位 | 基本篩選 | 快速條件 |
| StockTakingDifferenceBatchFilterTemplate | 2 | 基本篩選（倉庫） | 快速條件 |

#### 財務類（Financial）→ 建議 2 欄

| 模板 | 建議欄數 | 欄 1 | 欄 2 |
|---|---|---|---|
| AccountsReceivableFilterTemplate | 2 | 基本篩選（客戶） | 日期範圍 |
| AccountsReceivableSetoffBatchFilterTemplate | 2 | 基本篩選 | 日期範圍 |
| AccountsPayableSetoffBatchFilterTemplate | 2 | 基本篩選 | 日期範圍 |

#### 其他

| 模板 | 建議欄數 | 備註 |
|---|---|---|
| ProductionScheduleBatchFilterTemplate | 2~3 | 視欄位實際內容決定 |
| VehicleMaintenanceBatchFilterTemplate | 2 | 車輛 + 日期 |

> **注意：** 上表為依模板名稱的推測建議，實際改寫前請先閱讀各模板的欄位，確認分類正確後再套用。

---

## 注意事項

### 1. `keyword` + `checkbox` 應分拆為獨立 FilterFieldRow

原本常見的「關鍵字 + checkbox 同行」寫法：

```razor
@* 舊寫法：塞在同一個 FilterFieldRow *@
<FilterFieldRow Label="關鍵字">
    <div class="d-flex align-items-center gap-2">
        <input type="text" class="form-control" @bind="keyword" />
        <div class="form-check text-nowrap">
            <input class="form-check-input" type="checkbox" @bind="activeOnly">
            <label class="form-check-label">僅啟用</label>
        </div>
    </div>
</FilterFieldRow>
```

改為拆開，放入「快速條件」欄：

```razor
@* 新寫法：各自獨立 *@
<FilterSectionColumn Title="快速條件" Icon="bi bi-search">
    <FilterFieldRow Label="關鍵字">
        <input type="text" class="form-control" @bind="keyword" />
    </FilterFieldRow>

    <FilterFieldRow Label="顯示條件">
        <div class="form-check">
            <input class="form-check-input" type="checkbox" @bind="activeOnly">
            <label class="form-check-label">僅啟用</label>
        </div>
    </FilterFieldRow>
</FilterSectionColumn>
```

### 2. `@using` 和 `@inject` 指令不需改動

只改 HTML 標記區塊，`@code { }` 和所有 `@using`、`@inject`、`@implements` 指令完全不動。

### 3. 單欄模板也要套用 FilterSectionGroup

即使只有一個欄，仍建議套用，保持架構一致：

```razor
<FilterSectionGroup>
    <FilterSectionColumn Title="篩選條件" Icon="bi bi-funnel">
        <FilterFieldRow Label="...">...</FilterFieldRow>
    </FilterSectionColumn>
</FilterSectionGroup>
```

### 4. 舊的 `FilterSectionComponent` 已廢棄

若舊模板有使用 `<FilterSectionComponent>`，改寫時直接移除，改用新架構即可。

---

## 已建立的組件檔案位置

```
Components/Shared/Report/
├── FilterSectionGroup.razor          ← 新增：容器組件
├── FilterSectionGroup.razor.css      ← 新增：容器 CSS
├── FilterSectionColumn.razor         ← 新增：欄位組件
├── FilterSectionColumn.razor.css     ← 新增：欄位 CSS
├── FilterFieldRow.razor              （原有，不動）
└── FilterTemplates/
    └── EmployeeRosterBatchFilterTemplate.razor  ← 已完成改寫（參考範例）
```
