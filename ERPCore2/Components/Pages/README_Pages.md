# ERPCore2 頁面開發指南

## 概述

為確保系統頁面的一致性與使用者體驗，所有頁面都應遵循統一的設計規範和開發模式。本文件定義了標準頁面結構、設計風格和實作方式。

## 標準頁面類型

一般標準頁面包含以下3個核心頁面：

### 1. [名稱]Index - 列表頁面
- **用途**：顯示資料列表、搜尋篩選、分頁導航
- **檔案命名**：`[實體名稱]Index.razor`
- **路由**：`/[實體名稱複數形]`

### 2. [名稱]Edit - 編輯頁面  
- **用途**：新增和修改資料（共用同一頁面）
- **檔案命名**：`[實體名稱]Edit.razor`
- **路由**：
  - 新增：`/[實體名稱複數形]/edit`
  - 編輯：`/[實體名稱複數形]/edit/{id:int?}`

### 3. [名稱]Detail - 詳細頁面
- **用途**：檢視詳細資料、唯讀模式
- **檔案命名**：`[實體名稱]Detail.razor`
- **路由**：`/[實體名稱複數形]/detail/{id:int}`

## 檔案組織結構

頁面檔案應放置在以下目錄結構：

```
Components/
  Pages/
    [實體類型]/          # 例如：Customers、Products、Suppliers
      [實體名稱]Index.razor
      [實體名稱]Edit.razor
      [實體名稱]Detail.razor
```

**範例**：
- `Components/Pages/Customers/CustomerIndex.razor`
- `Components/Pages/Customers/CustomerEdit.razor`
- `Components/Pages/Customers/CustomerDetail.razor`

## 色彩設計規範

所有頁面都應採用 `wwwroot/css/variables.css` 中定義的設計變數：

### 主要色彩
- **主要藍色**：`var(--primary-blue)` - #1E3A8A
- **主要白色**：`var(--primary-white)` - #FFFFFF
- **淺灰背景**：`var(--primary-light-gray)` - #F9FAFB

### 文字色彩
- **主要文字**：`var(--text-primary)` - #374151
- **次要文字**：`var(--text-secondary)` - #6B7280
- **淺色文字**：`var(--text-light)` - #9CA3AF

### 狀態色彩
- **成功**：`var(--success-color)` - #059669
- **警告**：`var(--warning-color)` - #EA580C
- **危險**：`var(--danger-color)` - #DC2626
- **資訊**：`var(--info-color)` - #1E3A8A

### 使用方式
```html
<!-- 使用預定義的按鈕樣式 -->
<button class="btn btn-primary">主要按鈕</button>
<button class="btn btn-outline-secondary">次要按鈕</button>

<!-- 使用自定義表格標題樣式 -->
<thead class="table-header-primary">
  <tr><th>欄位名稱</th></tr>
</thead>

<!-- 使用表單區塊標題樣式 -->
<div class="card-header form-section-header-basic">基本資料</div>
<div class="card-header form-section-header-contact">聯絡資訊</div>
<div class="card-header form-section-header-address">地址資訊</div>
<div class="card-header form-section-header-financial">財務資訊</div>
<div class="card-header form-section-header-system">系統資訊</div>
```

## 標準頁面範本

### Index 頁面範本

參考 `Components/Pages/Customers/CustomerIndex.razor` 的實作方式：

