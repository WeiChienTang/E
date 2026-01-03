# Helpers 結構說明

## 概述

`Helpers` 資料夾存放所有 **純邏輯處理** 的輔助類別（C# 靜態類別或一般類別）。這些 Helper 不包含任何 UI 元素，專門負責封裝可重用的業務邏輯、資料處理、計算方法等。

> ⚠️ **重要區分**：如果你要建立的是有畫面的元件（.razor），請放到 `Components/Shared/`；如果是純邏輯處理（.cs），則放到 `Helpers/`。

---

## Helpers vs Components/Shared 的差異

| 比較項目 | Helpers | Components/Shared |
|---------|---------|-------------------|
| **檔案類型** | C# 類別 (.cs) | Blazor 元件 (.razor) |
| **是否有 UI** | ❌ 無，純邏輯 | ✅ 有，會渲染 HTML |
| **主要用途** | 邏輯處理、計算、資料轉換 | 可重用的視覺介面元件 |
| **使用方式** | 直接呼叫靜態方法或建立實例 | 在 .razor 中以標籤形式引用 |
| **測試方式** | 可獨立進行單元測試 | 需要 Blazor 測試環境 |
| **範例** | `TaxCalculationHelper.CalculateTax()` | `<GenericButtonComponent />` |

### 簡單判斷原則

```
需要建立新的共用功能時，問自己：
├─ 這個功能需要顯示畫面嗎？
│   ├─ 是 → 放 Components/Shared/
│   └─ 否 → 放 Helpers/
│
└─ 這是處理資料、計算、驗證的邏輯嗎？
    ├─ 是 → 放 Helpers/
    └─ 否（是 UI 相關）→ 放 Components/Shared/
```

---

## 目前資料夾結構

```
Helpers/
├── Common/                              # 通用輔助類別
│   ├── ConsoleHelper.cs                 # 主控台輸出輔助
│   ├── CurrentUserHelper.cs             # 當前使用者資訊輔助
│   ├── ErrorHandlingHelper.cs           # 統一錯誤處理（頁面層/服務層）
│   ├── FileUploadHelper.cs              # 檔案上傳處理
│   ├── GlobalExceptionHelper.cs         # 全域例外處理
│   ├── NavigationActionHelper.cs        # 導航操作輔助
│   ├── ReportPrintHelper.cs             # 報表列印輔助
│   ├── SeedDataHelper.cs                # 種子資料輔助
│   └── SimpleQueryCounterInterceptor.cs # 資料庫查詢計數器
│
├── EditModal/                           # 編輯 Modal 相關邏輯
│   ├── ActionButtonHelper.cs            # 動作按鈕配置
│   ├── ApprovalConfigHelper.cs          # 審核流程配置
│   ├── AutoCompleteConfigHelper.cs      # AutoComplete 配置生成（Builder 模式）
│   ├── ChildDocumentRefreshHelper.cs    # 子單據儲存後刷新父單據
│   ├── CodeGenerationHelper.cs          # 基礎編號生成
│   ├── DocumentConversionHelper.cs      # 轉單邏輯（A單轉B單）
│   ├── EntityCodeGenerationHelper.cs    # 實體編碼生成（支援 5 種策略）
│   ├── FormFieldLockHelper.cs           # 表單欄位鎖定/解鎖
│   ├── FormSectionHelper.cs             # 表單區段定義生成
│   ├── ModalManagerInitHelper.cs        # Modal Manager 初始化（Builder 模式）
│   ├── PrefilledValueHelper.cs          # 預填值處理
│   └── TaxCalculationHelper.cs          # 稅額計算
│
├── FieldConfigurationHelper/            # 欄位配置相關
│   └── FilterHelper.cs                  # 篩選器配置
│
├── IndexHelpers/                        # 列表頁相關邏輯
│   ├── BreadcrumbHelper.cs              # 麵包屑導航
│   └── DataLoaderHelper.cs              # 資料載入輔助
│
├── InteractiveTableComponentHelper/     # 互動式表格相關邏輯
│   ├── CalculationHelper.cs             # 表格內計算（小計、總計）
│   ├── DetailLockHelper.cs              # 明細鎖定邏輯
│   ├── DetailSyncHelper.cs              # 明細同步邏輯
│   ├── HistoryCheckHelper.cs            # 歷史記錄檢查
│   ├── InputEventHelper.cs              # 輸入事件處理
│   ├── ItemManagementHelper.cs          # 項目管理（新增、刪除）
│   ├── SearchableSelectHelper.cs        # 可搜尋下拉選單邏輯
│   └── ValidationHelper.cs              # 表格驗證邏輯
│
├── NumericHelpers/                      # 數字處理相關
│   ├── AdjustedBalanceHelper.cs         # 調整餘額計算
│   └── NumberFormatHelper.cs            # 數字格式化
│
├── ServiceHelper/                       # 服務層輔助
│   └── DependencyCheckHelper.cs         # 依賴性檢查（刪除前檢查）
│
├── EntityStatusHelper.cs                # 實體狀態處理
├── ModalHelper.cs                       # Modal 操作統一處理
├── RelatedDocumentsHelper.cs            # 相關單據查詢
└── RelatedEntityModalManagerHelper.cs   # 關聯實體 Modal 管理
```

