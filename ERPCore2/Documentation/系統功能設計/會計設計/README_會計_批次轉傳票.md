# 批次轉傳票設計

## 更新日期
2026-03-17

---

## 概述

業務單據（進貨入庫、進貨退回、銷貨出貨、銷貨退回、沖款單）確認後不立即轉傳票，改由會計人員月底審核完畢後，透過批次轉傳票頁面一次執行，轉換後自動過帳並鎖定。

**設計理由：** 業務單據確認後仍可能修改內容，即時轉傳票會造成傳票與單據金額不符。批次方式讓會計人員有充裕時間確認後再執行。

---

## 1. 業務 Entity IsJournalized 欄位

五個業務 Entity 各新增：

```csharp
public bool IsJournalized { get; set; } = false;
public DateTime? JournalizedAt { get; set; }
```

| Entity | 檔案路徑 |
|--------|---------|
| PurchaseReceiving | `Data/Entities/Purchase/PurchaseReceiving.cs` |
| PurchaseReturn | `Data/Entities/Purchase/PurchaseReturn.cs` |
| SalesDelivery | `Data/Entities/Sales/SalesDelivery.cs` |
| SalesReturn | `Data/Entities/Sales/SalesReturn.cs` |
| SetoffDocument | `Data/Entities/FinancialManagement/SetoffDocument.cs` |

---

## 2. 批次轉傳票服務

**檔案：** `Services/FinancialManagement/IJournalEntryAutoGenerationService.cs` / `JournalEntryAutoGenerationService.cs`

### 科目常數（商業會計項目表 112 年度）

| 常數 | 代碼 | 科目名稱 |
|------|------|---------|
| AccountReceivableCode | 1191 | 應收帳款 |
| AccountPayableCode | 2171 | 應付帳款 |
| InventoryCode | 1231 | 品項存貨 |
| SalesRevenueCode | 4111 | 銷貨收入 |
| CostOfGoodsSoldCode | 5111 | 銷貨成本 |
| InputVatCode | 1268 | 進項稅額 |
| OutputVatCode | 2204 | 銷項稅額 |
| BankDepositCode | 1113 | 銀行存款 |
| SalesAllowanceCode | 4114 | 銷貨折讓 |
| PurchaseAllowanceCode | 5124 | 進貨折讓 |
| AdvanceFromCustomerCode | 2221 | 預收貨款 |
| AdvanceToSupplierCode | 1266 | 預付貨款 |

### 分錄規則

> 稅額=0 時省略稅額行；**COGS 為必要分錄**，若找不到對應庫存異動（InventoryTransaction）則拒絕轉傳票並要求先完成出庫/入庫作業。

```
進貨入庫：
  借：品項存貨 (1231)  = TotalAmount
  借：進項稅額 (1268)  = TaxAmount
    貸：應付帳款 (2171) = TotalAmountIncludingTax

進貨退回：
  借：應付帳款 (2171)  = TotalReturnAmountWithTax
    貸：品項存貨 (1231) = TotalReturnAmount
    貸：進項稅額 (1268) = ReturnTaxAmount

銷貨出貨：
  借：應收帳款 (1191)  = TotalAmountWithTax
    貸：銷貨收入 (4111) = TotalAmount
    貸：銷項稅額 (2204) = TaxAmount
  借：銷貨成本 (5111)  = COGS（移動加權均價 × 出庫量）
    貸：品項存貨 (1231) = COGS

銷貨退回：
  借：銷貨收入 (4111)  = TotalReturnAmount
  借：銷項稅額 (2204)  = ReturnTaxAmount
    貸：應收帳款 (1191) = TotalReturnAmountWithTax
  借：品項存貨 (1231)  = COGS 沖回
    貸：銷貨成本 (5111) = COGS 沖回

應收沖款（IsAccountsReceivable = true）：
  借：銀行存款 (1113)  = TotalCollectionAmount
  借：銷貨折讓 (4114)  = TotalAllowanceAmount
  借：預收貨款 (2221)  = PrepaymentSetoffAmount
    貸：應收帳款 (1191) = CurrentSetoffAmount

應付沖款（IsAccountsReceivable = false）：
  借：應付帳款 (2171)  = CurrentSetoffAmount
    貸：銀行存款 (1113) = TotalCollectionAmount
    貸：進貨折讓 (5124) = TotalAllowanceAmount
    貸：預付貨款 (1266) = PrepaymentSetoffAmount
```

