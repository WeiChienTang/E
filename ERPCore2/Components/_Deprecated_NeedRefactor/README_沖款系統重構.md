# 沖款系統重構文檔

## 重構日期
2025年10月4日

## 重構目標
統一應收帳款和應付帳款的沖款處理，簡化系統架構，減少重複代碼。

## 架構設計方案

### ✅ 採用方案 A: FinancialTransaction 統一記錄

```
SetoffDocument (沖款單主檔)
├── SetoffDocumentDetail (沖款明細 - 記錄哪些單據被沖款)
└── FinancialTransaction (財務交易 - 統一記錄所有付款/預收預付)
    ├── TransactionType = Payment (付款)
    ├── TransactionType = Prepayment (預收款)
    ├── TransactionType = Prepaid (預付款)
    └── 包含: PaymentMethodId, BankId, CurrencyId, Amount 等
```

**設計優勢:**
- ✅ 所有財務記錄統一在 `FinancialTransaction`，便於追蹤和報表
- ✅ 不需要 `PrepaymentDetail` 關聯表，減少資料表數量
- ✅ 查詢和報表更簡單，減少 JOIN 操作
- ✅ 統一的財務交易記錄格式，易於審計

**實作要點:**
- `FinancialTransaction` 需包含：
  - `SetoffDocumentId` - 關聯沖款單
  - `PaymentMethodId` - 付款方式
  - `BankId` - 銀行 (可選)
  - `CurrencyId` - 貨幣
  - `TransactionType` - 區分付款/預收/預付
  - `Amount` - 金額
- 原有的 `Prepayment` 資料表可保留作為預收預付款的主檔
- `SetoffPaymentDetailManagerComponent` 改用 `FinancialTransaction` (TransactionType = Payment)
- `SetoffPrepaymentManagerComponent` 改用 `FinancialTransaction` (TransactionType = Prepayment/Prepaid)

## 主要變更

### 1. 資料表合併

#### 已刪除的舊資料表
- ✅ `AccountsReceivableSetoff` (應收帳款沖款單)
- ✅ `AccountsReceivableSetoffDetail` (應收帳款沖款明細)
- ✅ `AccountsReceivableSetoffPaymentDetail` (應收帳款付款明細)
- ✅ `AccountsPayableSetoff` (應付帳款沖款單)
- ✅ `AccountsPayableSetoffDetail` (應付帳款沖款明細)
- ✅ `AccountsPayableSetoffPaymentDetail` (應付帳款付款明細)
- ✅ `PrepaymentDetail` (預收/預付款明細) - 改用 FinancialTransaction

#### 新增的統一資料表
- ✅ `SetoffDocument` - 統一沖款單
  - 包含 `SetoffType` 欄位區分應收/應付
  - 包含 `RelatedPartyId` 和 `RelatedPartyType` 彈性處理客戶/供應商
  
- ✅ `SetoffDocumentDetail` - 統一沖款明細
  - 使用 `SourceDocumentType` 和 `SourceDocumentId` 識別來源單據
  - 支援銷貨/進貨/退貨等各種單據類型

- ✅ `SetoffType` (列舉)
  - `AccountsReceivable = 1` - 應收帳款沖款
  - `AccountsPayable = 2` - 應付帳款沖款

### 2. 服務層重構

#### 已刪除的舊服務
- ✅ `AccountsReceivableSetoffService`
- ✅ `IAccountsReceivableSetoffService`
- ✅ `AccountsReceivableSetoffDetailService`
- ✅ `IAccountsReceivableSetoffDetailService`
- ✅ `AccountsPayableSetoffService`
- ✅ `IAccountsPayableSetoffService`
- ✅ `AccountsPayableSetoffDetailService`
- ✅ `IAccountsPayableSetoffDetailService`
- ✅ `SetoffPaymentDetailService`
- ✅ `ISetoffPaymentDetailService`
- ✅ `AccountsPayableSetoffPaymentDetailService`
- ✅ `IAccountsPayableSetoffPaymentDetailService`
- ✅ `PrepaymentDetailService`
- ✅ `IPrepaymentDetailService`

#### 新增的統一服務
- ✅ `SetoffDocumentService` / `ISetoffDocumentService`
  - `GetByRelatedPartyAsync()` - 根據關聯方取得沖款單
  - `GetByDateRangeAsync()` - 根據日期範圍查詢
  - `IsSetoffNumberExistsAsync()` - 檢查單號重複

- ✅ `SetoffDocumentDetailService` / `ISetoffDocumentDetailService`
  - `GetBySetoffDocumentIdAsync()` - 根據沖款單ID取得明細
  - `GetBySourceDocumentAsync()` - 根據來源單據查詢
  - `GetTotalSetoffAmountBySourceAsync()` - 計算累計沖款金額

### 3. DbContext 更新

