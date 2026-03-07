# 圖表設計

## 更新日期
2026-03-07

---

## 概述

圖表系統類比報表系統架構，以 **Blazor-ApexCharts** v6.1.0 為基礎，提供可複用的 `GenericChartModalComponent`。各模組各自擁有圖表定義（`*ChartDefinitions.cs`），統一向 `ChartRegistry` 登記；`GenericChartModalComponent` 依 `Category` 讀取對應定義，自動渲染頁籤、圖表類型切換與 Drill-down 明細面板。

---

## 架構對照（對應報表系統）

| 報表系統 | 圖表系統 |
|---------|---------|
| `ReportIds.cs` | `ChartIds.cs` |
| `ReportDefinition.cs` + `ReportCategory` | `ChartDefinition.cs` + `ChartCategory` |
| `ReportRegistry.cs` | `ChartRegistry.cs` |
| `GenericReportIndexPage.razor` | `GenericChartModalComponent.razor` |
| `[Entity]ReportService` | `[Entity]ChartService` |

---

## 檔案結構

### 核心框架

| 檔案 | 路徑 | 說明 |
|------|------|------|
| 資料點模型 | `Models/Charts/ChartDataItem.cs` | `ChartDataItem`（圖表點）+ `ChartDetailItem`（Drill-down 明細）|
| 圖表 ID 常數 | `Models/Charts/ChartIds.cs` | 各模組圖表 ID 字串常數 |
| 圖表定義模型 | `Models/Charts/ChartDefinition.cs` | `ChartDefinition` + `ChartCategory` 常數 |
| 全域登記表 | `Data/Charts/ChartRegistry.cs` | 彙總器，只呼叫各模組的 `Register()` |
| 通用 Modal | `Components/Shared/Chart/GenericChartModalComponent.razor` | 頁籤 + 圖表類型切換 + Drill-down |
| Modal CSS | `Components/Shared/Chart/GenericChartModalComponent.razor.css` | 自訂 tab 按鈕樣式 |

### 客戶模組（各模組同此結構）

| 檔案 | 路徑 | 說明 |
|------|------|------|
| 服務介面 | `Services/Customers/ICustomerChartService.cs` | 圖表資料 + Drill-down 明細查詢定義 |
| 服務實作 | `Services/Customers/CustomerChartService.cs` | EF Core 查詢實作 |
| 圖表定義 | `Services/Customers/CustomerChartDefinitions.cs` | `Register()` 登記至 ChartRegistry |
| 薄包裝器 | `Components/Pages/Customers/CustomerChartModalComponent.razor` | 摘要卡片 + 呼叫 GenericChartModalComponent |

---

## ChartDefinition 屬性

| 屬性 | 型別 | 說明 |
|------|------|------|
| `ChartId` | `string` | 唯一 ID（如 `"CU001"`），定義於 `ChartIds.cs` |
| `Title` | `string` | 頁籤標題 |
| `Category` | `string` | `ChartCategory` 常數，決定歸屬於哪個 Modal |
| `SortOrder` | `int` | 頁籤排序 |
| `DefaultSeriesType` | `SeriesType` | 預設圖表類型 |
| `AllowedSeriesTypes` | `List<SeriesType>` | 使用者可切換的圖表類型清單 |
| `IsMoneyChart` | `bool` | `true` 時 Y 軸顯示千分位格式，預設 `false` |
| `DataFetcher` | `Func<IServiceProvider, Task<List<ChartDataItem>>>` | 圖表資料來源（必填） |
| `DetailFetcher` | `Func<IServiceProvider, string, Task<List<ChartDetailItem>>>?` | Drill-down 明細查詢（選填，傳入被點擊的 Label） |
| `DetailColumns` | `List<InteractiveColumnDefinition>?` | 明細欄位定義（選填，未設定使用預設代碼 + 名稱兩欄） |

---

## GenericChartModalComponent 參數

