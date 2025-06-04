# Loading 資料夾說明

## 1. 主要存放的組件類型
- **載入指示器組件** - 提供統一的資料載入和處理狀態顯示

## 2. 擁有的組件功能、適用場景

### LoadingComponent.razor
- **功能描述**: 可配置的載入指示器組件，提供視覺化的載入狀態回饋
- **適用場景**: 
  - 頁面資料載入時的等待狀態
  - 表單提交處理中的狀態顯示
  - API 呼叫等待的視覺回饋
  - 長時間操作的進度提示
  - 模態視窗中的載入狀態

## 3. 功能說明

### LoadingComponent 組件特性
- **Bootstrap 整合**: 使用 Bootstrap Spinner 組件
- **多尺寸支援**: 提供 Small, Normal, Large 三種尺寸選擇
- **文字說明**: 可選擇性顯示載入文字說明
- **居中佈局**: 支援自動居中和自定義佈局
- **主題色彩**: 使用系統主色調 (text-primary)
- **條件顯示**: 基於載入狀態的條件渲染

### 組件參數
- **IsLoading**: 控制載入狀態的顯示/隱藏
- **LoadingText**: 載入時顯示的文字說明
- **ShowText**: 是否顯示載入文字
- **Size**: 載入器尺寸 (Small/Normal/Large)
- **IsCentered**: 是否居中顯示
- **CssClass**: 額外的 CSS 樣式類別

### 使用方式
```razor
<!-- 基本使用 -->
<LoadingComponent IsLoading="@isDataLoading" 
                LoadingText="載入客戶資料中..." 
                ShowText="true" />

<!-- 小尺寸載入器 -->
<LoadingComponent IsLoading="@isSubmitting" 
                Size="LoadingSize.Small" 
                LoadingText="處理中..." 
                IsCentered="false" />

<!-- 大尺寸載入器 -->
<LoadingComponent IsLoading="@isInitializing" 
                Size="LoadingSize.Large" 
                LoadingText="系統初始化中，請稍候..." 
                ShowText="true" />
```

### 載入器尺寸
- **Small**: 適用於按鈕內或小區域的載入指示
- **Normal**: 標準尺寸，適用於一般載入場景
- **Large**: 大尺寸，適用於整頁載入或重要區域

### 佈局選項
- **居中顯示**: 自動在容器中垂直和水平居中
- **自定義佈局**: 可透過 CSS 類別進行自定義定位
- **內容區域**: 支援 padding 調整，確保適當的視覺間距

### 設計原則
- 提供一致的載入狀態視覺回饋
- 遵循 ERP 系統的色彩規範
- 支援無障礙設計 (包含 visually-hidden 文字)
- 確保良好的使用者體驗
