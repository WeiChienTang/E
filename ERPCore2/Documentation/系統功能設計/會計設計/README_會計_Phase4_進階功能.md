# 會計模組 Phase 4：進階功能

## 更新日期
2026-03-08

## 優先等級
🟢 低優先（Phase 1-3 完成後執行）

---

## 概述

Phase 4 包含進一步完善帳務管理的功能：**現金流量表**、**報表匯出**、**銀行對帳**、**傳票附件**。
這些功能不影響基礎帳務正確性，但能顯著提升財務管理品質。

---

## P4-A：現金流量表（Cash Flow Statement）

### 定義

現金流量表為三大財務報表之一（損益表、資產負債表、現金流量表），
顯示現金在特定期間的流入/流出，按三大類別分類：
- **營業活動**：日常業務產生的現金
- **投資活動**：購置/出售長期資產
- **籌資活動**：股東投資、借款、還款

### 採用方法：間接法（Indirect Method）

間接法從**稅前損益**出發，調整非現金項目得出營業活動現金流量。
較直接法容易實作（不需追蹤每筆現金流向）。

### FN014 — 現金流量表

**篩選條件：** StartDate / EndDate（FilterDateRange）

**報表結構（間接法）：**
```
一、營業活動之現金流量
  稅前損益（來自 FN007）                    xxx
  調整項目：
    加：折舊費用（Depreciation）              xxx
    加：應收帳款減少（或減：應收帳款增加）   (xxx)
    加：存貨減少（或減：存貨增加）           (xxx)
    加：應付帳款增加（或減：應付帳款減少）    xxx
  營業活動淨現金流量                         xxx

二、投資活動之現金流量
  購置固定資產                              (xxx)
  出售固定資產                               xxx
  投資活動淨現金流量                        (xxx)

三、籌資活動之現金流量
  股東增資                                   xxx
  償還借款                                  (xxx)
  籌資活動淨現金流量                         xxx

本期現金增減                                 xxx
期初現金餘額                                 xxx
期末現金餘額                                 xxx
```

### 實作注意事項

- 調整項目需識別各科目性質（哪些是應收類、應付類、存貨類）
- 若科目表與標準科目一致，可用科目代碼前綴判斷
- 投資活動 / 籌資活動若無對應業務模組（如固定資產管理），初版可留白

**依賴：** 需 Phase 1（期初餘額）完成後，期初/期末餘額差額才正確。

---

## P4-B：報表匯出（Excel / PDF）

### 現狀

所有報表（FN005-FN013）目前只支援瀏覽器預覽/列印，
無法直接匯出 Excel 或 PDF 供存檔。

### 設計方案

#### Excel 匯出

使用 `ClosedXML` 或 `EPPlus` 套件（評估現有專案是否已安裝）：
- 每份報表 Service 新增 `ExportToExcelAsync(criteria)` 方法
- 回傳 `MemoryStream`（`application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`）
- 報表預覽 Modal 的「匯出 Excel」按鈕觸發下載

#### PDF 匯出

選項 A：**瀏覽器列印轉 PDF**（目前已支援，使用者可在列印對話框選擇「儲存為 PDF」）
選項 B：**伺服器端 PDF 產生**（使用 `PuppeteerSharp` 或 `IronPdf`）

> **建議：** 初版以選項 A 為主（0 開發成本），未來有需求再升級為選項 B。

**Excel 欄位格式：**
- 數字欄：Number Format `#,##0.00`
- 日期欄：`yyyy/MM/dd`
- 標題行：粗體、背景色
- 分組小計行：底線

---

## P4-C：銀行對帳（Bank Reconciliation）

### 功能描述

將銀行存款科目的帳面餘額與銀行對帳單（Bank Statement）進行核對，
找出未入帳項目和差異。

### 基本架構

**新 Entity：** `BankStatement`（銀行對帳單）
| 欄位 | 說明 |
|------|------|
| BankAccountId | 對應銀行存款科目 FK |
| StatementDate | 對帳單日期 |
| ClosingBalance | 銀行對帳單結餘 |

**新 Entity：** `BankStatementLine`（對帳單明細）
| 欄位 | 說明 |
|------|------|
| TransactionDate | 交易日期 |
| Description | 交易說明 |
| DebitAmount | 支出 |
| CreditAmount | 收入 |
| IsMatched | 是否已配對傳票 |
| MatchedJournalEntryLineId | 配對的傳票分錄 FK |

**對帳流程：**
1. 匯入銀行對帳單（手動輸入或 CSV 匯入）
2. 系統自動比對銀行明細與傳票分錄（依金額、日期）
3. 顯示未配對項目（可能是未入帳或時間差）
4. 手動確認配對或新增調整分錄

**初版範圍：** 手動輸入對帳單 + 手動配對，不含自動比對演算法。

---

## P4-D：傳票附件（Journal Entry Attachments）

### 功能描述

傳票支援上傳掃描文件（發票、收據、合約等），提供數位存檔。

### 設計方案

沿用系統現有附件機制（若已有其他模組實作）：

**新 Entity：** `JournalEntryAttachment`
| 欄位 | 說明 |
|------|------|
| JournalEntryId | 傳票 FK |
| FileName | 原始檔名 |
| StoredFileName | 儲存路徑/名稱 |
| FileSize | 檔案大小（bytes） |
| ContentType | MIME 類型 |
| UploadedAt | 上傳時間 |
| UploadedByEmployeeId | 上傳人員 FK |

**UI：** `JournalEntryEditModalComponent.razor` 新增附件區塊
- 上傳按鈕（拖曳或點擊選檔）
- 已上傳清單（顯示檔名、大小、上傳人）
- 預覽按鈕（圖片/PDF inline 顯示）
- 下載按鈕
- 刪除按鈕（已過帳傳票的附件仍可刪除，因附件不影響帳務）

**儲存位置：** `wwwroot/uploads/journal-attachments/{year}/{month}/` 或 Azure Blob Storage

---

## 完成標準（Definition of Done）

### P4-A
- [ ] 現金流量表 Service 實作間接法邏輯
- [ ] FN014 加入 ReportRegistry.cs
- [ ] 三大類別（營業/投資/籌資）正確顯示
- [ ] 期末現金餘額與 FN008 銀行現金科目餘額吻合

### P4-B
- [ ] 所有財務報表（FN005-FN013）支援 Excel 匯出
- [ ] Excel 格式規範（標題、數字格式、分組）
- [ ] 報表預覽 Modal 匯出按鈕正確觸發下載

### P4-C
- [ ] BankStatement + BankStatementLine Entity + Migration
- [ ] 銀行對帳頁面：輸入對帳單、顯示配對狀態
- [ ] 基本手動配對功能可操作

### P4-D
- [ ] JournalEntryAttachment Entity + Migration
- [ ] 上傳/下載/刪除附件功能
- [ ] EditModal 附件區塊顯示正確

---

## 相關文件

- [README_會計設計總綱.md](README_會計設計總綱.md)
- [README_會計_財務報表.md](README_會計_財務報表.md)
- [README_會計_Phase3_帳務管理.md](README_會計_Phase3_帳務管理.md)
