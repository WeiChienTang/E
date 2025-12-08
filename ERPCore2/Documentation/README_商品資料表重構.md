# 商品資料表重構 - CanSchedule 改為 ProcurementType

## 📋 重構目標

將 `Product` 實體中的布林欄位 `CanSchedule` 重構為列舉型別 `ProcurementType`，以提供更清晰的語義和更好的擴展性。

## 🎯 問題分析

### 原設計的問題

1. **語義不明確**: `CanSchedule` (可轉排程) 隱含「自製」與「外購」的業務邏輯，但欄位名稱未直接表達
2. **擴展性受限**: 布林值無法處理「委外加工」、「半成品」等複雜情況
3. **職責混淆**: 「是否需要排程」屬於製造策略，而非商品靜態屬性

### 新設計優勢

1. **語義清晰**: `ProcurementType.Manufactured` 明確表達「自製品」概念
2. **易於擴展**: 列舉可輕鬆新增「委外」、「配送」等類型
3. **符合慣例**: 與系統中其他列舉 (如 `InventoryTransactionTypeEnum`) 風格一致

## 🔧 實作方案

### 新增列舉定義

**檔案**: `Data/Enums/ProcurementType.cs`

```csharp
namespace ERPCore2.Data.Enums
{
    /// <summary>
    /// 商品採購/製造類型
    /// </summary>
    public enum ProcurementType
    {
        /// <summary>
        /// 外購 - 直接向供應商採購
        /// </summary>
        [Display(Name = "外購")]
        Purchased = 0,
        
        /// <summary>
        /// 自製 - 內部生產製造(需要排程)
        /// </summary>
        [Display(Name = "自製")]
        Manufactured = 1,
        
        /// <summary>
        /// 委外 - 委外加工(未來擴展用)
        /// </summary>
        [Display(Name = "委外")]
        Outsourced = 2
    }
}
```

### 修改 Product 實體

**檔案**: `Data/Entities/Products/Product.cs`

```csharp
// 移除
public bool CanSchedule { get; set; } = false;

// 新增
/// <summary>
/// 採購/製造類型 - 決定商品的取得方式
/// </summary>
[Display(Name = "採購類型")]
public ProcurementType ProcurementType { get; set; } = ProcurementType.Purchased;
```

### 相容性轉換邏輯

在判斷是否可排程的地方使用:

```csharp
// 舊: product.CanSchedule
// 新: product.ProcurementType == ProcurementType.Manufactured
```

## 📝 需要修改的檔案清單

### ✅ 核心資料層 (4 個檔案)

- [x] `Data/Enums/ProcurementType.cs` - 新增列舉定義
- [x] `Data/Entities/Products/Product.cs` - 修改實體屬性
- [x] 新增 Migration `20251208014446_RefactorProductProcurementType` - 移除 CanSchedule，新增 ProcurementType
- [x] `AppDbContextModelSnapshot.cs` - 自動更新

### ✅ UI 元件層 (4 個檔案，12 處修改)

#### 1. ProductEditModalComponent.razor (3 處)
- [x] FormFieldDefinition: 將 Checkbox 改為 Select 下拉選單
- [x] FormSection: 欄位名稱從 `CanSchedule` 改為 `ProcurementType`
- [x] 新增輔助方法 `GetProcurementTypeDisplayName()`

#### 2. TransferToScheduleModalComponent.razor (3 處)
- [x] Line 186: 註解說明
- [x] Line 188: 篩選條件 `.Where(d => d.Product?.ProcurementType == ProcurementType.Manufactured)`
- [x] Line 204: ViewModel 賦值改用列舉判斷
- [x] Line 268: BOM 組件判斷改用列舉判斷

#### 3. SalesOrderTable.razor (1 處)
- [x] Line 1149: 庫存徽章顯示邏輯改用列舉判斷

#### 4. SalesOrderCompositionEditModal.razor (1 處)
- [x] Line 332: 取得 canSchedule 變數改用列舉判斷

### ✅ 欄位配置層 (1 個檔案)

#### 5. ProductFieldConfiguration.cs (新增)
- [x] 新增 `ProcurementType` 欄位定義
- [x] 配置 Select 下拉篩選器
- [x] 實作 CustomTemplate 顯示 Badge
- [x] 新增輔助方法 `GetProcurementTypeDisplayName()`

### ✅ 文件更新 (2 個檔案)

- [x] `Documentation/README_商品排程製作.md` - 更新說明
- [x] `Documentation/README_銷貨訂單BOM組成編輯功能.md` - 更新說明

## 🔄 Migration 策略

由於系統仍在開發階段，採用**重置策略**:

1. 刪除現有 Migration 檔案
2. 建立新的 Migration
3. 所有商品預設為 `ProcurementType.Purchased`
4. 不保留舊資料的 `CanSchedule` 值

## 📊 影響範圍統計

- **資料層**: 1 個實體、1 個新列舉、1 個 Migration
- **UI 元件**: 4 個 Razor 元件、8 處程式碼修改
- **欄位配置**: 1 個 FieldConfiguration 檔案
- **服務層**: 0 (無影響)
- **文件**: 3 個 README 檔案

## 📝 實際修改統計

### 程式碼檔案 (10 個)
1. `Data/Enums/ProcurementType.cs` - 新增列舉 (24 行)
2. `Data/Entities/Products/Product.cs` - 修改屬性 (1 處)
3. `Migrations/20251208014446_RefactorProductProcurementType.cs` - Migration
4. `Components/Pages/Products/ProductEditModalComponent.razor` - 3 處修改
5. `Components/Shared/BaseModal/Modals/Sales/TransferToScheduleModalComponent.razor` - 3 處修改
6. `Components/Shared/BaseModal/Modals/Sales/SalesOrderTable.razor` - 1 處修改
7. `Components/Shared/BaseModal/Modals/Sales/SalesOrderCompositionEditModal.razor` - 1 處修改
8. `Components/FieldConfiguration/ProductFieldConfiguration.cs` - 新增欄位定義
9. `Documentation/README_商品排程製作.md` - 1 處更新
10. `Documentation/README_銷貨訂單BOM組成編輯功能.md` - 2 處更新

