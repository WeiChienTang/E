# 創新滑入式 Toast 通知系統

## 系統概述

ERPCore2 採用了全新的創新滑入式 Toast 通知系統，解決了傳統通知系統會遮擋重要界面元素的問題。這個系統特別針對手機使用體驗進行了優化，確保通知不會干擾用戶的正常操作。

## 🚀 創新特色

### 1. 滑入式側邊通知
- **智能預覽模式**：只顯示訊息摘要，不佔用過多空間
- **可展開設計**：點擊展開按鈕查看完整內容
- **優雅滑入動畫**：從右側滑入，使用彈性動畫效果
- **智能計時**：展開時暫停自動關閉，收合後繼續計時

### 2. 手機友善設計
- **不遮擋上方按鈕**：解決了原本會遮擋導航欄的問題
- **響應式設計**：手機上自動調整大小和間距
- **觸控優化**：按鈕大小適合手指操作
- **無橫向捲軸**：確保在小螢幕上不會產生拉桿

### 3. 現代化視覺效果
- **圓角設計**：12px 圓角，現代感十足
- **柔和陰影**：0 8px 32px 陰影效果
- **彈性動畫**：使用 cubic-bezier 曲線實現彈性效果
- **類型區分**：不同類型有專屬的顏色和圖示

## 📱 位置策略

### 桌機版
- 位於右上角 (top: 20px, right: 20px)
- 寬度固定 320px
- 不會遮擋主要內容區域

### 手機版
- 自動調整為全寬設計
- 左右各保留 10px 邊距
- 確保不超過螢幕寬度 (max-width: calc(100vw - 20px))

## 🎯 解決的問題

### 原本問題
1. **遮擋導航欄**：在手機上會遮擋上方的返回按鈕
2. **干擾操作**：位於畫面中央會影響用戶點擊
3. **視覺突兀**：突然出現在中央位置

### 解決方案
1. **側邊滑入**：從右側滑入，不遮擋重要按鈕
2. **預覽模式**：只顯示關鍵資訊，減少干擾
3. **智能展開**：需要時才展開查看完整內容

## 💡 使用方式

### 基本 API（完全相容）
```csharp
// 原有的 API 完全不變
await NotificationService.ShowSuccessAsync("操作成功");
await NotificationService.ShowErrorAsync("發生錯誤");
await NotificationService.ShowWarningAsync("注意事項");
await NotificationService.ShowInfoAsync("系統通知");
```

### JavaScript 直接調用
```javascript
// 基本使用
showSuccess('操作成功', '成功');
showError('發生錯誤', '錯誤');
showWarning('注意事項', '警告');
showInfo('系統通知', '資訊');

// 或直接使用
showToast('success', '訊息內容', '標題');
```

### 防疊加功能
```csharp
// 自動限制訊息數量（預設最多 3 個）
await NotificationService.ShowInfoAsync("第一個訊息");
await NotificationService.ShowInfoAsync("第二個訊息");
await NotificationService.ShowInfoAsync("第三個訊息");
await NotificationService.ShowInfoAsync("第四個訊息"); // 會移除第一個

// 清除所有訊息
await NotificationService.ClearAllNotificationsAsync();

// 自訂最大數量
await NotificationService.SetMaxNotificationsAsync(5);
```

## 🎮 交互設計

### 預覽模式
- **緊湊顯示**：標題限制 15 字，訊息限制 30 字
- **一鍵展開**：點擊通知區域或 ← 按鈕展開
- **快速操作**：關閉按鈕 × 隨時可用

### 展開模式
- **完整內容**：顯示完整標題和訊息
- **暫停計時**：展開時停止自動關閉
- **收合操作**：點擊 → 按鈕回到預覽模式

### 自動關閉邏輯
- **預覽模式**：3 秒後自動關閉
- **展開模式**：不自動關閉，讓用戶有時間閱讀
- **收合後**：重新啟動 2 秒計時器

## 📋 完整使用範例

### 在 Blazor 元件中使用

