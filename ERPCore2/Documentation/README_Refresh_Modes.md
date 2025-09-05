# 頁面刷新模式使用指南

## 概述

為了解決頁面刷新時的問題，系統提供了：

### 刷新模式 (兩種模式)
1. **平滑刷新 (Smooth)** - 預設模式，僅重新載入資料，避免頁面閃爍
2. **強制刷新 (ForceReload)** - 重新載入整個頁面，確保完全重置

### 刷新觸發機制 (兩種觸發方式)
1. **標準機制**：使用 `modalHandler.OnEntitySavedAsync` (大部分頁面使用此方式)
2. **雙重保險機制**：自定義事件處理器 + 手動刷新 (特殊情況或有問題時使用)

### 目前頁面使用狀況
- **DepartmentIndex**：標準機制 + 預設平滑刷新模式
- **PurchaseOrderIndex**：標準機制 + 預設平滑刷新模式  
- **InventoryStockIndex**：雙重保險機制 + 預設平滑刷新模式 (因為之前有刷新問題)

## 刷新模式比較

| 特性 | 平滑刷新 (Smooth) | 強制刷新 (ForceReload) |
|------|------------------|----------------------|
| 頁面閃爍 | ✅ 無閃爍 | ❌ 短暫閃爍 |
| 載入速度 | ✅ 較快 | ❌ 較慢 |
| 資料一致性 | ✅ 確保最新資料 | ✅ 完全重置 |
| 使用者體驗 | ✅ 流暢 | ⚠️ 短暫中斷 |
| 記憶體清理 | ⚠️ 部分清理 | ✅ 完全清理 |

## 技術實作說明

### 關鍵修正 1 - 反射方法解析

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

### 關鍵修正 2 - 導航屬性 Null Reference 問題

在 `FieldConfiguration` 類別中，避免在查詢排序中使用導航屬性：

```csharp
// 修正前 - 會導致 Null Reference 錯誤
public override IQueryable<InventoryStock> GetDefaultSort(IQueryable<InventoryStock> query)
{
    return query.OrderBy(s => s.Product!.Code)  // 可能為 null
                .ThenBy(s => s.Warehouse!.Code); // 可能為 null
}

// 修正後 - 使用外鍵進行排序
public override IQueryable<InventoryStock> GetDefaultSort(IQueryable<InventoryStock> query)
{
    return query.OrderBy(s => s.ProductId)      // 外鍵不會為 null
                .ThenBy(s => s.WarehouseId);    // 外鍵不會為 null
}
```

**為什麼這個修正很重要？**

1. **Null Reference 安全**：外鍵欄位不會為 null，避免運行時錯誤
2. **Entity Framework 相容**：使用外鍵比導航屬性更容易轉換為 SQL
3. **效能優化**：直接使用外鍵進行排序比載入導航屬性更高效
4. **查詢穩定性**：避免因為 Include 語句不完整而導致的查詢錯誤

### 增強的錯誤記錄

新增了詳細的日誌記錄以便偵錯：

```csharp
// 在 ModalHelper.cs 中增加詳細日誌
Console.WriteLine($"[ModalHelper] 成功調用 {componentType.Name}.Refresh() 方法");
Console.Error.WriteLine($"[ModalHelper] 調用失敗: {ex.Message}");
```

這讓我們能夠：
1. **追蹤執行流程**：確認方法是否被正確調用
2. **識別錯誤來源**：快速定位問題所在
3. **監控系統狀態**：了解刷新機制是否正常運作
## 疑難排解

### Q: Modal 關閉後看不到新資料？
A: 這個問題已透過以下修正解決：
1. **反射方法解析修正**：確保 `Refresh()` 方法被正確調用
2. **導航屬性修正**：避免查詢中的 Null Reference 錯誤
3. **雙重保險機制**：如果標準流程失敗，會自動使用備用刷新機制

### Q: 出現 "Object reference not set to an instance of an object" 錯誤？
A: 這通常是因為查詢中使用了可能為 null 的導航屬性：
```csharp
// 問題代碼
query.OrderBy(s => s.Product!.Code)  // Product 可能為 null

// 解決方案
query.OrderBy(s => s.ProductId)      // 使用外鍵替代
```

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
4. 查看 Console 日誌確認刷新流程