---

## 分類規則

### 1. Common（通用輔助）

**用途**：存放與特定業務無關的通用輔助類別。

**放置條件**：
- ✅ 跨所有模組都可能使用
- ✅ 不依賴特定業務邏輯
- ✅ 處理基礎設施層面的功能（錯誤處理、檔案、導航等）

**範例**：
- `ErrorHandlingHelper` - 統一錯誤處理
- `FileUploadHelper` - 檔案上傳處理
- `CurrentUserHelper` - 當前使用者資訊

---

### 2. EditModal（編輯 Modal 邏輯）

**用途**：存放與編輯表單（Modal）相關的邏輯處理。

**放置條件**：
- ✅ 為 `GenericEditModalComponent` 或其衍生元件提供邏輯支援
- ✅ 處理表單欄位配置、驗證、計算
- ✅ 處理單據之間的轉換、刷新邏輯

**範例**：
- `FormFieldLockHelper` - 欄位鎖定邏輯
- `TaxCalculationHelper` - 稅額計算
- `DocumentConversionHelper` - 轉單邏輯
- `AutoCompleteConfigHelper` - AutoComplete 配置

**對應的 UI 元件**：`Components/Shared/PageTemplate/GenericEditModalComponent.razor`

---

### 3. IndexHelpers（列表頁邏輯）

**用途**：存放與列表頁（Index Page）相關的邏輯處理。

**放置條件**：
- ✅ 為 `GenericIndexPageComponent` 提供邏輯支援
- ✅ 處理資料載入、麵包屑導航等

**範例**：
- `BreadcrumbHelper` - 麵包屑導航配置
- `DataLoaderHelper` - 資料載入輔助

**對應的 UI 元件**：`Components/Shared/PageTemplate/GenericIndexPageComponent.razor`

---

### 4. InteractiveTableComponentHelper（互動式表格邏輯）

**用途**：存放與互動式表格（可編輯表格）相關的邏輯處理。

**放置條件**：
- ✅ 為 `InteractiveTableComponent` 提供邏輯支援
- ✅ 處理表格內的計算、驗證、事件處理

**範例**：
- `CalculationHelper` - 小計、總計計算
- `ValidationHelper` - 明細驗證
- `InputEventHelper` - 輸入事件處理

**對應的 UI 元件**：`Components/Shared/Base/InteractiveTableComponent.razor`

---

### 5. NumericHelpers（數字處理）

**用途**：存放數字計算、格式化相關的輔助類別。

**放置條件**：
- ✅ 處理數字格式化
- ✅ 處理金額、數量的計算邏輯

**範例**：
- `NumberFormatHelper` - 數字格式化（千分位、小數位）
- `AdjustedBalanceHelper` - 調整餘額計算

---

### 6. ServiceHelper（服務層輔助）

**用途**：存放為 Service 層提供支援的輔助類別。

**放置條件**：
- ✅ 被 Service 層呼叫
- ✅ 處理跨 Service 的共用邏輯

**範例**：
- `DependencyCheckHelper` - 刪除前依賴性檢查

---

### 7. 根目錄 Helper

**用途**：存放獨立的、不屬於特定子分類的 Helper。

**放置條件**：
- ✅ 功能獨立且明確
- ✅ 不適合歸類到現有子資料夾

**範例**：
- `ModalHelper` - Modal 操作統一處理
- `EntityStatusHelper` - 實體狀態處理
- `RelatedDocumentsHelper` - 相關單據查詢

---

## Helper 與 UI 元件的配對關係

