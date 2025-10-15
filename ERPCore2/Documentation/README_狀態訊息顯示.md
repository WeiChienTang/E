# GenericEditModalComponent - 通用狀態訊息顯示系統

## 📋 文件資訊

- **建立日期**: 2025/10/15
- **修改版本**: v2.0
- **相關元件**: `GenericEditModalComponent.razor`
- **適用範圍**: 所有使用 GenericEditModalComponent 的 Modal 視窗

---

## 🎯 功能概述

將原本僅用於「審核狀態顯示」的功能，升級為「通用狀態訊息顯示系統」，支援：

- ✅ **審核狀態顯示**（向下相容）
- ✅ **警告訊息**
- ✅ **提示訊息**
- ✅ **任何需要在 Modal 頂部顯示的狀態資訊**
- ✅ **8 種 Bootstrap 徽章顏色**
- ✅ **自訂圖示**
- ✅ **靜態和動態兩種模式**

---

## 🔄 修改內容

### **1. 新增 BadgeVariant 列舉**

```csharp
/// <summary>
/// Bootstrap 徽章顏色變體
/// </summary>
public enum BadgeVariant
{
    Primary,    // 藍色 - 主要
    Secondary,  // 灰色 - 次要
    Success,    // 綠色 - 成功
    Danger,     // 紅色 - 危險/錯誤
    Warning,    // 黃色 - 警告
    Info,       // 淺藍色 - 資訊（預設）
    Light,      // 淺色
    Dark        // 深色
}
```

### **2. 新增通用狀態訊息參數**

```csharp
// ===== 通用狀態訊息參數 =====

/// <summary>
/// 顯示狀態訊息（可用於審核狀態、警告、提示等任何情境）
/// </summary>
[Parameter] public string? StatusMessage { get; set; }

/// <summary>
/// 狀態訊息的徽章顏色變體
/// </summary>
[Parameter] public BadgeVariant StatusBadgeVariant { get; set; } = BadgeVariant.Info;

/// <summary>
/// 狀態訊息的圖示類別
/// </summary>
[Parameter] public string StatusIconClass { get; set; } = "fas fa-info-circle";

/// <summary>
/// 是否顯示狀態訊息（預設為 false）
/// </summary>
[Parameter] public bool ShowStatusMessage { get; set; } = false;

/// <summary>
/// 動態取得狀態訊息的函式（優先於 StatusMessage）
/// 用於審核狀態等需要動態計算的情境
/// </summary>
[Parameter] public Func<Task<(string Message, BadgeVariant Variant, string IconClass)?>>? GetStatusMessage { get; set; }
```

### **3. 移除舊參數**

```csharp
// ❌ 已移除
[Parameter] public Func<Task<string?>>? GetApprovalStatus { get; set; }

// ✅ 改用
[Parameter] public Func<Task<(string Message, BadgeVariant Variant, string IconClass)?>>? GetStatusMessage { get; set; }
```

### **4. 新增輔助方法**

- `ShouldShowStatusMessage()` - 判斷是否應該顯示狀態訊息
- `LoadStatusMessageData()` - 載入狀態訊息資料
- `GetBadgeColorClass()` - 取得徽章顏色 CSS 類別
- `ResetStatusMessage()` - 重置狀態訊息快取

---

## 📖 使用方式

### **方式 1: 靜態訊息（簡單場景）**

適用於：固定的警告訊息、提示訊息

```razor
<GenericEditModalComponent TEntity="MyEntity" 
                          TService="IMyService"
                          ShowStatusMessage="true"
                          StatusMessage="此單據已鎖定，無法修改"
                          StatusBadgeVariant="BadgeVariant.Warning"
                          StatusIconClass="fas fa-lock"
                          ... />
```

**顯示效果：** 🟡 🔒 此單據已鎖定，無法修改

---

### **方式 2: 動態訊息（建議用於審核）**

適用於：審核狀態、需要根據資料動態計算的訊息

#### **範例 1：審核狀態顯示**

