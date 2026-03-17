# 會計模組 Phase 3：帳務管理功能

## 更新日期
2026-03-14

## 優先等級
🟡 中優先（Phase 1 完成後可平行推進）

---

## 概述

Phase 3 新增三項帳務管理核心功能：**應收/應付帳齡分析**、**年底結帳**、**業務單據相關傳票顯示**。
這些功能對日常帳務管理非常重要，但不影響帳務資料的正確性。

---

## P3-A：應收/應付帳齡分析（AR/AP Aging Report）

### 問題說明

目前可透過子科目查詢應收/應付餘額，但無法看到「欠款多久了」。
帳齡分析是催收款項和付款排程的最重要工具。

### FN012 — 應收帳款帳齡分析

**檔案：**
| 檔案 | 路徑 |
|------|------|
| `ARAgingCriteria.cs` | `Models/Reports/FilterCriteria/` |
| `IARAgingReportService.cs` | `Services/Reports/Interfaces/` |
| `ARAgingReportService.cs` | `Services/Reports/` |

**篩選條件：**

| 欄位 | 類型 | 說明 |
|------|------|------|
| AsOfDate | FilterDate | 帳齡基準日（預設今日） |
| CustomerKeyword | FilterKeyword | 客戶名稱/代碼關鍵字 |
| ShowZeroBalance | FilterToggle | 顯示零餘額客戶（預設 false） |

**帳齡分組（標準分組，可依公司規則調整）：**

| 區間 | 說明 |
|------|------|
| 未到期 | 交易日距基準日 ≤ 30 天 |
| 31-60 天 | 逾期 31 到 60 天 |
| 61-90 天 | 逾期 61 到 90 天 |
| 91-120 天 | 逾期 91 到 120 天 |
| 120 天以上 | 嚴重逾期 |

**計算邏輯（決策：方法一，主檔層級）：**

方法一（採用）：以**銷貨出貨單（SalesDelivery）為計算主體**
- 每張 SalesDelivery 為一個應收款單位（非逐行品項明細）
- 帳齡天數基準 = 交貨日（`DeliveryDate`）+ `Customer.PaymentDays`（需新增，見下方說明）
- **未收金額 = `(SalesDelivery.TotalAmount + SalesDelivery.TaxAmount)` - `SUM(SetoffProductDetail.CurrentSetoffAmount WHERE SourceDetailType 關聯至此 SalesDelivery)`**

方法二（不採用）：以**傳票分錄**為計算基礎
- 依應收帳款科目（含子科目）的借貸明細追蹤
- 實作複雜，且與業務資料脫節

**⚠ 金額一致性（已確認）：**
- `JournalEntryAutoGenerationService` 的 AR 借方 = `TotalAmountWithTax`（含稅）
- `SetoffProductDetailService` 的沖款金額 = `CalculateTaxInclusiveAmount(...)`（含稅）
- **因此帳齡公式必須使用 `TotalAmount + TaxAmount`（含稅），而非僅 `TotalAmount`（稅前）**
- `SalesDelivery.DiscountAmount` 已計入 `TotalAmount`（`TotalAmount = Sum(SubtotalAmount)`，每行 `SubtotalAmount` 已扣除 `DiscountPercentage`），無需另外扣除

**查詢路徑（三層彙總）：**
```
SalesDelivery（主檔）
  └─ SalesDeliveryDetails（明細行）
       └─ SetoffProductDetail（WHERE SourceDetailType = SalesDeliveryDetail AND SourceDetailId IN detail IDs）
            └─ CurrentSetoffAmount 加總 = 已收款金額（含稅）
```

> ⚠ `SetoffProductDetail` 使用多型 FK（`SourceDetailType` + `SourceDetailId`），
> 無 DB 層外鍵約束，查詢時需用 `WHERE SourceDetailType = 5 AND SourceDetailId IN (...)` 方式處理，
> 需特別注意 N+1 查詢問題，建議一次性批量查詢後在記憶體彙總。

**客戶付款天數欄位（需新增）：**

`Customer` Entity 目前只有 `PaymentTerms`（`string?`，自由文字如「Net 30」）和 `CreditLimit`（信用額度），**沒有結構化的 `PaymentDays` 整數欄位**。