#### 已移除
```csharp
// 舊的 DbSet
public DbSet<AccountsReceivableSetoff> AccountsReceivableSetoffs { get; set; }
public DbSet<AccountsReceivableSetoffDetail> AccountsReceivableSetoffDetails { get; set; }
public DbSet<AccountsReceivableSetoffPaymentDetail> AccountsReceivableSetoffPaymentDetails { get; set; }
public DbSet<AccountsPayableSetoff> AccountsPayableSetoffs { get; set; }
public DbSet<AccountsPayableSetoffDetail> AccountsPayableSetoffDetails { get; set; }
public DbSet<AccountsPayableSetoffPaymentDetail> AccountsPayableSetoffPaymentDetails { get; set; }
public DbSet<PrepaymentDetail> PrepaymentDetails { get; set; }

// 舊的 Entity Configuration
modelBuilder.Entity<PrepaymentDetail>() ...
```

#### 已新增
```csharp
// 新的 DbSet
public DbSet<SetoffDocument> SetoffDocuments { get; set; }
public DbSet<SetoffDocumentDetail> SetoffDocumentDetails { get; set; }

// 新的 Entity Configuration
modelBuilder.Entity<SetoffDocument>() ...
modelBuilder.Entity<SetoffDocumentDetail>() ...
```

### 4. ServiceRegistration 更新

#### 已移除註冊
```csharp
services.AddScoped<IAccountsReceivableSetoffService, AccountsReceivableSetoffService>();
services.AddScoped<IAccountsReceivableSetoffDetailService, AccountsReceivableSetoffDetailService>();
services.AddScoped<IAccountsPayableSetoffService, AccountsPayableSetoffService>();
services.AddScoped<IAccountsPayableSetoffDetailService, AccountsPayableSetoffDetailService>();
services.AddScoped<ISettoffPaymentDetailService, SettoffPaymentDetailService>();
services.AddScoped<IAccountsPayableSetoffPaymentDetailService, AccountsPayableSetoffPaymentDetailService>();
services.AddScoped<IPrepaymentDetailService, PrepaymentDetailService>();
```

#### 已新增註冊
```csharp
services.AddScoped<ISetoffDocumentService, SetoffDocumentService>();
services.AddScoped<ISetoffDocumentDetailService, SetoffDocumentDetailService>();
```

## 待完成工作

### ⚠️ 需要處理的 Razor 組件和頁面

以下組件需要更新或刪除：

1. **FieldConfiguration**
   - `AccountsReceivableSetoffFieldConfiguration.cs` - 需要改用 SetoffDocument

2. **頁面組件**
   - `AccountsReceivableSetoffIndex.razor` - 需要重寫或刪除
   - `AccountsReceivableSetoffEditModalComponent.razor` - 需要重寫或刪除
   - `AccountsPayableSetoffIndex.razor` (如有) - 需要重寫或刪除
   - `AccountsPayableSetoffEditModalComponent.razor` (如有) - 需要重寫或刪除

3. **子組件**
   - `SetoffDetailManagerComponent.razor` - 需要改用新服務
   - `SetoffPaymentDetailManagerComponent.razor` - 需要刪除 (改用 FinancialTransaction)
   - `SetoffPrepaymentManagerComponent.razor` - 需要刪除 (改用 FinancialTransaction)

### 📋 實施計劃

#### 階段 1: 資料層與服務層準備 (進行中)
- ✅ 建立 `SetoffDocument` 實體
- ✅ 建立 `SetoffDocumentDetail` 實體
- ✅ 建立 `SetoffDocumentService` 和 `SetoffDocumentDetailService`
- ✅ 更新 `DbContext` (已新增 DbSet，但暫未 Migration)
- ⏸️ **暫緩 Migration** - 因舊有 Razor 組件仍在使用舊資料表

#### 階段 2: 調整 FinancialTransaction (待完成)
- ⚠️ 擴充 `FinancialTransaction` 以支援付款記錄
  - 確認欄位: `PaymentMethodId`, `BankId`, `CurrencyId`
  - 新增 `TransactionType` 列舉值: `Payment`, `Prepayment`, `Prepaid`
  - 建立相關服務方法處理沖款付款記錄

#### 階段 3: Razor 組件重構 (待評估)
- 🔍 **評估現有組件:**
  - `SetoffPaymentDetailManagerComponent.razor` - 改用 `FinancialTransaction` (Type=Payment)
  - `SetoffPrepaymentManagerComponent.razor` - 改用 `FinancialTransaction` (Type=Prepayment/Prepaid)
  - `AccountsReceivableSetoffIndex.razor` - 改用 `SetoffDocument`
  - `AccountsReceivableSetoffEditModalComponent.razor` - 改用 `SetoffDocument`

- 📝 **決策待定:**
  - 選項 A: 保留現有組件，逐步遷移到新架構
  - 選項 B: 刪除舊組件，重新設計新介面
  
