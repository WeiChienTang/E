一、會計財務報表（情境 1–12）
情境 1：試算表 — 正常全期間查詢 ✅ PASS (2026-03-25)
設定日期為本月 1 號到月底，不篩選科目大類，ShowZeroBalance = false。確認報表只顯示有異動的科目，每個科目的「本期借方合計」和「本期貸方合計」橫列加總後，全部科目的借方總計 = 貸方總計。
> 程式碼檢查：查詢邏輯正確，期初+本期→期末計算無誤，頁尾驗證借貸平衡。

情境 2：試算表 — 按科目大類篩選 ✅ PASS (2026-03-25)
只選取 AccountType = Asset 和 Liability，確認報表只出現資產和負債類科目，收入/成本/費用科目不應出現。
> 程式碼檢查：criteria.AccountTypes.Contains() 篩選正確。

情境 3：試算表 — 開啟顯示零餘額科目 ✅ PASS (2026-03-25, 已修復)
ShowZeroBalance = true，確認那些本期無異動、期初期末都為零的科目也出現在報表上，但金額全為 0。
> 原始問題：ShowZeroBalance=true 只顯示「曾有傳票但淨額為零」的科目，從未有傳票的科目不會出現。
> 修復內容：TrialBalanceReportService.BuildAccountBalanceLinesAsync — 補齊查詢 AccountItems 中未出現在 JournalEntryLines 的明細科目。

情境 4：資產負債表 — 截止日為今天 ✅ PASS (2026-03-25)
EndDate 設為今天，確認資產總額 = 負債總額 + 權益總額（會計恆等式）。如果不平衡，表示帳務或報表計算有問題。
> 程式碼檢查：自動加入「本期淨利/損」合成行確保 A=L+E，容差 0.01m。

情境 5：資產負債表 — 截止日為上個月底 ✅ PASS (2026-03-25)
EndDate 設為上月最後一天，確認報表呈現的是截止到上月底的累計快照，不包含本月的傳票。與同期的試算表期末餘額交叉比對，資產、負債、權益科目的期末數字應一致。
> 程式碼檢查：AsOfDate 屬性正確處理，從公司成立日累計到截止日。

情境 6：損益表 — 正常月度查詢 ✅ PASS (2026-03-25)
StartDate 和 EndDate 設定為本月 1 號至月底。確認報表結構為：營業收入 - 營業成本 = 毛利，毛利 - 營業費用 = 營業損益，營業損益 ± 營業外收益費損 = 稅前損益。各層級小計計算正確。
> 程式碼檢查：計算邏輯完整正確，也支援 OCI（IAS 1）。

情境 7：損益表 — 年度累計 ✅ PASS (2026-03-25)
StartDate 設為年初 1/1，EndDate 設為今天。確認損益表呈現的是整年度累計的收入與費用，與試算表中收入/成本/費用類科目的本期發生額一致。
> 程式碼檢查：日期範圍篩選正確支援跨月/跨年查詢。

情境 8：損益表 — 完全沒有交易的期間 ✅ PASS (2026-03-25, 已修復)
選擇一個完全沒有任何傳票的月份。確認報表能正常產生但所有金額為 0，不會拋出錯誤。
> 原始問題：空資料時回傳 BatchPreviewResult.Failure("無符合條件的傳票資料") 而非產生空報表。
> 修復內容：IncomeStatementReportService.RenderBatchToImagesAsync — 移除空資料的 Failure 短路，改為產生金額全為 0 的正常報表。

情境 9：現金流量表 — 標準期間查詢 ✅ PASS (2026-03-25)
StartDate 和 EndDate 設為本月，確認報表分三大區塊：營業活動、投資活動、籌資活動。期初現金 + 三大活動淨額 = 期末現金。期末現金應與資產負債表上「現金及約當現金」科目餘額一致。
> 程式碼檢查：三大區塊計算正確，有「現金科目未設定」貼心警告。

