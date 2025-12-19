# 頁面搜尋功能支援 Action 類型項目

## 問題描述

在導航選單中使用 `NavigationActionHelper.CreateActionItem` 建立的 Action 類型項目（例如：單位換算、權限組權限、應收帳款報表），無法透過全域頁面搜尋功能（Ctrl+K）查詢到。

### 問題範例

```csharp
// NavigationConfig.cs
NavigationActionHelper.CreateActionItem(
    name: "單位換算",
    description: "管理單位之間的換算規則（如：包→公斤）",
    iconClass: "bi bi-arrow-left-right",
    actionId: "OpenUnitConversionManagement",
    category: "商品管理",
    requiredPermission: "Unit.Read",
    searchKeywords: new List<string> { "單位換算", "單位轉換", "unit conversion", "換算", "包裝單位" }
)
```

使用者搜尋「單位換算」、「換算」等關鍵字時，無法在搜尋結果中找到此項目。

## 問題原因分析

### 1. Action 類型項目的特性

使用 `NavigationActionHelper.CreateActionItem` 建立的項目具有以下特性：

```csharp
public static NavigationItem CreateActionItem(...)
{
    return new NavigationItem
    {
        ItemType = NavigationItemType.Action,  // 類型為 Action
        Route = "",                             // Route 為空字串（不是路由導航）
        ActionId = actionId,                    // 透過 ActionId 執行動作
        // ...其他屬性
    };
}
```

### 2. 搜尋過濾邏輯的問題

`PageSearchModalComponent.razor` 的搜尋過濾邏輯：

```csharp
// ❌ 問題寫法
searchResults = NavigationSearchService.SearchNavigationItems(searchTerm)
    .Where(item => !item.IsParent && !string.IsNullOrEmpty(item.Route) && item.Route != "#")
    .ToList();
```

這個過濾條件要求：
- `item.Route` 不能為空
- `item.Route` 不能是 "#"

但 Action 類型項目的 `Route` 是空字串 `""`，因此被過濾掉了。

### 3. 點擊處理邏輯不完整

原本的點擊處理只支援路由導航：

```csharp
// ❌ 不完整的寫法
private async Task HandleNavigationCardClick(NavigationItem item)
{
    if (!string.IsNullOrEmpty(item.Route) && item.Route != "#")
    {
        await HandleClose();
        NavigationManager.NavigateTo(item.Route);
    }
}
```

沒有處理 Action 類型項目的執行邏輯。

## 解決方案

### 1. 修正搜尋過濾邏輯

更新 `PageSearchModalComponent.razor` 的 `PerformSearch` 方法：

```csharp
// ✅ 正確寫法
searchResults = NavigationSearchService.SearchNavigationItems(searchTerm)
    .Where(item => !item.IsParent && 
        (item.ItemType == NavigationItemType.Action ||      // 允許 Action 類型
         (!string.IsNullOrEmpty(item.Route) && item.Route != "#")))  // 或有效路由
    .ToList();
```

**說明**：
- 允許 `ItemType` 為 `Action` 的項目
- 或者有有效 `Route` 的項目（路由導航類型）

### 2. 新增 ActionRegistry 參數

在 `PageSearchModalComponent.razor` 中新增參數：

```csharp
[Parameter] public NavigationActionHelper.ActionHandlerRegistry? ActionRegistry { get; set; }
```

並新增 using：

```csharp
@using ERPCore2.Helpers
```

### 3. 更新點擊處理邏輯

完整處理兩種類型的導航項目：

```csharp
// ✅ 完整的寫法
private async Task HandleNavigationCardClick(NavigationItem item)
{
    if (item.ItemType == NavigationItemType.Action && !string.IsNullOrWhiteSpace(item.ActionId))
    {
        // 處理 Action 類型項目
        await HandleClose();
        
        // 使用 ActionHandlerRegistry 執行 Action
        if (ActionRegistry != null)
        {
            ActionRegistry.Execute(item.ActionId);
        }
    }
    else if (!string.IsNullOrEmpty(item.Route) && item.Route != "#")
    {
        // 處理路由類型項目
        await HandleClose();
        NavigationManager.NavigateTo(item.Route);
    }
}
```

### 4. 在 MainLayout 中傳遞 ActionRegistry

更新 `MainLayout.razor` 的組件宣告：

```csharp
@* 全域頁面搜尋 Modal *@
<PageSearchModalComponent @ref="pageSearchModal" 
                         IsVisible="@showPageSearch"
                         IsVisibleChanged="@((bool visible) => showPageSearch = visible)"
                         ActionRegistry="@actionRegistry" />
```

