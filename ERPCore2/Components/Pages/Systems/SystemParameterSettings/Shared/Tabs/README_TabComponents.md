# Tab 表單統一元件使用指南

## 📋 概述

為了確保系統參數設定頁面的 Tab UI 保持一致性和易維護性，我們創建了一套統一的表單元件。

## 🎯 核心元件

### 1. **TabFormFieldComponent** - 表單欄位元件

統一的欄位包裝元件，處理標籤、輸入框、幫助文字和驗證訊息的布局。

**參數：**
- `Label` (string) - 欄位標籤文字
- `IconClass` (string) - 圖示 CSS 類別 (Font Awesome)
- `IsRequired` (bool) - 是否為必填欄位，會顯示紅色星號
- `HelpText` (string) - 幫助文字說明
- `LabelColumnCssClass` (string) - 標籤欄位的 CSS，預設 "col-sm-3"
- `InputColumnCssClass` (string) - 輸入欄位的 CSS，預設 "col-sm-9"
- `ChildContent` (RenderFragment) - 輸入控制項內容
- `ValidationContent` (RenderFragment) - 驗證訊息內容

**使用範例：**
```razor
<TabFormFieldComponent Label="稅率設定" 
                      IconClass="fas fa-percentage"
                      IsRequired="true"
                      HelpText="設定系統預設稅率">
    <ChildContent>
        <div class="input-group">
            <InputNumber @bind-Value="Model.TaxRate" 
                       class="form-control" 
                       step="0.01" />
            <span class="input-group-text">%</span>
        </div>
    </ChildContent>
    <ValidationContent>
        <ValidationMessage For="@(() => Model.TaxRate)" class="text-danger" />
    </ValidationContent>
</TabFormFieldComponent>
```

---

### 2. **TabFormSectionComponent** - 表單區段元件

用於組織相關的表單欄位，提供區段標題和說明。

**參數：**
- `Title` (string) - 區段標題
- `Description` (string) - 區段說明文字
- `IconClass` (string) - 標題圖示 CSS 類別
- `CssClass` (string) - 區段容器的 CSS 類別
- `ContentCssClass` (string) - 內容區域的 CSS 類別
- `ChildContent` (RenderFragment) - 區段內容（包含多個 TabFormFieldComponent）

**使用範例：**
```razor
<TabFormSectionComponent Title="基本設定" 
                        IconClass="fas fa-cog"
                        Description="設定系統全域的基本參數">
    
    <TabFormFieldComponent Label="名稱" ...>
        ...
    </TabFormFieldComponent>
    
    <TabFormFieldComponent Label="數量" ...>
        ...
    </TabFormFieldComponent>
    
</TabFormSectionComponent>
```

---

### 3. **SystemParameterTabContainer** - Tab 容器元件（已存在）

處理 Tab 頁面的載入狀態、錯誤訊息和底部資訊。

**參數：**
- `IsLoading` (bool) - 是否正在載入
- `ErrorMessage` (string) - 錯誤訊息
- `ShowFooter` (bool) - 是否顯示底部資訊
- `Entity` (BaseEntity) - 實體資料（用於顯示建立/修改資訊）
- `LoadingText` (string) - 載入提示文字

---

### 4. **SystemParameterTabHeader** - Tab 表頭元件（已存在）

統一的標籤頁導航元件。

---

## 📝 完整使用流程

### 步驟 1：創建 Tab 檔案

在 `Components/Pages/Systems/SystemParameterSettings/Shared/Tabs/` 創建新的 Razor 元件。

### 步驟 2：引入必要的命名空間

```razor
@using Microsoft.AspNetCore.Components.Forms
@using ERPCore2.Data.Entities
@using ERPCore2.Components.Pages.Systems.SystemParameterSettings.Shared.UIComponent
```

### 步驟 3：使用 EditForm 包裝表單

```razor
<EditForm Model="@Model" OnValidSubmit="@HandleSubmit">
    <DataAnnotationsValidator />
    <ValidationSummary class="text-danger mb-3" />
    
    <!-- 在此處添加表單內容 -->
    
</EditForm>
```

### 步驟 4：組織表單結構

