# 應收帳款折讓與財務交易表優化 - 實作完成報告

## 📋 修改概述

本次修改主要解決兩個重要問題並**已完成實作**：
1. ✅ **新增折讓功能**：支援不收款但透過折讓來抵銷應收帳款的業務需求
2. ✅ **完善財務追蹤**：建立完整的明細級別財務流向記錄機制

## 🎯 業務需求背景

### 問題 1：缺乏折讓功能 ✅ 已解決
- **原況**：只能記錄實際收款的沖帳
- **需求**：需要支援不實際收款，但透過折讓方式抵銷應收帳款
- **場景**：商品瑕疵補償、促銷折扣、商務談判等情況
- **解決方案**：已實作完整的折讓功能，包含 UI 介面和財務記錄

### 問題 2：財務追蹤不完整 ✅ 已解決
- **原況**：`FinancialTransaction` 只記錄到單據層級
- **需求**：需要追蹤到具體明細項目的財務異動
- **重要性**：確保所有金錢流向都有完整的追蹤記錄
- **解決方案**：已新增 `SourceDetailId` 欄位實現明細級別追蹤

## 🏗️ 技術架構修改

### 1. 資料庫結構調整

#### FinancialTransaction 實體優化 ✅ 已完成
```csharp
// 已新增明細級別追蹤，折讓金額使用 Amount 欄位 + TransactionType 來區分
public class FinancialTransaction : BaseEntity
{
    // 原有屬性...
    public string? SourceDocumentType { get; set; }  // 單據類型
    public int? SourceDocumentId { get; set; }       // 單據ID
    public string? SourceDocumentNumber { get; set; } // 單據號碼
    
    // 已新增欄位
    public int? SourceDetailId { get; set; }         // 明細ID (已實作)
    
    // 關鍵：折讓金額儲存在 Amount 欄位，透過 TransactionType 區分
    public decimal Amount { get; set; }              // 交易金額（含折讓）
    public FinancialTransactionTypeEnum TransactionType { get; set; } // 交易類型
    
    // 沖銷相關欄位
    public bool IsReversed { get; set; }             // 是否已沖銷
    public DateTime? ReversedDate { get; set; }      // 沖銷日期
    public string? ReversalReason { get; set; }      // 沖銷原因
    public int? ReversalTransactionId { get; set; }  // 沖銷交易ID
}
```

#### FinancialTransactionTypeEnum 新增類型 ✅ 已完成
```csharp
public enum FinancialTransactionTypeEnum
{
    AccountsReceivableSetoff = 1,      // 應收沖款
    AccountsReceivableRefund = 2,      // 應收退款
    AccountsReceivableAdjustment = 3,  // 應收調整
    AccountsReceivableDiscount = 4,    // 應收折讓 (已實作)
    // ... 其他交易類型
}
```

### 2. 資料傳輸物件擴展

#### SetoffDetailDto 新增折讓屬性 ✅ 已完成
```csharp
public class SetoffDetailDto
{
    // 原有屬性...
    public decimal TotalAmount { get; set; }            // 總金額
    public decimal SettledAmount { get; set; }          // 已沖款金額
    
    // 已新增折讓相關屬性
    public decimal DiscountedAmount { get; set; }       // 已折讓金額
    public decimal ThisTimeDiscountAmount { get; set; } // 本次折讓金額
    
    // 修改後的待沖款計算邏輯
    public decimal PendingAmount => TotalAmount - SettledAmount - DiscountedAmount;
    
    // 驗證方法也已實作
    public (bool IsValid, string? ErrorMessage) ValidateThisTimeDiscountAmount() { /* 已實作 */ }
    public (bool IsValid, string? ErrorMessage) ValidateTotalThisTimeAmount() { /* 已實作 */ }
}
```

