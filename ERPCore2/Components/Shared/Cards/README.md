# 統計卡片元件使用說明

## 元件概述

新增的 Cards 模組包含以下元件：

1. **StatisticsCard** - 基礎統計卡片元件
2. **StatisticsCardContainer** - 統計卡片容器元件
3. **InventoryStatisticsCards** - 庫存專用統計卡片組

## 元件位置

```
Components/
  Shared/
    Cards/
      ├── StatisticsCard.razor
      ├── StatisticsCardContainer.razor
      └── InventoryStatisticsCards.razor
```

## 使用方式

### 1. 基礎統計卡片 (StatisticsCard)

```razor
<StatisticsCard Title="總商品數量"
               Value="@totalProducts"
               IconClass="fas fa-boxes"
               BorderColor="primary"
               TextColor="primary"
               IsCurrency="false"
               IsClickable="true"
               OnClick="@HandleCardClick" />
```

**參數說明：**
- `Title` (必填): 卡片標題
- `Value` (必填): 顯示的數值
- `IconClass` (必填): 圖示 CSS 類別 (Font Awesome)
- `BorderColor`: 邊框顏色 (default: "primary")
- `TextColor`: 文字顏色 (default: "primary")
- `IsCurrency`: 是否為貨幣格式 (default: false)
- `IsClickable`: 是否可點擊 (default: false)
- `OnClick`: 點擊事件回調

### 2. 統計卡片容器 (StatisticsCardContainer)

```razor
<StatisticsCardContainer>
    <StatisticsCard ... />
    <StatisticsCard ... />
    <StatisticsCard ... />
</StatisticsCardContainer>
```

### 3. 庫存統計卡片組 (InventoryStatisticsCards)

```razor
<InventoryStatisticsCards Statistics="@statisticsData" 
                        OnTotalProductsClick="@HandleTotalProductsClick"
                        OnTotalValueClick="@HandleTotalValueClick"
                        OnLowStockClick="@HandleLowStockClick" 
                        OnZeroStockClick="@HandleZeroStockClick" />
```

**參數說明：**
- `Statistics` (必填): 統計資料字典，包含以下 key:
  - "TotalProducts": 總商品數量
  - "TotalInventoryValue": 總庫存價值
  - "LowStockCount": 低庫存警戒數量
  - "ZeroStockCount": 零庫存商品數量
- `OnTotalProductsClick`: 總商品數量卡片點擊事件
- `OnTotalValueClick`: 總庫存價值卡片點擊事件
- `OnLowStockClick`: 低庫存警戒卡片點擊事件
- `OnZeroStockClick`: 零庫存商品卡片點擊事件

## 支援的顏色

Bootstrap 顏色主題：
- `primary` (藍色)
- `success` (綠色)
- `warning` (黃色)
- `danger` (紅色)
- `info` (淺藍色)
- `secondary` (灰色)

## 範例：自定義統計卡片

```razor
@code {
    private Dictionary<string, object> statistics = new()
    {
        { "TotalProducts", 150 },
        { "TotalInventoryValue", 1250000m },
        { "LowStockCount", 8 },
        { "ZeroStockCount", 3 }
    };

    private async Task HandleLowStockClick()
    {
        // 處理低庫存卡片點擊
        stockStatusFilter = "low";
        await RefreshData();
    }

    private async Task HandleZeroStockClick()
    {
        // 處理零庫存卡片點擊
        stockStatusFilter = "zero";
        await RefreshData();
    }
}
```

## 元件特色

1. **響應式設計**: 支援 Bootstrap 響應式格線系統
2. **可自定義**: 支援自定義顏色、圖示和點擊事件
3. **貨幣格式**: 自動格式化貨幣顯示
4. **可重用**: 可在多個頁面中重複使用
5. **型別安全**: 使用強型別參數確保正確性

## 升級說明

原本的庫存總覽頁面已升級使用新的統計卡片元件：

**原本：** 直接在頁面中定義 HTML 卡片
**現在：** 使用 `<InventoryStatisticsCards>` 元件

這樣的重構帶來以下好處：
- 程式碼重用性提高
- 維護更容易
- 一致的視覺效果
- 更好的封裝性
