# AddressManagerComponent 刪除功能修復紀錄

## 📋 概述

本文件記錄了 `AddressManagerComponent` 刪除功能的修復過程，解決了「在明細上刪除後，資料表並沒有真的刪除」的問題。此修復模式可應用於其他類似的子集合管理組件。

## 🔍 問題分析

### 原始問題
- **現象**：用戶在地址管理介面點擊刪除按鈕後，項目從表格中消失，但資料庫中的記錄依然存在
- **原因**：`AddressManagerComponent` 的 `RemoveItemAsync` 方法只處理前端 `Items` 列表的移除，沒有通知父組件進行資料庫刪除
- **影響**：造成資料不一致性，重新載入頁面後「已刪除」的項目又會重新出現

### 技術根因
```csharp
// 原始的 RemoveItemAsync 方法（問題版本）
public async Task RemoveItemAsync(int index)
{
    if (IsReadOnly || index < 0 || index >= Items.Count) return;
    
    var removedItem = Items[index];
    
    // ❌ 只處理前端移除，沒有資料庫刪除
    AutoEmptyRowHelper.For<TAddressEntity>.HandleItemRemove(
        Items, removedItem, IsEmptyRow, CreateEmptyItem, SetParentId, ParentEntityId);
    
    await ItemRemoved.InvokeAsync(removedItem);
    await ItemsChanged.InvokeAsync(Items);
    StateHasChanged();
}
```

---

## 🛠️ 解決方案

### 修復策略
採用與其他明細管理組件（如 `SalesOrderDetailManagerComponent`、`PurchaseReturnDetailManagerComponent`）相同的刪除處理模式：

1. **追蹤刪除ID**：記錄需要從資料庫刪除的實體ID
2. **事件通知**：通知父組件處理實際的資料庫刪除
3. **批次處理**：支援批次刪除以提高效能
4. **錯誤處理**：提供完整的錯誤處理和用戶反饋

---

## 📝 修改步驟

### 步驟 1：修改子組件 (`AddressManagerComponent.razor`)

#### 1.1 添加刪除追蹤欄位
```csharp
@code {
    // ===== 私有欄位 =====
    private readonly HashSet<int> _deletedDetailIds = new HashSet<int>();

    // 其他現有代碼...
}
```

#### 1.2 添加刪除通知事件參數
```csharp
// ===== 事件參數 =====
[Parameter] public EventCallback<List<TAddressEntity>> ItemsChanged { get; set; }
[Parameter] public EventCallback<TAddressEntity> ItemAdded { get; set; }
[Parameter] public EventCallback<TAddressEntity> ItemRemoved { get; set; }
[Parameter] public EventCallback<List<int>> OnDeletedDetailsChanged { get; set; } // 🆕 新增
```

#### 1.3 修改刪除方法
```csharp
public async Task RemoveItemAsync(int index)
{
    if (IsReadOnly || index < 0 || index >= Items.Count) return;
    
    var removedItem = Items[index];
    if (removedItem == null) return;
    
    // 🆕 記錄要刪除的資料庫實體ID
    if (removedItem.Id > 0)
    {
        _deletedDetailIds.Add(removedItem.Id);
    }
    
    // 使用 Helper 處理移除，自動確保空行
    AutoEmptyRowHelper.For<TAddressEntity>.HandleItemRemove(
        Items, removedItem, IsEmptyRow, CreateEmptyItem, SetParentId, ParentEntityId);
    
    await ItemRemoved.InvokeAsync(removedItem);
    await ItemsChanged.InvokeAsync(Items);
    
    // 🆕 通知已刪除的明細ID
    await NotifyDeletedDetailsChanged();
    
    StateHasChanged();
}
```

#### 1.4 添加通知方法
```csharp
/// <summary>
/// 通知已刪除的明細ID
/// </summary>
private async Task NotifyDeletedDetailsChanged()
{
    if (OnDeletedDetailsChanged.HasDelegate && _deletedDetailIds.Any())
    {
        await OnDeletedDetailsChanged.InvokeAsync(_deletedDetailIds.ToList());
        _deletedDetailIds.Clear(); // 清空已通知的刪除ID
    }
}
```

### 步驟 2：修改父組件

以 `CustomerEditModalComponent.razor` 為例：

#### 2.1 添加刪除事件處理
```razor
<AddressManagerComponent TAddressEntity="Address" 
                        TParentEntity="Customer"
                        Items="@customerAddresses"
                        Options="@addressTypeOptions"
                        ParentEntityId="@(CustomerId ?? editModalComponent?.Entity?.Id ?? 0)"
                        Title="地址資訊"
                        ItemDisplayName="地址"
                        TypeDisplayName="地址類型"
                        GetTypeId="@(address => address.AddressTypeId)"
                        GetAddress="@(address => address.AddressLine)"
                        GetOptionId="@(option => option.Id)"
                        GetOptionDisplayText="@(option => ((AddressType)option).TypeName)"
                        SetTypeId="@((address, typeId) => address.AddressTypeId = typeId)"
                        SetAddress="@((address, value) => address.AddressLine = value)"
                        SetParentId="@((address, parentId) => { address.OwnerType = AddressOwnerTypes.Customer; address.OwnerId = parentId; })"
                        ItemsChanged="@OnAddressesChanged"
                        OnDeletedDetailsChanged="@HandleDeletedAddressesChanged" />
```