## 正確的實作流程

### 導航項目的兩種類型

#### 1. 路由導航類型（Route）

適用於有獨立頁面的功能：

```csharp
new NavigationItem
{
    Name = "商品管理",
    Description = "管理商品資料和商品目錄",
    Route = "/products",                    // 有明確路由
    IconClass = "bi bi-caret-right-fill",
    Category = "商品管理",
    RequiredPermission = "Product.Read",
    SearchKeywords = new List<string> { "商品管理", "商品資料", "商品目錄" }
}
```

**特性**：
- 有明確的 `Route` 路徑
- 點擊後導航到指定頁面
- `ItemType` 預設為 `Route`

#### 2. Action 動作類型（Action）

適用於開啟 Modal、執行特定動作的功能：

```csharp
NavigationActionHelper.CreateActionItem(
    name: "單位換算",
    description: "管理單位之間的換算規則（如：包→公斤）",
    iconClass: "bi bi-arrow-left-right",
    actionId: "OpenUnitConversionManagement",  // Action 識別碼
    category: "商品管理",
    requiredPermission: "Unit.Read",
    searchKeywords: new List<string> { "單位換算", "單位轉換", "unit conversion" }
)
```

**特性**：
- `Route` 為空字串
- 有 `ActionId` 識別碼
- `ItemType` 為 `Action`
- 需要在 `MainLayout` 註冊對應的處理方法

### Action 類型的完整實作步驟

#### 步驟 1：在 NavigationConfig 中定義

```csharp
// Data/Navigation/NavigationConfig.cs
NavigationActionHelper.CreateActionItem(
    name: "單位換算",
    description: "管理單位之間的換算規則（如：包→公斤）",
    iconClass: "bi bi-arrow-left-right",
    actionId: "OpenUnitConversionManagement",  // ← 重要：Action 識別碼
    category: "商品管理",
    requiredPermission: "Unit.Read",
    searchKeywords: new List<string> { "單位換算", "單位轉換", "unit conversion", "換算" }
)
```

#### 步驟 2：在 MainLayout 中實作對應方法

```csharp
// Components/Layout/MainLayout.razor
@code {
    // 1. 定義狀態變數
    private bool showUnitConversionManagement = false;
    
    // 2. 在 OnInitialized 中註冊 Action
    protected override void OnInitialized()
    {
        actionRegistry = NavigationActionHelper.CreateRegistry();
        
        // 註冊各種 Action
        actionRegistry.Register("OpenUnitConversionManagement", OpenUnitConversionManagement);
        actionRegistry.Register("OpenRolePermissionManagement", OpenRolePermissionManagement);
        // ...其他 Action
    }
    
    // 3. 實作 Action 處理方法
    [JSInvokable]
    public void OpenUnitConversionManagement()
    {
        try
        {
            showUnitConversionManagement = true;
            StateHasChanged();
        }
        catch
        {
            // 錯誤處理
        }
    }
}
```

#### 步驟 3：在 MainLayout 的 UI 中加入 Modal

```razor
@* 單位換算管理 Modal *@
<UnitConversionManagementModal @bind-IsVisible="showUnitConversionManagement" />
```

## 完整程式碼範例

### PageSearchModalComponent.razor

```csharp
@using ERPCore2.Models
@using ERPCore2.Components.Shared.Modals
@using ERPCore2.Helpers
@inject INavigationSearchService NavigationSearchService
@inject IJSRuntime JSRuntime
@inject NavigationManager NavigationManager

@code {
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public NavigationActionHelper.ActionHandlerRegistry? ActionRegistry { get; set; }
    
    private List<NavigationItem> searchResults = new();
    
    private async Task PerformSearch()
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            searchResults.Clear();
        }
        else
        {
            // ✅ 支援 Action 和 Route 兩種類型
            searchResults = NavigationSearchService.SearchNavigationItems(searchTerm)
                .Where(item => !item.IsParent && 
                    (item.ItemType == NavigationItemType.Action || 
                     (!string.IsNullOrEmpty(item.Route) && item.Route != "#")))
                .ToList();
        }
        
        await Task.CompletedTask;
    }
    
    private async Task HandleNavigationCardClick(NavigationItem item)
    {
        // ✅ 區分處理 Action 和 Route
        if (item.ItemType == NavigationItemType.Action && !string.IsNullOrWhiteSpace(item.ActionId))
        {
            await HandleClose();
            ActionRegistry?.Execute(item.ActionId);
        }
        else if (!string.IsNullOrEmpty(item.Route) && item.Route != "#")
        {
            await HandleClose();
            NavigationManager.NavigateTo(item.Route);
        }
    }
}
```

