# 通用統計卡片套裝組件使用指南

## 元件概述

新的 `GenericStatisticsCards` 組件是一個可高度配置的統計卡片套裝組件，讓您只需輸入參數配置即可生成各種統計卡片組合。

## 元件結構

```
Components/
  Shared/
    Cards/
      ├── GenericStatisticsCards.razor          # 主要組件
      ├── StatisticsCardConfigProvider.cs       # 預設配置提供者
      ├── StatisticsCard.razor                  # 基礎卡片組件
      ├── StatisticsCardContainer.razor         # 容器組件
      └── InventoryStatisticsCards.razor        # 舊版庫存專用組件
```

## 使用方式

### 1. 使用預設配置（推薦）

```razor
<!-- 庫存管理統計卡片 -->
<GenericStatisticsCards CardConfigs="@inventoryConfigs"
                       Statistics="@statisticsData" />

@code {
    private Dictionary<string, object> statisticsData = new()
    {
        { "TotalProducts", 150 },
        { "TotalInventoryValue", 1250000m },
        { "LowStockCount", 8 },
        { "ZeroStockCount", 3 }
    };

    private List<GenericStatisticsCards.StatisticsCardConfig> inventoryConfigs;

    protected override void OnInitialized()
    {
        inventoryConfigs = StatisticsCardConfigProvider.InventoryConfigs(
            onTotalProductsClick: EventCallback.Factory.Create(this, HandleTotalProductsClick),
            onLowStockClick: EventCallback.Factory.Create(this, HandleLowStockClick),
            onZeroStockClick: EventCallback.Factory.Create(this, HandleZeroStockClick)
        );
    }

    private async Task HandleTotalProductsClick()
    {
        // 處理總商品數量卡片點擊
    }

    private async Task HandleLowStockClick()
    {
        // 處理低庫存警戒卡片點擊
        // 例如：篩選顯示低庫存商品
    }

    private async Task HandleZeroStockClick()
    {
        // 處理零庫存商品卡片點擊
        // 例如：篩選顯示零庫存商品
    }
}
```

### 2. 採購訂單統計卡片

```razor
<GenericStatisticsCards CardConfigs="@purchaseOrderConfigs"
                       Statistics="@purchaseStatistics" />

@code {
    private Dictionary<string, object> purchaseStatistics = new()
    {
        { "TotalOrders", filteredItems.Count },
        { "PendingOrders", filteredItems.Count(o => o.OrderStatus == PurchaseOrderStatus.Approved) },
        { "CompletedOrders", filteredItems.Count(o => o.OrderStatus == PurchaseOrderStatus.Completed) },
        { "TotalAmount", filteredItems.Sum(o => o.TotalAmount) }
    };

    private List<GenericStatisticsCards.StatisticsCardConfig> purchaseOrderConfigs;

    protected override void OnInitialized()
    {
        purchaseOrderConfigs = StatisticsCardConfigProvider.PurchaseOrderConfigs(
            onTotalOrdersClick: EventCallback.Factory.Create(this, () => ResetFilters()),
            onPendingOrdersClick: EventCallback.Factory.Create(this, () => FilterByStatus("pending"))
        );
    }
}
```

### 3. 自訂配置

```razor
<GenericStatisticsCards CardConfigs="@customConfigs"
                       Statistics="@customStatistics" />

@code {
    private List<GenericStatisticsCards.StatisticsCardConfig> customConfigs = new()
    {
        new()
        {
            Title = "本月銷售額",
            DataKey = "MonthlySales",
            IconClass = "fas fa-chart-line",
            BorderColor = "success",
            TextColor = "success",
            IsCurrency = true,
            OnClick = EventCallback.Factory.Create(this, HandleMonthlySalesClick)
        },
        new()
        {
            Title = "新客戶數",
            ValueCalculator = (stats) => 
            {
                // 自訂計算邏輯
                var totalCustomers = Convert.ToDecimal(stats.GetValueOrDefault("TotalCustomers", 0));
                var lastMonthCustomers = Convert.ToDecimal(stats.GetValueOrDefault("LastMonthCustomers", 0));
                return totalCustomers - lastMonthCustomers;
            },
            IconClass = "fas fa-user-plus",
            BorderColor = "primary",
            TextColor = "primary"
        },
        new()
        {
            Title = "客戶滿意度",
            DataKey = "CustomerSatisfaction",
            DefaultValue = 95,
            IconClass = "fas fa-star",
            BorderColor = "warning",
            TextColor = "warning"
        }
    };

    private Dictionary<string, object> customStatistics = new()
    {
        { "MonthlySales", 850000m },
        { "TotalCustomers", 1205 },
        { "LastMonthCustomers", 1180 },
        { "CustomerSatisfaction", 97.5m }
    };
}
```

