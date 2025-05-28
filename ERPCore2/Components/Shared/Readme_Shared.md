# ERPCore2 共享組件說明文件 (Shared Components)

本文件說明 ERPCore2 系統中共享組件的結構、功能與使用方式。所有組件都遵循 SOLID 原則設計，使用 Bootstrap 5 樣式系統，並採用一致的設計風格。

## 📁 資料夾結構

```
Components/Shared/
├── Actions/          # 操作相關組件
├── Alerts/           # 警告訊息組件
├── Badges/           # 徽章狀態組件
├── Buttons/          # 按鈕組件
├── Details/          # 詳細資料顯示組件
├── Forms/            # 表單相關組件
├── Headers/          # 頁面標題組件
├── Loading/          # 載入指示器組件
└── Tables/           # 表格組件
```

---

## 🎯 Actions 資料夾

### PageActionBar.razor
**用途**：頁面操作按鈕列，提供主要和次要操作按鈕的容器

**參數**：
- `PrimaryActions` (RenderFragment?) - 主要操作按鈕區域
- `SecondaryActions` (RenderFragment?) - 次要操作按鈕區域

**使用範例**：
```razor
<PageActionBar>
    <PrimaryActions>
        <ButtonComponent Text="儲存" Variant="ButtonVariant.Primary" />
    </PrimaryActions>
    <SecondaryActions>
        <ButtonComponent Text="取消" Variant="ButtonVariant.Secondary" />
    </SecondaryActions>
</PageActionBar>
```

---

## ⚠️ Alerts 資料夾

### AlertComponent.razor
**用途**：顯示各種類型的警告訊息，支援自動關閉功能

**參數**：
- `Message` (string) - 警告訊息內容
- `Type` (AlertType) - 警告類型：Success, Warning, Danger, Info
- `IsVisible` (bool) - 是否顯示，預設 true
- `IsDismissible` (bool) - 是否可關閉，預設 true
- `IconClass` (string) - 圖示 CSS 類別
- `OnDismiss` (EventCallback) - 關閉時的回呼事件
- `ChildContent` (RenderFragment?) - 額外內容

**使用範例**：
```razor
<AlertComponent Type="AlertType.Success" 
                Message="資料儲存成功！" 
                IconClass="fas fa-check" />
```

---

## 🏷️ Badges 資料夾

### StatusBadgeComponent.razor
**用途**：顯示實體狀態的徽章，支援自訂文字和樣式

**參數**：
- `Status` (EntityStatus) - 實體狀態：Active, Inactive, Deleted
- `CustomText` (string?) - 自訂顯示文字
- `IconClass` (string) - 圖示 CSS 類別
- `CssClass` (string) - 額外 CSS 類別
- `Size` (BadgeSize) - 徽章大小：Small, Normal

**使用範例**：
```razor
<StatusBadgeComponent Status="EntityStatus.Active" 
                      IconClass="fas fa-check" />
```

---

## 🔘 Buttons 資料夾

### ButtonComponent.razor
**用途**：基礎按鈕組件，支援多種變體、大小和狀態

**參數**：
- `Text` (string) - 按鈕文字
- `CssClass` (string) - 額外 CSS 類別
- `Title` (string) - 工具提示
- `Variant` (ButtonVariant) - 按鈕變體：Primary, Secondary, Success, Warning, Danger, Info, OutlinePrimary 等
- `Size` (ButtonSize) - 按鈕大小：Small, Normal, Large
- `Type` (string) - HTML 按鈕類型，預設 "button"
- `IsDisabled` (bool) - 是否禁用
- `IsLoading` (bool) - 是否顯示載入狀態
- `IsSubmit` (bool) - 是否為提交按鈕
- `IconClass` (string) - 圖示 CSS 類別
- `OnClick` (EventCallback) - 點擊事件
- `ChildContent` (RenderFragment?) - 額外內容

**使用範例**：
```razor
<ButtonComponent Text="儲存" 
                 Variant="ButtonVariant.Primary" 
                 IconClass="fas fa-save"
                 OnClick="SaveData" />
```

---

## 📋 Details 資料夾

### DetailItemComponent.razor
**用途**：顯示單一詳細資料項目，包含標籤和值

**參數**：
- `Label` (string) - 標籤文字
- `Value` (string?) - 顯示值
- `EmptyText` (string) - 空值時顯示文字，預設 "-"
- `IconClass` (string) - 圖示 CSS 類別
- `LabelClass` (string) - 標籤額外 CSS 類別
- `ValueClass` (string) - 值額外 CSS 類別
- `ChildContent` (RenderFragment?) - 自訂內容