### COGS 金額來源

`InventoryTransaction.TotalAmount`（`ReduceStockAsync` 出庫時記錄，等於出庫量 × 當時移動加權均價，無須重新計算）

### JournalizeXxxAsync 執行流程

1. `GetBySourceDocumentAsync` → 防重複（已有傳票則回傳失敗）
2. 讀取原始單據（含 Include Supplier/Customer）
3. `GetByCodeAsync` 取得所需 AccountItem
4. 建立 `JournalEntry` + `JournalEntryLine` 清單
5. `CreateAndPostEntryAsync`：`SaveWithLinesAsync` → `PostEntryAsync`（Draft → Posted）
6. 標記 `IsJournalized = true`、`JournalizedAt = DateTime.Now`

### 子科目整合

服務內部兩個私有 helper（詳見 [README_會計_子科目系統.md](README_會計_子科目系統.md)）：
- `GetARAccountForCustomerAsync(customerId)`：優先取客戶子科目，找不到 fallback `1191`
- `GetAPAccountForSupplierAsync(supplierId)`：優先取廠商子科目，找不到 fallback `2171`

涵蓋文件類型：進貨入庫、進貨退回、銷貨出貨、銷貨退回、沖款單、銷貨折讓、進貨折讓。

> 品項子科目（`1231.*`）本版未整合進 COGS 傳票（一張出貨單含多品項，拆分複雜）。

---

## 3. 批次轉傳票頁面

**檔案：** `Components/Pages/FinancialManagement/JournalEntryBatchPage.razor`

- 路由：`/journal-entry-batch`
- 頂部日期範圍篩選 + 查詢 / 清除按鈕
- 5 個可折疊 Section：進貨入庫、進貨退回、銷貨出貨、銷貨退回、沖款單
- 每列有「轉傳票」按鈕（單筆）
- 右上角「全部轉傳票（N 筆）」批次按鈕
- 查詢使用 `Task.WhenAll` 並行取得五類待轉清單

---

---

## 4. 業務單據相關傳票顯示（P3-C）

業務單據 EditModal 底部設有可折疊的「相關傳票」區塊，讓使用者在查看單據時可直接看到對應的傳票記錄（含沖銷傳票）。

### 實作方式

五個業務單據 EditModal 均已實作（2026-03-17 完成）：

| EditModal | 來源單據類型 |
|-----------|-------------|
| `SalesDeliveryEditModalComponent.razor` | `"SalesDelivery"` |
| `SalesReturnEditModalComponent.razor` | `"SalesReturn"` |
| `PurchaseReceivingEditModalComponent.razor` | `"PurchaseReceiving"` |
| `PurchaseReturnEditModalComponent.razor` | `"PurchaseReturn"` |
| `SetoffDocumentEditModalComponent.razor` | `"SetoffDocument"` |

### 技術架構

1. **資料查詢**：`JournalEntryService.GetAllBySourceDocumentAsync(sourceType, sourceId)`（回傳含沖銷傳票的所有相關傳票）
2. **UI 元件**：`RelatedDocumentsModalComponent`（折疊區塊）+ `JournalEntryDetailsTemplate.razor`（傳票展示模板）
3. **互動**：點擊傳票可開啟 `JournalEntryEditModalComponent` 進行詳細查看

### COGS 必要性驗證

`JournalizeSalesDeliveryAsync` 和 `JournalizeSalesReturnAsync` 對 COGS 採硬性封鎖：

- **銷貨出貨**：若 `InventoryTransaction` 找不到對應的出庫記錄（COGS = 0），**拒絕轉傳票**，提示先完成 `ReduceStockAsync`。
- **銷貨退回**：若找不到對應的退回入庫記錄（成本沖回金額 = 0），**拒絕轉傳票**，提示先完成 `AddStockAsync`。

這確保傳票借貸完整，避免損益計算失真。

---

## 相關文件

- [README_會計設計總綱.md](README_會計設計總綱.md)
- [README_會計_傳票系統.md](README_會計_傳票系統.md)（傳票 Entity 與 Service）
- [README_會計_子科目系統.md](README_會計_子科目系統.md)（子科目 fallback 邏輯）
- [README_會計_會計期間管理.md](README_會計_會計期間管理.md)（轉傳票時 PostEntryAsync 的期間驗證）
