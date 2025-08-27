# Blazor JavaScript Interop 錯誤處理指南

## 問題描述

在 Blazor Server 應用程序中，當組件被銷毀時，JavaScript interop 對象的清理可能會產生以下錯誤：

```
Error: There was an exception invoking '__Dispose'. For more details turn on detailed exceptions in 'CircuitOptions.DetailedErrors'
```

這個錯誤通常發生在：
1. 使用者關閉瀏覽器頁籤
2. 網路連接中斷
3. 組件快速重新渲染
4. DotNetObjectReference dispose 時機問題

## 解決方案

### 1. JavaScript 端改進

#### `modal-helpers.js` 修正
- 增加了更詳細的錯誤檢查
- 延長 dispose 延遲時間到 150ms
- 添加對象有效性驗證
- 將錯誤降級為 debug 訊息

```javascript
// 檢查對象是否仍然有效
if (tempRef && typeof tempRef.dispose === 'function') {
    tempRef.dispose();
}
```

### 2. Blazor 組件端改進

#### `GenericEditModalComponent.razor` 修正
- 添加 200ms 延遲讓 JavaScript 完成清理
- 捕獲並忽略 `ObjectDisposedException`
- 捕獲並忽略 `JSDisconnectedException`
- 捕獲並忽略 `TaskCanceledException`

```csharp
try
{
    refToDispose.Dispose();
}
catch (ObjectDisposedException)
{
    // 對象已被釋放，這是正常的
}
```

### 3. 全域錯誤處理

#### `blazor-error-handler.js`
創建了一個全域錯誤處理器來過濾已知的安全錯誤：

- 攔截 console.error 調用
- 過濾已知的 Blazor dispose 錯誤
- 將安全錯誤降級為 debug 訊息
- 處理 unhandled promise rejections

### 4. Program.cs 配置

添加了更好的 Circuit 配置：

```csharp
.AddInteractiveServerComponents(options =>
{
    options.DetailedErrors = builder.Environment.IsDevelopment();
    options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
    options.MaxBufferedUnacknowledgedRenderBatches = 10;
});
```

## 錯誤分類

### 🟢 安全錯誤（可忽略）
- `There was an exception invoking '__Dispose'`
- `JSDisconnectedException`
- `TaskCanceledException`
- `ObjectDisposedException`
- `cleanupEscKeyListener` 相關錯誤

### 🔴 真正的錯誤（需要注意）
- 業務邏輯錯誤
- 數據驗證錯誤
- 網路連接問題（非正常斷線）
- 授權錯誤

## 最佳實踐

### 1. DotNetObjectReference 管理
```csharp
// ✅ 正確的做法
try
{
    await JSRuntime.InvokeVoidAsync("cleanup");
    await Task.Delay(200); // 給 JS 時間清理
    dotNetRef?.Dispose();
}
catch (JSDisconnectedException) { /* 正常斷線 */ }
catch (ObjectDisposedException) { /* 已被清理 */ }
```

### 2. JavaScript 端錯誤處理
```javascript
// ✅ 正確的做法
try {
    if (tempRef && typeof tempRef.dispose === 'function') {
        tempRef.dispose();
    }
} catch (error) {
    console.debug('Safe dispose error:', error.message);
}
```

### 3. 組件生命週期管理
```csharp
// ✅ 在 Dispose 方法中安全清理
public async ValueTask DisposeAsync()
{
    try
    {
        await CleanupAsync();
    }
    catch (Exception ex) when (IsSafeDisposeError(ex))
    {
        // 忽略安全的清理錯誤
    }
}
```

## 監控和調試

### 開發環境
- 設定 `DetailedErrors = true`
- 使用 `console.debug` 查看過濾的錯誤
- 檢查網路連接狀態

### 生產環境
- 錯誤會被自動過濾
- 真正的錯誤仍會正常顯示
- 可透過日誌系統監控

## 相關檔案

- `/wwwroot/js/blazor-error-handler.js` - 全域錯誤處理
- `/wwwroot/js/modal-helpers.js` - Modal 相關 interop
- `/Components/Shared/PageModels/GenericEditModalComponent.razor` - 組件清理邏輯
- `/Program.cs` - Blazor 配置

## 結論

這些修正確保了：
1. ✅ Blazor dispose 錯誤不再污染控制台
2. ✅ 真正的錯誤依然會被正確顯示
3. ✅ 組件清理更加穩健
4. ✅ 使用者體驗不受影響

**注意**：這些錯誤通常不影響應用程序功能，主要是 Blazor 框架的生命週期管理特性。透過適當的錯誤處理，我們可以提供更乾淨的開發體驗。
