# SetoffPrepaymentUsage 重構完成報告

## ✅ 已完成項目（全部完成）

### 1. 資料庫層
- ✅ 建立 `SetoffPrepaymentUsage` Entity
  - 包含預收付款項ID、沖款單ID、使用金額、使用日期
  - 新增計算屬性：沖款單號、來源單號、剩餘可用金額
  - 外鍵設定為 `NO ACTION` 避免級聯刪除循環
  
- ✅ 修改 `SetoffPrepayment` Entity
  - 將 `SourceDocumentCode` 索引改為唯一索引
  - `SetoffDocumentId` 註解更新為「建立沖款單」
  - 新增 `UsageRecords` 集合導航屬性
  
- ✅ 修改 `SetoffDocument` Entity
  - 新增 `PrepaymentUsages` 集合導航屬性
  
- ✅ 在 `AppDbContext` 註冊 `SetoffPrepaymentUsages` DbSet
- ✅ 建立並執行 Migration: `AddSetoffPrepaymentUsageTable`
- ✅ 資料庫已成功更新

### 2. 服務層
- ✅ 建立 `ISetoffPrepaymentUsageService` 介面
  - GetByPrepaymentIdAsync：查詢某預收付款項的所有使用記錄
  - GetBySetoffDocumentIdAsync：查詢某沖款單的所有使用記錄
  - GetTotalUsedAmountAsync：計算總使用金額
  - ValidateUsageAmountAsync：驗證使用金額是否超過可用餘額
  - DeleteBySetoffDocumentIdAsync：刪除沖款單的所有使用記錄
  
- ✅ 建立 `SetoffPrepaymentUsageService` 實作
  - 自動更新預收付款項的 `UsedAmount`（建立/刪除時）
  - 完整的驗證邏輯
  - 實作 `SearchAsync` 方法支援搜尋功能
  
- ✅ 修改 `SetoffPrepaymentService` 驗證邏輯
  - 簡化為只驗證「預收/預付」主記錄（Amount > 0）
  - 移除轉沖款相關的驗證邏輯（由 SetoffPrepaymentUsageService 負責）
  
- ✅ 在 `ServiceRegistration.cs` 註冊新服務

### 3. 前端組件修改

#### 3.1 SetoffPrepaymentDetailManagerComponent.razor
- ✅ 注入 `ISetoffPrepaymentUsageService`
- ✅ 保持現有的 `NotifyPrepaymentsChanged()` 方法
  - 前端仍使用 `SetoffPrepayment` 作為 ViewModel
  - 父組件負責區分預收/預付和轉沖款的處理

#### 3.2 SetoffDocumentEditModalComponent.razor
- ✅ 注入 `ISetoffPrepaymentUsageService`
- ✅ 修改 `HandleSaveSuccess()` 方法：
  - **預收/預付（Amount > 0）**：調用 `SetoffPrepaymentService.CreateAsync()`
  - **轉沖款（UsedAmount > 0 且 Amount = 0）**：調用 `SetoffPrepaymentUsageService.CreateAsync()`
  - 從 `SourceDocumentCode` 查詢原始預收付款項並建立使用記錄
  
- ✅ 修改 `ValidateBeforeSave()` 方法：
  - 區分兩種類型的驗證邏輯
  - **預收/預付**：使用 `SetoffPrepaymentService.ValidateAsync()`
  - **轉沖款**：使用 `SetoffPrepaymentUsageService.ValidateUsageAmountAsync()`
  
- ✅ 修改 `LoadSetoffDocumentData()` 方法：
  - 編輯模式時同時載入預收付款項主記錄和使用記錄
  - 將使用記錄轉換為 `SetoffPrepayment` 格式供前端組件統一處理
  - **預收/預付記錄**：`Amount > 0, UsedAmount = 0`
  - **轉沖款記錄**：`Amount = 0, UsedAmount > 0`

---

