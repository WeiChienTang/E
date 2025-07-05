## 📋 概述

本文檔說明如何在 Blazor/Razor 頁面中實作統一且強化的錯誤處理機制，確保所有錯誤都能被適當記錄到資料庫、通知使用者，同時避免程式崩潰。

## 🎯 目標

- **統一錯誤處理**：所有 Razor 頁面使用相同的錯誤處理模式
- **錯誤記錄**：確保所有錯誤都記錄到資料庫，便於追蹤與分析
- **使用者通知**：根據錯誤類型決定是否通知使用者
- **程式穩定性**：避免因未處理異常導致程式崩潰
- **維護性**：提供清晰的錯誤處理架構，便於維護與擴展

## 🏗️ 架構設計

### 分層責任

1. **Service 層**：
   - 只負責記錄錯誤：`_errorLogService.LogErrorAsync()`
   - 不處理使用者通知
   - 向上拋出異常供頁面層處理

2. **Razor 頁面層**：
   - 使用 `ErrorHandlingHelper.HandleErrorSafelyAsync()` 統一處理錯誤
   - 同時記錄錯誤與通知使用者
   - 設置安全的預設值避免頁面崩潰

### 錯誤處理工具

- **`ErrorHandlingHelper.HandleErrorSafelyAsync()`**：統一錯誤處理方法
- **`IErrorLogService.LogErrorAsync()`**：純錯誤記錄方法
- **`INotificationService`**：使用者通知服務

## 🔧 實作方式

### 1. 基本錯誤處理模式

```csharp
try
{
    // 業務邏輯
    await SomeBusinessLogic();
}
catch (Exception ex)
{
    // 統一錯誤處理
    _ = ErrorHandlingHelper.HandleErrorSafelyAsync(
        ex, 
        nameof(MethodName),        // 方法名稱
        this.GetType(),           // 元件類型
        ErrorLogService,          // 錯誤記錄服務
        NotificationService,      // 通知服務
        showUserFriendlyMessage: true,  // 是否通知使用者
        additionalData: new { ... }     // 額外資料
    );
    
    // 設置預設值避免頁面崩潰
    SetSafeDefaults();
}
```

### 2. 錯誤處理決策

#### 需要通知使用者的錯誤 (`showUserFriendlyMessage: true`)
- 資料載入失敗
- 操作執行失敗
- 驗證錯誤
- 權限錯誤

#### 不需要通知使用者的錯誤 (`showUserFriendlyMessage: false`)
- 內部初始化錯誤
- 格式化錯誤
- 非關鍵功能錯誤
- 預設值設定錯誤

### 3. 預設值設定原則

```csharp
catch (Exception ex)
{
    // 錯誤處理
    _ = ErrorHandlingHelper.HandleErrorSafelyAsync(...);
    
    // 設置安全預設值
    customers = new List<Customer>();
    isLoading = false;
    totalCount = 0;
    // ... 其他必要的預設值
}
```

## 📝 實作檢查清單

### 頁面生命週期方法
- [ ] `OnInitializedAsync()` - 初始化錯誤處理
- [ ] `OnParametersSetAsync()` - 參數變更錯誤處理
- [ ] `OnAfterRenderAsync()` - 渲染後錯誤處理

### 資料操作方法
- [ ] 資料載入方法 - 設置預設值
- [ ] 資料篩選方法 - 維持現有狀態
- [ ] 資料刷新方法 - 恢復到安全狀態

### UI 互動方法
- [ ] 按鈕點擊處理 - 提供使用者回饋
- [ ] 表單提交處理 - 驗證與錯誤顯示
- [ ] 導航方法 - 防止導航失敗

### 輔助方法
- [ ] 格式化方法 - 返回安全的預設值
- [ ] 轉換方法 - 避免空值異常
- [ ] 驗證方法 - 提供明確的錯誤訊息

## 🎯 最佳實踐

### 1. 錯誤訊息設計
```csharp
// 好的做法：提供明確的錯誤訊息
additionalData: new { 
    CustomerId = CustomerId,
    MethodName = nameof(LoadCustomerData),
    FilterCondition = searchFilter
}

// 避免：提供敏感資料或過於詳細的技術訊息
```

### 2. 預設值設定
```csharp
// 好的做法：設置符合業務邏輯的預設值
customers = new List<Customer>();
isLoading = false;
hasError = true;

// 避免：設置可能引起連鎖錯誤的預設值
customers = null; // 會導致後續空值異常
```

### 3. 異步錯誤處理
```csharp
// 好的做法：使用 Fire-and-Forget 模式
_ = ErrorHandlingHelper.HandleErrorSafelyAsync(...);

// 避免：在錯誤處理中使用 await
await ErrorHandlingHelper.HandleErrorSafelyAsync(...); // 可能導致死鎖
```

## 📊 已完成的修改

### Customer 模組
- ✅ **CustomerIndex.razor** - 完整錯誤處理強化
- ✅ **CustomerEdit.razor** - 完整錯誤處理強化  
- ✅ **CustomerDetail.razor** - 完整錯誤處理強化

### 修改重點
1. **統一錯誤處理方法**：所有 try-catch 區塊改用 `ErrorHandlingHelper.HandleErrorSafelyAsync()`
2. **分層責任明確**：頁面層負責錯誤處理與通知，Service 層只記錄錯誤
3. **預設值保護**：所有錯誤處理都設置安全的預設值
4. **追蹤資料補強**：所有錯誤記錄都包含 `additionalData` 便於除錯
5. **使用者體驗**：區分內部錯誤與使用者錯誤，避免不必要的通知

## 🔄 套用到新頁面

### 1. 注入必要服務
```csharp
@inject IErrorLogService ErrorLogService
@inject INotificationService NotificationService
```

### 2. 包裝所有可能拋出異常的方法
```csharp
protected override async Task OnInitializedAsync()
{
    try
    {
        await InitializeData();
    }
    catch (Exception ex)
    {
        _ = ErrorHandlingHelper.HandleErrorSafelyAsync(
            ex, 
            nameof(OnInitializedAsync),
            this.GetType(),
            ErrorLogService, 
            NotificationService, 
            showUserFriendlyMessage: true,
            additionalData: new { PageName = "YourPageName" }
        );
        
        // 設置預設值
        SetSafeDefaults();
    }
}
```

### 3. 建立預設值設定方法
```csharp
private void SetSafeDefaults()
{
    // 根據頁面需求設定安全的預設值
    isLoading = false;
    hasError = true;
    data = new List<TEntity>();
    // ... 其他必要的預設值
}
```

## 🚨 注意事項

1. **避免重複通知**：確保 Service 層不會重複通知使用者
2. **預設值合理性**：設定的預設值應符合業務邏輯
3. **效能考量**：錯誤處理不應影響正常流程的效能
4. **記錄詳細度**：平衡錯誤記錄的詳細程度與儲存成本
5. **測試覆蓋**：確保錯誤處理邏輯有適當的測試覆蓋

## 📚 參考資料

- `ErrorHandlingHelper.cs` - 統一錯誤處理工具類
- `GlobalExceptionHelper.cs` - 全域異常處理中間件
- `IErrorLogService` - 錯誤記錄服務介面
- `INotificationService` - 通知服務介面