#### 新增驗證方法
```csharp
// 折讓金額驗證
public (bool IsValid, string? ErrorMessage) ValidateThisTimeDiscountAmount()
{
    if (ThisTimeDiscountAmount < 0)
        return (false, "折讓金額不能為負數");
    
    if (ThisTimeDiscountAmount > PendingAmount)
        return (false, $"折讓金額不能超過待沖款金額 {PendingAmount:N2}");
    
    return (true, null);
}

// 總金額驗證（沖款 + 折讓）
public (bool IsValid, string? ErrorMessage) ValidateTotalThisTimeAmount()
{
    var totalThisTime = ThisTimeAmount + ThisTimeDiscountAmount;
    
    if (totalThisTime > PendingAmount)
        return (false, $"沖款和折讓總金額 ({totalThisTime:N2}) 不能超過待沖款金額 {PendingAmount:N2}");
    
    return (true, null);
}
```

### 3. 使用者界面升級

#### 新增表格欄位
| 欄位 | 原有/新增 | 說明 |
|------|-----------|------|
| 總金額 | 原有 | 應收總金額 |
| 已沖款 | 原有 | 累計收款金額 |
| **已折讓** | **新增** | **累計折讓金額** |
| 待沖款 | 修改 | 總金額 - 已沖款 - 已折讓 |
| 本次沖款 | 原有 | 本次實際收款 |
| **本次折讓** | **新增** | **本次折讓金額** |

#### 智能驗證機制
```csharp
// 動態可用金額計算
private async Task HandleAmountChanged((SetoffDetailDto detail, string? value) args)
{
    // 計算可用於沖款的金額（扣除本次折讓）
    var availableAmount = args.detail.PendingAmount - args.detail.ThisTimeDiscountAmount;
    args.detail.ThisTimeAmount = Math.Max(0, Math.Min(amount, availableAmount));
}

private async Task HandleDiscountAmountChanged((SetoffDetailDto detail, string? value) args)
{
    // 計算可用於折讓的金額（扣除本次沖款）
    var availableAmount = args.detail.PendingAmount - args.detail.ThisTimeAmount;
    args.detail.ThisTimeDiscountAmount = Math.Max(0, Math.Min(amount, availableAmount));
}
```

## 📊 財務流向追蹤架構

### 雙層追蹤機制
```
FinancialTransaction 記錄結構：
┌─────────────────┐
│   單據層級追蹤   │
├─────────────────┤
│ SourceDocumentType: "AccountsReceivableSetoff"
│ SourceDocumentId: 123 (沖款單ID)
│ SourceDocumentNumber: "AR202501001"
└─────────────────┘
┌─────────────────┐
│   明細層級追蹤   │
├─────────────────┤
│ SourceDetailId: 456 (SalesOrderDetail.Id)
│ 可追蹤到具體銷貨明細
└─────────────────┘
```

### 折讓記錄範例
```csharp
// 沖款交易記錄
var setoffTransaction = new FinancialTransaction
{
    TransactionType = FinancialTransactionTypeEnum.AccountsReceivableSetoff,
    Amount = 8000m,  // 實際收款金額
    SourceDocumentType = "AccountsReceivableSetoff",
    SourceDocumentId = 123,
    SourceDetailId = 456  // 指向具體銷貨明細
};

// 折讓交易記錄
var discountTransaction = new FinancialTransaction
{
    TransactionType = FinancialTransactionTypeEnum.AccountsReceivableDiscount,
    Amount = 2000m,  // 折讓金額儲存在 Amount 欄位
    SourceDocumentType = "AccountsReceivableSetoff",
    SourceDocumentId = 123,
    SourceDetailId = 456  // 同一筆銷貨明細
};
```

## 🔄 業務流程示例

### 場景：商品瑕疵補償
1. **原始銷貨**：客戶購買商品 10,000 元
2. **發現瑕疵**：商品有瑕疵，協商補償 2,000 元
3. **沖款處理**：
   - 本次沖款：8,000 元（實際收款）
   - 本次折讓：2,000 元（瑕疵補償）
   - 總處理：10,000 元（完全結清）

