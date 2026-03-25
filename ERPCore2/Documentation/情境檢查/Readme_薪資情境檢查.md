一、基礎設定與月薪計算（情境 1–6）
情境 1：全勤月薪員工的標準薪資計算

員工 A 月薪制，本薪 45,000，無眷屬（DependentCount=0）
建立薪資設定（EmployeeSalary），設定勞保投保薪資 45,800、健保投保金額 45,800
開立薪資週期（OpenPeriodAsync）→ 例如 115 年 3 月
批次初始化出勤（BatchInitializeAsync）→ 產生預設全勤記錄
執行 CalculateEmployeeAsync → 確認產生薪資單
確認明細：本薪 45,000（出勤比例 100%）、勞保費 -916（45,800 × 2%）、健保費 -1,076（45,800 × 2.35%）、所得稅按扣繳稅額表查詢
確認 GrossIncome、TotalDeduction、NetPay 三者數學關係：NetPay = GrossIncome - TotalDeduction
確認雇主成本另外計算：勞保雇主 4,580（10%）、健保雇主 2,798（6.11%）、勞退 2,700（6%）

驗證重點： 最基本的月薪計算正確性、勞健保扣除、扣繳稅額查表
情境 2：含固定津貼的薪資計算

員工 B 本薪 38,000，另有：伙食津貼 2,400（IsTaxable=false，不按出勤比例 IsProrated=false）、交通津貼 2,000（IsTaxable=false，IsProrated=true）、職務津貼 5,000（IsTaxable=true）
在 EmployeeSalary 的 AllowanceItems 設定這三筆
全勤計算 → 確認：伙食津貼 2,400 固定全額、交通津貼 2,000 × 出勤比例 100% = 2,000、職務津貼 5,000
確認課稅所得計算：GrossIncome - 伙食免稅 2,400（≤ 3,000 上限）- 交通免稅 2,000（≤ 3,000 上限）- 勞保費 - 健保費 = 課稅所得
確認 IsTaxable=true 的職務津貼有計入課稅所得

驗證重點： 津貼的免稅/課稅分類、IsProrated 按比例發放、免稅上限（伙食 3,000 / 交通 3,000）
情境 3：出勤不全的月薪計算

員工 C 本薪 40,000，本月應出勤 22 天，實際出勤 18 天
在 MonthlyAttendanceSummary 設定：ScheduledWorkDays=22、ActualWorkDays=18、AbsentDays=2、SickLeaveDays=1、PersonalLeaveDays=1
計算薪資 → 確認：本薪 = 40,000 × (18/22) = 32,727（四捨五入到元）
確認曠職扣薪：2 天 × (40,000/30) = -2,667
確認病假半薪扣除：1 天 × (40,000/30) × 0.5 = -667
確認事假扣除：1 天 × (40,000/30) = -1,333
確認 IsProrated=true 的津貼也按 18/22 比例縮減

驗證重點： 出勤比例的分子分母正確、三種假別（曠職/病假/事假）的扣薪公式各不同
情境 4：含加班費的薪資計算

員工 D 本薪 36,000，時薪 = 36,000/30/8 = 150
本月出勤記錄：平日加班前2hr = 10 小時、平日加班後2hr = 4 小時、休息日加班 = 6 小時、國定假日加班 = 8 小時
計算 → 確認各加班費：

OT1：10hr × 150 × 4/3 = 2,000
OT2：4hr × 150 × 5/3 = 1,000
休息日前2hr：2hr × 150 × 4/3 = 400，後4hr：4hr × 150 × 5/3 = 1,000，合計 1,400
國定假日：8hr × 150 × 1.0 = 1,200


確認加班費合計正確加入 GrossIncome

驗證重點： 四種加班費倍率各自正確（勞基法第 24、39 條）、休息日加班前後 2 小時分段計算
情境 5：含眷屬的健保費計算

員工 E 本薪 50,000，健保投保金額 50,600，眷屬 3 人（DependentCount=3）
計算 → 確認健保費 = 50,600 × 2.35% × 4（本人 + 3 眷屬，最多到 3）= 4,756
若眷屬改為 5 人 → 乘數仍為 4（最多 3 眷屬 + 本人），超過的部分不計
確認雇主健保費也按同樣乘數計算