## 🎉 重構成功！編譯通過

## 🔄 完整資料流程

### 情境 A：建立預收/預付款項
```
使用者輸入 → SetoffPrepaymentItem (ViewModel)
          ↓
NotifyPrepaymentsChanged()
          ↓
建立 SetoffPrepayment 物件 (Amount > 0, UsedAmount = 0)
          ↓
ValidateBeforeSave() → SetoffPrepaymentService.ValidateAsync()
          ↓
HandleSaveSuccess() → SetoffPrepaymentService.CreateAsync()
          ↓
資料庫：SetoffPrepayments 表新增記錄
```

### 情境 B：使用預收/預付款項（轉沖款）
```
使用者選擇既有預收付 → SetoffPrepaymentItem (ViewModel)
                    ↓
NotifyPrepaymentsChanged()
                    ↓
建立 SetoffPrepayment 物件 (Amount = 0, UsedAmount > 0)
                    ↓
ValidateBeforeSave() → SetoffPrepaymentUsageService.ValidateUsageAmountAsync()
                    ↓
HandleSaveSuccess():
  1. 從 SourceDocumentCode 查詢原始 SetoffPrepayment
  2. 建立 SetoffPrepaymentUsage 記錄
  3. SetoffPrepaymentUsageService.CreateAsync()
                    ↓
資料庫：
  - SetoffPrepaymentUsages 表新增使用記錄
  - SetoffPrepayments.UsedAmount 自動更新（Service 層處理）
```

### 編輯模式載入流程
```
LoadSetoffDocumentData():
  1. 載入 SetoffPrepayment 主記錄 (Amount > 0)
  2. 載入 SetoffPrepaymentUsage 使用記錄
  3. 轉換 Usage → SetoffPrepayment 格式:
     - Amount = 0
     - UsedAmount = usage.UsedAmount
     - SourceDocumentCode = usage.SetoffPrepayment.SourceDocumentCode
                    ↓
合併為統一的 List<SetoffPrepayment> 供前端顯示
```

---

## 📊 資料庫關聯

```
SetoffDocument (沖款單)
    ├─ Prepayments (1:N) → SetoffPrepayment (建立的預收付款項)
    └─ PrepaymentUsages (1:N) → SetoffPrepaymentUsage (使用的預收付款項)

SetoffPrepayment (預收付款項主檔)
    ├─ SetoffDocument (N:1) → 建立此預收付款項的沖款單
    └─ UsageRecords (1:N) → SetoffPrepaymentUsage (使用記錄)

SetoffPrepaymentUsage (預收付款項使用記錄)
    ├─ SetoffPrepayment (N:1) → 被使用的預收付款項
    └─ SetoffDocument (N:1) → 使用預收付款項的沖款單
```

---

## ⚠️ 重要注意事項

1. **PrepaymentType 類型說明**
   - 「預收」和「預付」：建立 SetoffPrepayment 主記錄（Amount > 0）
   - 「預收轉沖款」和「預付轉沖款」：建立 SetoffPrepaymentUsage 使用記錄（UsedAmount > 0）

2. **UsedAmount 自動計算**
   - `SetoffPrepayment.UsedAmount` 會在建立/刪除 `SetoffPrepaymentUsage` 時自動更新
   - 由 `SetoffPrepaymentUsageService` 的 `UpdatePrepaymentUsedAmountAsync()` 方法處理
   - **請勿手動修改 UsedAmount**，可能導致資料不一致

3. **刪除沖款單的影響**
   - 刪除沖款單時會自動刪除相關的 `SetoffPrepaymentUsage` 記錄
   - `SetoffPrepayment.UsedAmount` 會自動回滾
   - 外鍵設定為 `Cascade` (SetoffDocument → SetoffPrepaymentUsage)