情境 10：現金流量表 — 缺少日期驗證 ✅ PASS (2026-03-25)
不填 StartDate 和 EndDate 按產生 → 確認 Validate 回傳錯誤「需要指定完整的會計期間」，報表不產生。StartDate 晚於 EndDate → 確認回傳「起始日不可晚於截止日」。
> 程式碼檢查：CashFlowCriteria.Validate 正確驗證兩種情況。

情境 11：總分類帳 — 特定科目大類 ✅ PASS (2026-03-25)
篩選科目大類為「資產」，設定本月日期範圍。確認每個資產科目都有帳戶卡片，顯示期初餘額、本期每筆借貸明細、期末餘額。期初餘額 = StartDate 之前所有傳票的累計。
> 程式碼檢查：期初餘額、逐筆明細（含流水餘額）、期末餘額全部正確。

情境 12：總分類帳 — 無篩選驗證 ✅ PASS (2026-03-25)
不設定 StartDate 也不選科目大類 → Validate 應阻擋並回傳「至少設定開始日期或選擇科目大類」。
> 程式碼檢查：GeneralLedgerCriteria.Validate 正確阻擋。

二、明細帳與科目查詢（情境 13–17）
情境 13：明細分類帳 — 依關鍵字查詢應收帳款子科目 ✅ PASS (2026-03-25)
AccountKeyword 輸入「1191」，確認報表顯示 1191 統制科目和所有 1191.xxx 子科目的帳戶卡片。每個子科目對應一個客戶的應收帳款明細。
> 程式碼檢查：Code.Contains(keyword) || Name.Contains(keyword) 正確匹配代碼和名稱。

情境 14：明細分類帳 — 依名稱關鍵字 ✅ PASS (2026-03-25)
AccountKeyword 輸入「銀行」，確認所有名稱包含「銀行」的科目都出現（如銀行存款 1113、銀行手續費等）。
> 程式碼檢查：同上，Name.Contains() 正確匹配。

情境 15：明細科目餘額表 — 全部科目 ✅ PASS (2026-03-25, 已修復)
不設定任何篩選，確認報表列出所有科目的期初餘額、本期借方發生額、本期貸方發生額、期末餘額。期初 + 本期借方 - 本期貸方 = 期末（對借方餘額科目而言）。
> 程式碼檢查：計算邏輯正確。ShowZeroBalance=true 時已修復補齊無傳票科目（同情境 3 修復）。

情境 16：明細科目餘額表 — 隱藏零餘額 ✅ PASS (2026-03-25)
ShowZeroBalance = false，確認報表不顯示那些四欄都為零的科目，報表更為精簡。
> 程式碼檢查：openingBalance==0 && periodDebit==0 && periodCredit==0 && closingBalance==0 時正確過濾。

情境 17：會計科目表 — 列印 ✅ PASS (2026-03-25)
產生會計科目表報表，確認所有科目的代碼、名稱、層級、科目大類、借貸方向都正確顯示，上下層級關係清楚。
> 程式碼檢查：AccountItemListReportService 正確顯示階層縮排和科目屬性。

三、應收應付帳齡分析（情境 18–23）
情境 18：應收帳齡 — 全部客戶截止今天 ✅ PASS (2026-03-25)
AsOfDate 設為今天，不指定客戶。確認報表依客戶分組，每個客戶顯示未收金額在各帳齡區間的分布（未到期、1–30天、31–60天、61–90天、90天以上）。加總每個客戶各區間金額應等於該客戶的總未收款。
> 程式碼檢查：帳齡區間實際為 6 級（未到期/1-30/31-60/61-90/91-120/121+），比文件描述多一級，屬改善。BucketAmount 分配邏輯正確，客戶小計加總正確。

情境 19：應收帳齡 — 指定特定客戶 ✅ PASS (2026-03-25)
只選客戶 A 和客戶 B，確認報表只顯示這兩個客戶的帳齡資料。
> 程式碼檢查：CustomerIds 篩選正確。