```razor
@page "/[實體複數形]"
@inject I[實體名稱]Service [實體名稱]Service
@inject NavigationManager Navigation
@inject IJSRuntime JSRuntime
@rendermode InteractiveServer

<PageTitle>[實體中文名]管理</PageTitle>

<!-- 頁面標題 -->
<GenericHeaderComponent Title="[實體中文名]管理"
                       Subtitle="管理所有[實體中文名]資料"
                       TitleIcon="[適當的圖示]"
                       HeadingLevel="h1"
                       BreadcrumbItems="@breadcrumbItems"
                       IsLoading="@isLoading"
                       LoadingText="載入中..."
                       ShowDivider="true">
    <ActionButtons>
        <button class="btn btn-primary" @onclick="ShowCreate[實體名稱]">
            <i class="bi bi-plus-circle me-1"></i>
            新增[實體中文名]
        </button>
        <button class="btn btn-outline-secondary" @onclick="RefreshData">
            <i class="bi bi-arrow-clockwise me-1"></i>
            重新整理
        </button>
    </ActionButtons>
</GenericHeaderComponent>

<!-- 主要內容區域 -->
<div class="row">
    <div class="col-12">
        <div class="card">
            <div class="card-header">
                <h5 class="card-title mb-0">
                    <i class="bi bi-search me-2"></i>
                    [實體中文名]搜尋與管理
                </h5>
            </div>
            <div class="card-body">
                <!-- 搜尋篩選區域 -->
                <GenericSearchFilterComponent TModel="[實體名稱]SearchModel"
                                             FilterDefinitions="@filterDefinitions"
                                             FilterModel="@searchModel"
                                             OnSearch="HandleSearch"
                                             OnFilterChanged="HandleFilterChanged"
                                             AutoSearch="true"
                                             ShowSearchButton="true"
                                             ShowAdvancedToggle="true"
                                             SearchDelayMs="500" />

                <!-- 資料表格 -->
                <div class="mt-4">
                    <GenericTableComponent TItem="[實體名稱]"
                                          Items="@paged[實體複數形]"
                                          ColumnDefinitions="@columnDefinitions"
                                          ShowActions="true"
                                          ActionsTemplate="@ActionsTemplate"
                                          EnableRowClick="true"
                                          OnRowClick="HandleRowClick"
                                          IsStriped="true"
                                          IsHoverable="true"
                                          EmptyMessage="沒有找到符合條件的[實體中文名]資料"
                                          ActionsHeader="操作"
                                          EnablePagination="true"
                                          CurrentPage="@currentPage"
                                          PageSize="@pageSize"
                                          TotalItems="@totalItems"
                                          OnPageChanged="HandlePageChanged"
                                          OnPageSizeChanged="HandlePageSizeChanged"
                                          ShowPageSizeSelector="true">
                    </GenericTableComponent>
                </div>
            </div>
        </div>
    </div>
</div>

@code {
    // 必要的程式碼實作...
}
```

### Edit 頁面範本

參考 `Components/Pages/Customers/CustomerEdit.razor` 的實作方式：

```razor
@page "/[實體複數形]/edit"
@page "/[實體複數形]/edit/{id:int?}"
@inject I[實體名稱]Service [實體名稱]Service
@inject NavigationManager Navigation
@inject IJSRuntime JSRuntime
@rendermode InteractiveServer

<PageTitle>@GetPageTitle()</PageTitle>

<!-- 頁面標題 -->
<GenericHeaderComponent Title="@GetPageTitle()"
                       Subtitle="@GetSubtitle()"
                       TitleIcon="@GetTitleIcon()"
                       HeadingLevel="h1"
                       BreadcrumbItems="@breadcrumbItems"
                       IsLoading="@isLoading"
                       LoadingText="載入中..."
                       ShowDivider="true">
    <ActionButtons>
        <button class="btn btn-success" @onclick="Save[實體名稱]" disabled="@(isSubmitting || isLoading)">
            @if (isSubmitting)
            {
                <div class="spinner-border spinner-border-sm me-2" role="status"></div>
            }
            else
            {
                <i class="bi bi-check-circle me-1"></i>
            }
            儲存
        </button>
        <button class="btn btn-outline-secondary" @onclick="Cancel" disabled="@isSubmitting">
            <i class="bi bi-x-circle me-1"></i>
            取消
        </button>
    </ActionButtons>
</GenericHeaderComponent>

<!-- 主要內容區域 -->
<div class="row">
    <div class="col-12">
        <!-- 基本資料表單 -->
        <div class="card mb-4">
            <div class="card-header form-section-header-basic">
                <h5 class="card-title mb-0">
                    <i class="bi bi-info-circle me-2"></i>
                    基本資料
                </h5>
            </div>
            <div class="card-body">
                <GenericFormComponent TModel="[實體名稱]"
                                    Model="@[實體變數名]"
                                    FieldDefinitions="@basicFormFields"
                                    FieldSections="@basicFormSections"
                                    ShowFormHeader="false"
                                    ShowFormButtons="false"
                                    ShowValidationSummary="true">
                </GenericFormComponent>
            </div>
        </div>

        <!-- 其他相關資料區塊... -->
    </div>
</div>

@code {
    [Parameter] public int? Id { get; set; }
    
    private [實體名稱] [實體變數名] = new();
    private bool isLoading = true;
    private bool isSubmitting = false;
    private bool isEditMode => Id.HasValue && Id.Value > 0;
    
    // 必要的程式碼實作...
}
```

