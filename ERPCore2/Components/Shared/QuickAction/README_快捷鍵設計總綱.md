# 快捷鍵與快速功能設計總綱

## 概述

ERPCore2 系統提供完整的**快捷鍵**與**快速功能**機制，讓使用者能夠高效地在系統中導航和執行操作。

此設計包含三個層次：
1. **全域快捷鍵** - 透過鍵盤組合鍵快速觸發功能
2. **快速功能表** - 右下角浮動按鈕選單
3. **通用搜尋 Modal** - 支援頁面搜尋和報表搜尋

---

## 架構圖

```
┌─────────────────────────────────────────────────────────────────┐
│                         MainLayout.razor                         │
│  ┌─────────────────────────────────────────────────────────────┐│
│  │                    全域快捷鍵監聽                             ││
│  │  Alt+S → 頁面搜尋  |  Alt+R → 報表搜尋  |  Alt+Q → 快速選單  ││
│  └─────────────────────────────────────────────────────────────┘│
│                              ↓                                   │
│  ┌─────────────────────┐  ┌─────────────────────┐              │
│  │ QuickActionMenu     │  │ GenericSearchModal  │              │
│  │ (浮動功能表)         │→ │ (通用搜尋 Modal)    │              │
│  └─────────────────────┘  └─────────────────────┘              │
│                              ↓                                   │
│  ┌─────────────────────────────────────────────────────────────┐│
│  │ NavigationConfig + NavigationActionHelper                   ││
│  │ (導航配置 + Action 處理器)                                   ││
│  └─────────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────────┘
```

---

## 資料夾結構

```
Components/
├── Layout/
│   └── MainLayout.razor          # 快捷鍵整合入口
├── Shared/
│   └── QuickAction/
│       ├── QuickActionMenu.razor        # 浮動快速功能表
│       ├── GenericSearchModalComponent.razor  # 通用搜尋 Modal
│       └── ShortcutKeysModalComponent.razor   # 快捷鍵說明 Modal
│
Data/
└── Navigation/
    └── NavigationConfig.cs       # 導航選單配置（唯一資料來源）

Helpers/
└── Common/
    └── NavigationActionHelper.cs # Action 建立與處理器註冊

Models/
└── Navigation/
    ├── NavigationItem.cs         # 導航項目模型
    └── ISearchableItem.cs        # 可搜尋項目介面

wwwroot/
└── js/
    └── keyboard-shortcuts.js     # 全域鍵盤快捷鍵 JS
```

---

## 系統快捷鍵一覽

| 快捷鍵 | 功能 | 分類 | 實作位置 |
|--------|------|------|----------|
| `Alt + S` | 開啟頁面搜尋視窗 | 導航 | MainLayout.razor |
| `Alt + R` | 開啟報表搜尋視窗 | 導航 | MainLayout.razor |
| `Alt + Q` | 開啟/關閉快速功能表 | 導航 | QuickActionMenu.razor |
| `Esc` | 關閉當前 Modal 視窗 | 視窗控制 | BaseModalComponent.razor |

---

## 核心組件說明

### 1. MainLayout.razor（整合入口）

所有快捷鍵和快速功能都在 `MainLayout.razor` 中整合。

**關鍵程式碼：**

```razor
@* 全域頁面搜尋 Modal *@
<GenericSearchModalComponent @ref="pageSearchModal" 
                            IsVisible="@showPageSearch"
                            IsVisibleChanged="@((bool visible) => showPageSearch = visible)"
                            ActionRegistry="@actionRegistry"
                            Title="頁面搜尋"
                            SearchFunction="@SearchNavigationItems" />

@* 全域報表搜尋 Modal *@
<GenericSearchModalComponent @ref="reportSearchModal" 
                            IsVisible="@showReportSearch"
                            SearchFunction="@SearchReports"
                            OnItemSelected="@HandleReportSelected" />

@* 快速功能表 *@
<QuickActionMenu OnPageSearchClick="@OpenPageSearch"
                OnReportSearchClick="@OpenReportSearch"
                OnShortcutKeysClick="@OpenShortcutKeys" />
```

