# FinancialTransaction 擴充說明

## 更新日期
2025年10月4日

## 擴充目標
將 `FinancialTransaction` 擴充以支援沖款系統的付款記錄和預收預付款管理，實現方案 A 的統一財務記錄架構。

---

## 主要變更

### 1. FinancialTransaction 實體擴充

#### 新增欄位

```csharp
/// <summary>
/// 銀行ID - 此交易使用的銀行 (用於沖款單付款記錄)
/// </summary>
[Display(Name = "銀行")]
public int? BankId { get; set; }

/// <summary>
/// 付款日期 - 實際付款的日期 (可能與交易日期不同)
/// </summary>
[Display(Name = "付款日期")]
public DateTime? PaymentDate { get; set; }
```

#### 新增導航屬性

```csharp
/// <summary>
/// 銀行導航屬性
/// </summary>
public Bank? Bank { get; set; }
```

#### 欄位說明調整

- `PaymentAccount`: 原為"收付款帳戶"，現擴充為包含"帳號/票號"
- `ReferenceNumber`: 原為"參考號碼"，現擴充為包含"交易參考號"

---

### 2. TransactionType 列舉擴充

#### 新增預收預付款類型

```csharp
// === 預收預付款相關 (新增) ===

/// <summary>
/// 預收款 - 客戶預先支付款項
/// </summary>
[Description("預收款")]
Prepayment = 41,

/// <summary>
/// 預付款 - 預先支付供應商款項
/// </summary>
[Description("預付款")]
Prepaid = 42,

/// <summary>
/// 預收款使用 - 使用預收款沖抵應收帳款
/// </summary>
[Description("預收款使用")]
PrepaymentUsage = 43,

/// <summary>
/// 預付款使用 - 使用預付款沖抵應付帳款
/// </summary>
[Description("預付款使用")]
PrepaidUsage = 44,
```

#### 新增沖款單付款記錄類型

```csharp
// === 沖款單付款記錄 (新增) ===

/// <summary>
/// 沖款單付款 - 沖款單的實際付款記錄
/// </summary>
[Description("沖款單付款")]
SetoffPayment = 51
```

---

### 3. 新增計算屬性

```csharp
/// <summary>
/// 是否為預收預付款相關交易
/// </summary>
[NotMapped]
public bool IsPrepaymentTransaction => TransactionType switch
{
    FinancialTransactionTypeEnum.Prepayment => true,
    FinancialTransactionTypeEnum.Prepaid => true,
    FinancialTransactionTypeEnum.PrepaymentUsage => true,
    FinancialTransactionTypeEnum.PrepaidUsage => true,
    _ => false
};

/// <summary>
/// 是否為沖款單付款記錄
/// </summary>
[NotMapped]
public bool IsSetoffPayment => TransactionType == FinancialTransactionTypeEnum.SetoffPayment;
```

---

## IFinancialTransactionService 介面擴充

### 沖款單付款記錄相關方法

```csharp
/// <summary>
/// 根據沖款單ID獲取付款記錄
/// </summary>
Task<List<FinancialTransaction>> GetPaymentsBySetoffDocumentIdAsync(int setoffDocumentId);

/// <summary>
/// 建立沖款單付款記錄
/// </summary>
Task<ServiceResult> CreateSetoffPaymentAsync(int setoffDocumentId, FinancialTransaction payment);

/// <summary>
/// 批次建立沖款單付款記錄
/// </summary>
Task<ServiceResult> CreateSetoffPaymentsBatchAsync(int setoffDocumentId, List<FinancialTransaction> payments);

/// <summary>
/// 刪除沖款單的所有付款記錄
/// </summary>
Task<ServiceResult> DeletePaymentsBySetoffDocumentIdAsync(int setoffDocumentId);
```

### 預收預付款相關方法

```csharp
/// <summary>
/// 根據客戶ID獲取預收款記錄
/// </summary>
Task<List<FinancialTransaction>> GetPrepaymentsByCustomerIdAsync(int customerId);

/// <summary>
/// 根據供應商ID獲取預付款記錄
/// </summary>
Task<List<FinancialTransaction>> GetPrepaidsBySupplierIdAsync(int supplierId);

/// <summary>
/// 計算預收款可用餘額
/// </summary>
Task<decimal> GetPrepaymentAvailableBalanceAsync(int prepaymentTransactionId);

/// <summary>
/// 計算預付款可用餘額
/// </summary>
Task<decimal> GetPrepaidAvailableBalanceAsync(int prepaidTransactionId);

/// <summary>
/// 建立預收款使用記錄
/// </summary>
Task<ServiceResult> CreatePrepaymentUsageAsync(int setoffDocumentId, int prepaymentTransactionId, decimal amount);

/// <summary>
/// 建立預付款使用記錄
/// </summary>
Task<ServiceResult> CreatePrepaidUsageAsync(int setoffDocumentId, int prepaidTransactionId, decimal amount);
```