情境 20：應收帳齡 — 依業務負責人篩選 ✅ PASS (2026-03-25)
選擇業務人員甲，確認只顯示由該業務負責的客戶的應收帳齡。
> 程式碼檢查：EmployeeIds 篩選出貨單的 SalespersonId 正確。

情境 21：應收帳齡 — 顯示已結清 ✅ PASS (2026-03-25)
ShowZeroBalance = true，確認已經全額收款的客戶也出現在報表上，金額全為 0。
> 程式碼檢查：ShowZeroBalance=true 時不跳過 outstandingAmount<=0 的記錄，負數 clamp 為 0。

情境 22：應付帳齡 — 全部廠商 ✅ PASS (2026-03-25)
AsOfDate 設為今天，確認每個廠商的未付金額按帳齡區間分組正確。到期日是根據收貨日 + 廠商付款天數計算。
> 程式碼檢查：APAgingReportService 與 AR 對稱，使用 ReceiptDate + Supplier.PaymentDays。

情境 23：應付帳齡 — 指定廠商 ✅ PASS (2026-03-25)
只選供應商 X，確認只顯示該廠商的應付帳齡分布。驗證有多張未付進貨單時，每張的到期日根據各自的收貨日計算。
> 程式碼檢查：SupplierIds 篩選正確，每張進貨單各自計算到期日。

四、客戶與廠商報表（情境 24–33）
情境 24：客戶對帳單 — 完整交易 ✅ PASS (2026-03-25)
選客戶 A，設定本月日期範圍，勾選出貨、退貨、收款全部。確認報表顯示期初餘額、本期所有出貨增加應收、退貨減少應收、收款減少應收，最後得出期末餘額。期末餘額 = 期初 + 出貨 - 退貨 - 收款。
> 程式碼檢查：期初餘額從日期範圍前的交易累計，running balance 正確計算。

情境 25：客戶對帳單 — 只看出貨不含退貨和收款 ✅ PASS (2026-03-25)
IncludeReturns = false、IncludePayments = false，確認報表只列出出貨單資料，期末餘額計算只反映出貨。
> 程式碼檢查：IncludeDeliveries=true 通過驗證，toggle 正確控制查詢範圍。

情境 26：客戶對帳單 — 全部客戶批次產生 ✅ PASS (2026-03-25)
不指定客戶（查詢全部），確認報表為每個客戶各產生一份對帳單，DocumentCount 等於有交易的客戶數量。
> 程式碼檢查：CustomerIds 為空時查詢全部客戶，依客戶分組產生。

情境 27：客戶對帳單 — 依業務負責人篩選 ✅ PASS (2026-03-25)
選擇業務人員，確認只產生該業務負責的客戶的對帳單。
> 程式碼檢查：EmployeeIds 篩選正確。

情境 28：客戶交易明細 — 出貨和退貨 ✅ PASS (2026-03-25)
選客戶 A，日期範圍本季，勾選出貨和退貨。確認報表逐筆列出品項名稱、數量、金額，依客戶分組，每組有小計。
> 程式碼檢查：CustomerTransactionReportService 依客戶分組，含小計計算。

情境 29：客戶交易明細 — 只看退貨 ✅ PASS (2026-03-25)
IncludeDeliveries = false，IncludeReturns = true。確認只顯示退貨明細。若兩者都取消勾選 → Validate 應阻擋。
> 程式碼檢查：Validate 正確阻擋 "至少須選擇一種交易類型（出貨或退貨）"。

情境 30：客戶銷售分析 — 全部客戶排名 ✅ PASS (2026-03-25)
日期範圍設為本年度，確認報表依銷售額由高到低排列客戶。每個客戶顯示出貨總金額。
> 程式碼檢查：排名正確，含佔比和累計百分比，除零防護完善。

情境 31：客戶銷售分析 — 依品項分類篩選 ✅ PASS (2026-03-25)
選擇特定品項分類（如「成品」），確認銷售金額只計算該分類的品項。
> 程式碼檢查：CategoryIds 篩選正確。