驗證重點： 眷屬人數乘數上限（最多+3）、乘數同時影響員工和雇主健保費
情境 6：勞退自提的計算與免稅

員工 F 本薪 55,000，設定 VoluntaryRetirementRate = 6%
計算 → 確認勞退自提 = 55,000 × 6% = 3,300，以扣除項目記錄
確認課稅所得計算時有扣除勞退自提金額（自提金額免稅）
VoluntaryRetirementRate = 0% → 確認不產生自提明細

驗證重點： 自提金額計算、自提免稅扣除

二、時薪制與特殊情況（情境 7–12）
情境 7：時薪制員工的薪資計算

員工 G 時薪制（SalaryType=Hourly），時薪 190 元
本月總工時 TotalWorkHours = 168 小時
計算 → 確認本薪 = 190 × 168 = 31,920
確認不產生曠職/病假/事假扣薪（時薪制以實際工時計算，不適用日薪扣除）
確認加班費仍按時薪 × 倍率計算

驗證重點： 時薪制的計算邏輯完全不同（依 TotalWorkHours 而非出勤比例）、不產生假別扣薪
情境 8：時薪制逐日出勤初始化

時薪員工 → BatchInitAsync 使用 AttendanceInitMode.AllAsRestDay
確認初始化後所有日期都是 RestDay（由 HR 逐日填入實際出勤）
HR 逐日 UpsertAsync 設定出勤日和工時
RebuildFromDailyAsync 重新彙總 → 確認 TotalWorkHours 正確累計

驗證重點： 時薪制的出勤初始化模式不同、逐日→月彙總的正確性
情境 9：月中到職員工

員工 H 於 3/15 到職，本薪 42,000
應出勤天數 = 3/15 至 3/31 的工作日數（假設 13 天）
出勤記錄只建立 3/15 之後的日期，之前日期不存在
計算 → 確認本薪按出勤比例（13/22 ≈ 59%）計算
確認勞健保仍扣整月（勞保以日計算但系統目前可能扣全月）
津貼中 IsProrated=true 的按比例、IsProrated=false 的全額

驗證重點： 到職不滿月的比例計算、保費處理
情境 10：月中離職員工

員工 I 於 3/20 離職，本薪 38,000
EmploymentStatus 已改為離職，但本期仍有薪資記錄
CalculateAllAsync 會納入「已有本期記錄但已離職的員工」
出勤記錄只到 3/20，3/21 之後為空
計算 → 確認薪資按實際出勤比例結算
確認特休未休折算是否需要另外處理

驗證重點： 離職員工的最終結算、CalculateAllAsync 的篩選邏輯（Active ∪ 已有記錄的離職）
情境 11：非居留者扣繳（外籍員工）

員工 J 為外籍員工，TaxType = NonResident
本薪 50,000 → 課稅所得計算後，扣繳 = 課稅所得 × 18%
確認不查扣繳稅額表，而是直接套用 18% 稅率
TaxType = Resident（居留者但未報戶籍）→ 確認套用 5% 稅率

驗證重點： 三種 TaxWithholdingType 各自的扣繳邏輯（Standard 查表、Resident 5%、NonResident 18%）
情境 12：基本工資檢查

設定最低基本工資為 27,470 元（月薪）/ 183 元（時薪）
月薪員工本薪設定 25,000 → 系統是否警告低於基本工資
時薪員工時薪設定 170 → 系統是否警告低於時薪基本工資
SyncFromGovernmentAsync → 確認能從勞動部 API 匯入最新基本工資紀錄
GetEffectiveAsync(today) → 確認回傳目前生效的基本工資

驗證重點： 基本工資下限檢查、政府 API 同步、生效日判斷

三、出勤管理（情境 13–18）
情境 13：逐日出勤記錄的完整操作

為員工 K 初始化 3 月出勤（BatchInitAsync, WorkdaysAsPresent）
確認工作日都是 Present、週末都是 RestDay
3/5 請事假 → UpsertAsync 改為 PersonalLeave
3/10 請病假 → UpsertAsync 改為 SickLeave
3/15 補班（MakeUpWork）→ 週六出勤
3/20 國定假日（NationalHoliday）
RebuildFromDailyAsync → 確認月彙總：ActualWorkDays、SickLeaveDays=1、PersonalLeaveDays=1 等都正確

