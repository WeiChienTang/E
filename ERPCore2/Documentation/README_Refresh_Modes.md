# 頁面刷新模式使用指南

## 概述

為了解決頁面刷新時的閃爍問題，現在提供了兩種刷新模式：

1. **平滑刷新 (Smooth)** - 預設模式，僅重新載入資料，避免頁面閃爍
2. **強制刷新 (ForceReload)** - 重新載入整個頁面，確保完全重置

## 刷新模式比較

| 特性 | 平滑刷新 (Smooth) | 強制刷新 (ForceReload) |
|------|------------------|----------------------|
| 頁面閃爍 | ✅ 無閃爍 | ❌ 短暫閃爍 |
| 載入速度 | ✅ 較快 | ❌ 較慢 |
| 資料一致性 | ✅ 確保最新資料 | ✅ 完全重置 |
| 使用者體驗 | ✅ 流暢 | ⚠️ 短暫中斷 |
| 記憶體清理 | ⚠️ 部分清理 | ✅ 完全清理 |

## 技術實作說明

### 關鍵修正 - 反射方法解析

在 `ModalHelper.cs` 中，最關鍵的修正是明確指定方法簽名：

```csharp
// 修正前 - 會因為方法重載而找不到正確的方法
var refreshMethod = indexComponent.GetType().GetMethod("Refresh");

// 修正後 - 明確指定無參數的 Refresh 方法
var refreshMethod = indexComponent.GetType().GetMethod("Refresh", new Type[0]);
```

**為什麼這個修正很重要？**

1. **方法重載問題**：`GenericIndexPageComponent` 有多個 `Refresh` 方法重載
2. **反射歧義**：`GetMethod("Refresh")` 在有多個重載時會返回 `null`
3. **明確指定**：`new Type[0]` 明確指定要無參數的 `Refresh()` 方法
4. **確保調用**：這確保了 Modal 儲存後能正確觸發頁面刷新
## 疑難排解

### Q: 平滑刷新後看不到新資料？
A: 這個問題已透過反射方法解析修正解決。如果仍有問題，請檢查：
1. 確認 DataLoader 方法沒有快取問題
2. 檢查資料庫事務是否已提交
3. 檢查瀏覽器 Console 是否有錯誤訊息

### Q: ModalHelper 調用 Refresh 失敗？
A: 確保使用正確的反射調用：
```csharp
var refreshMethod = indexComponent.GetType().GetMethod("Refresh", new Type[0]);
```
避免使用 `GetMethod("Refresh")` 因為會有方法重載歧義。

### Q: 如何確認我的資料確實已經保存到資料庫？
A: 
1. 在 Modal 關閉後直接查詢資料庫
2. 在另一個瀏覽器視窗重新載入頁面
3. 檢查服務層的 SaveChanges 調用

### Q: 什麼時候應該使用強制刷新？
A: 
- 涉及複雜的業務邏輯變更
- 需要確保所有快取和狀態完全重置
- 權限變更或使用者角色變更後
- 發生錯誤需要完全重新初始化時

## 最佳實務

### 1. 確認 DataLoader 方法實作
```csharp
private async Task<List<YourEntity>> LoadYourEntitiesAsync()
{
    try
    {
        // 確保這裡沒有使用快取，每次都從資料庫載入最新資料
        return await YourService.GetAllAsync();
    }
    catch (Exception ex)
    {
        // 適當的錯誤處理
        return new List<YourEntity>();
    }
}
```

### 2. 檢查服務層實作
確認服務的 `GetAllAsync()` 方法每次都從資料庫查詢最新資料：
```csharp
public async Task<List<YourEntity>> GetAllAsync()
{
    // 避免快取，確保每次都查詢最新資料
    return await _context.YourEntities
                        .AsNoTracking() // 避免快取
                        .ToListAsync();
}
```

### 3. 確保事務提交
```csharp
// 在服務層確保事務已提交
await context.SaveChangesAsync();
```

## 使用方式

### 1. 在 Index 頁面設定預設刷新模式

```html
<GenericIndexPageComponent TEntity="YourEntity" TService="IYourService"
                          Service="@YourService"
                          PageRefreshMode="GenericIndexPageComponent&lt;YourEntity, IYourService&gt;.RefreshMode.Smooth"
                          ... />
```

### 2. 程式碼中調用不同的刷新方法

