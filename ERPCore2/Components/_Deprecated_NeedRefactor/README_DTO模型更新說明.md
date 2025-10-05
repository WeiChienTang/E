# DTO 模型更新說明

## 更新日期
2025年10月4日

## 更新內容

### 1. 新建 SetoffDocumentDto.cs

此檔案包含三個主要的 DTO 類別：

#### SetoffDocumentDto（沖款單 DTO）

**用途**: 統一處理應收/應付帳款沖款單

**主要欄位**:
- 基本資訊: Id, SetoffType, SetoffNumber, SetoffDate
- 關聯方: RelatedPartyId, RelatedPartyType, RelatedPartyName
- 金額: TotalSetoffAmount
- 狀態: IsCompleted, CompletedDate
- 導航屬性: Details, PaymentRecords, PrepaymentUsages

**計算屬性**:
- `TotalPaymentAmount` - 總付款金額（從付款記錄計算）
- `TotalPrepaymentUsage` - 總預收預付使用金額
- `TotalAmountDue` - 應付總額
- `AmountDifference` - 金額差異
- `IsBalanced` - 是否金額平衡

**驗證方法**:
- `Validate()` - 驗證沖款單資料
- `ValidateBalance()` - 驗證金額平衡

**轉換方法**:
- `FromEntity()` - 從實體轉換為 DTO
- `ToEntity()` - 轉換為實體

#### SetoffDocumentDetailDto（沖款單明細 DTO）

**用途**: 記錄沖款單的明細項目

**主要欄位**:
- SetoffDocumentId, SourceDocumentId, SourceDocumentType
- OriginalAmount, SetoffAmount, AfterSetoffAmount
- DocumentNumber

**計算屬性**:
- `RemainingAmount` - 剩餘金額
- `IsFullySetoff` - 是否完全沖款

#### FinancialTransactionDto（財務交易 DTO）

**用途**: 簡化版的財務交易 DTO，用於付款記錄和預收預付款

**主要欄位**:
- 基本資訊: Id, TransactionNumber, TransactionType, TransactionDate, Amount
- 付款資訊: PaymentMethodId, BankId, PaymentAccount, ReferenceNumber, PaymentDate
- 來源資訊: SourceDocumentId, SourceDetailId
- 顯示名稱: PaymentMethodName, BankName

**計算屬性**:
- `TransactionTypeDisplayName` - 交易類型顯示名稱

---

## 使用範例

### 範例 1: 建立沖款單 DTO

```csharp
var setoffDto = new SetoffDocumentDto
{
    SetoffType = SetoffType.AccountsReceivable,
    SetoffNumber = "AR-20251004-001",
    SetoffDate = DateTime.Today,
    RelatedPartyId = 123, // 客戶ID
    RelatedPartyType = "Customer",
    CompanyId = 1,
    TotalSetoffAmount = 50000
};

// 新增明細
setoffDto.Details.Add(new SetoffDocumentDetailDto
{
    SourceDocumentId = 456,
    SourceDocumentType = "SalesOrderDetail",
    DocumentNumber = "SO-001",
    OriginalAmount = 50000,
    SetoffAmount = 50000
});

// 新增付款記錄
setoffDto.PaymentRecords.Add(new FinancialTransactionDto
{
    TransactionType = FinancialTransactionTypeEnum.SetoffPayment,
    Amount = 50000,
    PaymentMethodId = 1,
    PaymentDate = DateTime.Today
});

// 驗證
var validation = setoffDto.Validate();
if (!validation.IsValid)
{
    foreach (var error in validation.Errors)
    {
        Console.WriteLine(error);
    }
}
```

### 範例 2: 使用預收款

```csharp
// 新增預收款使用記錄
setoffDto.PrepaymentUsages.Add(new FinancialTransactionDto
{
    TransactionType = FinancialTransactionTypeEnum.PrepaymentUsage,
    Amount = 10000,
    SourceDetailId = 789 // 原始預收款的 FinancialTransaction.Id
});

// 更新付款記錄金額
setoffDto.PaymentRecords.First().Amount = 40000; // 只需付 40000 (50000 - 10000)

// 檢查是否平衡
var balanceValidation = setoffDto.ValidateBalance();
Console.WriteLine($"金額平衡: {balanceValidation.IsValid}");
Console.WriteLine($"總沖款金額: {setoffDto.TotalSetoffAmount:N2}");
Console.WriteLine($"預收款使用: {setoffDto.TotalPrepaymentUsage:N2}");
Console.WriteLine($"應付總額: {setoffDto.TotalAmountDue:N2}");
Console.WriteLine($"實付金額: {setoffDto.TotalPaymentAmount:N2}");
Console.WriteLine($"金額差異: {setoffDto.AmountDifference:N2}");
```

### 範例 3: 從實體轉換