```razor
<GenericEditModalComponent TEntity="PurchaseOrder" 
                          TService="IPurchaseOrderService"
                          ShowApprovalSection="true"
                          ApprovalPermission="PurchaseOrder.Approve"
                          OnApprove="@HandlePurchaseOrderApprove"
                          OnReject="@HandlePurchaseOrderReject"
                          GetStatusMessage="@GetPurchaseOrderStatusMessage"
                          ... />

@code {
    /// <summary>
    /// 取得採購單狀態訊息
    /// </summary>
    private async Task<(string Message, GenericEditModalComponent<PurchaseOrder, IPurchaseOrderService>.BadgeVariant Variant, string IconClass)?> GetPurchaseOrderStatusMessage()
    {
        try
        {
            if (editModalComponent?.Entity == null)
                return null;
            
            var purchaseOrder = editModalComponent.Entity;
            
            // 只有已審核通過或已駁回時才顯示訊息
            if (purchaseOrder.IsApproved && purchaseOrder.ApprovedAt.HasValue)
            {
                var approverName = purchaseOrder.ApprovedByUser?.Name ?? "審核人員";
                return (
                    $"已於 {purchaseOrder.ApprovedAt.Value:yyyy/MM/dd HH:mm} 由 {approverName} 審核通過",
                    GenericEditModalComponent<PurchaseOrder, IPurchaseOrderService>.BadgeVariant.Success,
                    "fas fa-check-circle"
                );
            }
            else if (!string.IsNullOrWhiteSpace(purchaseOrder.RejectReason))
            {
                return (
                    $"已駁回：{purchaseOrder.RejectReason}",
                    GenericEditModalComponent<PurchaseOrder, IPurchaseOrderService>.BadgeVariant.Danger,
                    "fas fa-times-circle"
                );
            }
            
            // 待審核狀態不顯示訊息
            return null;
        }
        catch (Exception ex)
        {
            await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetPurchaseOrderStatusMessage), GetType());
            return null;
        }
    }
}
```

**顯示效果：**
- ✅ 審核通過: 🟢 ✔️ 已於 2025/10/15 14:30 由 張三 審核通過
- ❌ 已駁回: 🔴 ✖️ 已駁回：價格過高需重新議價
- ⏳ 待審核: *(不顯示)*

---

#### **範例 2：庫存警告**

```razor
<GenericEditModalComponent TEntity="Product" 
                          TService="IProductService"
                          ShowStatusMessage="true"
                          GetStatusMessage="@GetStockWarningMessage"
                          ... />

@code {
    private async Task<(string Message, GenericEditModalComponent<Product, IProductService>.BadgeVariant Variant, string IconClass)?> GetStockWarningMessage()
    {
        if (editModalComponent?.Entity == null)
            return null;
        
        var product = editModalComponent.Entity;
        var stockLevel = await GetCurrentStockLevel(product.Id);
        
        if (stockLevel < 10)
        {
            return (
                $"庫存嚴重不足！目前僅剩 {stockLevel} 件",
                GenericEditModalComponent<Product, IProductService>.BadgeVariant.Danger,
                "fas fa-exclamation-triangle"
            );
        }
        else if (stockLevel < 50)
        {
            return (
                $"庫存偏低，剩餘 {stockLevel} 件",
                GenericEditModalComponent<Product, IProductService>.BadgeVariant.Warning,
                "fas fa-box"
            );
        }
        
        return null; // 庫存充足，不顯示訊息
    }
}
```

**顯示效果：**
- 🔴 ⚠️ 庫存嚴重不足！目前僅剩 5 件
- 🟡 📦 庫存偏低，剩餘 30 件

---

### **方式 3: 混合模式（靜態 + 動態）**

優先使用動態函式，如果沒有則使用靜態設定：

```razor
<GenericEditModalComponent TEntity="Invoice" 
                          TService="IInvoiceService"
                          ShowStatusMessage="true"
                          StatusMessage="預設訊息"
                          StatusBadgeVariant="BadgeVariant.Info"
                          GetStatusMessage="@GetDynamicMessage"
                          ... />
```

> **優先順序：** `GetStatusMessage` > `StatusMessage`

---

## 🎨 顏色和圖示指南

### **徽章顏色語意**

| 顏色 | BadgeVariant | 適用場景 | 視覺效果 |
|------|--------------|---------|---------|
| 🔵 藍色 | `Primary` | 主要資訊、強調 | `bg-primary` |
| ⚫ 灰色 | `Secondary` | 次要資訊 | `bg-secondary` |
| 🟢 綠色 | `Success` | 成功、審核通過、正常 | `bg-success` |
| 🔴 紅色 | `Danger` | 錯誤、駁回、嚴重警告 | `bg-danger` |
| 🟡 黃色 | `Warning` | 警告、注意事項 | `bg-warning` |
| 🔵 淺藍 | `Info` | 一般資訊、提示（預設） | `bg-info` |
| ⚪ 淺色 | `Light` | 低對比資訊 | `bg-light` |
| ⚫ 深色 | `Dark` | 深色背景資訊 | `bg-dark` |