### 財務記錄
```sql
-- 沖款記錄
INSERT INTO FinancialTransactions 
(TransactionType, Amount, SourceDetailId, Description)
VALUES 
(1, 8000.00, 456, '客戶實際付款');

-- 折讓記錄  
INSERT INTO FinancialTransactions 
(TransactionType, Amount, SourceDetailId, Description)
VALUES 
(4, 2000.00, 456, '商品瑕疵補償折讓');
```

## 📈 組件功能升級

### 新增公開方法
```csharp
public class AccountsReceivableSetoffDetailManagerComponent
{
    // 總金額（沖款 + 折讓）
    public decimal GetTotalAmount() => 
        SelectedDetails.Sum(d => d.ThisTimeAmount + d.ThisTimeDiscountAmount);
    
    // 純現金沖款金額
    public decimal GetTotalCashAmount() => 
        SelectedDetails.Sum(d => d.ThisTimeAmount);
    
    // 純折讓金額
    public decimal GetTotalDiscountAmount() => 
        SelectedDetails.Sum(d => d.ThisTimeDiscountAmount);
    
    // 完整驗證（包含折讓）
    public (bool IsValid, List<string> Errors) ValidateSelection()
    {
        // 同時驗證沖款和折讓金額的有效性
    }
}
```

### 選擇邏輯優化
```csharp
// 包含沖款或折讓的項目都視為已選擇
private List<SetoffDetailDto> SelectedDetails => 
    Details.Where(d => d.ThisTimeAmount > 0 || d.ThisTimeDiscountAmount > 0).ToList();
```

## � 實際實作細節

### 1. AccountsReceivableSetoffEditModalComponent 修改

#### 服務注入更新
```csharp
// 已更改為使用 IFinancialTransactionService
@inject IFinancialTransactionService FinancialTransactionService
// 移除了 @inject IDbContextFactory<AppDbContext> DbContextFactory
```

#### 財務交易記錄創建方法
```csharp
private async Task CreateFinancialTransactionRecordsAsync(AccountsReceivableSetoff setoff, List<SetoffDetailDto> selectedDetails)
{
    foreach (var detail in selectedDetails)
    {
        // 創建沖款交易記錄
        if (detail.ThisTimeAmount > 0)
        {
            var setoffTransaction = new FinancialTransaction
            {
                TransactionNumber = $"FT{DateTime.Now:yyyyMMddHHmmss}{detail.OriginalEntityId}",
                TransactionType = FinancialTransactionTypeEnum.AccountsReceivableSetoff,
                TransactionDate = setoff.SetoffDate,
                Amount = detail.ThisTimeAmount,
                Description = $"應收沖款 - {detail.DocumentNumber}",
                SourceDetailId = detail.OriginalEntityId, // 明細級別追蹤
                // ... 其他屬性
            };
            await CreateFinancialTransactionAsync(setoffTransaction);
        }

        // 創建折讓交易記錄 (新增)
        if (detail.ThisTimeDiscountAmount > 0)
        {
            var discountTransaction = new FinancialTransaction
            {
                TransactionType = FinancialTransactionTypeEnum.AccountsReceivableDiscount,
                Amount = detail.ThisTimeDiscountAmount,
                Description = $"應收折讓 - {detail.DocumentNumber}",
                SourceDetailId = detail.OriginalEntityId, // 明細級別追蹤
                // ... 其他屬性
            };
            await CreateFinancialTransactionAsync(discountTransaction);
        }
    }
}
```

#### 總金額計算優化
```csharp
// SaveSetoffDetailsAsync 方法中，重新計算並更新主檔總金額（包含沖款和折讓）
setoff.TotalSetoffAmount = selectedDetails.Sum(d => d.ThisTimeAmount + d.ThisTimeDiscountAmount);
await AccountsReceivableSetoffService.UpdateAsync(setoff);
```

### 2. FinancialTransactionEditModalComponent 修改