### MainLayout.razor

```razor
@* 傳遞 ActionRegistry *@
<PageSearchModalComponent @ref="pageSearchModal" 
                         IsVisible="@showPageSearch"
                         IsVisibleChanged="@((bool visible) => showPageSearch = visible)"
                         ActionRegistry="@actionRegistry" />

@code {
    private NavigationActionHelper.ActionHandlerRegistry? actionRegistry;
    
    protected override void OnInitialized()
    {
        // 建立並註冊所有 Action
        actionRegistry = NavigationActionHelper.CreateRegistry();
        actionRegistry.Register("OpenUnitConversionManagement", OpenUnitConversionManagement);
        actionRegistry.Register("OpenRolePermissionManagement", OpenRolePermissionManagement);
        actionRegistry.Register("OpenAccountsReceivableReport", OpenAccountsReceivableReport);
    }
}
```

## 注意事項與最佳實踐

### 1. ActionId 命名規範

- 使用 PascalCase 命名
- 使用動詞開頭（Open、Show、Execute 等）
- 清楚描述動作內容

```csharp
✅ 好的命名：
- "OpenUnitConversionManagement"
- "ShowAccountsReceivableReport"
- "ExecuteInventoryCheck"

❌ 不好的命名：
- "unitConversion"  // 應使用 PascalCase
- "manage"          // 不夠具體
- "Action1"         // 無意義
```

### 2. 必須註冊 Action

所有在 `NavigationConfig` 中使用的 `ActionId` 都必須在 `MainLayout.OnInitialized` 中註冊：

```csharp
// ❌ 忘記註冊會導致點擊無效
NavigationActionHelper.CreateActionItem(
    actionId: "OpenNewFeature",  // 定義了 ActionId
    // ...
)

// ✅ 必須在 MainLayout 中註冊
protected override void OnInitialized()
{
    actionRegistry.Register("OpenNewFeature", OpenNewFeature);
}
```

### 3. 搜尋關鍵字要完整

Action 類型項目特別需要完整的搜尋關鍵字：

```csharp
✅ 完整的關鍵字：
searchKeywords: new List<string> { 
    "單位換算",      // 完整名稱
    "單位轉換",      // 同義詞
    "unit conversion", // 英文
    "換算",          // 簡稱
    "包裝單位"       // 相關詞
}

❌ 不完整的關鍵字：
searchKeywords: new List<string> { "單位換算" }  // 太少
```

### 4. 區分使用時機

**使用 Route 類型**：
- 功能有獨立的完整頁面
- 需要 URL 路由和瀏覽記錄
- 例如：商品管理、客戶管理、訂單列表

**使用 Action 類型**：
- 功能是開啟 Modal 對話框
- 執行特定動作但不需要獨立頁面
- 例如：單位換算設定、權限管理、快速報表

### 5. 錯誤處理

在 Action 處理方法中加入錯誤處理：

```csharp
[JSInvokable]
public void OpenUnitConversionManagement()
{
    try
    {
        showUnitConversionManagement = true;
        StateHasChanged();
    }
    catch (Exception ex)
    {
        Logger?.LogError(ex, "開啟單位換算管理失敗");
        // 顯示錯誤訊息給使用者
    }
}
```

## 測試檢查清單

修改完成後，請確認以下項目：

- [ ] Action 項目能透過搜尋功能找到
- [ ] 搜尋關鍵字都能正確匹配
- [ ] 點擊 Action 項目能正確開啟對應 Modal
- [ ] Route 項目仍然能正常導航
- [ ] 鍵盤操作（方向鍵、Enter）正常
- [ ] 權限控制正常運作
- [ ] 無編譯錯誤或警告

## 相關文件

- `Data/Navigation/NavigationConfig.cs` - 導航選單配置
- `Helpers/Common/NavigationActionHelper.cs` - Action 輔助類別
- `Components/Shared/BaseModal/Modals/QuickActionMenu/PageSearchModalComponent.razor` - 搜尋組件
- `Components/Layout/MainLayout.razor` - 主版面配置

## 修改記錄

- **2025-12-17**：修正頁面搜尋功能支援 Action 類型項目
  - 更新搜尋過濾邏輯
  - 新增 ActionRegistry 參數傳遞
  - 完善點擊處理邏輯