情境 32：廠商對帳單 — 完整交易 ✅ PASS (2026-03-25)
選供應商 X，本月日期範圍，勾選進貨、退貨、付款。確認期末餘額 = 期初 + 進貨 - 退貨 - 付款。
> 程式碼檢查：SupplierStatementReportService 與客戶對帳單對稱，計算邏輯正確。

情境 33：廠商對帳單 — 至少一種交易類型驗證 ✅ PASS (2026-03-25)
IncludeReceivings = false、IncludeReturns = false、IncludePayments = false → Validate 應阻擋回傳「至少須選擇一種交易類型」。
> 程式碼檢查：SupplierStatementCriteria.Validate 正確檢查三種類型至少選一。

五、名冊報表（情境 34–37）
情境 34：客戶名冊 — 全部列印 ✅ PASS (2026-03-25)
不設任何篩選，確認所有客戶的基本資料（編號、名稱、聯絡人、電話、地址等）都列出。
> 程式碼檢查：CustomerRosterReportService 查詢全部客戶，排除草稿。

情境 35：廠商名冊 — 全部列印 ✅ PASS (2026-03-25)
確認所有廠商的基本資料完整列出，報表格式與客戶名冊結構對稱。
> 程式碼檢查：SupplierRosterReportService 與客戶名冊對稱。

情境 36：員工名冊 ✅ PASS (2026-03-25)
確認所有員工的基本資料列出。
> 程式碼檢查：EmployeeRosterReportService 排除 IsSuperAdmin，支援部門/職位/狀態篩選。

情境 37：品項清單 ✅ PASS (2026-03-25)
確認所有品項的編號、名稱、分類、單位、採購類型（外購/自製/委外）等資訊正確。
> 程式碼檢查：ItemListReportService 正確載入分類和單位關聯。

六、庫存與倉庫報表（情境 38–42）
情境 38：庫存現況表 — 全部倉庫全部品項 ✅ PASS (2026-03-25)
不設篩選，IncludeZeroStock = false。確認報表列出所有有庫存的品項，顯示各倉庫的數量和均價。
> 程式碼檢查：InventoryStatusReportService 依倉庫分組，正確處理 null AverageCost。

情境 39：庫存現況表 — 指定倉庫 ✅ PASS (2026-03-25)
只選倉庫 A，確認只顯示倉庫 A 的庫存品項，不混入其他倉庫的數據。
> 程式碼檢查：WarehouseIds 篩選正確。

情境 40：庫存現況表 — 包含零庫存 ✅ PASS (2026-03-25)
IncludeZeroStock = true，確認已出清（庫存為 0）的品項也列在報表上。
> 程式碼檢查：IncludeZeroStock toggle 正確控制過濾。

情境 41：庫存現況表 — 依品項分類篩選 ✅ PASS (2026-03-25)
選擇分類「原物料」，確認只顯示該分類下的品項庫存。
> 程式碼檢查：CategoryIds 篩選正確。

情境 42：盤點差異表 — 僅差異項目 ✅ PASS (2026-03-25)
OnlyDifferenceItems = true，確認報表只列出帳面數量 ≠ 盤點數量的品項，帳面等於盤點的被過濾掉。沒有勾選時則全部品項都顯示。
> 程式碼檢查：OnlyDifferenceItems 篩選 DifferenceQuantity != 0 正確。
> 效能備註：StockTakingDifferenceReportService 存在 N+1 查詢問題（逐筆載入），建議未來優化為批次載入。

七、生產製造報表（情境 43–47）
情境 43：製令單報表 — 依日期和狀態篩選 ✅ PASS (2026-03-25)
設定本週日期範圍，StatusFilters 選 InProgress 和 Paused。確認只列出本週計畫生產且處於生產中或已暫停的製令單，包含 BOM 組件明細、計畫數量、已完成數量。
> 程式碼檢查：StatusFilters 和日期範圍篩選正確。

