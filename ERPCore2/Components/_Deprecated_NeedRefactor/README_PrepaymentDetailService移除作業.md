# PrepaymentDetailService 移除與組件備份作業摘要

## 執行日期
2025年10月4日

## 問題描述
`PrepaymentDetailService.cs` 檔案嚴重損毀，程式碼重複且格式錯誤。根據系統重構文檔，`PrepaymentDetail` 實體已被移除，改用 `FinancialTransaction` 統一記錄財務交易。

## 執行動作

### 1. 刪除損毀的服務檔案
```
✅ 已刪除: Services/FinancialManagement/PrepaymentDetailService.cs
```

### 2. 移動使用舊架構的組件到備份資料夾
所有使用以下舊服務的組件都已移動並重新命名：
- `IAccountsReceivableSetoffService`
- `IAccountsReceivableSetoffDetailService`
- `IAccountsPayableSetoffService`
- `IAccountsPayableSetoffDetailService`
- `ISettoffPaymentDetailService`
- `IPrepaymentDetailService`

#### 已備份的組件清單：
```
Components/_Deprecated_NeedRefactor/
├── AccountsReceivableSetoffEditModalComponent.razor.bak
├── AccountsReceivableSetoffFieldConfiguration.cs.bak
├── AccountsReceivableSetoffIndex.razor.bak
├── SetoffDetailManagerComponent.razor.bak
├── SetoffPaymentDetailManagerComponent.razor.bak
├── SetoffPrepaymentManagerComponent.razor.bak
└── README.md (重構指南)
```

### 3. 編譯結果
```bash
dotnet build --no-incremental
```
**結果**: ✅ 建置成功，無錯誤

## 新架構說明

根據 `README_沖款系統重構.md`，新架構如下：

```
SetoffDocument (沖款單主檔)
├── SetoffDocumentDetail (沖款明細 - 記錄哪些單據被沖款)
└── FinancialTransaction (財務交易 - 統一記錄所有付款/預收預付)
    ├── TransactionType = Payment (付款)
    ├── TransactionType = Prepayment (預收款)
    ├── TransactionType = Prepaid (預付款)
    └── 包含: PaymentMethodId, BankId, CurrencyId, Amount 等
```

### 資料查詢方式

| 需求 | 舊方式 | 新方式 |
|------|--------|--------|
| 商品明細 | `AccountsReceivableSetoffDetailService` | `SetoffDocumentDetailService` |
| 付款記錄 | `SetoffPaymentDetailService` | `FinancialTransactionService` (Type=Payment) |
| 預收款 | `PrepaymentDetailService` | `FinancialTransactionService` (Type=Prepayment) |
| 預付款 | `PrepaymentDetailService` | `FinancialTransactionService` (Type=Prepaid) |
| 沖款單 | `AccountsReceivableSetoffService` | `SetoffDocumentService` (Type=AccountsReceivable) |

## 後續工作

### 需要重構的組件（依優先順序）

1. **高優先級** - 核心子組件
   - [ ] `SetoffDetailManagerComponent` - 沖款明細管理
   - [ ] `SetoffPaymentDetailManagerComponent` - 付款明細管理
   - [ ] `SetoffPrepaymentManagerComponent` - 預收/預付款管理

2. **中優先級** - 主要頁面
   - [ ] `AccountsReceivableSetoffEditModalComponent` - 編輯組件
   - [ ] `AccountsReceivableSetoffIndex` - 列表頁面

3. **低優先級** - 配置檔案
   - [ ] `AccountsReceivableSetoffFieldConfiguration` - 欄位配置

### 重構時的注意事項

1. **使用新服務**:
   - `ISetoffDocumentService` - 沖款單主檔操作
   - `ISetoffDocumentDetailService` - 沖款明細操作
   - `IFinancialTransactionService` - 財務交易記錄

2. **使用新實體**:
   - `SetoffDocument` 取代 `AccountsReceivableSetoff` / `AccountsPayableSetoff`
   - `SetoffDocumentDetail` 取代 `AccountsReceivableSetoffDetail` / `AccountsPayableSetoffDetail`
   - `FinancialTransaction` 取代 `SetoffPaymentDetail` / `PrepaymentDetail`

3. **參考舊組件**:
   - 備份的 `.bak` 檔案保留了原有的業務邏輯
   - 可參考但需要完全重寫以使用新架構

## 相關文檔

- `Documentation/README_沖款系統重構.md` - 系統重構完整說明
- `Components/_Deprecated_NeedRefactor/README.md` - 舊組件重構指南

## 狀態

- ✅ 損毀的服務檔案已移除
- ✅ 舊組件已備份
- ✅ 專案可正常編譯
- ⏳ 等待組件重構

---

**執行者**: GitHub Copilot  
**建立日期**: 2025年10月4日