```csharp
@inject INotificationService NotificationService

<div class="demo-buttons">
    <button class="btn btn-success" @onclick="ShowSuccess">成功通知</button>
    <button class="btn btn-danger" @onclick="ShowError">錯誤通知</button>
    <button class="btn btn-warning" @onclick="ShowWarning">警告通知</button>
    <button class="btn btn-info" @onclick="ShowInfo">資訊通知</button>
    <button class="btn btn-secondary" @onclick="ShowLongMessage">長文字測試</button>
    <button class="btn btn-primary" @onclick="TestMultiple">多個訊息</button>
    <button class="btn btn-outline-secondary" @onclick="ClearAll">清除全部</button>
</div>

@code {
    private async Task ShowSuccess()
    {
        await NotificationService.ShowSuccessAsync(
            "資料已成功儲存至資料庫！系統已自動備份相關資料。", 
            "操作成功"
        );
    }

    private async Task ShowError()
    {
        await NotificationService.ShowErrorAsync(
            "網路連線異常，請檢查您的網路設定或稍後再試。錯誤代碼：NET_001", 
            "連線錯誤"
        );
    }

    private async Task ShowWarning()
    {
        await NotificationService.ShowWarningAsync(
            "您的操作可能會影響其他使用者，建議先備份資料再繼續。", 
            "注意事項"
        );
    }

    private async Task ShowInfo()
    {
        await NotificationService.ShowInfoAsync(
            "系統將於今晚 23:00 進行維護，預計需要 2 小時，期間服務會暫時中斷。", 
            "系統通知"
        );
    }

    private async Task ShowLongMessage()
    {
        var longMessage = @"親愛的使用者您好，

感謝您使用我們的服務。為了提供更好的使用體驗，我們將在本週末進行系統升級。

升級內容包括：
1. 提升系統效能與穩定性
2. 新增多項便民功能
3. 強化資料安全防護
4. 優化使用者介面設計

升級期間可能會有短暫的服務中斷，造成不便敬請見諒。";

        await NotificationService.ShowInfoAsync(longMessage, "重要系統升級通知");
    }

    private async Task TestMultiple()
    {
        // 連續顯示多個訊息，只會保留最新的3個
        await NotificationService.ShowSuccessAsync("第一個通知：資料同步完成");
        await Task.Delay(500);
        await NotificationService.ShowWarningAsync("第二個通知：發現潛在風險");
        await Task.Delay(500);
        await NotificationService.ShowInfoAsync("第三個通知：系統狀態正常");
        await Task.Delay(500);
        await NotificationService.ShowErrorAsync("第四個通知：將會自動移除最舊的");
    }

    private async Task ClearAll()
    {
        await NotificationService.ClearAllNotificationsAsync();
    }
}
```

### 在控制器中使用

```csharp
public class ExampleController : Controller
{
    private readonly INotificationService _notificationService;

    public ExampleController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<IActionResult> ProcessData()
    {
        try
        {
            // 處理業務邏輯
            await ProcessBusinessLogic();
            
            await _notificationService.ShowSuccessAsync(
                "資料處理完成，共處理 1,234 筆記錄。", 
                "處理完成"
            );
        }
        catch (Exception ex)
        {
            await _notificationService.ShowErrorAsync(
                $"資料處理失敗：{ex.Message}", 
                "處理錯誤"
            );
        }
        
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> BatchOperation()
    {
        // 清除之前的訊息，避免干擾
        await _notificationService.ClearAllNotificationsAsync();
        
        // 設定為單一訊息模式
        await _notificationService.SetMaxNotificationsAsync(1);
        
        await _notificationService.ShowInfoAsync(
            "批次作業進行中，請勿關閉瀏覽器...", 
            "批次處理"
        );
        
        return View();
    }
}
```

## ⚙️ 技術實作詳解

### JavaScript 架構 (`toast.js`)

#### ToastManager 類別
```javascript
class ToastManager {
    constructor() {
        this.container = null;
        this.maxToasts = 3; // 防疊加機制
        this.initContainer();
    }
    
    // 創新的容器初始化
    initContainer() {
        this.container.className = 'toast-container-slide position-fixed';
        this.container.style.top = '20px';
        this.container.style.right = '20px';
        this.container.style.width = '320px';
        this.container.style.overflowY = 'visible'; // 避免拉桿
        this.container.style.pointerEvents = 'none'; // 容器不阻擋事件
    }
}
```