---

## 資料結構對應關係

### 方案 A 架構圖

```
SetoffDocument (沖款單主檔)
├── SetoffDocumentDetail (沖款明細 - 記錄哪些單據被沖款)
└── FinancialTransaction (財務交易 - 統一記錄所有付款/預收預付)
    ├── TransactionType = SetoffPayment (51) - 沖款單付款
    ├── TransactionType = Prepayment (41) - 預收款
    ├── TransactionType = Prepaid (42) - 預付款
    ├── TransactionType = PrepaymentUsage (43) - 預收款使用
    └── TransactionType = PrepaidUsage (44) - 預付款使用
```

### 舊有組件與新架構的對應

#### SetoffPaymentDetailManagerComponent
**舊資料結構:**
- `SetoffPaymentDetail` (已刪除)
  - PaymentMethodId
  - BankId
  - Amount
  - AccountNumber
  - TransactionReference
  - PaymentDate
  - Remarks

**新資料結構:**
- `FinancialTransaction` (TransactionType = SetoffPayment)
  - PaymentMethodId ✅
  - BankId ✅ (新增)
  - Amount ✅
  - PaymentAccount ✅ (對應 AccountNumber)
  - ReferenceNumber ✅ (對應 TransactionReference)
  - PaymentDate ✅ (新增)
  - Remarks (使用 SourceDocumentNumber 或新增欄位)
  - SourceDocumentId = SetoffDocumentId
  - SourceDocumentType = "SetoffDocument"

#### SetoffPrepaymentManagerComponent
**舊資料結構:**
- `PrepaymentDetail` (已刪除)
  - PrepaymentId
  - SetoffId
  - UseAmount
  
**新資料結構 - 選項 1: 使用 FinancialTransaction**
- `FinancialTransaction` (TransactionType = Prepayment/Prepaid)
  - 建立預收預付款: TransactionType = Prepayment/Prepaid
  - 使用預收預付款: TransactionType = PrepaymentUsage/PrepaidUsage
  - Amount = 使用金額
  - SourceDocumentId = SetoffDocumentId
  - SourceDocumentType = "SetoffDocument"
  - SourceDetailId = 原始 Prepayment TransactionId

**新資料結構 - 選項 2: 保留 Prepayment 實體**
- `Prepayment` - 預收預付款主檔
- `FinancialTransaction` (TransactionType = PrepaymentUsage/PrepaidUsage)
  - 記錄使用紀錄
  - SourceDocumentId = SetoffDocumentId
  - SourceDetailId = PrepaymentId

---

## 使用範例

### 範例 1: 建立沖款單付款記錄

```csharp
// 建立現金付款記錄
var payment = new FinancialTransaction
{
    TransactionType = FinancialTransactionTypeEnum.SetoffPayment,
    TransactionNumber = "PAY-20251004-001",
    TransactionDate = DateTime.Today,
    PaymentDate = DateTime.Today,
    Amount = 10000,
    PaymentMethodId = 1, // 現金
    BankId = null,
    PaymentAccount = null,
    ReferenceNumber = null,
    SourceDocumentType = "SetoffDocument",
    SourceDocumentId = 123, // SetoffDocument.Id
    CustomerId = 456,
    CompanyId = 1
};

await _financialTransactionService.CreateSetoffPaymentAsync(123, payment);
```

### 範例 2: 建立銀行轉帳付款記錄

```csharp
var payment = new FinancialTransaction
{
    TransactionType = FinancialTransactionTypeEnum.SetoffPayment,
    TransactionNumber = "PAY-20251004-002",
    TransactionDate = DateTime.Today,
    PaymentDate = DateTime.Today,
    Amount = 50000,
    PaymentMethodId = 2, // 銀行轉帳
    BankId = 5, // 台灣銀行
    PaymentAccount = "1234567890", // 帳號
    ReferenceNumber = "TXN-ABC123", // 交易參考號
    SourceDocumentType = "SetoffDocument",
    SourceDocumentId = 123,
    CustomerId = 456,
    CompanyId = 1
};

await _financialTransactionService.CreateSetoffPaymentAsync(123, payment);
```

