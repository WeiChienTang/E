# Headers 資料夾說明

## 1. 主要存放的組件類型
- **頁面標題組件** - 提供統一的頁面標題和操作區域佈局

## 2. 擁有的組件功能、適用場景

### PageHeaderComponent.razor
- **功能描述**: 統一的頁面標題組件，提供標題、副標題和操作按鈕的整合顯示
- **適用場景**: 
  - 頁面頂部標題區域
  - 卡片標題和操作按鈕
  - 模組標題的標準化顯示
  - 需要標題和操作按鈕並排的場景

## 3. 功能說明

### PageHeaderComponent 組件特性
- **標題系統**: 支援主標題和副標題的分層顯示
- **圖示支援**: 可配置 FontAwesome 圖示增強視覺識別
- **操作區域**: 提供右側操作按鈕的容器空間
- **響應式佈局**: 使用 Flexbox 確保在不同螢幕尺寸下的適當顯示
- **樣式整合**: 遵循 ERP 系統的設計色彩規範

### 組件參數
- **Title**: 主標題文字
- **Subtitle**: 副標題文字 (可選)
- **IconClass**: 標題前的圖示 CSS 類別
- **Actions**: 右側操作按鈕的內容容器
- **CssClass**: 額外的 CSS 樣式類別

### 使用方式
```razor
<PageHeaderComponent Title="客戶管理" 
                   Subtitle="客戶基本資料維護" 
                   IconClass="fas fa-users">
    <Actions>
        <ButtonComponent Text="新增客戶" 
                       Variant="ButtonVariant.Primary" 
                       IconClass="fas fa-plus" />
        <ButtonComponent Text="匯出資料" 
                       Variant="ButtonVariant.OutlineSecondary" 
                       IconClass="fas fa-download" />
    </Actions>
</PageHeaderComponent>
```

### 視覺設計
- **主標題**: 使用主要色彩 (text-primary-custom)
- **副標題**: 使用次要色彩 (text-secondary-custom)
- **佈局**: 左側標題，右側操作按鈕
- **間距**: 統一的元素間距和對齊

### 設計原則
- 提供一致的頁面標題體驗
- 支援彈性的內容配置
- 遵循 ERP 系統的視覺設計規範
- 確保良好的使用者體驗和可讀性