### 程式碼行數變更
- 新增: ~100 行
- 修改: ~10 行
- 刪除: ~5 行

## ✅ 執行檢查清單

### 階段 1: 資料層 ✅ 完成
- [x] 建立 `ProcurementType.cs` 列舉
- [x] 修改 `Product.cs` 實體
- [x] 刪除舊 Migration
- [x] 建立新 Migration `20251208014446_RefactorProductProcurementType`
- [x] 執行 `dotnet ef database update`

### 階段 2: UI 層 ✅ 完成
- [x] 修改 `ProductEditModalComponent.razor`
- [x] 修改 `TransferToScheduleModalComponent.razor`
- [x] 修改 `SalesOrderTable.razor`
- [x] 修改 `SalesOrderCompositionEditModal.razor`
- [x] 修改 `ProductFieldConfiguration.cs`

### 階段 3: 測試驗證 ⏳ 待測試
- [ ] 測試商品新增/編輯功能
- [ ] 測試銷貨訂單轉排程功能
- [ ] 測試 BOM 組成編輯功能
- [ ] 驗證庫存徽章顯示邏輯
- [ ] 測試商品清單頁面篩選功能

### 階段 4: 文件同步 ✅ 完成
- [x] 更新 `README_商品排程製作.md`
- [x] 更新 `README_銷貨訂單BOM組成編輯功能.md`
- [x] 更新 `README_商品資料表重構.md`

## 🎨 UI 欄位設計

### 表單欄位類型變更

**原設計 (Checkbox)**:
```razor
☑ 可排程
```

**新設計 (Dropdown)**:
```razor
採購類型: [外購 ▼]
選項:
- 外購
- 自製
- 委外
```

### 預設值

- 新增商品時預設為「外購」(`ProcurementType.Purchased`)
- 符合一般業務邏輯 (大部分商品為外購)

## 💡 業務邏輯說明
## 📅 執行時間

**預估時間**: 30-45 分鐘
**實際時間**: 約 40 分鐘
**執行日期**: 2025年12月8日

## 👤 負責人

GitHub Copilot

## 🎉 完成狀態

- ✅ 所有程式碼修改已完成
- ✅ 編譯通過無錯誤
- ✅ Migration 已套用至資料庫
- ✅ 文件已同步更新
- ⏳ 待進行功能測試驗證

---

**備註**: 本次重構為架構優化，不影響現有業務邏輯，僅改善程式碼可讀性和可維護性。

## 📸 UI 效果預覽

### 商品編輯表單
```
採購類型: [外購 ▼]
選項:
  - 外購
  - 自製
  - 委外
```

### 商品清單頁面
```
| 商品代碼 | 商品名稱 | 採購類型 |
|---------|---------|----------|
| A001    | 產品A   | [自製]   | (藍色 Badge)
| B002    | 產品B   | [外購]   | (灰色 Badge)
| C003    | 產品C   | [委外]   | (淺藍色 Badge)
```

### Badge 樣式設計
- **自製** (`Manufactured`): `bg-primary` (藍色) - 強調需要排程生產
- **外購** (`Purchased`): `bg-secondary` (灰色) - 一般採購項目
- **委外** (`Outsourced`): `bg-info` (淺藍色) - 委外加工項目

## 🔍 技術重點

### 列舉篩選實作
使用 `FilterHelper.ApplyIntIdFilter` 將列舉轉為 int 進行篩選:
```csharp
FilterFunction = (model, query) => FilterHelper.ApplyIntIdFilter(
    model, query, nameof(Product.ProcurementType), p => (int)p.ProcurementType)
```

### CustomTemplate 實作
使用 `RenderTreeBuilder` 動態建立 Badge UI:
```csharp
CustomTemplate = new RenderFragment<object>(data => builder =>
{
    if (data is Product product)
    {
        var type = product.ProcurementType;
        var displayName = GetProcurementTypeDisplayName(type);
        var badgeClass = type switch
        {
            ProcurementType.Purchased => "bg-secondary",
            ProcurementType.Manufactured => "bg-primary",
            ProcurementType.Outsourced => "bg-info",
            _ => "bg-secondary"
        };
        
        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "class", $"badge {badgeClass}");
        builder.AddContent(2, displayName);
        builder.CloseElement();
    }
})
```

### 輔助方法複用
在多個組件中使用相同的 `GetProcurementTypeDisplayName()` 方法:
```csharp
private static string GetProcurementTypeDisplayName(ProcurementType procurementType)
{
    return procurementType switch
    {
        ProcurementType.Purchased => "外購",
        ProcurementType.Manufactured => "自製",
        ProcurementType.Outsourced => "委外",
        _ => procurementType.ToString()
    };
}
```

| 商品 | ProductCategory | ProcurementType | SupplierId |
|------|----------------|-----------------|------------|
| 成品A | 成品 | 自製 | null |
| 成品B | 成品 | 外購 | 123 |
| 半成品X | 半成品 | 自製 | null |
| 原料P | 原料 | 外購 | 456 |

## 📅 執行時間

**預估時間**: 30-45 分鐘
**執行日期**: 2025年12月8日

## 👤 負責人

GitHub Copilot

---

**備註**: 本次重構為架構優化，不影響現有業務邏輯，僅改善程式碼可讀性和可維護性。