**快捷鍵註冊（JavaScript）：**

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        // 註冊 Alt+S 快捷鍵
        await JSRuntime.InvokeVoidAsync("eval", @"
            document.addEventListener('keydown', function(e) {
                if (e.altKey && e.key.toLowerCase() === 's') {
                    e.preventDefault();
                    // 觸發頁面搜尋
                }
            });
        ");
    }
}
```

---

### 2. QuickActionMenu.razor（浮動功能表）

右下角的浮動按鈕，提供快速存取常用功能。

**功能項目：**

| 項目 | 快捷鍵 | 說明 |
|------|--------|------|
| 頁面搜尋 | Alt + S | 搜尋並導航至功能頁面 |
| 報表搜尋 | Alt + R | 搜尋並開啟報表 |
| 快捷鍵說明 | - | 顯示所有可用快捷鍵 |
| 通知中心 | - | （預留功能） |
| 最近使用 | - | （預留功能） |
| 快速設定 | - | （預留功能） |

**參數：**

```csharp
[Parameter] public EventCallback OnPageSearchClick { get; set; }
[Parameter] public EventCallback OnReportSearchClick { get; set; }
[Parameter] public EventCallback OnShortcutKeysClick { get; set; }
[Parameter] public bool showNotifications { get; set; } = false;  // 預留
[Parameter] public bool showRecentPages { get; set; } = false;   // 預留
[Parameter] public bool showSettings { get; set; } = false;      // 預留
```

---

### 3. GenericSearchModalComponent.razor（通用搜尋）

可重用的搜尋 Modal 組件，支援頁面搜尋和報表搜尋。

**參數：**

| 參數 | 類型 | 說明 |
|------|------|------|
| `IsVisible` | `bool` | 控制 Modal 顯示 |
| `Title` | `string` | Modal 標題 |
| `Placeholder` | `string` | 搜尋框佔位符 |
| `SearchFunction` | `Func<string, List<ISearchableItem>>` | 搜尋函數 |
| `ActionRegistry` | `ActionHandlerRegistry` | Action 處理器 |
| `OnItemSelected` | `EventCallback<string>` | 選擇項目回調 |

**使用方式：**

```razor
<GenericSearchModalComponent 
    Title="頁面搜尋"
    Icon="bi-search"
    Placeholder="輸入功能名稱或關鍵字搜尋..."
    SearchFunction="@SearchNavigationItems"
    ActionRegistry="@actionRegistry" />
```

---

### 4. NavigationConfig.cs（導航配置）

系統唯一的選單資料來源，同時用於 NavMenu 渲染和搜尋功能。

**結構：**

```csharp
public static class NavigationConfig
{
    // 取得所有導航項目（包含父級和子級）
    public static List<NavigationItem> GetAllNavigationItems() { ... }
    
    // 取得所有選單群組（僅父級項目）- 用於 NavMenu
    public static List<NavigationItem> GetMenuGroups() { ... }
    
    // 取得扁平化的所有導航項目 - 用於搜尋
    public static List<NavigationItem> GetFlattenedNavigationItems() { ... }
}
```

**導航項目範例：**

```csharp
// 一般路由項目
new NavigationItem
{
    Name = "員工管理",
    Description = "管理員工資料和人事資訊",
    Route = "/employees",
    Category = "人力資源管理",
    RequiredPermission = "Employee.Read",
    SearchKeywords = new List<string> { "員工管理", "員工資料" }
}

// Action 類型項目（開啟 Modal）
NavigationActionHelper.CreateActionItem(
    name: "權限分配管理",
    description: "管理權限組與權限關係",
    iconClass: "bi bi-shield-lock",
    actionId: "OpenRolePermissionManagement",
    category: "人力資源管理",
    requiredPermission: "Role.Read"
)
```

---

### 5. NavigationActionHelper.cs（Action 處理器）

統一管理 Action 類型項目的建立、註冊和執行。

**核心功能：**

```csharp
public static class NavigationActionHelper
{
    // 建立 Action 類型導航項目
    public static NavigationItem CreateActionItem(
        string name,
        string description,
        string iconClass,
        string actionId,
        string category = "",
        string? requiredPermission = null,
        List<string>? searchKeywords = null
    ) { ... }