#### 新增 DiscountAmount 欄位顯示
```csharp
// 注意：FinancialTransaction 實體中沒有獨立的 DiscountAmount 欄位
// 折讓金額儲存在 Amount 欄位，透過 TransactionType = AccountsReceivableDiscount 區分
// UI 顯示時從 Amount 欄位讀取折讓金額
new FormFieldDefinition()
{
    PropertyName = nameof(FinancialTransaction.Amount),
    FieldType = FormFieldType.Number,
    IsReadOnly = true,
    Label = "交易金額",
    DisplayCondition = (item) => 
    {
        var transaction = item as FinancialTransaction;
        return transaction?.TransactionType == FinancialTransactionTypeEnum.AccountsReceivableDiscount;
    }
}
```

#### 沖銷功能完整實作
- 沖銷 Modal 介面
- 沖銷理由輸入
- 沖銷確認機制
- 沖銷後狀態顯示

### 1. AccountsReceivableSetoffDetailService 擴展 ✅ 已完成
```csharp
public async Task<List<SetoffDetailDto>> GetCustomerPendingDetailsAsync(int customerId)
{
    var details = await GetAccountsReceivableDetails(customerId);
    
    foreach (var detail in details)
    {
        // 從 FinancialTransaction 計算已沖款金額
        detail.SettledAmount = await GetSettledAmountFromFinancialTransactions(detail.Id);
        
        // 從 FinancialTransaction 計算已折讓金額 (已實作)
        detail.DiscountedAmount = await GetDiscountedAmountFromFinancialTransactions(detail.Id);
        
        // 重新計算待沖款金額
        detail.PendingAmount = detail.TotalAmount - detail.SettledAmount - detail.DiscountedAmount;
    }
    
    return details;
}

private async Task<decimal> GetDiscountedAmountFromFinancialTransactions(int detailId)
{
    return await _context.FinancialTransactions
        .Where(ft => ft.SourceDetailId == detailId 
                    && ft.TransactionType == FinancialTransactionTypeEnum.AccountsReceivableDiscount
                    && !ft.IsReversed)
        .SumAsync(ft => ft.Amount);  // 折讓金額從 Amount 欄位取得
}
```

### 2. 創建沖款時的財務記錄 ✅ 已完成
```csharp
public async Task<ServiceResult> CreateSetoffAsync(SetoffDto setoff)
{
    // 創建沖款單...
    
    foreach (var detail in setoffDetails)
    {
        // 創建沖款交易記錄
        if (detail.ThisTimeAmount > 0)
        {
            await CreateFinancialTransaction(new FinancialTransaction
            {
                TransactionType = FinancialTransactionTypeEnum.AccountsReceivableSetoff,
                Amount = detail.ThisTimeAmount,  // 沖款金額
                SourceDocumentType = "AccountsReceivableSetoff",
                SourceDocumentId = setoffId,
                SourceDetailId = detail.OriginalEntityId,  // 關鍵：明細級別追蹤
                Description = $"應收沖款 - {detail.DocumentNumber}"
            });
        }
        
        // 創建折讓交易記錄 (已實作)
        if (detail.ThisTimeDiscountAmount > 0)
        {
            await CreateFinancialTransaction(new FinancialTransaction
            {
                TransactionType = FinancialTransactionTypeEnum.AccountsReceivableDiscount,
                Amount = detail.ThisTimeDiscountAmount,  // 折讓金額儲存在 Amount 欄位
                SourceDocumentType = "AccountsReceivableSetoff",
                SourceDocumentId = setoffId,
                SourceDetailId = detail.OriginalEntityId,  // 關鍵：明細級別追蹤
                Description = $"應收折讓 - {detail.DocumentNumber}"
            });
        }
    }
}
```

## ✅ 實作完成項目

### 資料庫結構
- [x] **FinancialTransaction 實體新增明細關聯欄位** (`SourceDetailId`)
- [x] **新增 AccountsReceivableDiscount 交易類型** (使用 Amount 欄位儲存折讓金額)
- [x] **沖銷機制相關欄位** (`IsReversed`, `ReversedDate`, `ReversalReason`)
- ⚠️ **注意**：不需要獨立的 `DiscountAmount` 欄位，折讓金額透過 `Amount` + `TransactionType` 區分

