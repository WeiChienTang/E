# Shared 元件結構說明

## 概述

`Components/Shared` 資料夾存放所有 **跨業務共用** 的元件。大多數開發所需的基礎元件、UI 元件、頁面模板都可以在此找到。若需要新增元件，請務必依照以下分類規則放置。

---

## 目前資料夾結構

```
Components/Shared/
├── Base/                    # 基礎框架元件
│   ├── BaseModalComponent.razor         # Modal 對話框基礎元件
│   ├── InteractiveTableComponent.razor  # 互動式表格元件
│   ├── InteractiveColumnDefinition.cs   # 互動式表格欄位定義
│   └── InteractiveColumnType.cs         # 互動式表格欄位類型列舉
│
├── UI/                      # 通用 UI 元件
│   ├── Auth/                # 權限檢查元件
│   │   ├── NavigationPermissionCheck.razor
│   │   ├── PagePermissionCheck.razor
│   │   └── PermissionCheck.razor
│   │
│   ├── Badge/               # 狀態標籤元件
│   │   └── GenericStatusBadgeComponent.razor
│   │
│   ├── Button/              # 按鈕元件
│   │   ├── GenericButtonComponent.razor
│   │   ├── ButtonSize.cs
│   │   └── ButtonVariant.cs
│   │
│   ├── Form/                # 表單欄位元件
│   │   ├── FormTextField.razor
│   │   ├── FormNumberField.razor
│   │   ├── FormSelectField.razor
│   │   ├── FormSearchableSelectField.razor  # 可搜尋下拉選單（獨立元件）
│   │   ├── FormDateField.razor
│   │   ├── FormCheckboxField.razor
│   │   ├── FormRadioField.razor
│   │   ├── FormTextAreaField.razor
│   │   ├── FormAutoCompleteField.razor
│   │   ├── FormMobilePhoneField.razor
│   │   ├── CharacterCountTextAreaComponent.razor
│   │   ├── FormFieldDefinition.cs
│   │   ├── FormConstants.cs
│   │   └── AutoCompleteFieldState.cs
│   │
│   ├── Header/              # 頁面標頭元件
│   │   └── GenericHeaderComponent.razor
│   │
│   ├── Message/             # 訊息提示元件
│   │   └── GenericLockedFieldMessage.razor
│   │
│   └── NavMenu/             # 導覽選單元件
│       ├── NavMenuItem.razor
│       └── NavDropdownItem.razor
│
├── PageTemplate/            # 頁面骨架模板
│   ├── GenericIndexPageComponent.razor      # 列表頁面模板
│   ├── GenericEditModalComponent.razor      # 編輯 Modal 模板
│   ├── GenericFormComponent.razor           # 表單元件模板
│   ├── GenericTableComponent.razor          # 表格元件模板
│   ├── GenericSearchFilterComponent.razor   # 搜尋篩選模板
│   ├── GenericStatisticsCards.razor         # 統計卡片模板
│   ├── StatisticsCard.razor                 # 單一統計卡片
│   ├── IndexActionButtonsComponent.razor    # 列表頁動作按鈕
│   ├── TableColumnDefinition.cs             # 表格欄位定義
│   ├── SearchFilterDefinition.cs            # 搜尋篩選定義
│   └── RelatedEntityModalManager.cs         # 關聯實體 Modal 管理器
│
├── QuickAction/             # 快速動作元件
│   ├── QuickActionMenu.razor                # 快速動作選單
│   ├── PageSearchModalComponent.razor       # 頁面搜尋 Modal
│   └── ShortcutKeysModalComponent.razor     # 快捷鍵說明 Modal
│
├── RelatedDocument/         # 關聯單據元件
│   ├── RelatedDocumentsModalComponent.razor # 關聯單據 Modal 主元件
│   ├── Components/
│   │   └── RelatedDocumentSectionComponent.razor
│   ├── Config/
│   │   └── DocumentSectionConfig.cs
│   └── Templates/                           # 各類單據詳細資訊模板
│       ├── CompositionDetailsTemplate.razor
│       ├── ReceivingDetailsTemplate.razor
│       ├── ReturnDetailsTemplate.razor
│       ├── SalesOrderDetailsTemplate.razor
│       ├── SetoffDetailsTemplate.razor
│       └── SupplierRecommendationDetailsTemplate.razor
│
├── Report/                  # 報表篩選元件
│   ├── BatchPrintFilterModalComponent.razor # 批次列印篩選 Modal
│   ├── DateRangeFilterComponent.razor       # 日期區間篩選
│   ├── FilterSectionComponent.razor         # 篩選區段
│   └── MultiSelectFilterComponent.razor     # 多選篩選
│
└── RelatedEntityModalManagerExtensions.cs   # Modal 管理器擴充方法
```

---

## 分類規則

### 1. Base（基礎框架元件）

**用途**：提供最底層的框架元件，供其他元件繼承或組合使用。

**放置條件**：
- ✅ 作為其他元件的基底類別
- ✅ 提供核心互動功能（如 Modal 開關、表格互動）
- ✅ 不依賴特定業務邏輯

**範例**：
- `BaseModalComponent` - 所有 Modal 的基礎
- `InteractiveTableComponent` - 可編輯表格的基礎

---

### 2. UI（通用 UI 元件）

**用途**：存放獨立的 UI 元件，可在任何頁面中重複使用。