4. **驗證邏輯分離**
   - `SetoffPrepaymentService.ValidateAsync()`：驗證預收/預付主記錄
     - 檢查 Amount > 0
     - 檢查 SourceDocumentCode 唯一性
   - `SetoffPrepaymentUsageService.ValidateUsageAmountAsync()`：驗證轉沖款使用記錄
     - 檢查 UsedAmount > 0
     - 檢查使用金額不超過可用餘額

5. **級聯刪除設定**
   - `SetoffDocument` → `SetoffPrepaymentUsage`: **CASCADE**（刪除沖款單時自動刪除使用記錄）
   - `SetoffPrepayment` → `SetoffPrepaymentUsage`: **NO ACTION**（避免循環參照）
   - 刪除預收付款項時，需先確認沒有使用記錄

6. **資料一致性保證**
   - 前端使用 `Amount` 和 `UsedAmount` 區分兩種類型
   - 後端根據值的大小自動路由到不同的 Service
   - 中間表 `SetoffPrepaymentUsage` 完整記錄審計追蹤

---

## 📝 測試檢查清單

### 情境 A：預收/預付款項測試
- [ ] 建立新的預收款項（應收帳款）
- [ ] 建立新的預付款項（應付帳款）
- [ ] 驗證 `SetoffPrepayments` 表有新增記錄
- [ ] 驗證 `Amount > 0` 且 `UsedAmount = 0`
- [ ] 驗證 `SourceDocumentCode` 唯一性

### 情境 B：轉沖款測試
- [ ] 選擇既有預收款項進行轉沖
- [ ] 驗證可用餘額顯示正確
- [ ] 輸入使用金額（不超過可用餘額）
- [ ] 儲存後驗證 `SetoffPrepaymentUsages` 表有新增記錄
- [ ] 驗證原始 `SetoffPrepayment.UsedAmount` 自動更新
- [ ] 驗證可用餘額減少

### 編輯模式測試
- [ ] 編輯包含預收/預付的沖款單
- [ ] 編輯包含轉沖款的沖款單
- [ ] 編輯混合兩種類型的沖款單
- [ ] 驗證資料正確顯示
- [ ] 修改後儲存驗證資料正確更新

### 刪除測試
- [ ] 刪除包含轉沖款的沖款單
- [ ] 驗證 `SetoffPrepaymentUsages` 記錄被刪除
- [ ] 驗證原始 `SetoffPrepayment.UsedAmount` 回滾

### 驗證測試
- [ ] 測試使用金額超過可用餘額的錯誤訊息
- [ ] 測試重複來源單號的錯誤訊息
- [ ] 測試 Amount 和 UsedAmount 都為 0 的情況

---

## 🔍 疑難排解

### 問題 1：編譯錯誤 - 缺少 SearchAsync 方法
**解決方案**：在 `SetoffPrepaymentUsageService` 中實作 `SearchAsync` 方法  
**狀態**：✅ 已解決

### 問題 2：Migration 失敗 - 級聯刪除循環
**解決方案**：將 `SetoffPrepayment` → `SetoffPrepaymentUsage` 的外鍵改為 `NO ACTION`  
**狀態**：✅ 已解決

### 問題 3：UsedAmount 沒有自動更新
**原因**：`SetoffPrepaymentUsageService` 的 `CreateAsync` 或 `DeleteAsync` 方法沒有調用 `UpdatePrepaymentUsedAmountAsync()`  
**檢查**：確認這兩個方法都有更新邏輯  
**狀態**：✅ 已解決

### 問題 4：編輯模式資料顯示錯誤
**原因**：`LoadSetoffDocumentData()` 沒有正確合併兩種類型的記錄  
**檢查**：確認使用記錄轉換為 `SetoffPrepayment` 格式時 `Amount = 0`  
**狀態**：✅ 已解決