### **常用圖示**

| 圖示類別 | 視覺 | 適用場景 |
|---------|------|---------|
| `fas fa-check-circle` | ✅ | 成功、審核通過 |
| `fas fa-times-circle` | ❌ | 失敗、駁回 |
| `fas fa-exclamation-triangle` | ⚠️ | 警告 |
| `fas fa-info-circle` | ℹ️ | 資訊 |
| `fas fa-lock` | 🔒 | 鎖定 |
| `fas fa-unlock` | 🔓 | 解鎖 |
| `fas fa-clock` | 🕐 | 待處理 |
| `fas fa-box` | 📦 | 庫存 |
| `fas fa-dollar-sign` | 💲 | 金額 |
| `fas fa-user-check` | 👤✓ | 已驗證 |

---

## 🔧 升級指南（從舊版審核系統升級）

### **舊版寫法**

```razor
<GenericEditModalComponent 
    ShowApprovalSection="true"
    GetApprovalStatus="@GetApprovalStatus"
    ... />

@code {
    private async Task<string?> GetApprovalStatus()
    {
        if (Entity?.IsApproved == true)
            return "已審核通過";
        return "待審核";
    }
}
```

### **新版寫法**

```razor
<GenericEditModalComponent 
    ShowApprovalSection="true"
    GetStatusMessage="@GetStatusMessage"
    ... />

@code {
    private async Task<(string Message, BadgeVariant Variant, string IconClass)?> GetStatusMessage()
    {
        if (Entity?.IsApproved == true)
        {
            return (
                "已審核通過",
                GenericEditModalComponent<TEntity, TService>.BadgeVariant.Success,
                "fas fa-check-circle"
            );
        }
        
        // 待審核不顯示
        return null;
    }
}
```

### **升級步驟**

1. ✅ 將 `GetApprovalStatus` 改為 `GetStatusMessage`
2. ✅ 修改返回類型：`string?` → `(string Message, BadgeVariant Variant, string IconClass)?`
3. ✅ 返回 tuple，包含訊息、顏色、圖示
4. ✅ 不需要顯示訊息時返回 `null`
5. ✅ 使用語意化的顏色（Success = 綠色，Danger = 紅色等）

---

## 💡 最佳實踐

### **1. 何時顯示訊息**

✅ **應該顯示**
- 審核通過/駁回
- 嚴重警告（庫存不足、金額異常等）
- 重要狀態變更
- 鎖定/解鎖狀態

❌ **不應該顯示**
- 正常的待審核狀態
- 一般的新增/編輯狀態
- 不需要特別提醒的資訊

### **2. 顏色選擇原則**

```csharp
// ✅ 好的用法
if (isApproved) return (..., BadgeVariant.Success, ...);     // 綠色 = 成功
if (isRejected) return (..., BadgeVariant.Danger, ...);      // 紅色 = 錯誤
if (isWarning) return (..., BadgeVariant.Warning, ...);      // 黃色 = 警告

// ❌ 避免
if (isApproved) return (..., BadgeVariant.Danger, ...);      // 不要用紅色表示成功
if (isInfo) return (..., BadgeVariant.Success, ...);         // 不要濫用成功色
```

### **3. 訊息文字原則**

✅ **簡潔明確**
```csharp
"已於 2025/10/15 14:30 由 張三 審核通過"  // 包含關鍵資訊
"庫存不足！剩餘 5 件"                    // 直接明瞭
```

❌ **避免冗長**
```csharp
"此採購單已經在 2025 年 10 月 15 日下午 2 點 30 分由使用者張三進行審核，審核結果為通過"
```

### **4. 返回 null 的時機**

```csharp
// ✅ 適當使用 null
if (Entity == null) return null;           // 沒有資料
if (IsNormalState) return null;            // 正常狀態不顯示
if (!NeedNotification) return null;        // 不需要通知

// ❌ 不要返回空字串
return ("", BadgeVariant.Info, "");        // 不好的做法
```

---

## 🔍 完整範例：採購單審核