#### 2.2 實作刪除處理方法
```csharp
/// <summary>
/// 處理已刪除的地址ID - 實際從資料庫刪除
/// </summary>
private async Task HandleDeletedAddressesChanged(List<int> deletedAddressIds)
{
    try
    {
        if (deletedAddressIds?.Any() == true)
        {
            foreach (var addressId in deletedAddressIds)
            {
                await AddressService.DeleteAddressAsync(addressId);
            }
            
            // 顯示成功通知
            await NotificationService.ShowSuccessAsync($"已刪除 {deletedAddressIds.Count} 個地址");
        }
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"刪除地址時發生錯誤：{ex.Message}");
    }
}
```

---

## 🔧 技術要點

### 1. 刪除狀態追蹤
```csharp
// 使用 HashSet 避免重複ID
private readonly HashSet<int> _deletedDetailIds = new HashSet<int>();

// 只記錄真正存在於資料庫的記錄（ID > 0）
if (removedItem.Id > 0)
{
    _deletedDetailIds.Add(removedItem.Id);
}
```

### 2. 事件參數設計
```csharp
// 支援批次刪除ID傳遞
[Parameter] public EventCallback<List<int>> OnDeletedDetailsChanged { get; set; }

// 支援單個項目傳遞（含完整實體資訊）
[Parameter] public EventCallback<TAddressEntity> ItemRemoved { get; set; }
```

### 3. 通知時機
```csharp
// 在每次刪除操作後立即通知
await NotifyDeletedDetailsChanged();

// 通知後清空追蹤列表避免重複處理
_deletedDetailIds.Clear();
```

### 4. 錯誤處理
```csharp
try
{
    foreach (var addressId in deletedAddressIds)
    {
        await AddressService.DeleteAddressAsync(addressId);
    }
    await NotificationService.ShowSuccessAsync($"已刪除 {deletedAddressIds.Count} 個地址");
}
catch (Exception ex)
{
    await NotificationService.ShowErrorAsync($"刪除地址時發生錯誤：{ex.Message}");
}
```

---

## 📋 應用清單

本次修復已應用於以下組件：

### ✅ 已修復的組件
- **AddressManagerComponent.razor** - 地址管理組件（核心修改）
- **CustomerEditModalComponent.razor** - 客戶編輯組件
- **SupplierEditModalComponent.razor** - 廠商編輯組件  
- **EmployeeEditModalComponent.razor** - 員工編輯組件

### 🔄 可參考的類似組件
以下組件已使用類似的刪除處理模式：
- **SalesOrderDetailManagerComponent.razor** - 銷貨訂單明細管理
- **PurchaseReturnDetailManagerComponent.razor** - 進貨退回明細管理
- **PurchaseReceivingDetailManagerComponent.razor** - 進貨驗收明細管理

---

## 🚀 套用到其他組件的步驟

當需要修復其他子集合管理組件的刪除問題時，按照以下步驟：

### 1. 檢查組件是否有刪除問題
```csharp
// 檢查 RemoveItemAsync 或類似方法是否只處理前端移除
// 尋找是否缺少資料庫刪除邏輯
```

### 2. 修改子組件
```csharp
// a. 添加 _deletedDetailIds 追蹤欄位
// b. 添加 OnDeletedDetailsChanged 事件參數
// c. 修改刪除方法記錄ID
// d. 添加通知方法
```

### 3. 修改父組件
```csharp
// a. 在組件標籤添加 OnDeletedDetailsChanged 事件處理
// b. 實作刪除處理方法調用相應的服務
// c. 添加錯誤處理和用戶反饋
```

### 4. 測試驗證
```csharp
// a. 測試前端刪除功能
// b. 檢查資料庫是否真正刪除
// c. 測試錯誤情況處理
// d. 驗證用戶反饋訊息
```

---

## 🔍 檢查清單

修改完成後，請確認以下項目：

- [ ] **前端功能**：項目能從UI中正確移除
- [ ] **資料庫刪除**：記錄能從資料庫中真正刪除
- [ ] **錯誤處理**：刪除失敗時顯示適當錯誤訊息
- [ ] **成功反饋**：刪除成功時顯示確認訊息
- [ ] **重新載入測試**：重新載入頁面後確認項目不會重新出現
- [ ] **編譯測試**：確保所有修改都能正常編譯
- [ ] **空行處理**：確保自動空行功能正常運作

---

## 📚 相關文件

- **AutoEmptyRowHelper**：自動空行處理功能說明
- **InteractiveTableComponent**：互動式表格組件文件
- **GenericManagementService**：通用服務刪除方法說明
- **ErrorHandlingHelper**：錯誤處理輔助工具

---

## 🏷️ 標籤

`刪除修復` `子集合管理` `資料一致性` `AddressManager` `明細管理` `資料庫同步`

---

**修復完成**：此修復確保了前端操作與後端資料的一致性，解決了刪除功能的核心問題。其他組件可參考此模式進行類似的修復。