#### 階段 4: Migration 與資料遷移 (延後)
- ⏰ **執行時機:** 所有 Razor 組件完成重構後
- 📋 **待執行:**
  1. 評估生產環境是否有舊資料需要遷移
  2. 編寫資料遷移腳本 (如需要)
  3. 刪除舊的 Migration 檔案
  4. 執行 `dotnet ef migrations add SetoffSystemRefactor`
  5. 執行 `dotnet ef database update`

## 優勢分析

### ✅ 減少維護成本
- 從 6 個資料表 → 2 個資料表
- 從 14 個服務檔案 → 4 個服務檔案
- 統一的業務邏輯，修改一處即可

### ✅ 提升可擴展性
- 使用列舉區分類型，易於新增其他沖款類型
- 使用泛型欄位 (RelatedPartyId, SourceDocumentType) 易於支援新單據

### ✅ 符合 DRY 原則
- 消除重複代碼
- 統一錯誤處理邏輯
- 一致的 API 介面

### ✅ 簡化財務記錄
- 所有付款記錄統一使用 `FinancialTransaction`
- 減少資料表關聯複雜度
- 更清晰的資料流向

## 注意事項

⚠️ **這是破壞性變更**
- 所有使用舊服務的代碼都需要更新
- 建議在開發環境完全測試後再部署

⚠️ **資料遷移策略**
- **暫緩執行 Migration** - 等待 Razor 組件重構完成
- 現階段新舊資料表並存，確保系統穩定運行
- 如果生產環境有資料，需要編寫資料遷移腳本
- Migration 將在所有組件遷移完成後統一執行

⚠️ **前端組件並存期**
- 舊有 Razor 組件仍使用舊資料表和舊服務
- 新組件開發時使用新架構
- 逐步替換，降低風險
- 確保業務不中斷

⚠️ **FinancialTransaction 擴充需求**
- 需要確認現有 `FinancialTransaction` 是否已包含所需欄位
- 可能需要新增 `PaymentMethodId`, `BankId`, `CurrencyId` 等欄位
- 需要擴充 `TransactionType` 列舉以支援付款/預收/預付類型
- 相關服務層需要新增處理沖款付款的方法

## 設計模式

### Repository Pattern
所有服務繼承自 `GenericManagementService<T>`，提供標準 CRUD 操作。

### 單一職責原則
- `SetoffDocument` 只負責沖款單主檔
- `SetoffDocumentDetail` 只負責沖款明細
- `FinancialTransaction` 負責所有財務交易記錄

### 開放封閉原則
使用列舉和泛型欄位，易於擴展而不需修改現有代碼。

## 測試建議

1. **單元測試**
   - 測試 `SetoffDocumentService` 的所有方法
   - 測試 `SetoffDocumentDetailService` 的所有方法
   - 測試 `FinancialTransaction` 付款記錄的新增/查詢方法
   - 測試驗證邏輯

2. **整合測試**
   - 測試完整的沖款流程
   - 測試與 `FinancialTransaction` 的整合
   - 測試付款記錄和預收預付的記錄流程
   - 測試資料完整性約束

3. **UI 測試**
   - 測試新的沖款頁面
   - 測試舊有頁面的兼容性
   - 測試各種邊界條件
   - 測試錯誤處理

## 下一步行動項目

### 🎯 近期優先項目

1. **確認 FinancialTransaction 欄位設計**
   - 檢視現有欄位是否足夠
   - 確認是否需要新增 `PaymentMethodId`, `BankId`, `CurrencyId`
   - 確認 `TransactionType` 列舉是否需要擴充

2. **評估 Razor 組件現狀**
   - 盤點所有使用舊服務的組件
   - 分析哪些組件可以保留並遷移
   - 規劃組件重構的優先順序

3. **建立過渡期兼容策略**
   - 確保新舊服務可以並存
   - 設計逐步遷移的路徑
   - 準備回滾方案

### 📅 中長期規劃

1. **Phase 1: 完善 FinancialTransaction 服務層**
   - 實作付款記錄相關方法
   - 實作預收預付款查詢方法
   - 建立相關的 DTO

2. **Phase 2: 重構關鍵 Razor 組件**
   - 優先重構 `SetoffPaymentDetailManagerComponent`
   - 重構 `SetoffPrepaymentManagerComponent`
   - 測試並驗證功能

3. **Phase 3: 執行 Migration**
   - 確認所有組件已遷移完成
   - 執行資料庫遷移
   - 刪除舊資料表和舊服務
   - 清理相關代碼

---

**文檔建立者**: GitHub Copilot  
**最後更新**: 2025年10月4日  
**架構方案**: 方案 A - FinancialTransaction 統一記錄  
**Migration 狀態**: 暫緩 (等待 Razor 組件重構完成)
