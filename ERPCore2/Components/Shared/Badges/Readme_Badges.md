# Badges 資料夾說明

## 1. 主要存放的組件類型
- **徽章組件** - 提供狀態標識和標籤顯示功能

## 2. 擁有的組件功能、適用場景

### StatusBadgeComponent.razor
- **功能描述**: 專門用於顯示實體狀態的視覺化徽章組件
- **適用場景**: 
  - 實體狀態顯示 (啟用/停用/已刪除)
  - 數據列表中的狀態標識
  - 詳情頁面的狀態展示
  - 管理介面的快速狀態識別

## 3. 功能說明

### StatusBadgeComponent 組件特性
- **狀態映射**: 自動將 EntityStatus 映射為對應的視覺樣式
- **色彩系統**: 遵循設計規範的狀態色彩配置
  - 啟用 (Active): 綠色背景
  - 停用 (Inactive): 黃色背景，深色文字
  - 已刪除 (Deleted): 紅色背景
- **圖示支援**: 可配置狀態圖示增強識別性
- **尺寸控制**: 支援標準和小尺寸顯示
- **自定義文字**: 支援覆蓋預設狀態文字

### 組件參數
- **Status**: EntityStatus 枚舉值
- **CustomText**: 自定義顯示文字
- **IconClass**: 圖示 CSS 類別
- **CssClass**: 附加 CSS 樣式
- **Size**: 徽章尺寸 (Normal/Small)

### 使用方式
```razor
<StatusBadgeComponent Status="EntityStatus.Active" 
                    IconClass="fas fa-check" 
                    Size="BadgeSize.Small" />
```

### 狀態對應
- **EntityStatus.Active** → "啟用" (綠色)
- **EntityStatus.Inactive** → "停用" (黃色)
- **EntityStatus.Deleted** → "已刪除" (紅色)

### 設計原則
- 符合無障礙設計標準
- 提供一致的狀態視覺識別
- 支援響應式設計