| 參數 | 型別 | 說明 |
|------|------|------|
| `IsVisible` | `bool` | Modal 是否顯示 |
| `IsVisibleChanged` | `EventCallback<bool>` | 顯示狀態變更事件 |
| `Category` | `string` | `ChartCategory` 常數，決定顯示哪些圖表頁籤 |
| `Title` | `string` | Modal 標題（預設「分析圖表」） |
| `Icon` | `string` | Bootstrap Icon class（預設 `bi bi-bar-chart-fill`） |
| `SummaryContent` | `RenderFragment?` | 顯示於頁籤上方的摘要區塊（選填） |
| `OnDataLoading` | `EventCallback` | Modal 開啟時通知父元件同步載入（如摘要卡片） |

---

## 資料模型

### ChartDataItem（跨模組共用）

| 欄位 | 型別 | 說明 |
|------|------|------|
| `Label` | `string` | X 軸標籤或圓餅名稱 |
| `Value` | `decimal` | 數值 |

### ChartDetailItem（跨模組共用）

| 欄位 | 型別 | 說明 |
|------|------|------|
| `Id` | `int` | 實體 ID |
| `Name` | `string` | 主要顯示欄位（預設欄位：名稱） |
| `SubLabel` | `string?` | 次要顯示欄位（預設欄位：代碼） |

摘要模型（如 `CustomerChartSummary`）為各模組自行定義，放於 `Models/Charts/`。

---

## 支援的 SeriesType

| SeriesType | 中文名稱 | 適用資料 | 支援 IsMoneyChart |
|-----------|---------|---------|:-----------------:|
| `Bar` | 長條圖 | 分類計數 / 金額排行 | ✅ |
| `Pie` | 圓餅圖 | 分類計數 | ❌ |
| `Donut` | 甜甜圈圖 | 分類計數 | ❌ |
| `Line` | 折線圖 | 時間序列 | ✅ |
| `Area` | 面積圖 | 時間序列 | ✅ |
| `Treemap` | 樹狀圖 | 分類計數 | ❌ |
| `PolarArea` | 極區圖 | 分類計數 | ❌ |
| `Radar` | 雷達圖 | 多維比較 | ❌ |
| `RadialBar` | 儀表圖 | 比例 / 進度 | ❌ |

時間序列資料（月趨勢）只允許 `Line` 和 `Area`，Drill-down 不適用時省略 `DetailFetcher`。

---

## 現有客戶圖表

| ID | 標題 | 預設類型 | IsMoneyChart | Drill-down | 自訂欄位 |
|----|------|---------|:------------:|:----------:|:--------:|
| CU001 | 依業務負責人分布 | Donut | ❌ | ✅ | ❌ |
| CU002 | 依付款方式分布 | Bar | ❌ | ✅ | ❌ |
| CU003 | 每月新增趨勢 | Line | ❌ | ❌ | — |
| CU004 | 客戶狀態分布 | Pie | ❌ | ✅ | ❌ |
| CU005 | 信用額度分布 | Bar | ❌ | ✅ | ❌ |
| CU006 | 客戶銷售金額排行 Top 10 | Bar | ✅ | ✅ | 出貨日期 + 含稅金額 |
| CU007 | 每月銷售收入趨勢 | Area | ✅ | ❌ | — |
| CU008 | 應收餘額分布 | Bar | ✅ | ✅ | 公司名稱 + 應收餘額 |
| CU009 | 客戶退貨金額排行 Top 10 | Bar | ✅ | ✅ | 退回日期 + 含稅金額 |

---

## 重要設計規則

### 1. `@using ApexCharts` 禁止放入 `_Imports.razor`

`ApexCharts` 套件內含 `Size` 型別，與 `ERPCore2.Data.Entities.Size`（商品尺寸）同名，全域引入會導致商品相關頁面編譯錯誤。`@using ApexCharts` 只放在 `GenericChartModalComponent.razor` 頂端，模組包裝器不需再加。

### 2. Options 與 Callback 必須使用穩定物件參考

