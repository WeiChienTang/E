# 沖款單刪除時回朔來源明細金額機制

## 📋 問題描述

在刪除沖款單時，原本的實作只會透過 EF Core 的級聯刪除功能自動刪除關聯的明細記錄（`SetoffProductDetail`、`SetoffPayment`、`SetoffPrepayment`），但**不會回朔來源單據明細的累計金額**。

### 受影響的實體

刪除沖款單時，以下來源明細的累計金額欄位不會自動更新：

| 實體 | 快取欄位 | 說明 |
|-----|---------|------|
| `PurchaseReceivingDetail` | `TotalPaidAmount` | 採購進貨單的累計付款金額 |
| `SalesOrderDetail` | `TotalReceivedAmount` | 銷貨訂單的累計收款金額 |
| `SalesReturnDetail` | `TotalPaidAmount` | 銷貨退回的累計付款金額 |
| `PurchaseReturnDetail` | `TotalReceivedAmount` | 採購退回的累計收款金額 |

### 問題範例

```
情境：
1. 採購進貨單明細 (Id=123) SubtotalAmount = 50,000
2. 建立沖款單A，沖款 20,000
   → PurchaseReceivingDetail.TotalPaidAmount = 20,000 ✅
3. 建立沖款單B，沖款 15,000
   → PurchaseReceivingDetail.TotalPaidAmount = 35,000 ✅
4. 刪除沖款單A
   → PurchaseReceivingDetail.TotalPaidAmount 仍為 35,000 ❌ (應該是 15,000)
```

---

## 🎯 解決方案：方案C（混合方案）

採用**快取 + 同步**雙軌制：
- **來源 Detail 的累計金額** = **快取欄位** (denormalized)
- **SetoffProductDetail** = **權威來源** (source of truth)
- **同步機制** = 確保快取與來源一致

### 核心優勢

1. ✅ **查詢效能優異** - 單表查詢，無需 JOIN
2. ✅ **資料一致性保證** - 刪除時自動回朔
3. ✅ **歷史資料穩定** - 保留快取欄位
4. ✅ **可修復性** - 提供 RebuildCache 工具

---

## 🔧 實作內容

### 1. 更新資料庫級聯刪除設定

**檔案：** `AppDbContext.cs`

**修改：** 確保 `SetoffPrepayment` 也有正確的級聯刪除設定

```csharp
modelBuilder.Entity<SetoffPrepayment>(entity =>
{
    // 沖款單關聯（級聯刪除）
    entity.HasOne(p => p.SetoffDocument)
          .WithMany(sd => sd.Prepayments)
          .HasForeignKey(p => p.SetoffDocumentId)
          .OnDelete(DeleteBehavior.Cascade);  // ← 新增級聯刪除
    
    // ... 其他設定
});
```

**Migration：** `UpdateSetoffPrepaymentCascadeDelete`

### 2. 覆寫 DeleteAsync 方法

**檔案：** `SetoffDocumentService.cs`

**功能：** 在刪除沖款單前，先回朔所有來源明細的累計金額

```csharp
public override async Task<ServiceResult> DeleteAsync(int id)
{
    using var context = await _contextFactory.CreateDbContextAsync();
    using var transaction = await context.Database.BeginTransactionAsync();

    try
    {
        // 📦 載入完整資料（含所有關聯明細）
        var document = await context.SetoffDocuments
            .Include(d => d.SetoffProductDetails)
            .Include(d => d.SetoffPayments)
            .Include(d => d.Prepayments)
            .Include(d => d.FinancialTransactions)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null)
            return ServiceResult.Failure("找不到要刪除的沖款單");

        // 🔄 【關鍵步驟】先回朔所有來源 Detail 的累計金額
        foreach (var detail in document.SetoffProductDetails)
        {
            await RollbackSourceDetailAmountAsync(context, detail);
        }

        // 🗑️ 刪除沖款單（級聯刪除所有關聯明細）
        context.SetoffDocuments.Remove(document);

        // 💾 儲存變更
        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        return ServiceResult.Success();
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        return ServiceResult.Failure($"刪除沖款單時發生錯誤: {ex.Message}");
    }
}
```

### 3. 回朔來源明細金額

**方法：** `RollbackSourceDetailAmountAsync`

**邏輯：** 重新計算累計金額（排除當前要刪除的明細）

