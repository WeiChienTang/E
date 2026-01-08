# 報價單 BOM 列印控制功能

## 📅 修改日期：2026-01-05（v2 更新）

---

## 📋 需求說明

### 原始問題描述
一個商品的 BOM 組成，可能包含原物料或成品：
- **磚頭**：水泥（原物料）、碎粒石（原物料）
- **浴廁**：馬桶（成品）、洗手台（成品）、磚頭（成品）

列印報價單時：
- **磚頭**（BOM 全為原物料）→ ❌ 不需顯示 BOM，客戶不需知道
- **浴廁**（BOM 包含成品）→ ✅ 需要顯示 BOM（只顯示第一層，不展開磚頭的子 BOM）

### v2 更新：精細控制問題

**原方案問題**：控制點在「父商品」，無法個別控制子項目是否顯示。

例如「廁所」的 BOM 為：
- 磚頭（成品/自製）→ 客戶不需知道 ❌
- 馬桶（成品/外購）→ 客戶需要知道 ✅

若只能在「廁所」設定 ShowBomOnPrint，則兩個子項目都會顯示，無法單獨隱藏磚頭。

### 解決方案（v2）
採用 **組件層級控制** 方案：

將 `ShowBomOnPrint` 的語意從「這個商品的 BOM 是否顯示」改為「**這個商品作為 BOM 組件時，是否在列印中顯示**」。

**設計理由**：
- 控制粒度更細，可針對每個子項目個別設定
- 一個商品可能在多個父商品的 BOM 中，統一設定更一致
- 例如：「馬桶」設定為 true，則無論出現在哪個商品的 BOM 中，都會顯示

---

## 🔧 修改項目清單

### 1. 資料層修改

#### 1.1 Product.cs
**檔案位置：** `Data/Entities/Products/Product.cs`

**欄位定義（v2 語意）：**
```csharp
/// <summary>
/// 當此商品作為 BOM 組件時，是否在報價單/單據列印中顯示
/// true = 當此商品是別人的 BOM 組件時，會在列印時顯示
/// false = 不顯示（預設）
/// 範例：廁所的 BOM 包含「磚頭」和「馬桶」，
///       若只想讓客戶看到馬桶，則馬桶設為 true、磚頭設為 false
/// </summary>
[Display(Name = "BOM列印顯示")]
public bool ShowBomOnPrint { get; set; } = false;
```

#### 1.2 資料庫 Migration
```bash
dotnet ef migrations add AddProductShowBomOnPrint
dotnet ef migrations add ChangeShowBomOnPrintToNonNullable
dotnet ef database update
```

---

### 2. UI 層修改

#### 2.1 ProductFieldConfiguration.cs
**檔案位置：** `Components/FieldConfiguration/ProductFieldConfiguration.cs`

新增 `ShowBomOnPrint` 欄位的顯示設定，選項為「顯示」/「不顯示」。

#### 2.2 FilterHelper.cs
**檔案位置：** `Helpers/FieldConfigurationHelper/FilterHelper.cs`

新增 `ApplyBoolFilter` 方法支援布林值篩選。

---

### 3. 報表層修改

#### 3.1 QuotationReportService.cs
**檔案位置：** `Services/Reports/QuotationReportService.cs`

**修改方法：** `GenerateProductDetailSection` - 加入 BOM 列印邏輯

**修改方法（v2）：** 
- `ShouldShowBom` - 檢查是否有任何組件的 `ShowBomOnPrint = true`
- `GenerateBomCompositionRows` - 只輸出 `ComponentProduct.ShowBomOnPrint = true` 的組件

#### 3.2 QuotationService.cs
**檔案位置：** `Services/Sales/QuotationService.cs`

**修改方法：** `GetWithDetailsAsync` - 增加載入 CompositionDetails 及 ComponentProduct

---

## 📊 判斷邏輯流程（v2）

```
┌─────────────────────────────────────┐
│ 開始判斷是否顯示 BOM                 │
└─────────────────┬───────────────────┘
                  ▼
┌─────────────────────────────────────┐
│ 該商品是否有 BOM 組成資料？          │
│ (QuotationCompositionDetail)        │
└─────────┬───────────────┬───────────┘
          │ 無            │ 有
          ▼               ▼
     ❌ 不顯示    ┌──────────────────────────────────┐
                  │ 是否有任何組件的                  │
                  │ ComponentProduct.ShowBomOnPrint  │
                  │ = true ?                         │
                  └───────┬───────────────┬──────────┘
                          │ 無            │ 有
                          ▼               ▼
                     ❌ 不顯示     ✅ 只顯示設為 true 的組件
```

---

## 📝 列印結果範例（v2）

### 商品設定
| 商品 | ShowBomOnPrint | 說明 |
|------|----------------|------|
| 廁所 | - | 父商品，此欄位不影響 |
| 磚頭 | ❌ false | 自製品，不讓客戶看到 |
| 馬桶 | ✅ true | 外購成品，讓客戶知道 |
| 洗手台 | ✅ true | 外購成品，讓客戶知道 |

### 列印結果
```
項次  項目名稱/規格          單位  數量   單價    總價
────────────────────────────────────────────────────
1     廁所                   組    1    50,000  50,000
      馬桶、洗手台
      （磚頭不顯示，因為 ShowBomOnPrint = false）
────────────────────────────────────────────────────
```

> **2026-01-06 更新**：BOM 組成改為橫向顯示（用頓號分隔），避免多項組件時列表過於冗長。

---

## 🗂️ 相關檔案

| 檔案 | 修改類型 | 說明 |
|------|----------|------|
| `Data/Entities/Products/Product.cs` | 修改 | 新增 ShowBomOnPrint 欄位 (bool) |
| `Components/FieldConfiguration/ProductFieldConfiguration.cs` | 修改 | 新增欄位 UI 設定與篩選 |
| `Helpers/FieldConfigurationHelper/FilterHelper.cs` | 修改 | 新增 ApplyBoolFilter 方法 |
| `Services/Reports/QuotationReportService.cs` | 修改 | 加入 BOM 列印邏輯與 CSS 樣式 |
| `Services/Sales/QuotationService.cs` | 修改 | GetWithDetailsAsync 增加載入 CompositionDetails |
| `Migrations/20260105021154_AddProductShowBomOnPrint.cs` | 新增 | 新增欄位（nullable） |
| `Migrations/20260105023914_ChangeShowBomOnPrintToNonNullable.cs` | 新增 | 改為非 nullable，預設 false |

---

## ⚠️ 注意事項

1. **Include 載入**：列印報價單時，需確保 `QuotationDetail.CompositionDetails.ComponentProduct` 已載入
2. **向下相容**：現有資料 `ShowBomOnPrint` 預設為 `false`（不顯示）
3. **使用方式（v2）**：需要在 BOM 列印中顯示的**組件商品**，請在該商品編輯頁面勾選「BOM列印顯示」
4. **語意變更**：此欄位控制的是「當此商品作為別人的 BOM 組件時，是否顯示」，而非「此商品的 BOM 是否顯示」

---

## 🔄 後續擴展

此功能未來可擴展至：
- 銷貨訂單列印
- 送貨單列印
- 其他需要顯示 BOM 的單據
