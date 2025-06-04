# Actions 資料夾說明

## 1. 主要存放的組件類型
- **頁面操作欄組件** - 提供頁面級別的主要操作按鈕容器

## 2. 擁有的組件功能、適用場景

### PageActionBar.razor
- **功能描述**: 頁面操作欄組件，提供統一的頁面操作按鈕排版
- **適用場景**: 
  - 頁面頂部操作按鈕區域
  - 需要區分主要操作和次要操作的場景
  - 列表頁面的批量操作區域
  - 詳情頁面的編輯、刪除等操作

## 3. 功能說明

### PageActionBar 組件特性
- **彈性佈局**: 使用 Flexbox 進行按鈕排列，右對齊顯示
- **分組管理**: 支援主要操作 (PrimaryActions) 和次要操作 (SecondaryActions) 分組
- **間距控制**: 自動處理按鈕間距，提供一致的視覺效果
- **響應式設計**: 適應不同螢幕尺寸的顯示需求

### 使用方式
```razor
<PageActionBar>
    <PrimaryActions>
        <!-- 主要操作按鈕 -->
    </PrimaryActions>
    <SecondaryActions>
        <!-- 次要操作按鈕 -->
    </SecondaryActions>
</PageActionBar>
```

### 設計原則
- 遵循 ERP 系統的一致性設計規範
- 提供清晰的操作層級區分
- 支援靈活的內容自定義