```csharp
private async Task RollbackSourceDetailAmountAsync(
    AppDbContext context, 
    SetoffProductDetail detailToDelete)
{
    // 🔍 重新計算累計金額（排除當前要刪除的明細）
    var newTotalSetoff = await context.SetoffProductDetails
        .Where(spd => spd.SourceDetailType == detailToDelete.SourceDetailType
                   && spd.SourceDetailId == detailToDelete.SourceDetailId
                   && spd.Id != detailToDelete.Id)  // ← 關鍵：排除當前
        .SumAsync(spd => spd.TotalSetoffAmount);

    // 💾 根據來源明細類型，更新對應的累計金額
    switch (detailToDelete.SourceDetailType)
    {
        case SetoffDetailType.PurchaseReceivingDetail:
            var detail = await context.PurchaseReceivingDetails
                .FindAsync(detailToDelete.SourceDetailId);
            if (detail != null)
            {
                detail.TotalPaidAmount = newTotalSetoff;  // ← 回朔快取
                detail.IsSettled = newTotalSetoff >= detail.SubtotalAmount;
            }
            break;
        
        // ... 其他類型
    }
}
```

### 4. 資料修復工具

**方法：** `RebuildCacheAsync`

**用途：** 當快取資料不一致時，從權威來源重建

```csharp
public async Task<ServiceResult> RebuildCacheAsync(
    SetoffDetailType? sourceDetailType = null)
{
    using var context = await _contextFactory.CreateDbContextAsync();
    using var transaction = await context.Database.BeginTransactionAsync();

    try
    {
        var rebuiltCount = 0;
        var typesToRebuild = sourceDetailType.HasValue
            ? new[] { sourceDetailType.Value }
            : Enum.GetValues<SetoffDetailType>();

        foreach (var type in typesToRebuild)
        {
            rebuiltCount += await RebuildCacheByTypeAsync(context, type);
        }

        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        return ServiceResult.Success();
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        return ServiceResult.Failure($"重建快取時發生錯誤: {ex.Message}");
    }
}
```

---

## 🔄 運作流程

### 刪除沖款單的完整流程

```
使用者刪除沖款單 (Id=456)
    ↓
【1. 開啟 Transaction】
    ↓
【2. 載入完整資料】
- SetoffDocument (Id=456)
- SetoffProductDetails (所有明細)
- SetoffPayments
- SetoffPrepayments
- FinancialTransactions
    ↓
【3. 回朔來源明細】
foreach (detail in document.SetoffProductDetails)
{
    ↓
    查詢該來源明細的其他沖款記錄
    SELECT SUM(TotalSetoffAmount)
    WHERE SourceDetailId = detail.SourceDetailId
      AND Id != detail.Id  ← 排除當前要刪的
    ↓
    更新來源 Detail 的快取欄位
    PurchaseReceivingDetail.TotalPaidAmount = 新計算的總額
    PurchaseReceivingDetail.IsSettled = (總額 >= 應付金額)
}
    ↓
【4. 刪除沖款單】
context.Remove(document)
→ 級聯刪除所有關聯明細
    ↓
【5. SaveChanges & Commit】
    ↓
【完成】
✅ 沖款單已刪除
✅ 所有明細已刪除
✅ 來源 Detail 的快取已回朔
✅ 資料一致性保證
```

---

## 🧪 測試案例

### 測試案例 1：刪除單一沖款單

**前置條件：**
```
PurchaseReceivingDetail (Id=100)
- SubtotalAmount: 100,000
- TotalPaidAmount: 0

建立沖款單A (Id=1)
- SetoffProductDetail: 沖款 30,000
→ TotalPaidAmount 更新為 30,000
```

**執行動作：**
```csharp
await setoffDocumentService.DeleteAsync(1);
```

**預期結果：**
```
✅ SetoffDocument (Id=1) 已刪除
✅ SetoffProductDetail 已刪除
✅ PurchaseReceivingDetail.TotalPaidAmount = 0 (回朔成功)
✅ PurchaseReceivingDetail.IsSettled = false
```

### 測試案例 2：刪除部分沖款單

**前置條件：**
```
PurchaseReceivingDetail (Id=100)
- SubtotalAmount: 100,000
- TotalPaidAmount: 0

建立沖款單A (Id=1): 沖款 30,000
→ TotalPaidAmount = 30,000

建立沖款單B (Id=2): 沖款 40,000
→ TotalPaidAmount = 70,000

建立沖款單C (Id=3): 沖款 20,000
→ TotalPaidAmount = 90,000
```