## 可用的預設配置

### 庫存管理
- `StatisticsCardConfigProvider.InventoryConfigs()` - 總商品數量、總庫存價值、低庫存警戒、零庫存商品

### 採購管理
- `StatisticsCardConfigProvider.PurchaseOrderConfigs()` - 總訂單數、待進貨訂單、已完成訂單、訂單總金額
- `StatisticsCardConfigProvider.PurchaseReceiptConfigs()` - 總進貨單數、待驗收進貨、已完成進貨、進貨總金額

### 盤點管理
- `StatisticsCardConfigProvider.StockTakingConfigs()` - 總盤點數、進行中盤點、已完成盤點、盤點差異數

### 庫存異動
- `StatisticsCardConfigProvider.InventoryTransactionConfigs()` - 總異動筆數、入庫異動、出庫異動、調整異動

### 財務統計
- `StatisticsCardConfigProvider.FinancialConfigs()` - 總筆數、總金額、平均金額、最高金額

## 配置參數說明

### StatisticsCardConfig 屬性

| 屬性 | 型別 | 必填 | 說明 | 預設值 |
|------|------|------|------|--------|
| `Title` | string | ✓ | 卡片標題 | "" |
| `DataKey` | string? | | 資料索引鍵 (從 Statistics 字典取值) | null |
| `ValueCalculator` | Func<Dictionary<string, object>, decimal>? | | 自訂值計算器 (優先於 DataKey) | null |
| `DefaultValue` | decimal | | 找不到資料時的預設值 | 0 |
| `IconClass` | string | | Font Awesome 圖示 CSS 類別 | "fas fa-chart-bar" |
| `BorderColor` | string | | Bootstrap 顏色名稱 | "primary" |
| `TextColor` | string | | Bootstrap 顏色名稱 | "primary" |
| `IsCurrency` | bool | | 是否為貨幣格式 | false |
| `OnClick` | EventCallback | | 點擊事件回調 | default |

### 支援的顏色

- `primary` (藍色)
- `success` (綠色)
- `warning` (黃色)
- `danger` (紅色)
- `info` (淺藍色)
- `secondary` (灰色)

## 升級指南

### 從舊版卡片升級

**原本（在每個頁面重複寫 HTML）：**
```razor
<div class="row mb-4">
    <div class="col-xl-3 col-md-6 mb-4">
        <div class="card border-left-primary shadow h-100 py-2">
            <div class="card-body">
                <div class="row no-gutters align-items-center">
                    <div class="col mr-2">
                        <div class="text-xs font-weight-bold text-primary text-uppercase mb-1">
                            總訂單數
                        </div>
                        <div class="h5 mb-0 font-weight-bold text-gray-800">@filteredItems.Count</div>
                    </div>
                    <div class="col-auto">
                        <i class="fas fa-file-invoice fa-2x text-gray-300"></i>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <!-- 更多重複的卡片... -->
</div>
```

**現在（使用通用組件）：**
```razor
<GenericStatisticsCards CardConfigs="@purchaseOrderConfigs"
                       Statistics="@statisticsData" />
```

### 從 InventoryStatisticsCards 升級

```razor
<!-- 舊版 -->
<InventoryStatisticsCards Statistics="@statistics" 
                        OnLowStockClick="@ShowLowStockOnly" />

<!-- 新版 -->
<GenericStatisticsCards CardConfigs="@inventoryConfigs"
                       Statistics="@statistics" />
```

## 優點

1. **高度可重用**：一個組件可以應用於所有統計頁面
2. **配置驅動**：透過配置參數控制顯示，無需修改組件程式碼
3. **預設套裝**：提供常用的預設配置，開箱即用
4. **型別安全**：強型別參數確保正確性
5. **靈活計算**：支援自訂值計算邏輯
6. **響應式設計**：自動適應不同螢幕尺寸
7. **一致性**：確保整個應用程式的視覺一致性
8. **易於維護**：集中管理卡片邏輯，維護更容易

## 實際應用場景

- 庫存總覽頁面
- 採購訂單列表頁面
- 進貨單列表頁面  
- 盤點管理頁面
- 庫存異動歷史頁面
- 財務報表頁面
- 儀表板頁面
- 任何需要統計卡片的頁面