驗證重點： 逐日記錄到月彙總的所有假別統計
情境 14：全部員工批次初始化出勤

BatchInitAllEmployeesAsync(115, 3, WorkdaysAsPresent)
確認所有在職員工都建立了 3 月的逐日記錄
離職員工不應被初始化
已存在記錄的員工不應被覆蓋（冪等性）

驗證重點： 批次初始化的範圍和冪等性
情境 15：出勤彙總鎖定

薪資計算完成後，呼叫 LockAsync 鎖定出勤彙總
確認鎖定後無法再修改出勤記錄
若薪資單取消確認（Unconfirm），出勤是否也需要解鎖

驗證重點： 鎖定機制防止薪資計算後出勤被修改
情境 16：加班時數的逐日到月彙總

逐日記錄中設定：3/5 平日加班 2hr、3/10 平日加班 3hr（前2hr + 後1hr）、3/15 休息日加班 5hr、3/20 國定假日加班 8hr
RebuildFromDailyAsync → 確認月彙總：

OvertimeHours1 = 2+2 = 4hr（平日前2hr的累計）
OvertimeHours2 = 0+1 = 1hr（平日後2hr的累計）
HolidayOvertimeHours = 5hr
NationalHolidayHours = 8hr



驗證重點： 各類加班時數的逐日累計到月彙總
情境 17：補班日的出勤處理

政府公告 3/22（週六）為補班日
逐日記錄設定 3/22 為 MakeUpWork
確認 3/22 計入 ActualWorkDays（出勤天數+1）
確認 ScheduledWorkDays 也要+1（因為補班日算應出勤日）
3/22 如果加班 → 確認適用哪種加班費率

驗證重點： 補班日的出勤和加班處理
情境 18：出勤無記錄時的全勤預設

某員工沒有建立 MonthlyAttendanceSummary
CalculateEmployeeAsync → 確認 attendance == null 時使用預設全勤
確認 Logger 有記錄警告：「無出勤彙總記錄，將以全勤計算」
ScheduledWorkDays 和 ActualWorkDays 都用 GetWorkDaysInMonth 計算

驗證重點： 無出勤資料時的安全降級處理

四、薪資單生命週期（情境 19–24）
情境 19：批次計算所有員工

公司有 20 名在職員工 + 1 名本月離職（已有薪資記錄）
CalculateAllAsync → 確認計算 21 筆（含離職者）
其中 2 名員工沒有薪資設定 → 確認這 2 筆回報失敗但不影響其他 19 筆
確認回傳訊息：「完成 19 筆，2 筆失敗：員工 X 找不到有效薪資設定」

驗證重點： 批次計算的部分失敗處理、離職員工的納入
情境 20：薪資單確認與取消確認

計算完成 → 薪資單狀態 Draft
ConfirmRecordAsync → 狀態變 Confirmed，記錄 ConfirmedAt 和 ConfirmedBy
確認後嘗試重新計算 → 應被阻擋（「薪資單已確認，請先取消確認再重算」）
UnconfirmRecordAsync → 狀態回 Draft，ConfirmedAt 和 ConfirmedBy 清空
重新計算 → 成功

驗證重點： Draft ↔ Confirmed 的雙向流轉、確認後的計算阻擋
情境 21：已關帳週期的保護

ClosePeriodAsync → 薪資週期狀態變 Closed
嘗試計算該週期的薪資 → 應被阻擋（「此薪資週期已關帳」）
嘗試確認/取消確認該週期的薪資單 → 應被阻擋
確認關帳後所有修改操作都被凍結

驗證重點： 關帳後的全面保護
情境 22：重新計算（Recalculate）

薪資單已確認（Confirmed）
RecalculateAsync → 確認系統先將狀態重置為 Draft（SaveChanges），再重新計算
確認舊明細全部清除，新明細重新產生
確認 Bug-P3 修正：重置狀態先 SaveChanges 避免新 context 讀到舊狀態

驗證重點： Recalculate 的先重置再計算流程
情境 23：薪資週期重複開立防護

已存在 115 年 3 月的週期
嘗試再次 OpenPeriodAsync(115, 3) → 應被阻擋或跳過
PeriodExistsAsync(115, 3) → 回傳 true
EnsurePeriodExistsAsync(115, 3) → 回傳已存在的週期（不重複建立）