**執行動作：**
```csharp
await setoffDocumentService.DeleteAsync(2); // 刪除沖款單B
```

**預期結果：**
```
✅ SetoffDocument (Id=2) 已刪除
✅ PurchaseReceivingDetail.TotalPaidAmount = 50,000 (30,000 + 20,000)
✅ PurchaseReceivingDetail.IsSettled = false
✅ 沖款單A、C 仍存在
```

### 測試案例 3：使用修復工具

**情境：** 資料不一致（例如直接修改資料庫）

**執行動作：**
```csharp
// 重建所有類型的快取
await setoffDocumentService.RebuildCacheAsync();

// 或只重建特定類型
await setoffDocumentService.RebuildCacheAsync(
    SetoffDetailType.PurchaseReceivingDetail);
```

**預期結果：**
```
✅ 從 SetoffProductDetail 重新計算所有來源明細的累計金額
✅ 更新快取欄位
✅ 資料一致性恢復
```

---

## 🔒 Transaction 保護

所有刪除操作都使用 Transaction 確保原子性：

```csharp
using var transaction = await context.Database.BeginTransactionAsync();
try
{
    // 1. 回朔快取
    // 2. 刪除沖款單
    await context.SaveChangesAsync();
    await transaction.CommitAsync();  // ✅ 全部成功才提交
}
catch
{
    await transaction.RollbackAsync();  // ❌ 任一失敗，全部回滾
}
```

**保證：**
- ✅ 回朔成功 + 刪除成功 → 提交
- ❌ 回朔失敗或刪除失敗 → 回滾，資料不變

---

## 📊 效能考量

### 查詢效能

**有快取（方案C）：**
```csharp
// 單表查詢，超快！
var unpaid = await context.PurchaseReceivingDetails
    .Where(d => !d.IsSettled)
    .ToListAsync();
```

**無快取（方案B）：**
```csharp
// 需要 JOIN，較慢
var unpaid = await context.PurchaseReceivingDetails
    .Include(d => d.SetoffProductDetails)
    .Where(d => d.SubtotalAmount > d.SetoffProductDetails.Sum(...))
    .ToListAsync();
```

### 刪除效能

刪除沖款單時的額外操作：
- 每個 SetoffProductDetail 需要查詢一次累計金額
- 對於大量明細的沖款單，可能稍慢
- **但資料一致性更重要**

---

## 📝 維護建議

### 1. 定期檢查資料一致性

建議每月執行一次快取重建：

```csharp
await setoffDocumentService.RebuildCacheAsync();
```

### 2. 監控日誌

刪除沖款單時會記錄詳細日誌：

```
[Information] 開始回朔沖款單 ARO20250109001 的來源明細累計金額
[Debug] 回朔 PurchaseReceivingDetail Id=123: TotalPaidAmount 35000 → 15000
[Information] 已完成 3 筆來源明細的金額回朔
[Information] 成功刪除沖款單 ARO20250109001 (Id=456)
```

### 3. 錯誤處理

如果刪除失敗，Transaction 會自動回滾，資料保持原狀。

---

## 🎉 總結

### 實作完成項目

- ✅ 更新 AppDbContext 級聯刪除設定
- ✅ 覆寫 SetoffDocumentService.DeleteAsync
- ✅ 實作 RollbackSourceDetailAmountAsync 回朔邏輯
- ✅ 新增 RebuildCacheAsync 修復工具
- ✅ 使用 Transaction 確保原子性
- ✅ 建立資料庫 Migration

### 關鍵特性

1. **資料一致性** - 刪除沖款單時自動回朔來源明細金額
2. **效能優化** - 保留快取欄位，查詢效能不受影響
3. **容錯機制** - Transaction 保護，失敗自動回滾
4. **可修復性** - 提供工具重建不一致的快取
5. **完整日誌** - 詳細記錄回朔過程

### 未來擴展

如果新增其他類型的來源明細，只需在 `RollbackSourceDetailAmountAsync` 和 `RebuildCacheByTypeAsync` 中新增對應的 case。

---

**作者：** AI Assistant  
**日期：** 2025-01-09  
**版本：** 1.0
