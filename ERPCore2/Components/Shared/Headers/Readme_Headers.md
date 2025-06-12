# PageHeader 組件

一個功能完整且高度可定制的 Blazor Server 表頭組件，適用於頁面標題、卡片標題等多種場景。

## 📝 用途與時機

### 適用場景

- **頁面主標題**：管理介面的頁面頂部標題區域
- **卡片標題**：各種卡片容器的標題列
- **模組標題**：系統不同功能模組的標題顯示
- **導航標題**：需要麵包屑導航的頁面標題

### 使用時機

- 需要統一的標題樣式和佈局
- 希望減少重複的標題結構代碼
- 需要響應式的標題設計
- 要求無障礙訪問支援
- 需要動態載入狀態顯示

## 🔧 參數說明

### 基本屬性

| 參數 | 類型 | 預設值 | 說明 |
|-----|------|-------|------|
| `Title` | string | "" | 主標題文字 |
| `Subtitle` | string | "" | 副標題文字 |
| `IconClass` | string | "" | 自定義圖標 CSS 類別 |
| `TitleIcon` | string | "" | Bootstrap Icons 圖標名稱（不含 bi- 前綴） |

### 麵包屑導航

| 參數 | 類型 | 預設值 | 說明 |
|-----|------|-------|------|
| `BreadcrumbItems` | List&lt;BreadcrumbItem&gt; | null | 麵包屑項目列表 |

### 動作區域

| 參數 | 類型 | 預設值 | 說明 |
|-----|------|-------|------|
| `Actions` | RenderFragment | null | 自定義動作內容 |
| `ActionButtons` | RenderFragment | null | 動作按鈕區域 |

### 樣式控制

| 參數 | 類型 | 預設值 | 說明 |
|-----|------|-------|------|
| `CssClass` | string | "" | 容器額外 CSS 類別 |
| `TitleClass` | string | "" | 標題額外 CSS 類別 |
| `SubtitleClass` | string | "" | 副標題額外 CSS 類別 |
| `DefaultTitleClass` | string | "fw-bold" | 標題預設樣式 |
| `DefaultSubtitleClass` | string | "text-muted mb-0" | 副標題預設樣式 |

### 佈局控制

| 參數 | 類型 | 預設值 | 說明 |
|-----|------|-------|------|
| `IsCardHeader` | bool | false | 是否為卡片標題模式 |
| `ShowDivider` | bool | false | 是否顯示底部分隔線 |
| `StackOnMobile` | bool | true | 手機版是否堆疊顯示 |

### 無障礙支援

| 參數 | 類型 | 預設值 | 說明 |
|-----|------|-------|------|
| `HeadingLevel` | string | "h2" | HTML 標題層級 (h1-h6) |
| `AriaLabel` | string | "" | 無障礙標籤 |

### 載入狀態

| 參數 | 類型 | 預設值 | 說明 |
|-----|------|-------|------|
| `IsLoading` | bool | false | 是否顯示載入狀態 |
| `LoadingText` | string | "載入中..." | 載入提示文字 |

## 📚 用法範例

### 基本頁面標題

```html
<PageHeader Title="用戶管理" 
           Subtitle="管理系統用戶與權限設定"
           TitleIcon="people" />
```

### 帶麵包屑導航的頁面

```html
<PageHeader Title="編輯用戶資料" 
           Subtitle="修改用戶基本資訊與權限設定"
           TitleIcon="person-gear"
           BreadcrumbItems="@breadcrumbs"
           HeadingLevel="h1"
           ShowDivider="true">
    <ActionButtons>
        <button class="btn btn-primary">
            <i class="bi bi-check-lg me-1"></i>儲存變更
        </button>
        <button class="btn btn-outline-secondary">
            <i class="bi bi-x-lg me-1"></i>取消
        </button>
    </ActionButtons>
</PageHeader>

@code {
    private List<PageHeader.BreadcrumbItem> breadcrumbs = new()
    {
        new("首頁", "/dashboard"),
        new("用戶管理", "/users"),
        new("編輯用戶") // 無 href 為當前頁面
    };
}
```

### 卡片標題模式