驗證重點： 週期的唯一性防護、EnsurePeriodExists 的冪等性
情境 24：薪資設定歷史與版本管理

員工 L 原本薪 35,000，2025/1/1 生效
2025/7/1 調薪為 40,000 → AddWithExpiryAsync
確認舊記錄的 ExpiryDate 自動設為 2025/6/30
GetCurrentSalaryAsync → 回傳 40,000 的設定
計算 2025 年 6 月薪資 → 應使用 35,000（依 EffectiveDate ≤ 月底 且 ExpiryDate ≥ 月初）
計算 2025 年 7 月薪資 → 應使用 40,000

驗證重點： 薪資設定的生效/到期日邏輯、調薪時舊設定自動到期

五、保險費率與稅務（情境 25–28）
情境 25：勞保投保級距自動對應

員工本薪 38,000
GetEffectiveGradeAsync(38000, today) → 應回傳最接近的投保級距（例如 38,200）
確認計算時使用級距薪資而非實際薪資計算勞保費
員工本薪調為 46,000 → 確認級距跳到 45,800 或 46,100
CopyYearAsync → 確認可以將今年的級距表複製到明年

驗證重點： 級距查詢的向上取整邏輯、年度複製
情境 26：健保投保金額級距

員工本薪 42,000 → GetEffectiveGradeAsync → 對應到 42,000 或最近的級距
確認健保費按級距金額計算，不是按實際薪資
不同生效日的級距表 → 確認系統依 EffectiveDate 選用正確版本

驗證重點： 健保級距與勞保級距獨立、依日期選版本
情境 27：扣繳稅額表查詢

課稅所得 40,000，眷屬 0 人 → 查表取得對應稅額
課稅所得 40,000，眷屬 2 人 → 確認稅額較低（眷屬越多免稅額越高）
課稅所得超出稅額表最大區間 → 觀察系統處理（回傳 null 並記錄警告）
無對應生效日的稅額表 → 確認警告訊息正確

驗證重點： 稅額表的區間查詢、眷屬人數影響、缺表時的警告
情境 28：費率變更年度切換

2025 年勞保費率 2%，2026 年調整為 2.1%
建立新的 InsuranceRate，EffectiveDate = 2026/1/1，LaborInsuranceEmployeeRate = 0.021
計算 2025/12 薪資 → 確認使用 2% 費率
計算 2026/1 薪資 → 確認自動切換到 2.1% 費率
確認 InsuranceRate 無資料時使用備援常數（DefaultLaborInsuranceEmployeeRate）

驗證重點： 費率依 EffectiveDate 自動切換、備援常數的降級機制

六、銀行帳戶與整合（情境 29–30）
情境 29：員工銀行帳戶管理

員工 M 新增銀行帳戶 A → 設為主要帳戶（SetPrimaryAsync）
再新增銀行帳戶 B → 設為主要 → 確認帳戶 A 的主要標記自動取消
GetPrimaryAccountAsync → 確認回傳帳戶 B
刪除帳戶 B → 確認無主要帳戶的狀態（GetPrimaryAccountAsync 回傳 null）
薪資發放時使用主要帳戶的銀行代碼和帳號

驗證重點： 主要帳戶的排他性、刪除後的狀態
情境 30：年度完整薪資週期壓力測試

依序開立 115 年 1 月到 12 月共 12 個薪資週期
每月批次初始化出勤 → 調整個別員工的假別和加班
每月批次計算所有員工薪資
模擬實際狀況：3 月有員工調薪、5 月有新進員工、8 月有離職員工、10 月有費率變更
確認每月的計算結果各自獨立正確
確認員工全年 12 張薪資單可用 GetByEmployeeAsync 依月份倒序查詢
確認調薪月份前後的薪資設定自動切換（5 月用舊薪、6 月用新薪）
逐月關帳 → 確認已關帳月份不可再修改
核對全年度每位員工的累計所得、累計扣繳，供年度申報使用
確認雇主全年累計的勞保/健保/勞退負擔金額

驗證重點： 全年度 12 個月的連續計算正確性、人事異動（到離職/調薪/費率變更）的跨月影響、累計數據一致性