### 問題 5：編輯模式載入時 PrepaymentType 為 null
**症狀**：編輯沖款單時，預收付款項只顯示「預收/付」，沒有正確顯示「預收/付轉沖款」，且金額為 0  
**原因**：Service 層查詢時沒有 Include `PrepaymentType` 導航屬性  
**解決方案**：
- 修改 `SetoffPrepaymentService.GetBySetoffDocumentIdAsync`：新增 `.Include(sp => sp.PrepaymentType)`
- 修改 `SetoffPrepaymentUsageService.GetBySetoffDocumentIdAsync`：新增 `.ThenInclude(p => p.PrepaymentType)`  

**狀態**：✅ 已解決

### 問題 6：編輯模式顯示錯誤的 PrepaymentType
**症狀**：編輯沖款單時，轉沖款類型顯示為「預收」而非「預收轉沖款」，且使用金額顯示為 0  
**原因**：`LoadSetoffDocumentData` 中轉換 `SetoffPrepaymentUsage` 為 `SetoffPrepayment` 時，使用了**來源預收付款項的 TypeId**（例如「預收」），而不是「預收轉沖款」的 TypeId  
**解決方案**：根據來源預收付款項的類型名稱，查詢對應的「轉沖款」類型 ID

**狀態**：✅ 已解決

**修正前**：
```csharp
setoffPrepayments.Add(new SetoffPrepayment
{
    PrepaymentTypeId = usage.SetoffPrepayment.PrepaymentTypeId,  // ❌ 錯誤：這是「預收」的ID
    // ...
});
```

**修正後**：
```csharp
// 根據來源類型判斷應使用的轉沖款類型
var sourceType = usage.SetoffPrepayment.PrepaymentType?.Name ?? string.Empty;
var transferTypeName = sourceType.Contains("預收") ? "預收轉沖款" : "預付轉沖款";
var transferType = allPrepaymentTypes.FirstOrDefault(pt => pt.Name == transferTypeName);

setoffPrepayments.Add(new SetoffPrepayment
{
    PrepaymentTypeId = transferType.Id,  // ✅ 正確：使用「預收轉沖款」的ID
    Amount = 0,  // 轉沖款：Amount 為 0
    UsedAmount = usage.UsedAmount,  // 轉沖款：記錄使用金額
    // ...
});
```

### 問題 7：編輯模式無法正確顯示「總金額」欄位
**症狀**：編輯沖款單時，轉沖款類型的「總金額」欄位顯示為 0 或空白  
**原因**：
1. 載入時 `Amount` 設為 0，沒有保存來源預收付款項的總金額
2. 前端組件依賴 `AvailablePrepayments` 查詢總金額，但編輯模式時該清單可能不包含已使用的預收付款項
3. 沒有設定 `SourcePrepaymentId`，無法正確比對來源

**解決方案**：
1. **後端載入**：保存來源的總金額到 `Amount` 欄位
2. **前端比對**：新增 `MatchSourcePrepaymentIds()` 方法，根據 `SourceDocumentCode` 自動設定 `SourcePrepaymentId`
3. **前端顯示**：優先使用 `item.Amount`，若為 0 才從 `AvailablePrepayments` 查詢

**狀態**：✅ 已解決

**修正前**：
```csharp
// SetoffDocumentEditModalComponent.razor
setoffPrepayments.Add(new SetoffPrepayment
{
    Amount = 0,  // ❌ 總金額遺失
    UsedAmount = usage.UsedAmount,
    // ...
});

// SetoffPrepaymentDetailManagerComponent.razor
var sourcePrepayment = AvailablePrepayments.FirstOrDefault(p => p.Id == prepaymentItem.SourcePrepaymentId);
var amount = sourcePrepayment?.Amount ?? 0;  // ❌ SourcePrepaymentId 為 null，找不到資料
```