```razor
<div class="row">
    <div class="col-lg-8">
        
        <!-- 區段 1 -->
        <TabFormSectionComponent Title="基本資訊" IconClass="fas fa-info-circle">
            <!-- 欄位 -->
        </TabFormSectionComponent>
        
        <!-- 區段 2（可選） -->
        <TabFormSectionComponent Title="進階設定" IconClass="fas fa-cogs">
            <!-- 欄位 -->
        </TabFormSectionComponent>
        
        <!-- 按鈕區域 -->
        <div class="row">
            <div class="col-sm-9 offset-sm-3">
                <!-- 按鈕 -->
            </div>
        </div>
        
    </div>
</div>
```

### 步驟 5：添加欄位

根據輸入類型選擇適當的控制項：

**文字輸入：**
```razor
<TabFormFieldComponent Label="名稱" IconClass="fas fa-tag" IsRequired="true">
    <ChildContent>
        <InputText @bind-Value="Model.Name" class="form-control" />
    </ChildContent>
    <ValidationContent>
        <ValidationMessage For="@(() => Model.Name)" class="text-danger" />
    </ValidationContent>
</TabFormFieldComponent>
```

**數字輸入：**
```razor
<TabFormFieldComponent Label="數量" IconClass="fas fa-hashtag">
    <ChildContent>
        <InputNumber @bind-Value="Model.Quantity" class="form-control" />
    </ChildContent>
</TabFormFieldComponent>
```

**帶單位的輸入：**
```razor
<TabFormFieldComponent Label="價格" IconClass="fas fa-dollar-sign">
    <ChildContent>
        <div class="input-group">
            <span class="input-group-text">NT$</span>
            <InputNumber @bind-Value="Model.Price" class="form-control" />
        </div>
    </ChildContent>
</TabFormFieldComponent>
```

**下拉選單：**
```razor
<TabFormFieldComponent Label="類型" IconClass="fas fa-list">
    <ChildContent>
        <InputSelect @bind-Value="Model.Type" class="form-select">
            <option value="">-- 請選擇 --</option>
            <option value="Type1">類型一</option>
        </InputSelect>
    </ChildContent>
</TabFormFieldComponent>
```

**核取方塊：**
```razor
<TabFormFieldComponent Label="啟用" IconClass="fas fa-toggle-on">
    <ChildContent>
        <div class="form-check form-switch">
            <InputCheckbox @bind-Value="Model.IsActive" class="form-check-input" />
            <label class="form-check-label">啟用此功能</label>
        </div>
    </ChildContent>
</TabFormFieldComponent>
```

**多行文字：**
```razor
<TabFormFieldComponent Label="備註" IconClass="fas fa-comment">
    <ChildContent>
        <InputTextArea @bind-Value="Model.Remarks" class="form-control" rows="4" />
    </ChildContent>
</TabFormFieldComponent>
```

---

## 🎨 樣式自訂

### 自訂欄位寬度

預設是 `col-sm-3` (標籤) 和 `col-sm-9` (輸入)，可以自訂：

```razor
<TabFormFieldComponent Label="自訂欄位" 
                      LabelColumnCssClass="col-sm-4"
                      InputColumnCssClass="col-sm-8">
    <!-- 內容 -->
</TabFormFieldComponent>
```

---

## 📦 參考檔案

- **範本檔案**: `_TabTemplate.razor` - 完整的 Tab 開發範本
- **實際範例**: `BasicSettingsTab.razor` - 已重構的實際範例
- **元件位置**: `Shared/UIComponent/`

---

## ✅ 優點

1. **UI 一致性** - 所有 Tab 使用相同的布局和樣式
2. **易於維護** - 修改元件即可影響所有使用處
3. **減少重複代碼** - 不需要每次都寫相同的 HTML 結構
4. **提升開發效率** - 使用範本快速創建新的 Tab
5. **響應式設計** - 自動適應不同螢幕尺寸

---

## 🚀 快速開始

1. 複製 `_TabTemplate.razor`
2. 重新命名為您的功能名稱
3. 修改 Model 和欄位定義
4. 在主頁面中註冊新的 Tab

完成！您的 Tab 將自動擁有統一的 UI 風格。