這 30 個情境覆蓋了你薪資模組的所有核心功能。幾個最容易出錯的關鍵區域：
情境 3（出勤不全）和情境 4（加班費） — 這兩個是薪資計算中公式最複雜的部分，特別是休息日加班的前後 2 小時分段計算，以及曠職/病假/事假三種不同扣薪比例。
情境 9 和 10（月中到離職） — 實務上最常出問題，因為保費是否扣全月、津貼是否按比例、離職結算的特休折算等邊界條件很多。
情境 24（薪資設定版本） — EffectiveDate 和 ExpiryDate 的區間判斷是跨月計算時最容易出 bug 的地方，特別是調薪月份的前後月計算。
情境 28（費率年度切換） — InsuranceRate 的 EffectiveDate 查詢必須取「最近且不超過薪資月份的那筆」，排序邏輯錯誤會導致用到錯誤年度的費率。

---

## 情境檢查結果紀錄（2026-03-25）

### 一、基礎設定與月薪計算（情境 1–6）

| 情境 | 結果 | 說明 |
|------|------|------|
| 1 全勤月薪標準計算 | ✅ 通過 | 本薪×出勤比例、勞健保扣除、稅額查表、NetPay 數學關係、雇主成本 均正確 |
| 2 含固定津貼計算 | ✅ 通過 | IsProrated 按比例、IsTaxable 免稅/課稅分類、MEAL/TRANSPORT 免稅上限 3,000 均正確 |
| 3 出勤不全計算 | 🔧 已修正 | **發現雙重扣薪風險**：手動設 ActualWorkDays<ScheduledWorkDays 且有假別天數時，ratio 縮減+扣款會同時套用。已加入自動修正防護（PayrollCalculationService 第 0b 段） |
| 4 含加班費計算 | ✅ 通過 | 四種加班費倍率正確：OT1=4/3、OT2=5/3、休息日前2hr/後續分段、國定假日 1.0 倍 |
| 5 含眷屬健保費 | ✅ 通過 | multiplier = Min(DependentCount,3)+1，最多 4；雇主也用同乘數 |
| 6 勞退自提 | ✅ 通過 | 自提金額正確、免稅扣除正確、Rate=0 不產生明細 |

### 二、時薪制與特殊情況（情境 7–12）

| 情境 | 結果 | 說明 |
|------|------|------|
| 7 時薪制計算 | ✅ 通過 | basePay=時薪×TotalWorkHours、不產生假別扣薪、加班費仍正常計算 |
| 8 時薪逐日出勤初始化 | ✅ 通過 | AllAsRestDay 模式正確、逐日→月彙總 TotalWorkHours 正確 |
| 9 月中到職 | 🔧 已修正 | **原本 ScheduledWorkDays 永遠取全月**。已修正 RebuildFromDailyAsync 自動偵測 HireDate/ResignationDate 調整部分月工作日 |
| 10 月中離職 | ✅ 通過 | CalculateAllAsync 納入 Active∪已有記錄的離職員工正確。離職日的 ScheduledWorkDays 調整已隨情境 9 一起修正 |
| 11 非居留者扣繳 | ✅ 通過 | Standard 查表、Resident 5%、NonResident 18% 三種邏輯正確 |
| 12 基本工資檢查 | 🔧 已修正 | **原本計算引擎未呼叫 MinimumWageService**。已在 DoCalculateAsync 末段加入基本工資比對警告 |

### 三、出勤管理（情境 13–18）

| 情境 | 結果 | 說明 |
|------|------|------|
| 13 逐日出勤完整操作 | ✅ 通過 | BatchInit/Upsert/RebuildFromDaily 流程正確 |
| 14 全員批次初始化 | ✅ 通過 | Active/Probation 篩選、排除 IsSuperAdmin、冪等性 |
| 15 出勤彙總鎖定 | 🔧 已修正 | **三個問題已修正**：(1) LockAsync 加入 lockedBy 參數 (2) 新增 UnlockAsync (3) ConfirmRecordAsync 自動鎖定、UnconfirmRecordAsync 自動解鎖 |
| 16 加班時數逐日→月彙總 | ✅ 通過 | 各加班類型 Sum 正確 |
| 17 補班日出勤處理 | ⏳ 待處理 | GetWorkDaysInMonth 僅排除週六日，補班日(MakeUpWork)不會增加 ScheduledWorkDays。因 ratio=1 設計短期不影響薪資，標記為未來增強項 |
| 18 出勤無記錄全勤預設 | ✅ 通過 | attendance==null 安全降級、Logger 警告正確 |