### Detail 頁面範本

參考 `Components/Pages/Customers/CustomerDetail.razor` 的實作方式：

```razor
@page "/[實體複數形]/detail/{[實體變數名]Id:int}"
@inject I[實體名稱]Service [實體名稱]Service
@inject IJSRuntime JSRuntime
@inject NavigationManager Navigation
@rendermode InteractiveServer

@if (isLoading)
{
    <div class="d-flex justify-content-center align-items-center" style="min-height: 400px;">
        <div class="text-center">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">載入中...</span>
            </div>
            <div class="mt-2">載入[實體中文名]資料中...</div>
        </div>
    </div>
}
else if ([實體變數名] == null)
{
    <div class="alert alert-warning">
        <h4>找不到[實體中文名]資料</h4>
        <p>指定的[實體中文名]不存在或已被刪除。</p>
        <button class="btn btn-primary" @onclick="BackTo[實體名稱]List">
            返回[實體中文名]列表
        </button>
    </div>
}
else
{
    <GenericDetailsComponent Configuration="@detailConfiguration" 
                           OnSectionLoad="HandleSectionLoad"
                           OnItemClick="HandleItemClick"
                           ActionButtons="@ActionButtons" />
}

@code {
    [Parameter] public int [實體名稱]Id { get; set; }

    private [實體名稱]? [實體變數名];
    private bool isLoading = true;
    private DetailViewConfiguration detailConfiguration = new();

    // 必要的程式碼實作...
}
```

#### Detail 頁面按鈕格式規範

Detail 頁面的操作按鈕必須遵循統一的格式規範，以確保所有頁面的一致性：

##### 基本格式要求

1. **使用 `btn-group` 包裹**：所有按鈕都必須包在 `<div class="btn-group">` 中
2. **標準按鈕大小**：不使用 `btn-sm` 或其他大小修飾符
3. **移除手動間距**：不使用 `me-2` 等間距類別，由 `btn-group` 自動處理
4. **統一圖示庫**：全部使用 Bootstrap Icons (`bi`) 而非 Font Awesome (`fas`)

##### 標準按鈕實作範例

```razor
// Action Buttons - 正確格式
private RenderFragment ActionButtons => __builder =>
{
    <div class="btn-group">
        <button class="btn btn-primary" @onclick="Edit[實體名稱]">
            <i class="bi bi-pencil me-1"></i>
            編輯
        </button>
        <button class="btn btn-outline-secondary" @onclick="Print[實體名稱]">
            <i class="bi bi-printer me-1"></i>
            列印
        </button>
        <button class="btn btn-outline-secondary" @onclick="BackToList">
            <i class="bi bi-arrow-left me-1"></i>
            返回列表
        </button>
    </div>
};
```

##### 按鈕樣式規範