**子資料夾分類規則**：

| 子資料夾 | 用途 | 範例 |
|---------|------|------|
| `Auth/` | 權限檢查相關 | PermissionCheck |
| `Badge/` | 狀態標籤、徽章 | GenericStatusBadgeComponent |
| `Button/` | 按鈕相關 | GenericButtonComponent |
| `Form/` | 表單欄位元件 | FormTextField, FormSelectField |
| `Header/` | 頁面標頭 | GenericHeaderComponent |
| `Message/` | 訊息提示 | GenericLockedFieldMessage |
| `NavMenu/` | 導覽選單 | NavMenuItem, NavDropdownItem |

**放置條件**：
- ✅ 單一職責的 UI 元件
- ✅ 不包含業務邏輯
- ✅ 可透過參數配置外觀和行為

**新增子資料夾規則**：
若有新類型的 UI 元件（如 `Tab/`、`Modal/`、`Loading/`），可新增對應子資料夾。

---

### 3. PageTemplate（頁面骨架模板）

**用途**：提供標準化的頁面架構，確保所有頁面有一致的外觀和行為。

**放置條件**：
- ✅ 組合多個 UI 元件形成完整頁面區塊
- ✅ 定義頁面的標準結構（列表頁、編輯頁、表單）
- ✅ 包含通用的頁面邏輯（搜尋、分頁、表格顯示）

**範例**：
- `GenericIndexPageComponent` - 列表頁的標準結構
- `GenericEditModalComponent` - 編輯 Modal 的標準結構
- `GenericFormComponent` - 表單的標準結構

---

### 4. QuickAction（快速動作元件）

**用途**：提供跨頁面的快速操作功能。

**放置條件**：
- ✅ 全域性的快捷功能
- ✅ 不限定於特定業務模組
- ✅ 提供效率提升的輔助功能

**範例**：
- `QuickActionMenu` - 快速動作浮動選單
- `PageSearchModalComponent` - 跨頁面搜尋
- `ShortcutKeysModalComponent` - 快捷鍵說明

---

### 5. RelatedDocument（關聯單據元件）

**用途**：處理不同業務單據之間的關聯顯示。

**放置條件**：
- ✅ 顯示單據間的關聯關係
- ✅ 跨多個業務模組共用
- ✅ 單據詳細資訊的展示模板

**子資料夾**：
- `Components/` - 關聯單據的子元件
- `Config/` - 設定檔
- `Templates/` - 各類單據的詳細資訊模板

---

### 6. Report（報表篩選元件）

**用途**：提供報表相關的篩選和列印功能。

**放置條件**：
- ✅ 報表的共用篩選元件
- ✅ 批次列印相關功能
- ✅ 日期區間、多選等篩選器

---

## 業務專屬元件放置位置

⚠️ **注意**：業務專屬的元件 **不應該** 放在 Shared 資料夾！

應放置於對應的 Pages 資料夾：

```
Components/Pages/
├── Purchase/        # 進貨相關的業務元件
├── Sales/           # 銷貨相關的業務元件
├── Products/        # 商品相關的業務元件
├── Warehouse/       # 倉庫相關的業務元件
├── ProductionManagement/  # 生產管理相關的業務元件
├── FinancialManagement/   # 財務管理相關的業務元件
└── Suppliers/       # 供應商相關的業務元件
```

**判斷標準**：
- 只有某一個業務模組會使用 → 放 `Pages/[業務模組]/`
- 多個業務模組都會使用 → 放 `Shared/`

---

## 新增元件的決策流程

```
新增元件
    │
    ├─ 只有單一業務模組使用？
    │       │
    │       └─ 是 → 放到 Pages/[業務模組]/
    │
    └─ 多個業務模組共用？
            │
            ├─ 是最底層框架元件？ → Shared/Base/
            │
            ├─ 是獨立 UI 元件？ → Shared/UI/[類型]/
            │
            ├─ 是頁面架構模板？ → Shared/PageTemplate/
            │
            ├─ 是快速動作功能？ → Shared/QuickAction/
            │
            ├─ 是關聯單據相關？ → Shared/RelatedDocument/
            │
            └─ 是報表篩選相關？ → Shared/Report/
```

---

## 命名規範

### 元件命名
- 使用 PascalCase
- 以功能描述為主，後綴加上 `Component`
- 範例：`GenericButtonComponent`、`FormTextField`

### 定義類別命名
- 欄位定義：`[類型]ColumnDefinition.cs`
- 篩選定義：`[類型]FilterDefinition.cs`
- 狀態/列舉：`[功能]Type.cs` 或 `[功能]State.cs`

### 資料夾命名
- 使用 PascalCase
- 以功能分類命名
- 避免使用縮寫

---

## 注意事項

1. **避免重複造輪子**：新增元件前，請先檢查 Shared 資料夾是否已有類似功能的元件。

2. **保持單一職責**：每個元件應只負責一項功能，避免建立過於複雜的巨型元件。

3. **文件同步更新**：若新增新的資料夾或重要元件，請更新此文件。

4. **相關文件**：
   - [README_套用基礎Modal.md](../Shared/Base/README_套用基礎Modal.md)
   - [README_GenericFormComponent結構.md](README_GenericFormComponent結構.md)
   - [README_互動Table說明.md](README_互動Table說明.md)