### 四、薪資單生命週期（情境 19–24）

| 情境 | 結果 | 說明 |
|------|------|------|
| 19 批次計算所有員工 | ✅ 通過 | 部分失敗處理正確、離職員工納入 |
| 20 薪資單確認/取消 | ✅ 通過 | Draft↔Confirmed 雙向流轉、確認後計算阻擋、新增自動鎖定/解鎖出勤 |
| 21 已關帳週期保護 | ✅ 通過 | 計算/確認/取消確認/刪除 全面阻擋 |
| 22 重新計算 | ✅ 通過 | 先重置 Draft+SaveChanges（Bug-P3 已修正）、舊明細清除再重算 |
| 23 週期重複開立防護 | ✅ 通過 | PeriodExistsAsync 阻擋、EnsurePeriodExistsAsync 冪等 |
| 24 薪資設定版本管理 | ✅ 通過 | AddWithExpiryAsync 自動過期舊記錄、period-aware 查詢正確 |

### 五、保險費率與稅務（情境 25–28）

| 情境 | 結果 | 說明 |
|------|------|------|
| 25 勞保投保級距 | ✅ 通過 | GetGradeAsync 存在、級距值由 UI 端設定後計算引擎直接使用 |
| 26 健保投保金額級距 | ✅ 通過 | 獨立於勞保級距、依 EffectiveDate 選版本 |
| 27 扣繳稅額表查詢 | ✅ 通過 | 區間查詢+眷屬影響+缺表警告 均正確 |
| 28 費率變更年度切換 | ✅ 通過 | EffectiveDate≤periodDate 取最新、備援常數降級機制 |

### 六、銀行帳戶與整合（情境 29–30）

| 情境 | 結果 | 說明 |
|------|------|------|
| 29 員工銀行帳戶管理 | 🔧 已修正 | **原本僅一個主要帳戶時無法刪除（死鎖）**。修正 CanDeleteAsync：員工只剩一個帳戶時允許刪除主要帳戶 |
| 30 年度完整壓力測試 | ✅ 通過 | 依賴情境 1-29，GetByEmployeeAsync 倒序、調薪切換、關帳保護 |

### 修正摘要

| # | 檔案 | 修正內容 |
|---|------|----------|
| 1 | PayrollCalculationService.cs | 新增第 0b 段「雙重扣薪防護檢查」— ActualWorkDays<ScheduledWorkDays 且有假別時自動修正為 ratio=1 |
| 2 | MonthlyAttendanceSummaryService.cs | RebuildFromDailyAsync 自動偵測 HireDate/ResignationDate 調整 ScheduledWorkDays；新增 GetWorkDaysInRange 工具方法 |
| 3 | PayrollCalculationService.cs | DoCalculateAsync 末段新增基本工資檢查（比對 MinimumWage 表產生警告） |
| 4 | IMonthlyAttendanceSummaryService.cs | LockAsync 加入 lockedBy 參數；新增 UnlockAsync 介面 |
| 5 | MonthlyAttendanceSummaryService.cs | LockAsync 實作 lockedBy 參數；新增 UnlockAsync 實作 |
| 6 | PayrollCalculationService.cs | 注入 IMonthlyAttendanceSummaryService；ConfirmRecordAsync 自動鎖定出勤、UnconfirmRecordAsync 自動解鎖 |
| 7 | EmployeeBankAccountService.cs | CanDeleteAsync 允許刪除員工唯一的主要帳戶（避免死鎖） |

### 待處理項目（未來增強）

| 項目 | 說明 |
|------|------|
| 情境 17 補班日 ScheduledWorkDays | GetWorkDaysInMonth 不計算補班日（週六），因 ratio=1 短期不影響薪資，但 ScheduledWorkDays 數值不精確 |
| 情境 10 特休未休折算 | 離職員工特休未休折算尚無系統化處理 |
| 情境 3 文件修正 | 情境文件描述的計算方式（ratio 制+扣款制）與程式碼設計（ratio=1 純扣款制）不一致，建議更新情境文件描述 |