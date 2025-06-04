# Buttons 資料夾說明

## 1. 主要存放的組件類型
- **基礎按鈕組件** - 提供統一的按鈕樣式和互動功能

## 2. 擁有的組件功能、適用場景

### ButtonComponent.razor
- **功能描述**: 標準化的按鈕組件，支援多種樣式變體和互動狀態
- **適用場景**: 
  - 表單提交按鈕
  - 頁面操作按鈕
  - 導航和連結按鈕
  - 模態視窗的確認/取消按鈕
  - 列表項目的操作按鈕

## 3. 功能說明

### ButtonComponent 組件特性
- **多樣式變體**: 支援 10 種不同的按鈕樣式
  - 實心樣式: Primary, Secondary, Success, Warning, Danger, Info
  - 外框樣式: OutlinePrimary, OutlineSecondary, OutlineWarning, OutlineDanger
- **尺寸控制**: 支援 Small, Normal, Large 三種尺寸
- **互動狀態**: 完整的載入和禁用狀態管理
- **圖示支援**: 可配置 FontAwesome 圖示
- **事件處理**: 完整的點擊事件回調機制

### 組件參數
- **Text**: 按鈕顯示文字
- **Variant**: 按鈕樣式變體
- **Size**: 按鈕尺寸
- **IconClass**: 圖示 CSS 類別
- **IsDisabled**: 禁用狀態
- **IsLoading**: 載入狀態 (顯示 spinner)
- **IsSubmit**: 是否為表單提交按鈕
- **OnClick**: 點擊事件回調
- **Title**: 滑鼠懸停提示文字

### 使用方式
```razor
<ButtonComponent Text="儲存" 
               Variant="ButtonVariant.Primary" 
               IconClass="fas fa-save"
               IsLoading="@isSubmitting"
               OnClick="SaveData" />
```

### 樣式變體說明
- **Primary**: 主要操作按鈕 (深藍色)
- **Secondary**: 次要操作按鈕
- **Success**: 成功/確認操作 (綠色)
- **Warning**: 警告操作 (黃色)
- **Danger**: 危險操作 (紅色)
- **Info**: 資訊操作 (藍色)
- **Outline 系列**: 對應的外框樣式

### 設計原則
- 遵循 ERP 系統的深藍色主色調設計
- 提供一致的互動回饋
- 支援無障礙設計標準
