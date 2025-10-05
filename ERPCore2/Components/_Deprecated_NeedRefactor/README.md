# 已棄用的組件 - 等待重構

## 概述
此資料夾包含使用舊架構的組件，這些組件在系統重構後已無法使用，需要改用新的 `SetoffDocument` 和 `FinancialTransaction` 架構。

## 檔案清單

### 1. AccountsReceivableSetoffEditModalComponent.razor.bak
- **原路徑**: `Components/Pages/FinancialManagement/`
- **用途**: 應收帳款沖款編輯組件
- **依賴的舊服務**:
  - `IAccountsReceivableSetoffService`
  - `IAccountsReceivableSetoffDetailService`
  - `ISettoffPaymentDetailService`
  - `IPrepaymentDetailService`
- **需要改用**:
  - `ISetoffDocumentService` (沖款單主檔)
  - `ISetoffDocumentDetailService` (沖款明細)
  - `IFinancialTransactionService` (付款和預收款記錄)

### 2. AccountsReceivableSetoffIndex.razor.bak
- **原路徑**: `Components/Pages/FinancialManagement/`
- **用途**: 應收帳款沖款列表頁面
- **依賴的舊服務**: `IAccountsReceivableSetoffService`
- **需要改用**: `ISetoffDocumentService` (SetoffType = AccountsReceivable)

### 3. AccountsReceivableSetoffFieldConfiguration.cs.bak
- **原路徑**: `Components/FieldConfiguration/`
- **用途**: 應收帳款沖款欄位配置
- **依賴的舊實體**: `AccountsReceivableSetoff`
- **需要改用**: `SetoffDocument`

### 4. SetoffDetailManagerComponent.razor.bak
- **原路徑**: `Components/Shared/SubCollections/`
- **用途**: 沖款明細管理組件（管理哪些單據被沖款）
- **依賴的舊服務**:
  - `IAccountsReceivableSetoffDetailService`
  - `IAccountsPayableSetoffDetailService`
- **需要改用**: `ISetoffDocumentDetailService`

### 5. SetoffPaymentDetailManagerComponent.razor.bak
- **原路徑**: `Components/Shared/SubCollections/`
- **用途**: 沖款付款明細管理組件
- **依賴的舊服務**: `ISettoffPaymentDetailService`
- **需要改用**: `IFinancialTransactionService` (TransactionType = Payment)

### 6. SetoffPrepaymentManagerComponent.razor.bak
- **原路徑**: `Components/Shared/SubCollections/`
- **用途**: 沖款預收/預付款管理組件
- **依賴的舊服務**: `IPrepaymentDetailService`
- **需要改用**: `IFinancialTransactionService` (TransactionType = Prepayment/Prepaid)

## 新架構設計

```
SetoffDocument (沖款單主檔)
├── SetoffDocumentDetail (沖款明細 - 記錄哪些單據被沖款)
└── FinancialTransaction (財務交易 - 統一記錄所有付款/預收預付)
    ├── TransactionType = Payment (付款)
    ├── TransactionType = Prepayment (預收款)
    ├── TransactionType = Prepaid (預付款)
    └── 包含: PaymentMethodId, BankId, CurrencyId, Amount 等
```

## 重構指南

### 商品明細查詢
- **舊方式**: 使用 `AccountsReceivableSetoffDetailService` 或 `AccountsPayableSetoffDetailService`
- **新方式**: 使用 `SetoffDocumentDetailService.GetBySetoffDocumentIdAsync()`

### 付款記錄查詢
- **舊方式**: 使用 `SetoffPaymentDetailService`
- **新方式**: 使用 `FinancialTransactionService`，篩選 `TransactionType = Payment`

### 預收/預付款記錄查詢
- **舊方式**: 使用 `PrepaymentDetailService`
- **新方式**: 使用 `FinancialTransactionService`，篩選 `TransactionType = Prepayment/Prepaid`

### 沖款單查詢
- **舊方式**: 使用 `AccountsReceivableSetoffService` 或 `AccountsPayableSetoffService`
- **新方式**: 使用 `SetoffDocumentService.GetByRelatedPartyAsync()`，並指定 `SetoffType`

## 重構優先順序

1. **高優先**: `SetoffDetailManagerComponent` - 核心功能，影響範圍大
2. **高優先**: `SetoffPaymentDetailManagerComponent` - 財務記錄關鍵組件
3. **高優先**: `SetoffPrepaymentManagerComponent` - 財務記錄關鍵組件
4. **中優先**: `AccountsReceivableSetoffEditModalComponent` - 主要編輯介面
5. **中優先**: `AccountsReceivableSetoffIndex` - 列表頁面
6. **低優先**: `AccountsReceivableSetoffFieldConfiguration` - 配置檔案

## 注意事項

1. 這些組件已被重新命名為 `.bak` 避免編譯錯誤
2. 重構時請參考這些檔案的業務邏輯
3. 新組件應該建立在原本的路徑下
4. 完成重構並測試通過後，可以刪除這些 `.bak` 檔案

## 相關文檔

- `Documentation/README_沖款系統重構.md` - 系統重構詳細說明
- `Documentation/README_FinancialTransaction_擴充說明.md` - 財務交易擴充文檔

---

**建立日期**: 2025年10月4日  
**原因**: PrepaymentDetailService 嚴重損毀，且舊架構已移除