| 按鈕類型 | CSS 類別 | 圖示 | 說明 |
|---------|---------|------|------|
| 編輯 | `btn btn-primary` | `bi-pencil` | 主要操作，藍色背景 |
| 列印 | `btn btn-outline-secondary` | `bi-printer` | 次要操作，灰色邊框 |
| 返回列表 | `btn btn-outline-secondary` | `bi-arrow-left` | 導航操作，灰色邊框 |
| 解鎖/啟用 | `btn btn-success` | `bi-unlock` | 成功操作，綠色背景 |
| 鎖定/停用 | `btn btn-warning` | `bi-lock` | 警告操作，黃色背景 |
| 刪除 | `btn btn-danger` | `bi-trash` | 危險操作，紅色背景 |
| 設為預設 | `btn btn-outline-success` | `bi-star` | 特殊操作，綠色邊框 |
| 管理轉換 | `btn btn-outline-info` | `bi-arrow-left-right` | 資訊操作，藍色邊框 |

##### 條件性按鈕

對於需要根據狀態顯示不同按鈕的情況：

```razor
<div class="btn-group">
    <button class="btn btn-primary" @onclick="Edit[實體名稱]">
        <i class="bi bi-pencil me-1"></i>
        編輯
    </button>
    
    @if ([實體變數名].IsLocked)
    {
        <button class="btn btn-success" @onclick="Unlock[實體名稱]">
            <i class="bi bi-unlock me-1"></i>
            解鎖帳號
        </button>
    }
    else
    {
        <button class="btn btn-warning" @onclick="Lock[實體名稱]">
            <i class="bi bi-lock me-1"></i>
            鎖定帳號
        </button>
    }
    
    <button class="btn btn-outline-secondary" @onclick="BackToList">
        <i class="bi bi-arrow-left me-1"></i>
        返回列表
    </button>
</div>
```

##### 下拉選單按鈕組合

對於需要更多操作選項的情況，可以使用下拉選單：

```razor
<div class="btn-group">
    <button class="btn btn-primary" @onclick="Edit[實體名稱]">
        <i class="bi bi-pencil me-1"></i>
        編輯
    </button>
    <button class="btn btn-outline-secondary dropdown-toggle dropdown-toggle-split" 
            data-bs-toggle="dropdown" aria-expanded="false">
        <span class="visually-hidden">更多操作</span>
    </button>
    <ul class="dropdown-menu">
        <li>
            <button class="dropdown-item" @onclick="Toggle[實體名稱]Status">
                <i class="bi bi-power me-2"></i>
                @([實體變數名]?.Status == EntityStatus.Active ? "停用" : "啟用")[實體中文名]
            </button>
        </li>
        <li><hr class="dropdown-divider"></li>
        <li>
            <button class="dropdown-item text-danger" @onclick="Delete[實體名稱]">
                <i class="bi bi-trash me-2"></i>
                刪除[實體中文名]
            </button>
        </li>
    </ul>
</div>

<button class="btn btn-outline-secondary ms-2" @onclick="BackTo[實體名稱]List">
    <i class="bi bi-arrow-left me-1"></i>
    返回列表
</button>
```

##### 常見錯誤格式

以下是應該避免的不正確格式：

```razor
// ❌ 錯誤：使用舊的格式
private RenderFragment ActionButtons => __builder =>
{
    <button class="btn btn-primary btn-sm me-2" @onclick="EditCustomer">
        <i class="fas fa-edit me-1"></i>編輯
    </button>
    <button class="btn btn-outline-secondary btn-sm me-2" @onclick="PrintCustomer">
        <i class="fas fa-print me-1"></i>列印
    </button>
    <button class="btn btn-outline-primary btn-sm" @onclick="BackToList">
        <i class="fas fa-arrow-left me-1"></i>返回列表
    </button>
};
```

問題點：
- ❌ 缺少 `btn-group` 包裹
- ❌ 使用了 `btn-sm`（小按鈕）
- ❌ 使用了 `me-2`（手動間距）
- ❌ 使用了 Font Awesome 圖示 (`fas`)
- ❌ 返回按鈕使用了 `btn-outline-primary`

