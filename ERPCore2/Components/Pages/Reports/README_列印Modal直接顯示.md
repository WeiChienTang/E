# 列印 Modal 直接顯示 - 實作指南

## 1️⃣ 建立 Action 類型導航項目

### 位置
`Data/Navigation/NavigationConfig.cs`

### 代碼
```csharp
// 加入 using
using ERPCore2.Helpers;

// 在適當的 Children 清單中加入
NavigationActionHelper.CreateActionItem(
    name: "應收帳款報表",
    description: "查詢和列印客戶應收帳款資料",
    iconClass: "bi bi-caret-right-fill",
    actionId: "OpenAccountsReceivableReport",
    category: "財務管理",
    requiredPermission: "SetoffDocument.Read",
    searchKeywords: new List<string> { "應收帳款報表", "AR報表", "receivable report", "收款報表" }
)
```

---

## 2️⃣ 建立 Modal 組件

### 位置
`Components/Pages/Reports/AccountsReceivableReportPage.razor`

### 代碼
```csharp
@* 移除 @page 指令，改為 Parameter 控制顯示 *@
@using ERPCore2.Services.Interfaces

@inject ICustomerService CustomerService
@inject INotificationService NotificationService

<BatchPrintFilterModalComponent 
    IsVisible="@IsVisible"
    IsVisibleChanged="@IsVisibleChanged"
    Title="應收帳款報表"
    Customers="@customers"
    OnPrint="@HandlePrint" />

@code {
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    
    private List<Customer> customers = new();

    protected override async Task OnParametersSetAsync()
    {
        if (IsVisible)
        {
            await LoadDataAsync();
        }
    }

    private async Task LoadDataAsync()
    {
        customers = await CustomerService.GetAllAsync();
    }

    private async Task HandlePrint(BatchPrintCriteria criteria)
    {
        // 列印邏輯
    }
}
```

---

## 3️⃣ 在 MainLayout 中註冊 Modal

### 位置
`Components/Layout/MainLayout.razor`

### 代碼

#### 3.1 加入 using
```csharp
@using ERPCore2.Helpers
```

#### 3.2 加入 Modal 宣告
```razor
@* 應收帳款報表 Modal *@
<AccountsReceivableReportPage IsVisible="@showAccountsReceivableReport"
                             IsVisibleChanged="@((bool visible) => showAccountsReceivableReport = visible)" />
```

#### 3.3 在 @code 中加入狀態變數
```csharp
// 應收帳款報表 Modal 狀態
private bool showAccountsReceivableReport = false;

// Action 處理器註冊表
private NavigationActionHelper.ActionHandlerRegistry? actionRegistry;
```

#### 3.4 在 OnInitialized 中註冊 Action
```csharp
protected override void OnInitialized()
{
    // 初始化 Action 註冊表
    actionRegistry = NavigationActionHelper.CreateRegistry();
    
    // 註冊 Action 處理器
    actionRegistry.Register("OpenAccountsReceivableReport", OpenAccountsReceivableReport);
}
```

#### 3.5 加入開啟 Modal 的方法
```csharp
[JSInvokable]
public void OpenAccountsReceivableReport()
{
    try
    {
        showAccountsReceivableReport = true;
        StateHasChanged();
    }
    catch
    {
        // 忽略錯誤
    }
}
```

#### 3.6 更新 HandleNavigationAction 方法
```csharp
private void HandleNavigationAction(string actionId)
{
    // 使用 ActionHandlerRegistry 執行 Action
    actionRegistry?.Execute(actionId);
}
```

---

## 4️⃣ NavigationItem 模型擴充

### 位置
`Models/NavigationItem.cs`

### 代碼
```csharp
/// <summary>
/// 導航項目類型
/// </summary>
public enum NavigationItemType
{
    Route,
    Action
}

public class NavigationItem
{
    // 現有屬性...
    
    /// <summary>
    /// 導航項目類型
    /// </summary>
    public NavigationItemType ItemType { get; set; } = NavigationItemType.Route;
    
    /// <summary>
    /// 動作識別碼（當 ItemType 為 Action 時使用）
    /// </summary>
    public string? ActionId { get; set; }
}
```

---

## 5️⃣ NavMenu 組件更新

### 位置
`Components/Layout/NavMenu.razor`

### 代碼

#### 5.1 加入 EventCallback
```csharp
[Parameter] public EventCallback<string> OnActionTriggered { get; set; }
```

#### 5.2 包裝 CascadingValue
```razor
<CascadingValue Value="@this" Name="NavMenuInstance">
    @* 原有的 NavMenu 內容 *@
</CascadingValue>
```

#### 5.3 加入觸發方法
```csharp
public async Task TriggerActionAsync(string actionId)
{
    if (OnActionTriggered.HasDelegate)
    {
        await OnActionTriggered.InvokeAsync(actionId);
    }
}
```

---

## 6️⃣ NavDropdownItem 組件更新

### 位置
`Components/Shared/GenericComponent/NavMenu/NavDropdownItem.razor`

### 代碼

#### 6.1 加入 CascadingParameter
```csharp
[CascadingParameter(Name = "NavMenuInstance")]
public ERPCore2.Components.Layout.NavMenu? NavMenuInstance { get; set; }
```

#### 6.2 條件渲染
```razor
@if (Item.ItemType == NavigationItemType.Action)
{
    <button class="dropdown-item" @onclick="HandleActionClickAsync">
        <i class="@Item.IconClass"></i> @Item.Name
    </button>
}
else
{
    <a class="dropdown-item" href="@Item.Route">
        <i class="@Item.IconClass"></i> @Item.Name
    </a>
}
```

#### 6.3 加入點擊處理
```csharp
private async Task HandleActionClickAsync()
{
    if (NavMenuInstance != null && !string.IsNullOrEmpty(Item.ActionId))
    {
        await NavMenuInstance.TriggerActionAsync(Item.ActionId);
    }
}
```
