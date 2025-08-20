# 手機導航選單自動隱藏功能

## 功能描述

此功能為 ERP 系統的導航選單添加了手機模式下的自動隱藏機制，提升了手機端的使用者體驗。

## 主要特性

### 自動隱藏觸發條件
- **點擊導航連結**：當用戶點擊任何導航選單項目時，選單會自動隱藏
- **點擊內容區域**：當用戶點擊導航選單外的內容區域時，選單會自動隱藏  
- **按下 ESC 鍵**：用戶可以使用 ESC 鍵快速關閉導航選單
- **螢幕尺寸變化**：當螢幕從手機模式切換到桌面模式時，選單會自動隱藏

### 手機模式檢測
- 自動檢測當前是否為手機模式（螢幕寬度 ≤ 640px）
- 只在手機模式下啟用自動隱藏功能
- 桌面模式下保持原有的導航選單行為

### 無障礙設計改善
- 增加了觸控目標的最小尺寸（48px）
- 支援鍵盤操作（ESC 鍵）
- 保持良好的視覺回饋

## 實作檔案

### JavaScript 檔案
- `wwwroot/js/nav-menu-mobile.js` - 主要功能實作

### Razor 元件
- `Components/Layout/NavMenu.razor` - 導航選單主元件
- `Components/Shared/NavMenus/NavMenuItem.razor` - 導航選單項目元件
- `Components/Shared/NavMenus/NavDropdownItem.razor` - 下拉選單項目元件
- `Components/Layout/MainLayout.razor` - 主版面配置

### CSS 樣式
- `Components/Layout/NavMenu.razor.css` - 導航選單樣式
- `Components/Layout/MainLayout.razor.css` - 主版面樣式

### 應用程式設定
- `Components/App.razor` - 引入 JavaScript 檔案

## 技術實作細節

### 事件監聽器
```javascript
// 點擊事件監聽
document.addEventListener('click', handleNavClick);
document.addEventListener('click', handleContentClick);

// 鍵盤事件監聽
document.addEventListener('keydown', handleKeyDown);

// 視窗大小變化監聽
window.addEventListener('resize', handleResize);
```

### CSS 類別識別
- `.nav-menu-nav-link` - 主導航連結
- `.dropdown-item` - 下拉選單項目
- `.nav-scrollable` - 導航選單容器
- `.navbar-toggler` - 手機模式切換按鈕

### 手機模式檢測邏輯
```javascript
function isMobileMode() {
    const toggler = document.querySelector('.navbar-toggler');
    return toggler && window.getComputedStyle(toggler).display !== 'none';
}
```

## 使用者體驗改善

1. **流暢的導航體驗**：點擊任何選單項目後，選單立即隱藏，讓用戶專注於內容
2. **直觀的操作**：點擊選單外部或按 ESC 鍵可以快速關閉選單
3. **響應式設計**：桌面和手機模式下都有最佳的使用體驗
4. **無障礙支援**：符合無障礙設計標準，支援鍵盤操作

## 瀏覽器相容性

- Chrome/Edge 60+
- Firefox 55+
- Safari 11+
- 所有現代行動瀏覽器

## 效能考量

- 使用事件委派機制，避免過多的事件監聽器
- 輕量級實作，不依賴額外的函式庫
- 在頁面載入完成後自動初始化，不影響載入效能