由於不同客戶有不同付款天數，需補充：
```csharp
// 新增至 Customer Entity
public int PaymentDays { get; set; } = 30;  // 付款天數，預設 30 天

// 新增至 Supplier Entity（AP 帳齡同樣需要）
public int PaymentDays { get; set; } = 30;
```

> Migration：`AddCustomerSupplierPaymentDays`

**報表格式：**
```
基準日：2026-03-08

客戶代碼  客戶名稱    未到期      31-60天    61-90天    91-120天   120天+    合計
C0001    客戶甲      50,000                 30,000               20,000    100,000
C0002    客戶乙      80,000      40,000                                     120,000
─────────────────────────────────────────────────────────────────────────────────
合計              130,000      40,000     30,000        0       20,000    220,000
```

### FN013 — 應付帳款帳齡分析

設計與 FN012 對稱，資料來源改為：
- 進貨入庫單 `PurchaseReceiving`（應付帳款來源）
- 應付沖款單（`SetoffType = AccountsPayable`）
- 篩選改為廠商關鍵字
- **未付金額 = `(PurchaseReceiving.TotalAmount + PurchaseReceiving.PurchaseReceivingTaxAmount)` - `SUM(SetoffProductDetail.CurrentSetoffAmount)`**（同樣使用含稅金額）
- 帳齡基準 = 入庫日 + `Supplier.PaymentDays`（需新增欄位，同 Customer.PaymentDays）

> ⚠ `SetoffProductDetail` 同樣以多型 FK 追蹤 `PurchaseReceivingDetail`（SourceDetailType = 3），
> 查詢邏輯與 FN012 相同，可共用同一個彙總查詢基底。

---

## P3-B：年底結帳功能（Year-End Closing）

### 問題說明

損益表科目（AccountType: Revenue/Cost/Expense/NonOperating）
在會計年度結束時需歸零，將本年度損益轉入權益科目。
否則下一年度的損益科目會繼承上年度餘額，報表數字錯誤。

### 結帳流程

```
年底結帳（執行一次）
  Step 1：確認所有月份已關帳（FiscalPeriod.Status == Closed）
  Step 2：產生 Closing Entry（JournalEntryType.Closing）
           收入科目餘額 → 本期損益（3351）
           成本科目餘額 → 本期損益（3351）
           費用科目餘額 → 本期損益（3351）
           營業外收支餘額 → 本期損益（3351）
  Step 3：產生第二筆 Closing Entry
           本期損益（3351） → 保留盈餘（3361）
  Step 4：鎖定年度所有期間（FiscalPeriodStatus.Locked）
  Step 5：初始化下一年度 12 個期間（FiscalPeriodStatus.Open）
```

### 分錄規則

**Step 2：損益科目歸零**
```
收入類（AccountType = Revenue，Credit 正常）：
  借：各收入科目  = 科目貸方餘額（清零）
    貸：本期損益 (3351) = 收入合計

成本/費用類（AccountType = Cost/Expense，Debit 正常）：
  借：本期損益 (3351) = 成本+費用合計
    貸：各成本/費用科目 = 科目借方餘額（清零）
```

**Step 3：本期損益轉保留盈餘**
```
若本年度盈利：
  借：本期損益 (3351) = 淨利
    貸：保留盈餘 (3361) = 淨利

若本年度虧損：
  借：保留盈餘 (3361) = 淨損
    貸：本期損益 (3351) = 淨損
```

### 科目常數（補充）

| 常數 | 代碼 | 科目名稱 |
|------|------|---------|
| RetainedEarningsCode | 3361 | 保留盈餘（法定盈餘公積） |
| CurrentPeriodIncomeCode | 3351 | 本期損益 |

### Service 設計

**方法：** `FiscalYearClosingService.ExecuteYearEndClosingAsync(year)`

| 步驟 | 說明 |
|------|------|
| 0. 冪等性檢查 | 確認該年度尚未結帳：若已存在 `JournalEntryType.Closing` 的傳票（FiscalYear = year），**拋出錯誤，禁止重複執行** |
| 1. 前置檢查 | 確認所有 12 個期間均已 Closed（非 Open / Closing）；操作人員需 `Accounting.YearEndClosing` 權限 |
| 2. 計算損益科目餘額 | 依 AccountType 彙總科目餘額（Revenue / Cost / Expense / NonOperating） |
| 3. 建立結帳傳票 (Closing) | Step 2 分錄（損益科目歸零 → 本期損益） |
| 4. 建立轉帳傳票 (Closing) | Step 3 分錄（本期損益 → 保留盈餘） |
| 5. 鎖定年度期間 | 所有 FiscalPeriod → Locked（Locked 不可重開） |
| 6. 初始化下年度 | 呼叫 FiscalPeriodService.InitializeYearAsync(year+1) |