```html
<div class="card">
    <PageHeader Title="個人設定" 
               Subtitle="管理您的帳戶偏好設定"
               IsCardHeader="true"
               TitleIcon="gear">
        <Actions>
            <button class="btn btn-sm btn-outline-primary">編輯</button>
        </Actions>
    </PageHeader>
    <div class="card-body">
        <!-- 卡片內容 -->
    </div>
</div>
```

### 響應式設計範例

```html
<!-- 手機版會自動堆疊，桌面版水平排列 -->
<PageHeader Title="銷售報表" 
           Subtitle="2024年第一季度銷售數據分析"
           TitleIcon="graph-up"
           StackOnMobile="true">
    <ActionButtons>
        <div class="btn-group" role="group">
            <button class="btn btn-outline-primary">匯出 PDF</button>
            <button class="btn btn-outline-primary">匯出 Excel</button>
        </div>
        <button class="btn btn-primary">重新整理</button>
    </ActionButtons>
</PageHeader>
```

### 載入狀態顯示

```html
<PageHeader Title="資料載入" 
           Subtitle="正在載入最新資料"
           IsLoading="@isLoading"
           LoadingText="載入用戶資料中，請稍候..." />

@code {
    private bool isLoading = true;
    
    protected override async Task OnInitializedAsync()
    {
        // 模擬資料載入
        await Task.Delay(3000);
        isLoading = false;
        StateHasChanged();
    }
}
```

### 自定義樣式範例

```html
<PageHeader Title="重要公告" 
           Subtitle="系統維護通知"
           TitleIcon="exclamation-triangle"
           CssClass="bg-warning-subtle border border-warning rounded p-3"
           TitleClass="text-warning-emphasis"
           DefaultTitleClass="fw-bold fs-4"
           HeadingLevel="h1" />
```

### 複雜動作區域

```html
<PageHeader Title="產品管理" 
           Subtitle="管理商品資訊與庫存">
    <Actions>
        <!-- 搜尋功能 -->
        <div class="input-group" style="width: 300px;">
            <input type="text" class="form-control" placeholder="搜尋產品..." />
            <button class="btn btn-outline-secondary">
                <i class="bi bi-search"></i>
            </button>
        </div>
    </Actions>
    <ActionButtons>
        <!-- 篩選與新增 -->
        <div class="dropdown">
            <button class="btn btn-outline-primary dropdown-toggle" 
                    data-bs-toggle="dropdown">
                <i class="bi bi-funnel me-1"></i>篩選
            </button>
            <ul class="dropdown-menu">
                <li><a class="dropdown-item" href="#">依分類</a></li>
                <li><a class="dropdown-item" href="#">依狀態</a></li>
            </ul>
        </div>
        <button class="btn btn-primary">
            <i class="bi bi-plus-lg me-1"></i>新增產品
        </button>
    </ActionButtons>
</PageHeader>
```

## 🎨 CSS 自定義

組件使用 Bootstrap 5 類別，你可以透過以下方式自定義樣式：

```css
/* 自定義主要色彩 */
.text-primary-custom {
    color: #your-brand-color !important;
}

.text-secondary-custom {
    color: #your-secondary-color !important;
}

/* 自定義卡片標題樣式 */
.card-header .card-title {
    font-size: 1.1rem;
    font-weight: 600;
}

/* 響應式調整 */
@media (max-width: 576px) {
    .pageheader-mobile-stack .btn-group {
        width: 100%;
    }
}
```

## 🛠️ 最佳實踐

1. **語意化 HTML**：根據頁面結構選擇適當的 `HeadingLevel`
2. **無障礙性**：為重要標題提供 `AriaLabel`
3. **響應式設計**：善用 `StackOnMobile` 確保手機版體驗
4. **載入狀態**：在資料載入時使用 `IsLoading` 提升用戶體驗
5. **一致性**：在整個應用程式中保持標題樣式的一致性

## 📱 瀏覽器支援

- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## 🔄 版本資訊

- **1.0.0**：初始版本，支援基本標題功能
- **2.0.0**：加入響應式設計、載入狀態、強化麵包屑支援