    // Action 處理器註冊表
    public class ActionHandlerRegistry
    {
        public void Register(string actionId, Action handler) { ... }
        public bool Execute(string actionId) { ... }
    }
}
```

**使用方式：**

```csharp
// 在 MainLayout.razor 中初始化
protected override Task OnInitializedAsync()
{
    actionRegistry = NavigationActionHelper.CreateRegistry();
    
    // 註冊所有 Action 處理器
    actionRegistry.Register("OpenAccountsReceivableReport", OpenAccountsReceivableReport);
    actionRegistry.Register("OpenRolePermissionManagement", OpenRolePermissionManagement);
    actionRegistry.Register("OpenCustomerReportIndex", OpenCustomerReportIndex);
}
```

---

### 6. NavigationItem.cs（導航項目模型）

```csharp
public class NavigationItem : ISearchableItem
{
    public string Name { get; set; }              // 功能名稱
    public string Description { get; set; }       // 功能描述
    public string Route { get; set; }             // 路由路徑
    public NavigationItemType ItemType { get; set; }  // 項目類型
    public string? ActionId { get; set; }         // 動作識別碼
    public string IconClass { get; set; }         // 圖示類別
    public string Category { get; set; }          // 分類
    public string? RequiredPermission { get; set; } // 權限要求
    public List<string> SearchKeywords { get; set; } // 搜尋關鍵字
    public bool IsParent { get; set; }            // 是否為父選單
    public List<NavigationItem> Children { get; set; } // 子選單
}
```

**項目類型：**

```csharp
public enum NavigationItemType
{
    Route,   // 路由導航（預設）
    Action   // 觸發動作（如開啟 Modal）
}
```

---

### 7. keyboard-shortcuts.js（全域快捷鍵）

處理系統級的快捷鍵，避免與輸入元素和 Modal 衝突。

**功能：**
- 自動偵測是否在輸入元素中（避免干擾使用者輸入）
- 偵測是否有 Modal 開啟（避免多重 Modal 衝突）
- 支援透過 .NET 互操作呼叫 C# 方法

```javascript
window.KeyboardShortcuts = {
    initialize: function(dotNetHelper) {
        document.addEventListener('keydown', this.handleKeyDown);
    },
    
    handleKeyDown: function(event) {
        // 1. 檢查是否在輸入元素中
        if (this.isInputElement(event.target)) return;
        
        // 2. 檢查是否有 Modal 開啟
        if (this.hasOpenModal()) return;
        
        // 3. 處理快捷鍵
        if (event.altKey && event.key === 's') {
            event.preventDefault();
            this.dotNetHelper.invokeMethodAsync('OpenPageSearch');
        }
    },
    
    isInputElement: function(element) { ... },
    hasOpenModal: function() { ... },
    dispose: function() { ... }
};
```

---

## 新增快捷鍵指南

### 步驟 1：決定觸發方式

選擇適合的快捷鍵組合，建議使用 `Alt + 字母` 格式。

### 步驟 2：在 MainLayout.razor 註冊

```csharp
// 在 OnAfterRenderAsync 中註冊
await JSRuntime.InvokeVoidAsync("eval", @"
    document.addEventListener('keydown', function(e) {
        if (e.altKey && e.key.toLowerCase() === 'x') {
            e.preventDefault();
            // 執行對應動作
        }
    });
");
```

### 步驟 3：更新快捷鍵說明

修改 `ShortcutKeysModalComponent.razor` 中的 `InitializeShortcutKeys` 方法：

```csharp
shortcutKeys.Add(new ShortcutKeyInfo
{
    Keys = "Alt + X",
    Description = "新功能說明",
    Category = "分類名稱"
});
```

---

## 新增 Action 類型功能指南

### 步驟 1：在 NavigationConfig.cs 新增項目

```csharp
NavigationActionHelper.CreateActionItem(
    name: "新功能名稱",
    description: "功能描述",
    iconClass: "bi bi-xxx",
    actionId: "OpenNewFeature",
    category: "分類",
    requiredPermission: "Feature.Read",
    searchKeywords: new List<string> { "關鍵字1", "關鍵字2" }
)
```

### 步驟 2：在 MainLayout.razor 註冊處理器

```csharp
// 在 OnInitializedAsync 中
actionRegistry.Register("OpenNewFeature", OpenNewFeature);

// 實作處理方法
private void OpenNewFeature()
{
    showNewFeatureModal = true;
    StateHasChanged();
}
```

### 步驟 3：放置對應的 Modal

```razor
<NewFeatureModalComponent IsVisible="@showNewFeatureModal"
                         IsVisibleChanged="@((bool v) => showNewFeatureModal = v)" />
```

---

## 設計原則

1. **衝突避免**：快捷鍵不應與瀏覽器預設或作業系統快捷鍵衝突
2. **輸入保護**：在輸入元素中時自動停用快捷鍵
3. **Modal 感知**：當有 Modal 開啟時，不觸發全域快捷鍵
4. **統一入口**：所有快捷鍵在 MainLayout 統一管理
5. **可搜尋性**：所有功能都可透過搜尋找到（SearchKeywords）
6. **權限控制**：導航項目支援 RequiredPermission 權限檢查

---

## 相關檔案

- [Components/Layout/MainLayout.razor](../Components/Layout/MainLayout.razor) - 快捷鍵整合入口
- [Components/Shared/QuickAction/](../Components/Shared/QuickAction/) - 快速功能組件
- [Data/Navigation/NavigationConfig.cs](../Data/Navigation/NavigationConfig.cs) - 導航配置
- [Helpers/Common/NavigationActionHelper.cs](../Helpers/Common/NavigationActionHelper.cs) - Action 輔助類
- [wwwroot/js/keyboard-shortcuts.js](../wwwroot/js/keyboard-shortcuts.js) - 全域快捷鍵 JS