```razor
@* PurchaseOrderEditModalComponent.razor *@

<GenericEditModalComponent TEntity="PurchaseOrder" 
                          TService="IPurchaseOrderService"
                          @ref="editModalComponent"
                          IsVisible="@IsVisible"
                          IsVisibleChanged="@IsVisibleChanged"
                          Id="@PurchaseOrderId"
                          Service="@PurchaseOrderService"
                          EntityName="採購單"
                          ModalTitle="@(PurchaseOrderId.HasValue ? "編輯採購單" : "新增採購單")"
                          ShowApprovalSection="@ShouldShowApprovalSection()"
                          ApprovalPermission="PurchaseOrder.Approve"
                          OnApprove="@HandlePurchaseOrderApprove"
                          OnReject="@HandlePurchaseOrderReject"
                          GetStatusMessage="@GetPurchaseOrderStatusMessage"
                          ... />

@code {
    private GenericEditModalComponent<PurchaseOrder, IPurchaseOrderService>? editModalComponent;
    
    /// <summary>
    /// 判斷是否應該顯示審核區域
    /// </summary>
    private bool ShouldShowApprovalSection()
    {
        // 只有編輯現有採購單時才顯示審核區域
        return PurchaseOrderId.HasValue && PurchaseOrderId.Value > 0;
    }
    
    /// <summary>
    /// 取得採購單狀態訊息（整合審核狀態和其他訊息）
    /// </summary>
    private async Task<(string Message, GenericEditModalComponent<PurchaseOrder, IPurchaseOrderService>.BadgeVariant Variant, string IconClass)?> GetPurchaseOrderStatusMessage()
    {
        try
        {
            if (editModalComponent?.Entity == null)
                return null;
            
            var purchaseOrder = editModalComponent.Entity;
            
            // 只有已審核通過或已駁回時才顯示訊息
            if (purchaseOrder.IsApproved && purchaseOrder.ApprovedAt.HasValue)
            {
                var approverName = purchaseOrder.ApprovedByUser?.Name ?? "審核人員";
                return (
                    $"已於 {purchaseOrder.ApprovedAt.Value:yyyy/MM/dd HH:mm} 由 {approverName} 審核通過",
                    GenericEditModalComponent<PurchaseOrder, IPurchaseOrderService>.BadgeVariant.Success,
                    "fas fa-check-circle"
                );
            }
            else if (!string.IsNullOrWhiteSpace(purchaseOrder.RejectReason))
            {
                return (
                    $"已駁回：{purchaseOrder.RejectReason}",
                    GenericEditModalComponent<PurchaseOrder, IPurchaseOrderService>.BadgeVariant.Danger,
                    "fas fa-times-circle"
                );
            }
            
            // 待審核狀態不顯示訊息
            return null;
        }
        catch (Exception ex)
        {
            await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetPurchaseOrderStatusMessage), GetType());
            return null;
        }
    }
    
    /// <summary>
    /// 處理採購單審核通過
    /// </summary>
    private async Task<bool> HandlePurchaseOrderApprove()
    {
        // ... 審核邏輯
    }
    
    /// <summary>
    /// 處理採購單審核駁回
    /// </summary>
    private async Task<bool> HandlePurchaseOrderReject()
    {
        // ... 駁回邏輯
    }
}
```

---

## 📊 視覺效果對照表

### **審核狀態**

| 狀態 | 顯示效果 | 徽章顏色 | 圖示 |
|------|---------|---------|------|
| 待審核 | *(不顯示)* | - | - |
| 審核通過 | 🟢 已於 2025/10/15 14:30 由 張三 審核通過 | Success | fa-check-circle |
| 已駁回 | 🔴 已駁回：價格過高需重新議價 | Danger | fa-times-circle |

### **庫存警告**

| 庫存量 | 顯示效果 | 徽章顏色 | 圖示 |
|-------|---------|---------|------|
| >= 50 | *(不顯示)* | - | - |
| 10-49 | 🟡 庫存偏低，剩餘 30 件 | Warning | fa-box |
| < 10 | 🔴 庫存嚴重不足！目前僅剩 5 件 | Danger | fa-exclamation-triangle |

### **鎖定狀態**

| 狀態 | 顯示效果 | 徽章顏色 | 圖示 |
|------|---------|---------|------|
| 已鎖定 | 🟡 此單據已鎖定，無法修改 | Warning | fa-lock |
| 已解鎖 | *(不顯示)* | - | - |

---

## 🚨 注意事項