```csharp
// 使用預設設定的刷新模式
await indexComponent.Refresh();

// 強制使用平滑刷新
await indexComponent.SmoothRefresh();

// 強制使用頁面重新載入
await indexComponent.ForceRefresh();

// 使用指定的刷新模式
await indexComponent.Refresh(GenericIndexPageComponent<YourEntity, IYourService>.RefreshMode.Smooth);
```

### 3. 在 ModalHelper 中的應用

Modal 儲存成功後會自動使用頁面設定的刷新模式：

```csharp
// ModalHelper.HandleEntitySavedAsync 會自動調用 indexComponent.Refresh()
// 使用頁面設定的 PageRefreshMode
```

## 建議使用場景

### 使用平滑刷新 (Smooth) 的情況：
- 一般的資料新增、編輯、刪除操作
- 需要保持使用者體驗流暢的場景
- 資料量較大，頁面載入時間較長的情況

### 使用強制刷新 (ForceReload) 的情況：
- 涉及複雜的業務邏輯變更
- 需要確保所有快取和狀態完全重置
- 發生錯誤後需要完全重新初始化
- 權限變更或使用者角色變更後
- 平滑刷新出現問題時的暫時解決方案

## 實作範例

### 在採購單頁面使用平滑刷新

```html
<GenericIndexPageComponent TEntity="PurchaseOrder" TService="IPurchaseOrderService"
                          Service="@PurchaseOrderService"
                          PageTitle="採購單維護"
                          PageRefreshMode="GenericIndexPageComponent&lt;PurchaseOrder, IPurchaseOrderService&gt;.RefreshMode.Smooth"
                          DataLoader="@LoadPurchaseOrdersAsync"
                          FilterApplier="@ApplyPurchaseOrderFilters"
                          OnAddClick="@modalHandler.ShowAddModalAsync"
                          OnRowClick="@modalHandler.ShowEditModalAsync"
                          @ref="indexComponent" />
```

### 自訂刷新邏輯

```csharp
private async Task HandleSpecialOperation()
{
    try
    {
        // 執行特殊操作
        await SomeComplexBusinessLogic();
        
        // 根據操作結果選擇刷新方式
        if (needsFullReset)
        {
            await indexComponent.ForceRefresh();
        }
        else
        {
            await indexComponent.SmoothRefresh();
        }
    }
    catch (Exception ex)
    {
        // 發生錯誤時強制重新載入頁面
        await indexComponent.ForceRefresh();
    }
}
```

## 常見問題排解

### Q: 平滑刷新後看不到新資料？
A: 這個問題已透過反射方法解析修正解決。如果仍有問題，請檢查：
1. 確認 DataLoader 方法沒有快取問題
2. 檢查資料庫事務是否已提交
3. 檢查瀏覽器 Console 是否有錯誤訊息

### Q: ModalHelper 調用 Refresh 失敗？
A: 確保使用正確的反射調用：
```csharp
var refreshMethod = indexComponent.GetType().GetMethod("Refresh", new Type[0]);
```
避免使用 `GetMethod("Refresh")` 因為會有方法重載歧義。

### Q: 如何確認我的資料確實已經保存到資料庫？
A: 
1. 在 Modal 關閉後直接查詢資料庫
2. 在另一個瀏覽器視窗重新載入頁面
3. 檢查服務層的 SaveChanges 調用

### Q: 什麼時候應該使用強制刷新？
A: 
- 涉及複雜的業務邏輯變更
- 需要確保所有快取和狀態完全重置
- 權限變更或使用者角色變更後
- 發生錯誤需要完全重新初始化時

## 注意事項

1. **預設模式**：新的預設模式是平滑刷新，提供更好的使用者體驗
2. **向後相容**：現有的程式碼會自動使用平滑刷新，無需修改
3. **反射修正**：確保 ModalHelper 使用 `GetMethod("Refresh", new Type[0])` 避免方法重載問題
4. **效能考量**：平滑刷新會重新載入所有資料，包括基礎資料和統計資料
5. **錯誤處理**：如果平滑刷新失敗，建議使用強制刷新作為後備方案

## 升級指引

如果你的專案目前使用舊版的強制刷新，可以透過以下步驟升級：

1. **保持現有行為**：設定 `PageRefreshMode="ForceReload"`
2. **逐步測試**：在個別頁面改為 `Smooth` 模式進行測試
3. **全面升級**：確認沒有問題後，移除 `PageRefreshMode` 參數使用預設的平滑刷新
4. **檢查 ModalHelper**：確保使用最新的反射調用方式

這樣可以在保持系統穩定的同時，逐步改善使用者體驗。