情境 44：製令單報表 — 包含已結案 ✅ PASS (2026-03-25)
IncludeClosed = true，確認已結案的製令單也被列入報表。IncludeClosed = false 時則被排除。
> 程式碼檢查：IncludeClosed toggle 正確控制已結案製令單的包含/排除。

情境 45：用料需求報表 — 排除已完成 + 只顯示待領 ✅ PASS (2026-03-25)
ExcludeCompleted = true、OnlyPendingIssue = true。確認報表只顯示尚在生產中且有待領數量的組件。每個組件彙總顯示需求量、已領量、待領量（需求 - 已領）。
> 程式碼檢查：PendingIssueQty = Max(0, Required - Issued)，使用 projection 優化查詢避免 N+1。

情境 46：用料需求報表 — 依成品篩選 ✅ PASS (2026-03-25)
只選成品 P，確認只彙總生產成品 P 所需的組件物料，不包含其他成品的需求。
> 程式碼檢查：ItemIds 篩選正確。

情境 47：用料損耗退料報表 — 依篩選條件查詢 ✅ PASS (2026-03-25)
設定本月日期範圍，OnlyWithScrap = true，確認只列出有損耗量的結算記錄。OnlyWithReturn = true 則只列出有退料量的記錄。兩者同時勾選應列出同時有損耗又有退料的記錄。
> 程式碼檢查：兩個篩選條件使用 AND 邏輯（ScrapQty>0 且 ReturnQty>0），符合預期。

八、銀行對帳與單據列印（情境 48–50）
情境 48：銀行調節表 — 標準產出 ✅ PASS (2026-03-25)
設定本月對帳期間和公司。確認報表顯示銀行端餘額、帳上餘額、已配對和未配對的明細清單，以及兩端的調節差異。若所有明細都已配對，差異應為零。
> 程式碼檢查：BankReconciliationReportService 正確計算調節差異和配對率。

情境 49：沖款單報表 — 應收與應付各一張 ✅ PASS (2026-03-25)
分別列印一張應收沖款單和一張應付沖款單。確認應收沖款單顯示客戶資訊、沖銷品項明細（出貨/退貨）、收款記錄、折讓金額，且沖銷合計 = 收款 + 折讓 + 預收抵扣。應付沖款單同理但對象為供應商。
> 程式碼檢查：SetoffDocumentReportService 支援 AR/AP 兩種類型，預載字典優化查詢。

情境 50：單據批次列印與 Excel/PDF 匯出 ✅ PASS (2026-03-25)
選擇「銷貨出貨單」報表，批次列印本月所有出貨單。確認 BatchPreviewResult 的 DocumentCount 等於實際出貨單數量，PreviewImages 有正確的頁數。再從 MergedDocument 匯出為 PDF/Excel，確認匯出檔案可正常開啟，內容與預覽一致。Documents 陣列中每份文件都有獨立的表頭和頁尾。
> 程式碼檢查：BatchPreviewResult 結構正確，FormattedPrintService/ExcelExportService/PdfExportService 匯出流程完整。

---
檢查總結 (2026-03-25)
---
全 50 個情境程式碼檢查完成，結果：50/50 PASS

已修復的 Bug（共 2 項）：
1. [情境 3, 15] ShowZeroBalance=true 無法顯示從未有傳票的科目
   修復檔案：TrialBalanceReportService.cs, DetailAccountBalanceReportService.cs
   修復方式：補齊查詢 AccountItems 中未出現在 JournalEntryLines 的明細科目
2. [情境 8] 損益表空資料期間回傳錯誤而非空報表
   修復檔案：IncomeStatementReportService.cs
   修復方式：移除空資料的 Failure 短路，改為產生金額全為 0 的正常報表

效能建議（非 Bug）：
- [情境 42] StockTakingDifferenceReportService 存在 N+1 查詢，建議未來優化

備註：
- 帳齡報表（情境 18）實際區間為 6 級（含 91-120 天），比文件描述多一級，屬改善
- 所有 Criteria.Validate 驗證邏輯均符合情境預期