### DetailSectionComponent.razor
**用途**：詳細資料區段組件，用於組織詳細資料的分組

**參數**：
- `Title` (string) - 區段標題
- `IconClass` (string) - 圖示 CSS 類別
- `TitleClass` (string) - 標題額外 CSS 類別
- `ShowDivider` (bool) - 是否顯示分隔線，預設 true
- `ChildContent` (RenderFragment?) - 區段內容

### RelatedDataCardComponent.razor
**用途**：相關資料卡片組件，用於顯示關聯資料（如聯絡方式、地址等）

**參數**：
- `Title` (string) - 卡片標題
- `IconClass` (string) - 圖示 CSS 類別
- `Count` (int) - 資料筆數
- `ShowCount` (bool) - 是否顯示筆數，預設 true
- `HasData` (bool) - 是否有資料
- `EmptyMessage` (string) - 無資料時顯示訊息，預設 "尚未建立資料"
- `ShowMoreIndicator` (bool) - 是否顯示更多指示器，預設 true
- `DisplayLimit` (int) - 顯示限制筆數，預設 3
- `ChildContent` (RenderFragment?) - 卡片內容

---

## 📝 Forms 資料夾

### AddressManagement.razor
**用途**：地址管理組件，支援多個地址的新增、編輯和管理

**參數**：
- `Addresses` (List<CustomerAddress>) - 地址清單
- `AddressesChanged` (EventCallback<List<CustomerAddress>>) - 地址清單變更事件
- `AddressTypes` (List<AddressType>) - 地址類型清單
- `ShowAddButton` (bool) - 是否顯示新增按鈕，預設 true
- `ShowRemoveButton` (bool) - 是否顯示移除按鈕，預設 true
- `MinAddressCount` (int) - 最少地址數量，預設 1
- 各種事件回呼：`OnAddAddress`, `OnRemoveAddress`, `OnSetPrimaryAddress` 等

### FormSectionComponent.razor
**用途**：表單區塊組件，提供表單欄位分組和完成度指示

**參數**：
- `Title` (string) - 區塊標題
- `IconClass` (string) - 圖示 CSS 類別
- `SectionType` (FormSectionType) - 區塊類型：Basic, Contact, Address, Financial, System
- `ShowCompletionStatus` (bool) - 是否顯示完成狀態，預設 true
- `RequiredFieldsCount` (int) - 必填欄位數量
- `CompletedFieldsCount` (int) - 已完成欄位數量
- `CssClass` (string) - 額外 CSS 類別
- `BodyCssClass` (string) - 內容區域 CSS 類別
- `ChildContent` (RenderFragment?) - 表單內容

### InputComponent.razor
**用途**：統一的輸入欄位組件，支援各種輸入類型

**參數**：
- `Id` (string) - 欄位 ID
- `Label` (string) - 標籤文字
- `Value` (string) - 欄位值
- `ValueChanged` (EventCallback<string>) - 值變更事件
- `InputType` (string) - 輸入類型，預設 "text"
- `Placeholder` (string) - 佔位符文字
- `IsRequired` (bool) - 是否必填
- `IsDisabled` (bool) - 是否禁用
- `IsReadOnly` (bool) - 是否唯讀
- `CssClass` (string) - 額外 CSS 類別
- `ContainerCssClass` (string) - 容器 CSS 類別
- `HelpText` (string) - 說明文字
- `ErrorMessage` (string) - 錯誤訊息
- `Rows` (int) - 文字區域行數，預設 3
- `BindEvent` (string) - 綁定事件，預設 "onchange"

### SearchComponent.razor
**用途**：搜尋欄組件，用於列表頁面的搜尋功能

**參數**：
- `SearchTerm` (string) - 搜尋關鍵字
- `SearchTermChanged` (EventCallback<string>) - 搜尋關鍵字變更事件
- `OnSearch` (EventCallback<string>) - 搜尋事件
- `Placeholder` (string) - 佔位符文字，預設 "請輸入搜尋關鍵字..."
- `ShowClearButton` (bool) - 是否顯示清除按鈕，預設 true
- `CssClass` (string) - 額外 CSS 類別

### SelectComponent<TValue>.razor
**用途**：泛型下拉選單組件，支援各種資料類型

