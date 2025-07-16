# CurrentUserHelper 快速參考

## 快速開始

### 1. 添加必要的 using 和注入
```csharp
@using Microsoft.AspNetCore.Components.Authorization
@using ERPCore2.Helpers
@inject AuthenticationStateProvider AuthenticationStateProvider
```

### 2. 標準初始化模式
```csharp
protected override async Task OnInitializedAsync()
{
    // ... 其他初始化
    await InitializeDefaultValues();
}

private async Task InitializeDefaultValues()
{
    if (!Id.HasValue) // 只在新增模式
    {
        entity.OperatorName = await CurrentUserHelper.GetCurrentUserFullNameAsync(AuthenticationStateProvider);
    }
}
```

### 3. 表單欄位設定
```csharp
new()
{
    PropertyName = nameof(Entity.OperatorName),
    Label = "操作人員",
    FieldType = FormFieldType.Text,
    IsReadOnly = true,
    ContainerCssClass = "col-md-6"
},
```

## 常用方法

| 方法 | 用途 | 返回範例 |
|------|------|----------|
| `GetCurrentUserFullNameAsync()` | 取得完整姓名 | "系統管理員" |
| `GetCurrentUserNameAsync()` | 取得使用者名稱 | "admin" |
| `GetCurrentEmployeeCodeAsync()` | 取得員工代碼 | "ADMIN001" |
| `GetCurrentDepartmentAsync()` | 取得部門 | "資訊部" |

## 常見應用場景

- 採購訂單 → PurchasePersonnel
- 銷貨訂單 → SalesPersonnel  
- 庫存異動 → OperatorName
- 盤點作業 → StockTaker