### **1. 類型安全**

```csharp
// ✅ 正確：使用完整命名空間
GenericEditModalComponent<PurchaseOrder, IPurchaseOrderService>.BadgeVariant.Success

// ❌ 錯誤：會找不到類型
BadgeVariant.Success  // 編譯錯誤
```

### **2. Null 返回**

```csharp
// ✅ 正確：不需要顯示時返回 null
if (!shouldShow) return null;

// ❌ 錯誤：會顯示空白徽章
return ("", BadgeVariant.Info, "");
```

### **3. 向下相容**

```csharp
// ✅ 舊的審核功能仍然可用
ShowApprovalSection="true"        // 仍然有效
ApprovalPermission="..."          // 仍然有效
OnApprove="..."                   // 仍然有效
OnReject="..."                    // 仍然有效

// ❌ 已移除的參數
GetApprovalStatus="..."           // 不再支援，請改用 GetStatusMessage
```

### **4. 非同步方法**

```csharp
// ✅ 正確：使用 async/await
private async Task<(...)> GetStatusMessage()
{
    var data = await LoadDataAsync();
    return (...);
}

// ⚠️ 注意：即使不需要 await，也要保持 async 簽名
private async Task<(...)> GetStatusMessage()
{
    // 沒有 await，但方法簽名仍須是 async Task
    return (...);
}
```

---

## 📝 相關文件

- [GenericEditModalComponent 使用說明](./README_GenericEditModalComponent.md)
- [審核功能實作指南](./Readme_AddApprove.md)
- [AutoComplete 使用說明](./README_AutoComplete_ReadOnly_Fix.md)

---

## 🔧 技術細節

### **內部實作**

```csharp
// 快取狀態訊息
private string? _cachedStatusMessage;
private BadgeVariant _cachedStatusVariant = BadgeVariant.Info;
private string _cachedStatusIcon = "fas fa-info-circle";

// 載入狀態訊息
private async Task LoadStatusMessageData()
{
    if (!ShouldShowStatusMessage() || Entity == null || Entity.Id <= 0)
        return;

    // 優先使用動態取得函式
    if (GetStatusMessage != null)
    {
        var result = await GetStatusMessage();
        if (result.HasValue)
        {
            _cachedStatusMessage = result.Value.Message;
            _cachedStatusVariant = result.Value.Variant;
            _cachedStatusIcon = result.Value.IconClass;
        }
    }
    // 其次使用靜態設定值
    else if (!string.IsNullOrEmpty(StatusMessage))
    {
        _cachedStatusMessage = StatusMessage;
        _cachedStatusVariant = StatusBadgeVariant;
        _cachedStatusIcon = StatusIconClass;
    }
}
```

### **顯示邏輯**

```razor
@if (ShouldShowStatusMessage() && !IsLoading && Entity != null && Entity.Id > 0 && !string.IsNullOrEmpty(_cachedStatusMessage))
{
    <span class="badge bg-@GetBadgeColorClass(_cachedStatusVariant) me-2">
        <i class="@_cachedStatusIcon me-1"></i>@_cachedStatusMessage
    </span>
}
```

---

## ✅ 檢查清單

完成實作時，請確認：

- [ ] 已將 `GetApprovalStatus` 改為 `GetStatusMessage`
- [ ] 返回類型為 `(string Message, BadgeVariant Variant, string IconClass)?`
- [ ] 使用語意化的徽章顏色（Success/Danger/Warning/Info）
- [ ] 選擇適當的圖示
- [ ] 不需要顯示時返回 `null`
- [ ] 測試審核通過/駁回的顯示效果
- [ ] 測試待審核時不顯示訊息
- [ ] 確認無編譯錯誤

---

## 📅 版本歷史

| 版本 | 日期 | 修改內容 |
|------|------|---------|
| v2.0 | 2025/10/15 | 重構為通用狀態訊息系統，支援多種顏色和圖示 |
| v1.0 | 2025/10/01 | 初始版本，僅支援審核狀態顯示 |

---

## 👨‍💻 維護資訊

- **負責人**: 開發團隊
- **最後更新**: 2025/10/15
- **相關元件**: `GenericEditModalComponent.razor`, `PurchaseOrderEditModalComponent.razor`
- **測試狀態**: ✅ 已測試通過

---

**📌 提示**: 如有任何問題或建議，請聯繫開發團隊或查閱相關文件。