**操作前需輸入確認（含警告）：**
> ⚠ 年底結帳執行後將鎖定 {year} 年所有期間，無法修改該年度任何傳票。請確認所有傳票已完成審核。

### UI 設計

**位置：** `FiscalPeriodIndex.razor` 新增「年度結帳」按鈕（需 SuperAdmin 或特別權限）
- 顯示結帳前置檢查結果
- 二次確認對話框
- 執行結果顯示

---

## P3-C：業務單據相關傳票顯示

> ✅ **可立即執行：** `JournalEntryService.GetBySourceDocumentAsync` 已實作，
> 五個 EditModal 的 UI 折疊區塊是唯一待完成部分，無任何後端依賴。

### 問題說明

目前業務單據（進貨入庫、銷貨出貨等）的 EditModal 無法直接看到對應傳票，
需要去傳票列表搜尋 SourceDocumentCode 才能找到。

### 設計方案

在以下 EditModal 的**底部**新增一個折疊區塊「相關傳票」：
- `PurchaseReceivingEditModal.razor`
- `SalesDeliveryEditModal.razor`
- `PurchaseReturnEditModal.razor`
- `SalesReturnEditModal.razor`
- `SetoffDocumentEditModal.razor`

**折疊區塊內容：**
| 欄位 | 說明 |
|------|------|
| 傳票號碼 | 超連結，點擊開啟 JournalEntryEditModal |
| 傳票日期 | |
| 狀態 badge | Draft / Posted / Reversed |
| 借方合計 / 貸方合計 | |

**查詢方式：**
```csharp
// SourceDocumentType = "PurchaseReceiving"，SourceDocumentId = 當前單據 ID
var entries = await JournalEntryService.GetBySourceDocumentAsync(
    "PurchaseReceiving", document.Id);
```

**未轉傳票時顯示：**
> 此單據尚未轉傳票。[前往批次轉傳票] （連結）

---

## 完成標準（Definition of Done）

### P3-A
- [ ] `Customer` + `Supplier` 新增 `PaymentDays` (int, 預設 30) 欄位 + Migration
- [ ] `ARAgingCriteria.cs` + Service + 報表實作
  - 計算單位：SalesDelivery 主檔
  - 未收金額 = `TotalAmount + TaxAmount - SUM(SetoffProductDetail.CurrentSetoffAmount)`
  - 帳齡基準 = `DeliveryDate + Customer.PaymentDays`
- [ ] `APAgingCriteria.cs` + Service + 報表實作
  - 計算單位：PurchaseReceiving 主檔
  - 未付金額 = `TotalAmount + PurchaseReceivingTaxAmount - SUM(SetoffProductDetail.CurrentSetoffAmount)`
  - 帳齡基準 = `ReceiptDate + Supplier.PaymentDays`
- [ ] SetoffProductDetail 多型 FK 批量查詢（避免 N+1）
- [ ] FN012 / FN013 加入 ReportRegistry.cs
- [ ] 報表格式正確顯示帳齡分組

### P3-B
- [ ] `FiscalYearClosingService` 實作完整結帳流程
- [ ] 結帳後損益科目餘額歸零（JournalEntry 方式驗證）
- [ ] 結帳後資產負債表顯示保留盈餘正確增加
- [ ] FiscalPeriodIndex UI 支援年度結帳操作
- [ ] 結帳後無法對已鎖定期間過帳

### P3-C
- [ ] 五個 EditModal 新增「相關傳票」折疊區塊
- [ ] 傳票連結正確開啟 JournalEntryEditModal
- [ ] 未轉傳票時顯示提示連結

---

## 相關文件

- [README_會計設計總綱.md](README_會計設計總綱.md)
- [README_會計_傳票系統.md](README_會計_傳票系統.md)
- [README_會計_財務報表.md](README_會計_財務報表.md)
- [README_會計_Phase1_基礎補強.md](README_會計_Phase1_基礎補強.md)
- [README_會計_Phase4_進階功能.md](README_會計_Phase4_進階功能.md)