#### 滑入式動畫系統
```css
.toast-slide {
    transform: translateX(100%);
    transition: all 0.4s cubic-bezier(0.68, -0.55, 0.265, 1.55);
    opacity: 0;
    pointer-events: auto; /* 確保 toast 可接收事件 */
}

.toast-slide.slide-in {
    transform: translateX(0);
    opacity: 1;
}
```

#### 智能展開系統
```javascript
toggleExpand(toastId) {
    const toast = document.getElementById(toastId);
    const isExpanded = toast.classList.contains('expanded');
    
    if (isExpanded) {
        toast.classList.remove('expanded');
        // 重新啟動自動隱藏
        toast.autoHideTimeout = setTimeout(() => {
            this.hide(toast);
        }, 4000);
    } else {
        toast.classList.add('expanded');
        // 取消自動隱藏，讓用戶有時間閱讀
        if (toast.autoHideTimeout) {
            clearTimeout(toast.autoHideTimeout);
        }
    }
}
```

### CSS 樣式系統 (`toast.css`)

#### 響應式設計
```css
/* 桌機版 */
.toast-container-slide {
    z-index: 9999;
    pointer-events: none;
}

/* 手機版 */
@media (max-width: 768px) {
    .toast-container-slide {
        right: 10px !important;
        left: 10px !important;
        width: auto !important;
        max-width: calc(100vw - 20px) !important;
    }
}
```

#### 動畫效果
```css
/* 展開動畫 */
@keyframes expandDown {
    from {
        opacity: 0;
        max-height: 0;
        transform: translateY(-10px);
    }
    to {
        opacity: 1;
        max-height: 200px;
        transform: translateY(0);
    }
}

/* 無障礙支援 */
@media (prefers-reduced-motion: reduce) {
    .toast-slide {
        transition: opacity 0.2s ease;
    }
}
```

## 🔧 進階配置

### 自訂最大訊息數量
```csharp
// 單一訊息模式（類似傳統 alert）
await NotificationService.SetMaxNotificationsAsync(1);

// 標準模式（預設）
await NotificationService.SetMaxNotificationsAsync(3);

// 批次操作模式
await NotificationService.SetMaxNotificationsAsync(5);

// 無限制模式（不建議）
await NotificationService.SetMaxNotificationsAsync(999);
```

### 自訂顯示時間
```javascript
// 快速通知 (1秒自動關閉)
setToastAutoHideDelay(1000);
setToastCollapseDelay(500);

// 標準模式 (3秒自動關閉，預設)
setToastAutoHideDelay(3000);
setToastCollapseDelay(2000);

// 慢速模式 (5秒自動關閉)
setToastAutoHideDelay(5000);
setToastCollapseDelay(3000);

// 顯示通知
showInfo('自訂時間的通知訊息', '時間測試');
```

### 時機控制
```csharp
// 頁面載入時清除舊訊息
protected override async Task OnInitializedAsync()
{
    await NotificationService.ClearAllNotificationsAsync();
}

// 表單提交前清除訊息
private async Task OnSubmit()
{
    await NotificationService.ClearAllNotificationsAsync();
    
    // 處理表單邏輯
    var result = await ProcessForm();
    
    if (result.Success)
    {
        await NotificationService.ShowSuccessAsync(result.Message);
    }
}
```

### JavaScript 直接控制
```javascript
// 設定最大數量
setMaxToasts(5);

// 清除所有訊息
clearAllToasts();

// 手動展開/收合
toastManager.toggleExpand('toast-id');

// 立即隱藏
toastManager.hide(toastElement);
```

## 🎨 視覺設計規範

### 顏色系統
- **成功 (Success)**：#10b981 - 綠色系，表示操作成功
- **錯誤 (Error)**：#ef4444 - 紅色系，表示錯誤或失敗
- **警告 (Warning)**：#f59e0b - 橘色系，表示注意事項
- **資訊 (Info)**：#3b82f6 - 藍色系，表示一般資訊