### 資料傳輸物件
- [x] **SetoffDetailDto 新增折讓相關屬性**
- [x] **新增折讓金額驗證邏輯**
- [x] **新增總金額驗證邏輯** (沖款 + 折讓)

### 使用者界面組件
- [x] **AccountsReceivableSetoffEditModalComponent 財務交易記錄功能**
- [x] **使用 IFinancialTransactionService 代替 DbContextFactory**
- [x] **CreateFinancialTransactionRecordsAsync 方法實作** (v2.2 重構完成)
- [x] **SaveSetoffDetailsAsync 方法優化**
- [x] **折讓金額處理和總金額計算**
- [x] **編輯模式財務記錄邏輯修正** (v2.2 新增)
- [x] **CreateOrUpdateFinancialTransactionAsync 方法** (v2.2 新增)
- [x] **CleanupObsoleteFinancialTransactionsAsync 方法** (v2.2 新增)

### 財務交易組件
- [x] **FinancialTransactionEditModalComponent 正確顯示交易金額** (根據 TransactionType 判斷是否為折讓)
- [x] **沖銷功能完整實作** (含 Modal 和沖銷理由)

### 服務層架構
- [x] **明細級別財務追蹤機制** (透過 `SourceDetailId` 實現)
- [x] **同時創建沖款和折讓交易記錄** (使用不同的 `TransactionType`)
- [x] **使用服務層一致性架構**
- [x] **已修正 GetCustomerPendingDetailsAsync 和 GetCustomerAllDetailsForEditAsync** (計算已折讓金額)

## 🎆 實作成果與效益

### 業務效益 ✅ 已實現
1. **完整業務支援**：支援折讓這種常見的商業處理方式
2. **精確財務管控**：區分實際收款和折讓，提供準確的現金流分析
3. **靈活處理方式**：同一筆應收可同時進行部分收款和部分折讓
4. **完整財務追蹤**：每筆財務異動都可追蹤到具體明細來源

### 技術效益 ✅ 已實現
1. **服務層一致性**：使用 IFinancialTransactionService 保持架構一致性
2. **資料一致性**：所有金額計算都基於 FinancialTransaction 記錄
3. **擴展性良好**：架構支援未來更多財務交易類型
4. **明細級別追蹤**：透過 SourceDetailId 實現完整的財務流向記錄

### 使用者體驗 ✅ 已優化
1. **直觀操作界面**：清楚區分收款和折讓欄位
2. **智能驗證提示**：即時檢查金額合理性
3. **完整資訊顯示**：一目了然的應收帳款處理狀況
4. **沖銷功能**：完整的財務交易沖銷機制

## 📝 後續建議和注意事項

### 後續優化建議
1. **UI 組件擴展**：可考慮在 AccountsReceivableSetoffDetailManagerComponent 中新增折讓欄位
2. **服務層擴展**：實作 GetDiscountedAmountFromFinancialTransactions 方法
3. **報表功能**：新增折讓明細報表和統計功能

### 注意事項
1. **資料庫架構**：折讓金額儲存在 FinancialTransaction.Amount 欄位，無需額外的 DiscountAmount 欄位
2. **測試建議**：
   - 重點測試金額驗證邏輯和財務記錄的正確性
   - **新增模式**：確認正常創建財務記錄
   - **編輯模式**：確認不會重複創建記錄，能正確更新現有記錄
   - **明細變更**：測試新增/移除/修改明細的財務記錄處理
   - **折讓金額**：驗證折讓金額的計算和追蹤邏輯
3. **權限控制**：考慮是否需要對折讓功能設置特殊權限
4. **效能最佳化**：大量財務交易記錄時的查詢效能優化
5. **審計軌跡**：確保編輯時的記錄清理透過沖銷機制保持完整軌跡