```
Helpers/                          ←→    Components/Shared/
├── EditModal/                    ←→    PageTemplate/GenericEditModalComponent.razor
├── IndexHelpers/                 ←→    PageTemplate/GenericIndexPageComponent.razor
├── InteractiveTableComponentHelper/ ←→ Base/InteractiveTableComponent.razor
├── ModalHelper.cs                ←→    Base/BaseModalComponent.razor
└── RelatedDocumentsHelper.cs     ←→    RelatedDocument/RelatedDocumentsModalComponent.razor
```

---

## 新增 Helper 的決策流程

```
需要新增輔助類別
    │
    ├─ 這是處理 UI 元件的邏輯嗎？
    │   │
    │   ├─ 是 EditModal 相關？ → Helpers/EditModal/
    │   ├─ 是 Index 列表頁相關？ → Helpers/IndexHelpers/
    │   ├─ 是 InteractiveTable 相關？ → Helpers/InteractiveTableComponentHelper/
    │   └─ 是其他 UI 元件？ → 考慮建立新的子資料夾
    │
    ├─ 這是通用的基礎設施功能嗎？
    │   └─ 是 → Helpers/Common/
    │
    ├─ 這是數字計算/格式化嗎？
    │   └─ 是 → Helpers/NumericHelpers/
    │
    ├─ 這是 Service 層的輔助嗎？
    │   └─ 是 → Helpers/ServiceHelper/
    │
    └─ 獨立功能，不屬於以上分類？
        └─ 放在 Helpers/ 根目錄
```

---

## 命名規範

### 類別命名
- 使用 PascalCase
- 以功能描述為主，後綴加上 `Helper`
- 範例：`TaxCalculationHelper`、`FormFieldLockHelper`

### 靜態 vs 實例

| 類型 | 使用時機 | 範例 |
|------|---------|------|
| **靜態類別** | 純函式、無狀態 | `TaxCalculationHelper.CalculateTax()` |
| **實例類別** | 需要注入服務、有狀態 | `new RelatedDocumentsHelper(contextFactory)` |

### Builder 模式
對於複雜的配置類 Helper，使用 Builder 模式提升可讀性：
```csharp
// ✅ 推薦：Builder 模式
var config = AutoCompleteConfigHelper.CreateBuilder<Customer>()
    .AddField(nameof(Customer.EmployeeId), "Name", employees)
    .AddField(nameof(Customer.PaymentMethodId), "Name", paymentMethods)
    .Build();

// ❌ 避免：大量參數的方法呼叫
var config = CreateConfig(prop1, prop2, prop3, prop4, prop5, ...);
```

---

## 使用範例

### 在 Razor 元件中使用靜態 Helper

```razor
@code {
    private void CalculateTax()
    {
        // 直接呼叫靜態方法
        entity.TaxAmount = TaxCalculationHelper.CalculateTax(entity.TotalAmount, taxRate);
    }
}
```

### 在 Razor 元件中使用需注入的 Helper

```razor
@inject RelatedDocumentsHelper RelatedDocumentsHelper

@code {
    private async Task LoadRelatedDocuments()
    {
        var documents = await RelatedDocumentsHelper
            .GetRelatedDocumentsForSalesOrderDetailAsync(detailId);
    }
}
```

### 使用 Builder 模式配置

```razor
@code {
    private AutoCompleteConfig? autoCompleteConfig;
    
    protected override void OnInitialized()
    {
        autoCompleteConfig = AutoCompleteConfigHelper.CreateBuilder<Customer>()
            .AddField(nameof(Customer.EmployeeId), "Name", availableEmployees)
            .Build();
    }
}
```

---

## 注意事項

1. **保持純邏輯**：Helper 不應該包含任何 Blazor 元件程式碼（如 `StateHasChanged`），這些應該透過委派傳入

2. **避免循環依賴**：Helper 之間盡量不要互相依賴，如有需要，考慮合併或重構

3. **文件同步更新**：新增 Helper 時，請更新此文件

4. **單元測試**：Helper 類別應該要能夠獨立進行單元測試

5. **相關文件**：
   - [Shared 元件結構說明](../Components/Shared/README_Shared結構圖.md)
   - [EditModal Helper 建構指南](./EditModal/README_EditModal_Helper建構.md)
   - [InteractiveTable Helper 說明](./InteractiveTableComponentHelper/README_InteractiveTable重構計劃.md)