### 範例 3: 建立預收款記錄

```csharp
var prepayment = new FinancialTransaction
{
    TransactionType = FinancialTransactionTypeEnum.Prepayment,
    TransactionNumber = "PRE-20251004-001",
    TransactionDate = DateTime.Today,
    PaymentDate = DateTime.Today,
    Amount = 20000,
    PaymentMethodId = 1,
    CustomerId = 456,
    CompanyId = 1,
    SourceDocumentType = "SetoffDocument",
    SourceDocumentId = 123
};

await _financialTransactionService.CreateAsync(prepayment);
```

### 範例 4: 使用預收款

```csharp
// 查詢客戶的可用預收款
var prepayments = await _financialTransactionService.GetPrepaymentsByCustomerIdAsync(456);

// 選擇要使用的預收款
var selectedPrepayment = prepayments.First();
var availableBalance = await _financialTransactionService.GetPrepaymentAvailableBalanceAsync(selectedPrepayment.Id);

// 建立使用記錄
await _financialTransactionService.CreatePrepaymentUsageAsync(
    setoffDocumentId: 123,
    prepaymentTransactionId: selectedPrepayment.Id,
    amount: 5000
);
```

---

## 查詢範例

### 查詢沖款單的所有付款記錄

```csharp
var payments = await _financialTransactionService.GetPaymentsBySetoffDocumentIdAsync(123);

// payments 會包含所有 TransactionType = SetoffPayment 且 SourceDocumentId = 123 的記錄
```

### 查詢客戶的預收款餘額

```csharp
var prepayments = await _financialTransactionService.GetPrepaymentsByCustomerIdAsync(456);

foreach (var prepayment in prepayments)
{
    var availableBalance = await _financialTransactionService.GetPrepaymentAvailableBalanceAsync(prepayment.Id);
    Console.WriteLine($"預收款 {prepayment.TransactionNumber}: 原始金額 {prepayment.Amount}, 可用餘額 {availableBalance}");
}
```

---

## 資料完整性

### 必填欄位檢查

**SetoffPayment 類型:**
- ✅ TransactionType = SetoffPayment
- ✅ TransactionNumber (唯一)
- ✅ TransactionDate
- ✅ Amount > 0
- ✅ PaymentMethodId (必須)
- ✅ SourceDocumentId (SetoffDocument.Id)
- ✅ SourceDocumentType = "SetoffDocument"
- ⚠️ BankId (視付款方式而定)
- ⚠️ PaymentAccount (視付款方式而定)

**Prepayment/Prepaid 類型:**
- ✅ TransactionType = Prepayment 或 Prepaid
- ✅ TransactionNumber (唯一)
- ✅ TransactionDate
- ✅ Amount > 0
- ✅ CustomerId (Prepayment) 或 VendorId (Prepaid)
- ✅ PaymentMethodId (建議)

**PrepaymentUsage/PrepaidUsage 類型:**
- ✅ TransactionType = PrepaymentUsage 或 PrepaidUsage
- ✅ Amount > 0
- ✅ SourceDocumentId (SetoffDocument.Id)
- ✅ SourceDetailId (原始 Prepayment Transaction.Id)

---

## 待實作項目

### ✅ 已完成
1. 擴充 `FinancialTransaction` 實體欄位
2. 擴充 `FinancialTransactionTypeEnum` 列舉
3. 新增計算屬性
4. 擴充 `IFinancialTransactionService` 介面

### ⏳ 進行中
1. 實作 `FinancialTransactionService` 的新方法

### 📋 待完成
1. 建立或更新 DTO 模型
2. 更新 DbContext 配置 (Foreign Key 關聯)
3. 重構 `SetoffPaymentDetailManagerComponent`
4. 重構 `SetoffPrepaymentManagerComponent`
5. 建立單元測試
6. 建立整合測試

---

## 注意事項

⚠️ **暫不執行 Migration**
- 等待所有服務層和 Razor 組件重構完成
- 確保新舊系統可以並存過渡

⚠️ **資料一致性**
- 使用 Transaction 確保付款記錄的原子性
- 預收預付款的餘額計算需要即時準確

⚠️ **效能考量**
- 預收預付款餘額查詢可能需要快取
- 大量付款記錄的查詢需要分頁

⚠️ **向後相容**
- 保留舊有 DTO 的相容性
- 提供遷移輔助工具

---

**文檔建立者**: GitHub Copilot  
**最後更新**: 2025年10月4日  
**相關文檔**: README_沖款系統重構.md
