# Alerts 資料夾說明

## 1. 主要存放的組件類型
- **警告訊息組件** - 提供統一的系統訊息提示功能

## 2. 擁有的組件功能、適用場景

### AlertComponent.razor
- **功能描述**: 可配置的警告訊息組件，支援多種訊息類型和自動關閉功能
- **適用場景**: 
  - 操作成功/失敗的回饋訊息
  - 系統警告和錯誤提示
  - 表單驗證錯誤顯示
  - 重要資訊的強調提醒

## 3. 功能說明

### AlertComponent 組件特性
- **多種訊息類型**: 支援 Success、Warning、Danger、Info 四種訊息類型
- **Bootstrap 整合**: 使用 Bootstrap Alert 樣式系統
- **圖示支援**: 可配置自定義圖示增強視覺效果
- **可關閉功能**: 支援手動關閉和回調事件
- **內容自定義**: 支援純文字和自定義 HTML 內容

### 組件參數
- **Message**: 訊息文字內容
- **Type**: 訊息類型 (Success/Warning/Danger/Info)
- **IsVisible**: 控制顯示/隱藏狀態
- **IsDismissible**: 是否可關閉
- **IconClass**: 自定義圖示 CSS 類別
- **OnDismiss**: 關閉時的回調事件

### 使用方式
```razor
<AlertComponent Type="AlertType.Success" 
              Message="操作成功完成！" 
              IconClass="fas fa-check-circle"
              OnDismiss="HandleDismiss" />
```

### 設計原則
- 遵循 ERP 系統的色彩設計規範
- 提供一致的使用者體驗
- 支援無障礙設計標準