**參數**：
- `Id` (string) - 欄位 ID
- `Label` (string) - 標籤文字
- `Value` (TValue?) - 選中值
- `ValueChanged` (EventCallback<TValue?>) - 值變更事件
- `Items` (IEnumerable<object>) - 選項清單
- `GetItemText` (Func<object, string>) - 取得選項顯示文字的函數
- `GetItemValue` (Func<object, object>) - 取得選項值的函數
- `IsRequired` (bool) - 是否必填
- `IsDisabled` (bool) - 是否禁用
- `ShowEmptyOption` (bool) - 是否顯示空選項，預設 true
- `EmptyOptionText` (string) - 空選項文字，預設 "請選擇..."
- `CssClass` (string) - 額外 CSS 類別
- `HelpText` (string) - 說明文字

---

## 📑 Headers 資料夾

### PageHeaderComponent.razor
**用途**：統一的頁面標題組件

**參數**：
- `Title` (string) - 主標題
- `Subtitle` (string) - 副標題
- `IconClass` (string) - 圖示 CSS 類別
- `Actions` (RenderFragment?) - 操作按鈕區域
- `CssClass` (string) - 額外 CSS 類別

---

## ⏳ Loading 資料夾

### LoadingComponent.razor
**用途**：載入指示器組件，提供統一的載入動畫

**參數**：
- `IsLoading` (bool) - 是否顯示載入狀態，預設 true
- `LoadingText` (string) - 載入文字，預設 "載入中..."
- `ShowText` (bool) - 是否顯示載入文字
- `Size` (LoadingSize) - 載入器大小：Small, Normal, Large
- `IsCentered` (bool) - 是否置中，預設 true
- `CssClass` (string) - 額外 CSS 類別

---

## 📊 Tables 資料夾

### TableComponent<TItem>.razor
**用途**：泛型表格組件，支援各種表格樣式和功能

**參數**：
- `Items` (IEnumerable<TItem>?) - 資料清單
- `Headers` (List<string>?) - 表頭清單
- `RowTemplate` (RenderFragment<TItem>) - 資料列模板
- `ActionsTemplate` (RenderFragment<TItem>?) - 操作欄模板
- `EmptyTemplate` (RenderFragment?) - 空資料模板
- `ShowHeader` (bool) - 是否顯示表頭，預設 true
- `ShowActions` (bool) - 是否顯示操作欄
- `IsStriped` (bool) - 是否使用條紋樣式，預設 true
- `IsHoverable` (bool) - 是否支援懸停效果，預設 true
- `IsBordered` (bool) - 是否顯示邊框
- `Size` (TableSize) - 表格大小：Small, Normal, Large
- `CssClass` (string) - 額外 CSS 類別
- `EmptyMessage` (string) - 空資料訊息，預設 "沒有找到資料"
- `ActionsHeader` (string) - 操作欄標題，預設 "操作"
- `GetRowCssClass` (Func<TItem, string>?) - 取得資料列 CSS 類別的函數

---

## 🎨 設計原則

### 色彩系統
- **主色調**：深藍色 (`text-primary-custom`)
- **次要色彩**：灰色 (`text-secondary-custom`)
- **輔助色彩**：淺灰色 (`text-light-custom`)

### 命名規範
- 所有組件都以 `Component` 結尾
- 使用 Pascal Case 命名
- 參數使用描述性名稱

### 共同特色
- 支援 Bootstrap 5 樣式系統
- 遵循 SOLID 原則設計
- 提供一致的使用者體驗
- 支援事件回呼機制
- 具備完整的型別安全

---

## 📖 使用指南

### 基本原則
1. **一致性**：所有組件都遵循相同的設計原則和命名規範
2. **可重用性**：組件設計為高度可重用，適用於不同場景
3. **型別安全**：充分利用 C# 的型別系統，避免執行時錯誤
4. **效能最佳化**：使用適當的生命週期管理和狀態更新機制

### 最佳實踐
1. 在使用組件前，先了解其參數和事件
2. 適當使用 CSS 類別來自訂樣式
3. 確保事件回呼的正確實作
4. 注意組件的相依性和命名空間

### 擴展建議
如需新增共享組件：
1. 遵循現有的資料夾結構
2. 使用一致的命名規範
3. 提供完整的參數文件
4. 確保型別安全和效能

---

## 📝 版本資訊

- **版本**：2.0
- **技術棧**：ASP.NET Blazor Server .NET 9.0
- **UI 框架**：Bootstrap 5
- **最後更新**：2025年5月