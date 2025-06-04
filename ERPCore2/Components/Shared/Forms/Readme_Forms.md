# Forms 資料夾說明

## 1. 主要存放的組件類型
- **表單輸入組件** - 提供統一的表單輸入控制項和相關功能

## 2. 擁有的組件功能、適用場景

### FormSectionComponent.razor
- **功能描述**: 表單區段組件，將表單欄位分組並提供完成度指示
- **適用場景**: 
  - 複雜表單的邏輯分組
  - 多步驟表單的進度追蹤
  - 必填/選填欄位的視覺區分

### InputComponent.razor
- **功能描述**: 統一的輸入欄位組件，支援多種輸入類型
- **適用場景**: 
  - 文字輸入欄位
  - 數字、日期、電子郵件等特殊輸入
  - 多行文字區域 (textarea)
  - 表單驗證和錯誤顯示

### SelectComponent.razor
- **功能描述**: 泛型下拉選單組件，支援各種資料類型
- **適用場景**: 
  - 靜態選項列表選擇
  - 枚舉值選擇
  - 簡單的資料類型選擇

### DatabaseSelectComponent.razor
- **功能描述**: 支援資料庫載入的下拉選單組件
- **適用場景**: 
  - 動態載入的選項列表
  - 關聯實體的選擇 (如客戶類型、行業別)
  - 大量資料的下拉選擇

### DatabaseSelectWithManagementComponent.razor
- **功能描述**: 整合管理功能的資料庫下拉選單
- **適用場景**: 
  - 需要即時新增選項的下拉選單
  - 系統設定類型的選擇
  - 主檔維護整合的選擇控制項

### SearchComponent.razor
- **功能描述**: 搜尋輸入組件，提供即時搜尋功能
- **適用場景**: 
  - 列表頁面的搜尋功能
  - 資料篩選輸入
  - 即時搜尋建議

### AddressManagement.razor
- **功能描述**: 地址管理組件，提供完整的地址輸入和管理功能
- **適用場景**: 
  - 客戶地址資訊輸入
  - 多地址管理
  - 地址類型分類

## 3. 功能說明

### FormSectionComponent 特性
- **完成度追蹤**: 自動計算和顯示表單完成進度
- **視覺回饋**: 根據完成狀態提供不同的視覺提示
- **區段分類**: 支援不同類型的表單區段樣式
- **響應式設計**: 適應不同螢幕尺寸

### InputComponent 特性
- **多輸入類型**: 支援 text, number, email, password, textarea 等
- **驗證整合**: 完整的錯誤訊息顯示機制
- **狀態管理**: 支援禁用、只讀等狀態
- **輔助文字**: 提供欄位說明和幫助資訊

### DatabaseSelectComponent 特性
- **非同步載入**: 支援資料庫資料的非同步載入
- **載入指示**: 提供載入狀態的視覺回饋
- **泛型支援**: 支援任意實體類型的選擇
- **錯誤處理**: 完整的載入失敗處理機制

### 使用方式
```razor
<!-- 表單區段 -->
<FormSectionComponent Title="基本資訊" 
                    RequiredFieldsCount="3" 
                    CompletedFieldsCount="@completedCount">
    <!-- 表單欄位 -->
</FormSectionComponent>

<!-- 輸入組件 -->
<InputComponent Label="客戶名稱" 
              @bind-Value="customer.Name" 
              IsRequired="true" 
              Placeholder="請輸入客戶名稱" />

<!-- 資料庫下拉選單 -->
<DatabaseSelectComponent TValue="int?" 
                       TEntity="CustomerType"
                       Label="客戶類型" 
                       @bind-Value="customer.CustomerTypeId"
                       Service="@customerTypeService" />
```

### 設計原則
- 提供一致的表單使用者體驗
- 支援完整的驗證和錯誤處理
- 遵循無障礙設計標準
- 整合 ERP 系統的設計規範