`ApexChart` 的 `Options` 和 `OnDataPointSelection` 若每次 re-render 傳入新物件，ApexChart 會重新初始化 JS，導致事件監聽器失效、後續點擊完全無反應。

`GenericChartModalComponent` 透過兩個快取字典解決：
- `_optionsCache`：Key 為 `"{chartId}_{seriesType}"`，金額圖與非金額圖分開快取（Y 軸格式不同）
- `_callbackCache`：`ChartId → EventCallback`，在 LoadData 時建立一次，後續重用

**務必透過 `GetCachedOptions(chart, selectedType)` 取得 Options，不可每次建立新物件。**

### 3. EF Core GroupBy + JOIN 欄位的限制

當 LINQ GroupBy 的 key 包含 JOIN 來的欄位，且 Sum() 內含加法運算時，EF Core 無法正確翻譯為 SQL，會只回傳一筆結果。解決方式：先 `.ToListAsync()` 載入記憶體，再用 LINQ to Objects 做 GroupBy。月份趨勢圖同樣適用此規則。

### 4. 月份趨勢需補零

DB 查詢後在記憶體補齊缺少月份（以 `Dictionary<(year, month), value>` 搭配迴圈），避免折線圖出現跳躍。

### 5. 銷售金額使用 SalesDelivery（出貨單）

`SalesOrder`（銷售訂單）僅為承諾，不應作為收入統計基礎；`SalesDelivery`（出貨單）才是實際出貨、產生應收帳款的正確來源。注意欄位名稱：`SalesDelivery.TaxAmount`（非 `SalesTaxAmount`）。

### 6. Drill-down 行為由 GenericChartModalComponent 內建處理

有 `DetailFetcher` 時自動顯示提示與明細面板；無 `DetailFetcher` 時點擊不觸發任何動作。切換頁籤或圖表類型時自動關閉明細面板。快速連續點擊以 `_detailLoadSeq` 防止非同步競爭。

### 7. 圖表類型切換以 @key 強制重建

切換 SeriesType 時，`GenericChartModalComponent` 使用 `@key="$"{chartId}_{type}""` 強制重新建立 ApexChart 元件，確保舊資料完全清除後重新渲染。

---

## 新增功能快速參考

### 在現有模組新增圖表

1. 在 `IXxxChartService.cs` 新增資料方法（回傳 `List<ChartDataItem>`）和可選的明細方法（回傳 `List<ChartDetailItem>`）
2. 在 `XxxChartService.cs` 實作查詢（使用 `IDbContextFactory`，先 ToListAsync 再 GroupBy）
3. 在 `ChartIds.cs` 新增常數
4. 在 `XxxChartDefinitions.cs` 的 `Register()` 新增 `ChartDefinition`（`GenericChartModalComponent` 自動讀取，無需修改 UI）

### 為新模組新增圖表

1. 建立 `IXxxChartService.cs` + `XxxChartService.cs`，在 `ServiceRegistration.cs` 登記 DI
2. 在 `ChartIds.cs` 新增本模組的 ID 常數
3. 建立 `XxxChartDefinitions.cs`，實作 `Register(List<ChartDefinition> definitions)`
4. 在 `ChartRegistry.Initialize()` 呼叫 `XxxChartDefinitions.Register(_definitions)`
5. 建立薄包裝器 `XxxChartModalComponent.razor`，傳入 `Category` 給 `GenericChartModalComponent`
6. 在 `NavigationConfig.cs` 直接宣告 NavigationItem（設 `IsChartWidget = true`，**不使用** `CreateActionItem()`）
7. 在 `MainLayout.razor` 新增 state 變數、Handler 和 Modal 宣告（不使用 `@if` 包裹）
8. 在 5 個 `.resx` 檔新增 `Nav.XxxCharts` 多語系 key

---

## 相關文件

- [README_共用元件設計總綱.md](README_共用元件設計總綱.md)
- [README_首頁客製化設計.md](../完整頁面設計/README_首頁客製化設計.md)