### 尺寸規範
- **桌機寬度**：320px 固定寬度
- **手機寬度**：自適應，左右各 10px 邊距
- **圓角半徑**：12px
- **內邊距**：緊湊模式 16px，展開模式 20px

### 動畫參數
- **滑入時間**：0.4 秒
- **彈性曲線**：cubic-bezier(0.68, -0.55, 0.265, 1.55)
- **展開動畫**：0.3 秒 ease-out
- **自動關閉**：預覽 3 秒，收合後 2 秒

## 🛠️ 故障排除

### 常見問題

#### 1. 通知沒有出現
```javascript
// 檢查容器是否正確初始化
console.log(document.getElementById('toast-container'));

// 手動重新初始化
toastManager.initContainer();
```

#### 2. 展開功能無效
確保事件處理沒有被阻擋：
```javascript
// 檢查事件監聽器
toast.addEventListener('click', function(e) {
    console.log('Toast clicked:', e.target);
});
```

#### 3. 手機版顯示異常
檢查 CSS 媒體查詢：
```css
/* 確保響應式樣式正確載入 */
@media (max-width: 768px) {
    .toast-container-slide {
        right: 10px !important;
        left: 10px !important;
    }
}
```

### 除錯工具
```javascript
// 開啟除錯模式
toastManager.debugMode = true;

// 檢查當前狀態
console.log('Active toasts:', document.querySelectorAll('.toast-slide').length);
console.log('Max toasts:', toastManager.maxToasts);
```

## 📊 效能考量

### 記憶體管理
- 自動清理過期的 DOM 元素
- 取消未使用的 setTimeout
- 移除事件監聽器防止記憶體洩漏

### 動畫效能
- 使用 CSS Transform 而非改變位置屬性
- 避免在動畫過程中操作 DOM
- 使用 `will-change` 屬性優化渲染

## ♿ 無障礙設計

### 鍵盤支援
- Tab 鍵可以聚焦到按鈕
- Enter 鍵可以觸發操作
- Escape 鍵可以關閉通知

### 螢幕閱讀器
```html
<!-- 正確的 ARIA 標籤 -->
<div role="alert" aria-live="assertive" aria-atomic="true">
    <button aria-label="展開完整訊息">
    <button aria-label="關閉通知">
</div>
```

### 高對比模式
```css
@media (prefers-contrast: high) {
    .toast-slide {
        border: 2px solid;
        box-shadow: 0 4px 8px rgba(0, 0, 0, 0.3);
    }
}
```

## 🔗 相關檔案

### 核心檔案
- `/wwwroot/js/toast.js` - JavaScript 主要邏輯
- `/wwwroot/css/toast.css` - 樣式定義
- `/wwwroot/toast-demo.html` - 完整展示頁面

### 整合檔案
- `/Components/App.razor` - 載入順序配置
- `/Services/NotificationService.cs` - C# 介面實作
- `/wwwroot/js/blazor-error-handler.js` - 錯誤處理

### 錯誤處理
- `/Documentation/README_Blazor_Error_Handling.md` - Blazor 錯誤處理指南

## 📈 更新歷程

### v2.0 - 創新滑入式設計 (2025/08/28)
- ✅ 全新滑入式側邊通知設計
- ✅ 智能預覽與展開機制
- ✅ 手機友善的響應式設計
- ✅ 解決遮擋導航欄問題
- ✅ 現代化視覺效果與動畫
- ✅ 完善的無障礙支援

### v1.1 - 防疊加機制
- ✅ 自動限制訊息數量
- ✅ 手動清除功能
- ✅ 自訂最大數量

### v1.0 - 基礎功能
- ✅ 基本 Toast 通知功能
- ✅ 四種訊息類型
- ✅ 自動關閉機制

## 💡 未來規劃

### 預計功能
- 🔄 滑動手勢關閉
- 🔄 聲音通知選項
- 🔄 自訂動畫效果
- 🔄 通知歷史記錄
- 🔄 分組通知功能

---

**開發團隊**：ERPCore2 開發組  
**最後更新**：2025年8月28日  
**版本**：v2.0 創新滑入式版本