```csharp
// 從資料庫讀取實體
var entity = await context.SetoffDocuments
    .Include(s => s.Company)
    .Include(s => s.PaymentMethod)
    .Include(s => s.SetoffDetails)
    .Include(s => s.FinancialTransactions)
    .FirstOrDefaultAsync(s => s.Id == 1);

// 轉換為 DTO
var dto = SetoffDocumentDto.FromEntity(entity);

// 載入明細
dto.Details = entity.SetoffDetails
    .Select(d => new SetoffDocumentDetailDto
    {
        Id = d.Id,
        SetoffDocumentId = d.SetoffDocumentId,
        SourceDocumentId = d.SourceDocumentId,
        SourceDocumentType = d.SourceDocumentType,
        DocumentNumber = d.DocumentNumber,
        OriginalAmount = d.OriginalAmount,
        SetoffAmount = d.SetoffAmount,
        AfterSetoffAmount = d.AfterSetoffAmount
    })
    .ToList();

// 載入付款記錄
dto.PaymentRecords = entity.FinancialTransactions
    .Where(ft => ft.TransactionType == FinancialTransactionTypeEnum.SetoffPayment)
    .Select(ft => new FinancialTransactionDto
    {
        Id = ft.Id,
        TransactionNumber = ft.TransactionNumber,
        TransactionType = ft.TransactionType,
        Amount = ft.Amount,
        PaymentMethodId = ft.PaymentMethodId,
        BankId = ft.BankId,
        PaymentAccount = ft.PaymentAccount,
        ReferenceNumber = ft.ReferenceNumber,
        PaymentDate = ft.PaymentDate
    })
    .ToList();
```

---

## 與舊 DTO 的對應關係

### SetoffPaymentDetailDto → FinancialTransactionDto

| 舊欄位 (SetoffPaymentDetailDto) | 新欄位 (FinancialTransactionDto) | 說明 |
|----------------------------------|----------------------------------|------|
| Id                               | Id                               | ✅ 相同 |
| SetoffId                         | SourceDocumentId                 | ✅ 對應到來源單據ID |
| PaymentMethodId                  | PaymentMethodId                  | ✅ 相同 |
| PaymentMethodName                | PaymentMethodName                | ✅ 相同 |
| BankId                           | BankId                           | ✅ 相同 |
| BankName                         | BankName                         | ✅ 相同 |
| Amount                           | Amount                           | ✅ 相同 |
| AccountNumber                    | PaymentAccount                   | ✅ 重新命名 |
| TransactionReference             | ReferenceNumber                  | ✅ 重新命名 |
| PaymentDate                      | PaymentDate                      | ✅ 相同 |
| Remarks                          | Remarks                          | ✅ 相同 |

### PrepaymentDto → FinancialTransactionDto（部分）

預收預付款的資料結構變更較大，主要變更：

1. **建立預收款**: 使用 `FinancialTransaction` (TransactionType = Prepayment/Prepaid)
2. **使用預收款**: 使用 `FinancialTransaction` (TransactionType = PrepaymentUsage/PrepaidUsage)
   - `SourceDetailId` 指向原始預收款的 TransactionId

---

## 驗證邏輯

### 沖款單驗證規則

```csharp
public (bool IsValid, List<string> Errors) Validate()
{
    var errors = new List<string>();

    // 1. 必填欄位驗證
    if (string.IsNullOrWhiteSpace(SetoffNumber))
        errors.Add("沖款單號為必填");

    if (RelatedPartyId <= 0)
        errors.Add("請選擇關聯方（客戶或供應商）");

    if (CompanyId <= 0)
        errors.Add("請選擇公司");

    // 2. 明細驗證
    if (Details == null || !Details.Any())
        errors.Add("請至少新增一筆沖款明細");

    // 3. 金額平衡驗證
    if (PaymentRecords != null && PaymentRecords.Any())
    {
        if (!IsBalanced)
        {
            errors.Add($"付款總額與應付總額不一致，差異: {AmountDifference:N2}");
        }
    }

    return (errors.Count == 0, errors);
}
```

### 金額計算邏輯

```csharp
// 總付款金額 = 所有付款記錄的總和
TotalPaymentAmount = PaymentRecords.Sum(p => p.Amount)

// 總預收預付使用 = 所有預收預付使用記錄的總和
TotalPrepaymentUsage = PrepaymentUsages.Sum(p => p.Amount)

// 應付總額 = 沖款金額 + 預收預付使用
TotalAmountDue = TotalSetoffAmount + TotalPrepaymentUsage

// 金額差異 = 付款總額 - 應付總額
AmountDifference = TotalPaymentAmount - TotalAmountDue

// 是否平衡 = 差異 < 0.01
IsBalanced = Math.Abs(AmountDifference) < 0.01m
```

---

## 注意事項

### ⚠️ 向後相容性

1. **舊的 SetoffPaymentDetailDto 和 PrepaymentDto 暫時保留**
   - 等待 Razor 組件完成遷移後再刪除
   - 確保現有功能不受影響

2. **新 DTO 的使用場景**
   - 新開發的頁面和組件使用新 DTO
   - 舊有組件逐步遷移

### ⚠️ 資料轉換

1. **實體轉 DTO** - 使用 `FromEntity()` 方法
2. **DTO 轉實體** - 使用 `ToEntity()` 方法
3. **導航屬性需要額外處理** - Details, PaymentRecords, PrepaymentUsages

### ⚠️ 驗證時機

1. **儲存前** - 呼叫 `Validate()` 確保資料完整
2. **金額變更時** - 呼叫 `ValidateBalance()` 確保金額平衡
3. **前端驗證** - 配合 Data Annotations 進行欄位驗證

---

## 下一步

1. ✅ 建立 SetoffDocumentDto、SetoffDocumentDetailDto、FinancialTransactionDto
2. ⏳ 建立 Mapper 或擴充方法簡化轉換
3. ⏳ 更新 Razor 組件使用新 DTO
4. ⏳ 建立單元測試

---

**文檔建立者**: GitHub Copilot  
**最後更新**: 2025年10月4日  
**相關文檔**: 
- README_沖款系統重構.md
- README_FinancialTransaction_擴充說明.md
- README_已完成變更摘要.md