### Q: 什麼時候應該使用強制刷新？
A: 
- 涉及複雜的業務邏輯變更
- 需要確保所有快取和狀態完全重置
- 權限變更或使用者角色變更後
- 發生錯誤需要完全重新初始化時
- 平滑刷新出現 Null Reference 錯誤時

## 最佳實務

### 1. FieldConfiguration 中避免使用導航屬性
```csharp
// ❌ 避免在查詢中使用導航屬性
public override IQueryable<YourEntity> GetDefaultSort(IQueryable<YourEntity> query)
{
    return query.OrderBy(e => e.RelatedEntity!.Name); // 可能導致 Null Reference
}

// ✅ 使用外鍵進行排序
public override IQueryable<YourEntity> GetDefaultSort(IQueryable<YourEntity> query)
{
    return query.OrderBy(e => e.RelatedEntityId); // 安全且高效
}
```

### 2. 確認 DataLoader 方法實作
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
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(LoadYourEntitiesAsync), GetType());
        return new List<YourEntity>();
    }
}
```

### 3. 檢查服務層實作
確認服務的 `GetAllAsync()` 方法每次都從資料庫查詢最新資料：
```csharp
public async Task<List<YourEntity>> GetAllAsync()
{
    return await _context.YourEntities
                        .Include(e => e.RelatedEntity) // 必要時包含導航屬性
                        .AsNoTracking() // 避免快取
                        .OrderBy(e => e.Id) // 使用安全的排序
                        .ToListAsync();
}
```

### 4. 確保事務提交
```csharp
// 在服務層確保事務已提交
await context.SaveChangesAsync();
```

### 5. 實作雙重保險的 Modal 處理
```csharp
private async Task OnEntitySavedHandler(YourEntity savedEntity)
{
    try
    {
        // 先使用標準的 modalHandler 處理
        await modalHandler.OnEntitySavedAsync(savedEntity);
        
        // 如果需要額外確保刷新，可以加上手動刷新
        if (indexComponent != null)
        {
            await indexComponent.Refresh();
        }
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"處理儲存事件時發生錯誤：{ex.Message}");
    }
}
```

## 使用方式

### 1. 標準機制 (推薦使用)

大部分頁面使用此方式，簡單且可靠：

```html
<!-- 在 Index 頁面中 -->
<GenericIndexPageComponent TEntity="YourEntity" TService="IYourService"
                          Service="@YourService"
                          OnAddClick="@modalHandler.ShowAddModalAsync"
                          OnRowClick="@modalHandler.ShowEditModalAsync"
                          @ref="indexComponent" />

<!-- 在 Modal 組件中 -->
<YourEditModalComponent IsVisible="@showEditModal"
                       OnYourEntitySaved="@modalHandler.OnEntitySavedAsync"
                       OnCancel="@modalHandler.OnModalCancelAsync" />
```

### 2. 設定刷新模式 (可選)

如果需要明確指定刷新模式：

```html
<GenericIndexPageComponent TEntity="YourEntity" TService="IYourService"
                          Service="@YourService"
                          PageRefreshMode="GenericIndexPageComponent&lt;YourEntity, IYourService&gt;.RefreshMode.Smooth"
                          ... />
```

### 3. 雙重保險機制 (問題頁面使用)

當標準機制有問題時，使用此方式：

```csharp
// 在 Index 頁面中自定義事件處理器
private async Task OnEntitySavedHandler(YourEntity savedEntity)
{
    try
    {
        // 先使用標準處理
        await modalHandler.OnEntitySavedAsync(savedEntity);
        
        // 額外手動刷新作為保險
        if (indexComponent != null)
        {
            await indexComponent.Refresh();
        }
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"處理儲存事件時發生錯誤：{ex.Message}");
    }
}
```

然後在 Modal 中使用自定義處理器：
```html
<YourEditModalComponent OnYourEntitySaved="@OnEntitySavedHandler" />
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

### 3. Modal 刷新的自動處理

**標準機制**：Modal 儲存成功後會自動使用頁面設定的刷新模式
```csharp
// ModalHelper.HandleEntitySavedAsync 會自動調用 indexComponent.Refresh()
// 使用頁面設定的 PageRefreshMode (預設為 Smooth)
```