### 已解決的技術問題
- ✅ DbContextFactory vs Service Layer 架構一致性
- ✅ 折讓金額的儲存和追蹤 (透過 Amount + TransactionType 實現)
- ✅ 財務交易記錄的創建和管理
- ✅ 明細級別的財務流向追蹤 (透過 SourceDetailId 實現)
- ✅ 已折讓金額的正確計算和顯示
- ✅ **編輯模式重複創建財務記錄問題** (v2.2 修正)
- ✅ **財務記錄的更新邏輯優化** (v2.2 修正)
- ✅ **不再需要記錄的自動清理機制** (v2.2 修正)

### 最新修正 (2025年9月29日)

#### v2.1 - 修正已折讓金額計算問題
- ✅ **修正 GetCustomerPendingDetailsAsync 方法**：加入從 FinancialTransaction 計算已折讓金額的邏輯
- ✅ **修正 GetCustomerAllDetailsForEditAsync 方法**：編輯模式下也能正確計算已折讓金額
- ✅ **確認資料架構**：折讓金額使用 Amount 欄位 + TransactionType = AccountsReceivableDiscount
- ✅ **驗證創建邏輯**：確認折讓記錄創建程式碼正確實作

#### v2.2 - 修正編輯模式重複創建財務記錄問題
- ✅ **問題識別**：編輯模式下每次儲存都會重複創建 FinancialTransaction 記錄
- ✅ **核心修正**：重構 `CreateFinancialTransactionRecordsAsync` 方法，區分新增/編輯模式
- ✅ **新增方法**：`CreateOrUpdateFinancialTransactionAsync` - 統一處理財務交易記錄的創建和更新
- ✅ **清理機制**：`CleanupObsoleteFinancialTransactionsAsync` - 自動清理編輯時不再需要的財務記錄
- ✅ **效能優化**：使用 `GetTransactionsByCustomerIdAsync` 取代 `GetAllAsync` 提升查詢效率
- ✅ **資料一致性**：透過沖銷機制處理記錄清理，保持完整的審計軌跡

#### 編輯模式邏輯優化
```csharp
// 修正後的財務記錄處理邏輯
private async Task CreateFinancialTransactionRecordsAsync(AccountsReceivableSetoff setoff, List<SetoffDetailDto> selectedDetails)
{
    bool isEditMode = SetoffId.HasValue && SetoffId.Value > 0;
    
    if (isEditMode)
    {
        // 編輯模式：先清理不再需要的記錄
        await CleanupObsoleteFinancialTransactionsAsync(setoff, selectedDetails);
    }
    
    // 處理當前選擇的明細項目
    foreach (var detail in selectedDetails)
    {
        // 沖款記錄：檢查是否需要更新或創建
        if (detail.ThisTimeAmount > 0)
            await CreateOrUpdateFinancialTransactionAsync(/* 參數 */);
            
        // 折讓記錄：檢查是否需要更新或創建  
        if (detail.ThisTimeDiscountAmount > 0)
            await CreateOrUpdateFinancialTransactionAsync(/* 參數 */);
    }
}
```

#### 關鍵改進項目
1. **智能記錄管理**：
   - 新增模式：直接創建新記錄
   - 編輯模式：檢查現有記錄並決定更新或創建
   - 自動清理：移除不再需要的財務記錄

2. **明細級別追蹤**：
   - 使用 `SourceDetailId` 精確關聯到具體明細
   - 支援同一沖款單內不同明細的獨立財務記錄更新

3. **沖銷機制應用**：
   - 不直接刪除財務記錄，而是透過沖銷保持審計軌跡
   - 清理編輯時移除的明細對應的財務記錄

4. **效能最佳化**：
   - 改用客戶ID篩選減少查詢範圍
   - 避免全表掃描提升處理效率

---

*最後更新：2025年9月29日*  
*版本：v2.2 - 修正編輯模式重複創建財務記錄問題*  
*狀態：✅ 已完成主要功能實作並修正編輯邏輯問題*