**修正後**：
```csharp
// SetoffDocumentEditModalComponent.razor (載入時保存總金額)
setoffPrepayments.Add(new SetoffPrepayment
{
    Amount = usage.SetoffPrepayment.Amount,  // ✅ 保存來源的總金額
    UsedAmount = usage.UsedAmount,
    // ...
});

// SetoffPrepaymentDetailManagerComponent.razor (新增自動比對方法)
private void MatchSourcePrepaymentIds()
{
    foreach (var item in PrepaymentItems)
    {
        if (IsTransferType && !item.SourcePrepaymentId.HasValue && !string.IsNullOrEmpty(item.SourceDocumentCode))
        {
            var sourcePrepayment = AvailablePrepayments.FirstOrDefault(
                p => p.SourceDocumentCode == item.SourceDocumentCode);
            if (sourcePrepayment != null)
            {
                item.SourcePrepaymentId = sourcePrepayment.Id;  // ✅ 自動設定
                if (item.Amount == 0)
                {
                    item.Amount = sourcePrepayment.Amount;  // ✅ 同步總金額
                }
            }
        }
    }
}

// 顯示時優先使用已保存的 Amount
var amount = prepaymentItem.Amount;  // ✅ 編輯模式直接取得
if (amount == 0 && prepaymentItem.SourcePrepaymentId.HasValue)
{
    var sourcePrepayment = AvailablePrepayments.FirstOrDefault(p => p.Id == prepaymentItem.SourcePrepaymentId);
    amount = sourcePrepayment?.Amount ?? 0;  // 新增模式才查詢
}
```

**技術細節**：
- **Amount 欄位的雙重用途**：
  - 預收/預付類型：記錄預收付的金額
  - 轉沖款類型：記錄來源預收付款項的總金額（用於前端顯示「總金額」欄位）
- **SourcePrepaymentId 的設定時機**：
  - 新增模式：使用者選擇來源時設定
  - 編輯模式：載入後根據 `SourceDocumentCode` 自動比對設定
- **可用餘額的計算**：仍然從 `AvailablePrepayments` 查詢（因為餘額會變動）

---

**技術細節**：
```csharp
// SetoffPrepaymentService.cs
public async Task<List<SetoffPrepayment>> GetBySetoffDocumentIdAsync(int setoffDocumentId)
{
    return await context.SetoffPrepayments
        .AsNoTracking()
        .Include(sp => sp.PrepaymentType)  // ← 新增這行
        .Include(sp => sp.Customer)
        .Include(sp => sp.Supplier)
        .Include(sp => sp.SetoffDocument)
        .Where(sp => sp.SetoffDocumentId == setoffDocumentId)
        .OrderBy(sp => sp.CreatedAt)
        .ToListAsync();
}

// SetoffPrepaymentUsageService.cs
public async Task<List<SetoffPrepaymentUsage>> GetBySetoffDocumentIdAsync(int setoffDocumentId)
{
    return await context.SetoffPrepaymentUsages
        .AsNoTracking()
        .Include(u => u.SetoffPrepayment)
            .ThenInclude(p => p.PrepaymentType)  // ← 新增這行
        .Where(u => u.SetoffDocumentId == setoffDocumentId)
        .OrderBy(u => u.CreatedAt)
        .ToListAsync();
}
```

---

## 📚 相關文件連結

- [沖款單設計文件](./README_Index_Design.md)
- [預收付款項管理器文件](./README_SetoffPrepaymentDetailManager.md)
- [服務層設計](./README_Services.md)

---

## 🎯 總結

此次重構成功將預收付款項的「主記錄」和「使用記錄」分離到不同的資料表：

✅ **解決的問題**：
- 驗證邏輯衝突（Amount = 0 無法通過驗證）
- 來源單號重複問題
- 審計追蹤不完整

✅ **帶來的好處**：
- 清晰的資料結構（主記錄 vs 使用記錄）
- 完整的審計追蹤（可查詢每筆預收付被哪些沖款單使用）
- 自動化的 UsedAmount 計算
- 更好的資料完整性保證

✅ **編譯狀態**：成功通過編譯，準備進行功能測試！

