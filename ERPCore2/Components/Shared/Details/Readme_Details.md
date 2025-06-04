# Details 資料夾說明

## 1. 主要存放的組件類型
- **詳情展示組件** - 提供結構化的數據詳情顯示功能

## 2. 擁有的組件功能、適用場景

### DetailItemComponent.razor
- **功能描述**: 單個詳情項目組件，用於標籤-值對的顯示
- **適用場景**: 
  - 實體詳情頁面的欄位顯示
  - 表單的只讀欄位展示
  - 資料預覽和確認頁面

### DetailSectionComponent.razor
- **功能描述**: 詳情區段組件，用於將相關詳情項目分組顯示
- **適用場景**: 
  - 複雜實體的分區域顯示
  - 表單的邏輯分組展示
  - 多步驟流程的資料確認

### RelatedDataCardComponent.razor
- **功能描述**: 相關資料卡片組件，用於顯示關聯實體資料
- **適用場景**: 
  - 客戶的聯絡方式列表
  - 客戶的地址資訊列表
  - 一對多關係的資料展示
  - 相關記錄的摘要顯示

## 3. 功能說明

### DetailItemComponent 特性
- **標籤系統**: 支援自定義標籤和圖示
- **值顯示**: 支援文字值和自定義內容
- **樣式控制**: 可自定義標籤和值的 CSS 樣式
- **空值處理**: 提供預設的空值顯示文字

### DetailSectionComponent 特性
- **區段標題**: 支援圖示和自定義樣式的標題
- **響應式佈局**: 使用 Bootstrap Grid 系統
- **分隔線控制**: 可選擇性顯示區段分隔線
- **內容容器**: 提供結構化的內容佈局

### RelatedDataCardComponent 特性
- **計數顯示**: 自動顯示相關資料數量
- **空狀態處理**: 優雅的無資料狀態展示
- **內容限制**: 支援顯示數量限制和更多提示
- **卡片佈局**: 使用 Bootstrap Card 結構

### 使用方式
```razor
<!-- 詳情項目 -->
<DetailItemComponent Label="客戶名稱" 
                   Value="@customer.Name" 
                   IconClass="fas fa-user" />

<!-- 詳情區段 -->
<DetailSectionComponent Title="基本資訊" IconClass="fas fa-info-circle">
    <!-- 詳情項目群組 -->
</DetailSectionComponent>

<!-- 相關資料卡片 -->
<RelatedDataCardComponent Title="聯絡方式" 
                        Count="@contacts.Count" 
                        HasData="@contacts.Any()"
                        IconClass="fas fa-phone">
    <!-- 相關資料內容 -->
</RelatedDataCardComponent>
```

### 設計原則
- 提供一致的詳情展示體驗
- 支援響應式和無障礙設計
- 遵循 ERP 系統的視覺設計規範