##### 圖示對應表

| 功能 | Bootstrap Icons | Font Awesome (避免使用) |
|------|----------------|------------------------|
| 編輯 | `bi-pencil` | `fas fa-edit` |
| 列印 | `bi-printer` | `fas fa-print` |
| 返回 | `bi-arrow-left` | `fas fa-arrow-left` |
| 轉換 | `bi-arrow-left-right` | `fas fa-exchange-alt` |
| 星號 | `bi-star` | `fas fa-star` |
| 解鎖 | `bi-unlock` | `fas fa-unlock` |
| 鎖定 | `bi-lock` | `fas fa-lock` |
| 刪除 | `bi-trash` | `fas fa-trash` |

遵循這些按鈕格式規範可以確保：
- 視覺一致性
- 使用者體驗統一
- 程式碼維護性
- 無障礙網頁標準

## 共用元件使用

所有頁面都應使用以下共用元件來確保一致性：

### 1. GenericHeaderComponent
用於頁面標題、麵包屑導航和操作按鈕。

### 2. GenericSearchFilterComponent  
用於搜尋和篩選功能。

### 3. GenericTableComponent
用於資料表格顯示。

### 4. GenericFormComponent
用於表單輸入和驗證。

### 5. GenericDetailsComponent
用於詳細資料顯示。

## 命名規範

### 檔案命名
- Index 頁面：`[實體名稱]Index.razor`
- Edit 頁面：`[實體名稱]Edit.razor`  
- Detail 頁面：`[實體名稱]Detail.razor`

### 路由命名
- 列表：`/[實體複數形]`
- 新增：`/[實體複數形]/edit`
- 編輯：`/[實體複數形]/edit/{id:int?}`
- 詳細：`/[實體複數形]/detail/{id:int}`

### 變數命名
- 實體物件：使用小寫開頭的實體名稱（如：`customer`、`product`）
- 列表集合：使用複數形式（如：`customers`、`products`）
- 服務注入：使用介面名稱（如：`ICustomerService`）

## 圖示使用規範

使用 Bootstrap Icons，常用圖示對應：

- **列表頁面**：`people-fill`、`box-seam`、`building`
- **新增**：`plus-circle`、`plus-square`
- **編輯**：`pencil-square`、`gear`
- **詳細**：`eye`、`info-circle`
- **刪除**：`trash`、`x-circle`
- **搜尋**：`search`
- **重新整理**：`arrow-clockwise`
- **儲存**：`check-circle`
- **取消**：`x-circle`
- **返回**：`arrow-left`

## 響應式設計

所有頁面都應支援響應式設計：

- 使用 Bootstrap 的網格系統
- 適當的 `col-*` 類別
- 行動裝置友善的按鈕大小
- 適當的間距和邊距

## 一致性檢查清單

在開發新頁面時，請確認以下項目：

- [ ] 使用正確的檔案命名和目錄結構
- [ ] 遵循路由命名規範
- [ ] 使用 variables.css 中定義的色彩變數
- [ ] 實作標準的三個頁面類型
- [ ] 使用共用元件
- [ ] 支援響應式設計
- [ ] 包含適當的載入狀態
- [ ] 包含錯誤處理
- [ ] 使用一致的圖示
- [ ] 實作麵包屑導航
- [ ] 包含適當的操作按鈕

## 範例參考

最佳實作範例：`Components/Pages/Customers/` 目錄下的所有檔案

## 備註

這些規範的目的是：
1. **一致性**：確保所有頁面的外觀和行為一致
2. **維護性**：使程式碼易於理解和維護
3. **使用者體驗**：提供直觀和一致的操作體驗
4. **開發效率**：透過標準化減少重複工作

遵循這些規範將有助於建立高品質、易維護的 ERP 系統界面。