**雙重保險機制**：額外確保刷新成功
```csharp
// 先執行標準流程，再執行手動刷新
await modalHandler.OnEntitySavedAsync(savedEntity);
await indexComponent.Refresh(); // 額外保險
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

### Q: Modal 關閉後看不到新資料？
A: 這個問題已透過以下修正解決：
1. **反射方法解析修正**：確保 `Refresh()` 方法被正確調用
2. **導航屬性修正**：避免查詢中的 Null Reference 錯誤
3. **雙重保險機制**：如果標準流程失敗，會自動使用備用刷新機制

### Q: 出現 "Object reference not set to an instance of an object" 錯誤？
A: 這通常是因為查詢中使用了可能為 null 的導航屬性：
```csharp
// 問題代碼
query.OrderBy(s => s.Product!.Code)  // Product 可能為 null

// 解決方案
query.OrderBy(s => s.ProductId)      // 使用外鍵替代
```

### Q: ModalHelper 調用 Refresh 失敗？
A: 確保使用正確的反射調用：
```csharp
var refreshMethod = indexComponent.GetType().GetMethod("Refresh", new Type[0]);
```
避免使用 `GetMethod("Refresh")` 因為會有方法重載歧義。

### Q: 如何確認刷新機制是否正常運作？
A: 查看 Console 日誌，應該看到類似訊息：
```
[ModalHelper] 成功調用 GenericIndexPageComponent`2.Refresh() 方法
[YourPageIndex] 手動調用 indexComponent.Refresh()
[YourPageIndex] 手動刷新完成
```

### Q: 如何確認我的資料確實已經保存到資料庫？
A: 
1. 在 Modal 關閉後直接查詢資料庫
2. 在另一個瀏覽器視窗重新載入頁面
3. 檢查服務層的 SaveChanges 調用
4. 查看 Console 日誌確認刷新流程

### Q: 什麼時候應該使用強制刷新？
A: 
- 涉及複雜的業務邏輯變更
- 需要確保所有快取和狀態完全重置
- 權限變更或使用者角色變更後
- 發生錯誤需要完全重新初始化時
- 平滑刷新出現 Null Reference 錯誤時

## 注意事項

1. **預設模式**：新的預設模式是平滑刷新，提供更好的使用者體驗
2. **向後相容**：現有的程式碼會自動使用平滑刷新，無需修改
3. **反射修正**：確保 ModalHelper 使用 `GetMethod("Refresh", new Type[0])` 避免方法重載問題
4. **導航屬性安全**：在 FieldConfiguration 中避免使用導航屬性進行排序，改用外鍵
5. **錯誤處理**：如果平滑刷新失敗，建議使用強制刷新作為後備方案
6. **效能考量**：平滑刷新會重新載入所有資料，包括基礎資料和統計資料
7. **日誌監控**：增強的日誌記錄可以幫助快速定位刷新相關問題

## 升級指引

如果你的專案目前使用舊版的強制刷新，可以透過以下步驟升級：

### 階段 1：修正導航屬性問題
```csharp
// 檢查所有 FieldConfiguration 類別中的 GetDefaultSort 方法
// 將導航屬性改為外鍵
public override IQueryable<YourEntity> GetDefaultSort(IQueryable<YourEntity> query)
{
    // 修正前
    // return query.OrderBy(e => e.RelatedEntity!.Name);
    
    // 修正後
    return query.OrderBy(e => e.RelatedEntityId);
}
```

### 階段 2：更新 ModalHelper 調用
```csharp
// 確保使用正確的反射方法
var refreshMethod = indexComponent.GetType().GetMethod("Refresh", new Type[0]);
```

### 階段 3：測試刷新機制
1. **保持現有行為**：設定 `PageRefreshMode="ForceReload"`
2. **逐步測試**：在個別頁面改為 `Smooth` 模式進行測試
3. **檢查日誌**：確認 Console 中出現成功調用的訊息
4. **全面升級**：確認沒有問題後，移除 `PageRefreshMode` 參數使用預設的平滑刷新

### 階段 4：實作雙重保險（可選）
對於重要的頁面，可以實作雙重保險機制：
```csharp
// 在 Index 頁面中實作自訂的事件處理器
private async Task OnEntitySavedHandler(YourEntity savedEntity)
{
    try
    {
        await modalHandler.OnEntitySavedAsync(savedEntity);
        
        // 額外的手動刷新作為保險
        if (indexComponent != null)
        {
            await indexComponent.Refresh();
        }
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"處理儲存事件時發生錯誤：{ex.Message}");
    }
}
```

這樣可以在保持系統穩定的同時，逐步改善使用者體驗，並確保所有刷新機制都能正常運作。
